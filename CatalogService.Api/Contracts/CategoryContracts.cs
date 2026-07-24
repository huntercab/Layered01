namespace CatalogService.Api.Contracts;

public record CreateCategoryRequest(
string Name,
string? Image,
int? ParentCategoryId);

public record UpdateCategoryRequest(
    string Name,
    string? Image,
    int? ParentCategoryId);

public record CategoryResponse(
    int Id,
    string Name,
    string? Image,
    int? ParentCategoryId,
    List<LinkDto>? Links = null);
