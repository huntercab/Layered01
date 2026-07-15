using CatalogService.Domain.ValueObject;

namespace CatalogService.Application.Products
{
    public sealed record UpdateProductCommand(
    int ProductId,
    string Name,
    string? Description,
    string? ImageUrl,
    int CategoryId,
    Money Price,
    int Amount);
}
