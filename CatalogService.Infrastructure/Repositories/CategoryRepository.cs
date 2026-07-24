using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _catalogContext;

    public CategoryRepository(CatalogDbContext catalogDbContext)
    {
        _catalogContext = catalogDbContext;
    }
    public async Task AddAsync(Category category)
    {
        await _catalogContext.Categories.AddAsync(category);
        await _catalogContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        _catalogContext.Categories.Remove(category);
        await _catalogContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await _catalogContext.Categories.Include(c => c.ParentCategory).AsNoTracking().ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _catalogContext.Categories.Include(c => c.ParentCategory).FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await _catalogContext.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<bool> HasSubCategoriesAsync(int categoryId)
    {
        return await _catalogContext.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);
    }

    public async Task UpdateAsync(Category category)
    {
        _catalogContext.Categories.Update(category);
        await _catalogContext.SaveChangesAsync();
    }
}
