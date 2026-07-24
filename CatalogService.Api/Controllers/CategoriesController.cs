using Asp.Versioning;
using CatalogService.Api.Contracts;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        var response = categories
            .Select(category => ToResponse(category))
            .ToList();

        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "GetCategoryById")]
    public async Task<ActionResult<CategoryResponse>> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        return category is null ? (ActionResult<CategoryResponse>)NotFound() : (ActionResult<CategoryResponse>)Ok(ToResponseWithLinks(category));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        await _categoryService.AddAsync(
            request.Name,
            request.Image,
            request.ParentCategoryId);

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCategoryRequest request)
    {
        await _categoryService.UpdateAsync(
            id,
            request.Name,
            request.Image,
            request.ParentCategoryId);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);

        return NoContent();
    }

    private static CategoryResponse ToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Image,
            category.ParentCategoryId);
    }

    private CategoryResponse ToResponseWithLinks(Category category)
    {
        var version = "1.0";

        var links = new List<LinkDto>
    {
        new("self", Url.Link("GetCategoryById", new { version, id = category.Id })!, "GET"),
        new("update", $"/api/v{version}/categories/{category.Id}", "PUT"),
        new("delete", $"/api/v{version}/categories/{category.Id}", "DELETE"),
        new("all-categories", $"/api/v{version}/categories", "GET")
    };

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Image,
            category.ParentCategoryId,
            links);
    }
}
