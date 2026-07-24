using CatalogService.Application.Common;
using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces;

public interface IProductRepository
{
    Task<PagedResult<Product>> GetPagedAsync(int? categoryId, int pageNumber, int pageSize);

    Task<IReadOnlyList<Product>> GetByCategoryIdAsync(int categoryId);

    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Product product);
}
