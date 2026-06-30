namespace CatalogService.Api.Contracts
{
    public record CreateProductRequest(
        string Name,
        string? Description,
        string? Image,
        int CategoryId,
        MoneyDto Price,
        int Amount);

    public record UpdateProductRequest(
        string Name,
        string? Description,
        string? Image,
        int CategoryId,
        MoneyDto Price,
        int Amount);

    public record ProductResponse(
        int Id,
        string Name,
        string? Description,
        string? Image,
        int CategoryId,
        string? CategoryName,
        MoneyDto Price,
        int Amount,
        List<LinkDto>? Links = null);
}
