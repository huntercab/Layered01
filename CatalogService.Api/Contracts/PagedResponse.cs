namespace CatalogService.Api.Contracts
{
    public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    List<LinkDto>? Links = null);
}
