using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Domain.ValueObject;

namespace CatalogService.Application.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;            
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
