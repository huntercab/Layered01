using CatalogService.Domain.Entities;
using CatalogService.Domain.Outbox;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Application.Interfaces;

public interface ICatalogDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
