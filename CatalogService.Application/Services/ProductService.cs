using CatalogService.Application.Common;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Outbox;
using CatalogService.Domain.ValueObject;
using Shared.Messaging.Contracts;
using System.Text.Json;

namespace CatalogService.Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICatalogDbContext _dbContext;

        public ProductService(ICategoryRepository categoryRepository, IProductRepository productRepository, ICatalogDbContext dbContext)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _dbContext = dbContext;
        }

        public async Task<PagedResult<Product>> GetPagedAsync(int? categoryId, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            if (categoryId.HasValue && categoryId.Value <= 0)
                throw new ArgumentException("Invalid category id.");

            return await _productRepository.GetPagedAsync(categoryId, pageNumber, pageSize);
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid product id", nameof(id));

            return await _productRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(string name,int categoryId, decimal priceAmount, string priceCurrency, int amount, string? description = null, string? image = null)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category is null)
                throw new ArgumentException("Product category doesn't exist.", nameof(category));

            var price = new Money(priceAmount, priceCurrency);

            var product = new Product(name,categoryId, price,amount,description, image);

            await _productRepository.AddAsync(product);
        }

        public async Task UpdateAsync(int id, string name, int categoryId, decimal priceAmount, string priceCurrency, int amount, string? description = null, string? image = null)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Product id.", nameof(id));

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
                throw new ArgumentException("Product doesn't exist.", nameof(product));

            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category is null)
                throw new ArgumentException("Product category doesn't exist.", nameof(category));

            var price = new Money(priceAmount, priceCurrency);

            product.Update(name, categoryId, price, amount, description, image);
            await _productRepository.UpdateAsync(product);

            var integrationEvent = new ProductUpdatedIntegrationEvent(
            MessageId: Guid.NewGuid(),
            ProductId: product.Id,
            Name: product.Name,
            PriceAmount: product.Price.Amount,
            Currency: product.Price.Currency,
            Image: product.Image,
            OccurredOnUtc: DateTimeOffset.UtcNow);

            var outboxMessage = new OutboxMessage(
                id: integrationEvent.MessageId,
                type: nameof(ProductUpdatedIntegrationEvent),
                content: JsonSerializer.Serialize(integrationEvent),
                occurredOnUtc: integrationEvent.OccurredOnUtc);

            _dbContext.OutboxMessages.Add(outboxMessage);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Product id.", nameof(id));

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
                throw new ArgumentException("Product doesn't exist.", nameof(product));

            await _productRepository.DeleteAsync(product);
        }
    }
}
