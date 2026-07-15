using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using CatalogService.Api.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using CatalogService.Api.Contracts;
using Microsoft.AspNetCore.Http;

namespace CatalogService.UnitTests
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductRepository> _productRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<ICatalogDbContext> _dbContextMock;
        private readonly ProductService _productService;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _dbContextMock = new Mock<ICatalogDbContext>();
            _productService = new ProductService(
                _categoryRepositoryMock.Object,
                _productRepositoryMock.Object,
                _dbContextMock.Object
                );

            _controller = new ProductsController(_productService);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            _productRepositoryMock
                .Setup(repository => repository.GetByIdAsync(99))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
        {
            // Arrange
            var category = new Category("Electronics");

            _categoryRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync(category);

            var request = new CreateProductRequest(
                Name: "Laptop",
                Description: "<p>Powerful laptop</p>",
                Image: "https://example.com/laptop.jpg",
                CategoryId: 1,
                Price: new MoneyDto(1200m, "USD"),
                Amount: 10);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);

            Assert.Equal(StatusCodes.Status201Created, statusCodeResult.StatusCode);

            _productRepositoryMock.Verify(
                repository => repository.AddAsync(It.Is<Product>(product =>
                    product.Name == "Laptop" &&
                    product.CategoryId == 1 &&
                    product.Price.Amount == 1200m &&
                    product.Price.Currency == "USD" &&
                    product.Amount == 10 &&
                    product.Description == "<p>Powerful laptop</p>" &&
                    product.Image == "https://example.com/laptop.jpg")),
                Times.Once);
        }
    }
}
