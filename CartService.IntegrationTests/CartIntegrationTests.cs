using CartService.DataAccess.Repositories;
using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;
using LiteDB;

namespace CartService.IntegrationTests;

public class CartIntegrationTests : IDisposable
{
    private readonly string _databaseFilePath;
    private readonly ILiteDatabase _database;
    private readonly LiteDbCartRepository _cartRepository;
    private readonly Business.Services.CartService _cartService;

    public CartIntegrationTests()
    {
        _databaseFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.db");

        var connectionString =
            $"Filename={_databaseFilePath};Connection=shared";

        _database = new LiteDatabase(connectionString);

        _cartRepository = new LiteDbCartRepository(_database);

        _cartService =
            new Business.Services.CartService(_cartRepository);
    }

    [Fact]
    public async Task AddItemAsync_ShouldPresistItem_InLiteDb()
    {
        var cartId = Guid.NewGuid();

        var item = CreateCartItem(id: 1, name: "item1", quantity: 1);

        await _cartService.AddItemAsync(cartId, item);

        var items = await _cartService.GetCartItemsAsync(cartId);

        Assert.Single(items);

        var savedItem = items.First();

        Assert.Equal(1, savedItem.Id);
        Assert.Equal("item1", savedItem.Name);
        Assert.Equal(1, savedItem.Quantity);
        Assert.Equal(25, savedItem.Price.Amount);
        Assert.Equal("USD", savedItem.Price.Currency);
    }

    [Fact]
    public async Task AddItemAsync_ShouldIncreaseQuantity_WhenSameItemIsAddedTwice()
    {
        var cartId = Guid.NewGuid();

        await _cartService.AddItemAsync(cartId, CreateCartItem(quantity: 1));
        await _cartService.AddItemAsync(cartId, CreateCartItem(quantity: 2));

        var items = await _cartService.GetCartItemsAsync(cartId);

        Assert.Single(items);
        Assert.Equal(3, items.First().Quantity);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldDeleteItem_FromPersistedCart()
    {
        var cartId = Guid.NewGuid();

        await _cartService.AddItemAsync(cartId, CreateCartItem(id: 1, name: "item1"));
        await _cartService.AddItemAsync(cartId, CreateCartItem(id: 2, name: "item2"));

        await _cartService.RemoveItemAsync(cartId, 1);

        var items = await _cartService.GetCartItemsAsync(cartId);

        Assert.Single(items);
        Assert.Equal(2, items.First().Id);
        Assert.Equal("item2", items.First().Name);
    }

    [Fact]
    public async Task AddItemAsync_ShouldPersistImageInfo_WhenImageIsProvided()
    {
        var cartId = Guid.NewGuid();

        var image = ImageInfo.Create("http://url.com/item1", "item1 desc");

        var item = new CartItem(5, "item1", Money.Create(11, "USD"), 1, image);

        await _cartService.AddItemAsync(cartId, item);

        var items = await _cartService.GetCartItemsAsync(cartId);
        var savedItem = items.First();

        Assert.NotNull(savedItem.Image);
        Assert.Equal("http://url.com/item1", savedItem.Image.Url);
        Assert.Equal("item1 desc", savedItem.Image.AltText);
    }

    private static CartItem CreateCartItem(
        int id = 1,
        string name = "book1",
        decimal amount = 25,
        string currency = "USD",
        int quantity = 1
        )
    {
        return new CartItem(
            id: id,
            name: name,
            price: Money.Create(amount, currency),
            quantity: quantity
            );
    }

    public void Dispose()
    {
        _database.Dispose();

        if (File.Exists(_databaseFilePath))
        {
            File.Delete(_databaseFilePath);
        }

        var logFilePath = _databaseFilePath + "-log";

        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }
    }
}
