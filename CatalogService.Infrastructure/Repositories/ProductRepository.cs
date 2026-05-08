using Microsoft.EntityFrameworkCore;
using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Data;
using CatalogService.Domain.Entities;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _catalogDbContext;

        public ProductRepository(CatalogDbContext catalogDbContext)
        {
            _catalogDbContext = catalogDbContext;
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
