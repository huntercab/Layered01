using CartService.Business.Interfaces;
using CartService.DataAccess.Interfaces;
using CartService.Domain.ValueObjects;
using LiteDB;
using Shared.Messaging.Contracts;

namespace CartService.Business.CatalogEvents;

public sealed class ProductUpdatedEventHandler
{
    private readonly ILiteDatabase _database;
    private readonly ICartRepository _cartRepository;
    private readonly IInboxRepository _inboxRepository;

    public ProductUpdatedEventHandler(
        ILiteDatabase database,
        ICartRepository cartRepository,
        IInboxRepository inboxRepository)
    {
        _database = database;
        _cartRepository = cartRepository;
        _inboxRepository = inboxRepository;
    }

    public async Task HandleAsync(
        ProductUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        if (await _inboxRepository.ExistsAsync(
                integrationEvent.MessageId,
                cancellationToken))
        {
            return;
        }

        _database.BeginTrans();

        try
        {
            await _cartRepository.UpdateProductInAllCartsAsync(
                integrationEvent.ProductId,
                integrationEvent.Name,
                Money.Create(integrationEvent.PriceAmount, integrationEvent.Currency),
                ImageInfo.Create(integrationEvent.Image, ""));

            await _inboxRepository.AddAsync(
                integrationEvent.MessageId,
                cancellationToken);

            _database.Commit();
        }
        catch
        {
            _database.Rollback();
            throw;
        }
    }
}
