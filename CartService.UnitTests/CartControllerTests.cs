using CartService.API.Contracts;
using CartService.API.Controllers.V1;
using CartService.Business.Interfaces;
using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CartService.UnitTests;

public sealed class CartControllerTests
{
    private readonly Mock<ICartService> _cartServiceMock;
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _cartServiceMock = new Mock<ICartService>();
        _controller = new CartController(_cartServiceMock.Object);
    }

    [Fact]
    public async Task GetCart_ShouldReturnOk_WithCartResponse()
    {
        var cartId = Guid.NewGuid();

        var items = new List<CartItem>
    {
        new CartItem(
            id: 1,
            name: "Keyboard",
            price: Money.Create(50, "USD"),
            quantity: 2
        )
    };

        _cartServiceMock
            .Setup(x => x.GetCartItemsAsync(cartId))
            .ReturnsAsync(items);

        var result = await _controller.GetCart(cartId.ToString());

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<CartResponse>(okResult.Value);

        Assert.Equal(cartId.ToString(), response.CartKey);
        Assert.Single(response.Items);
        Assert.Equal("Keyboard", response.Items.First().Name);
    }

    [Fact]
    public async Task AddItem_ShouldReturnBadRequest_WhenCartKeyIsInvalid()
    {
        var request = new CartItemRequest
        {
            Id = 1,
            Name = "Mouse",
            PriceAmount = 25,
            PriceCurrency = "USD",
            Quantity = 1
        };

        var result = await _controller.AddItem("invalid-key", request);

        Assert.IsType<BadRequestObjectResult>(result);

        _cartServiceMock.Verify(
            x => x.AddItemAsync(It.IsAny<Guid>(), It.IsAny<CartItem>()),
            Times.Never
        );
    }
}
