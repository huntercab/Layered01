using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;

namespace CartService.UnitTests;

public class CartItemTests
{
    [Fact]
    public void Constructor_ShouldCreateCartItem_WhenDataIsValid()
    {
        var item = new CartItem(1, "itemName1", Money.Create(25, "USD"), 2);

        Assert.Equal(1, item.Id);
        Assert.Equal("itemName1", item.Name);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() =>
            new CartItem(1, "", Money.Create(25, "USD"), 1)
        );
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenQuantityIsInvalid()
    {
        Assert.Throws<ArgumentException>(() =>
            new CartItem(1, "ietmName1", Money.Create(25, "USD"), 0)
        );
    }
}
