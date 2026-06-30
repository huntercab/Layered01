using CatalogService.Application.Common;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _catalogDbContext;

        public ProductRepository(CatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;
        }

        public async Task<PagedResult<Product>> GetPagedAsync(int? categoryId, int pageNumber, int pageSize)
        {
            var query = _catalogDbContext.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>(
                items,
                pageNumber,
                pageSize,
                totalCount);
        }

        public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _catalogDbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _catalogDbContext.Products.AddAsync(product);
            await _catalogDbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _catalogDbContext.Products.Remove(product);
            await _catalogDbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            return await _catalogDbContext.Products.Include(p => p.Category).AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _catalogDbContext.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task UpdateAsync(Product product)
        {
            _catalogDbContext.Products.Update(product);
            await _catalogDbContext.SaveChangesAsync();
        }
    }
}
