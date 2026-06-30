using Asp.Versioning;
using CatalogService.Api.Contracts;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<ProductResponse>>> GetPaged(
            [FromQuery] int? categoryId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetPagedAsync(
                categoryId,
                pageNumber,
                pageSize);

            var items = result.Items
                .Select(product => ToResponse(product))
                .ToList();

            var version = "1.0";

            var links = new List<LinkDto>
        {
            new("self", $"/api/v{version}/products?categoryId={categoryId}&pageNumber={pageNumber}&pageSize={pageSize}", "GET"),
            new("create-product", $"/api/v{version}/products", "POST")
        };

            return Ok(new PagedResponse<ProductResponse>(
                items,
                result.PageNumber,
                result.PageSize,
                result.TotalCount,
                result.TotalPages,
                links));
        }

        [HttpGet("{id:int}", Name = "GetProductById")]
        public async Task<ActionResult<ProductResponse>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product is null)
                return NotFound();

            return Ok(ToResponseWithLinks(product));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            await _productService.AddAsync(
                request.Name,
                request.CategoryId,
                request.Price.Amount,
                request.Price.Currency,
                request.Amount,
                request.Description,
                request.Image);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateProductRequest request)
        {
            await _productService.UpdateAsync(
                id,
                request.Name,
                request.CategoryId,
                request.Price.Amount,
                request.Price.Currency,
                request.Amount,
                request.Description,
                request.Image);

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }

        private static ProductResponse ToResponse(Product product)
        {
            return new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Image,
                product.CategoryId,
                product.Category?.Name,
                new MoneyDto(product.Price.Amount, product.Price.Currency),
                product.Amount);
        }

        private ProductResponse ToResponseWithLinks(Product product)
        {
            var version = "1.0";

            var links = new List<LinkDto>
        {
            new("self", Url.Link("GetProductById", new { version, id = product.Id })!, "GET"),
            new("update", $"/api/v{version}/products/{product.Id}", "PUT"),
            new("delete", $"/api/v{version}/products/{product.Id}", "DELETE"),
            new("category", $"/api/v{version}/categories/{product.CategoryId}", "GET"),
            new("all-products", $"/api/v{version}/products", "GET")
        };

            return new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Image,
                product.CategoryId,
                product.Category?.Name,
                new MoneyDto(product.Price.Amount, product.Price.Currency),
                product.Amount,
                links);
        }
    }
}
