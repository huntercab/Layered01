using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CatalogService.Application.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;

        public CategoryService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Category id", nameof(id));

            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(string name, string? image = null, int? parentCategoryId = null)
        {
            if (parentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(parentCategoryId.Value);

                if(parentCategory is null)
                    throw new ArgumentException("Parent Category doesn't exist.", nameof(parentCategory));
            }

            var category = new Category(name, image, parentCategoryId);
            await _categoryRepository.AddAsync(category);
        }

        public async Task UpdateAsync(int id, string name, string? image = null, int? parentCategoryId = null)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Category id.", nameof(id));

            if (parentCategoryId.HasValue && parentCategoryId.Value == id)
                throw new ArgumentException("Parent Category and current category can not be the same", nameof(parentCategoryId));

            var category = new Category(name, image, parentCategoryId);

            if(category is null)
                throw new ArgumentException("Category not found", nameof(category));

            if (parentCategoryId.HasValue)
            {
                var parentCategory = await _categoryRepository.GetByIdAsync(parentCategoryId.Value);

                if (parentCategory is null)
                    throw new ArgumentException("Parent Category doesn't exist.", nameof(parentCategory));
            }

            category.Update(name, image,parentCategoryId);
            await _categoryRepository.UpdateAsync(category);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid Category id.", nameof(id));

            var category = await _categoryRepository.GetByIdAsync(id);

            if (category is null)
                throw new ArgumentException("Category not found", nameof(category));

            var hasChildren = await _categoryRepository.HasSubCategoriesAsync(id);
            if (hasChildren)
                throw new ArgumentException("Can not delete a category with subcategories");

            var relatedProducts = await _productRepository.GetByCategoryIdAsync(id);

            foreach (var product in relatedProducts)
            {
                await _productRepository.DeleteAsync(product);
            }

            await _categoryRepository.DeleteAsync(category);
        }
    }
}
