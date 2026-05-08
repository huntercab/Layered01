using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyList<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);

        Task<bool> HasProductsAsync(int categoryId);
        Task<bool> HasSubCategoriesAsync(int categoryId);
    }
}
