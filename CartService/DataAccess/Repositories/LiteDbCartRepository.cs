using CartService.DataAccess.Documents;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Mappers;
using CartService.Domain.Entities;
using CartService.Domain.ValueObjects;
using LiteDB;

namespace CartService.DataAccess.Repositories;

public class LiteDbCartRepository : ICartRepository
{
    private const string CollectionName = "cart";
    private readonly ILiteDatabase _database;

    public LiteDbCartRepository(ILiteDatabase database)
    {
        _database = database;
    }

    public Task<Cart?> GetByIdAsync(Guid cartId)
    {
        var collection = _database.GetCollection<CartDocument>(CollectionName);

        var document = collection.FindById(cartId);

        return Task.FromResult(document is null ? null : CartMapper.ToDomain(document));
    }

    public Task UpdateProductInAllCartsAsync(int productId, string name, Money price, ImageInfo? image)
    {
        var collection = _database.GetCollection<CartDocument>(CollectionName);

        var documents = collection.FindAll().ToList();

        foreach (var document in documents)
        {
            Cart cart = CartMapper.ToDomain(document);

            CartItem? item = cart.Items
                .FirstOrDefault(x => x.Id == productId);

            if (item is null)
            {
                continue;
            }

            item.SynchronizeCatalogData(
                name,
                price,
                image);

            collection.Update(
                CartMapper.ToDocument(cart));
        }

        return Task.CompletedTask;
    }

    public Task SaveAsync(Cart cart)
    {
        ArgumentNullException.ThrowIfNull(cart);

        var collection = _database.GetCollection<CartDocument>(CollectionName);

        var document = CartMapper.ToDocument(cart);

        collection.Upsert(document);

        return Task.CompletedTask;
    }
}
