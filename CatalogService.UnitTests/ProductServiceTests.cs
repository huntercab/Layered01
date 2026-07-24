using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using Moq;

namespace CatalogService.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ICatalogDbContext> _catalogDbContextMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _catalogDbContextMock = new Mock<ICatalogDbContext>();

        _productService = new ProductService(_categoryRepositoryMock.Object, _productRepositoryMock.Object, _catalogDbContextMock.Object);
    }


    [Fact]
    public async Task AddAsync_ShouldThrowException_WhenCategoryDoesntExist()
    {
        _categoryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        await Assert.ThrowsAsync<ArgumentException>(() =>
        _productService.AddAsync("prod1", 99, 25.25m, "USD", 2));

        _productRepositoryMock.Verify(
            repo => repo.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    //ShouldAdd_WhenCategoryExist, ShoudlUpdate, ShouldDelete, Exceptions
}
