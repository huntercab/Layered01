using CatalogService.Application.Interfaces;
using CatalogService.Domain.Outbox;
using Microsoft.EntityFrameworkCore;
using CatalogService.Domain.ValueObject;
using Shared.Messaging.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CatalogService.Application.Products
{
    public sealed class UpdateProductCommandHandler
    {
        private readonly ICatalogDbContext _dbContext;

        public UpdateProductCommandHandler(
            ICatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(
            UpdateProductCommand command,
            CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products
                .SingleOrDefaultAsync(
                    x => x.Id == command.ProductId,
                    cancellationToken);

            if (product is null)
            {
                throw new KeyNotFoundException(
                    $"Product {command.ProductId} was not found.");
            }

            product.Update(
                command.Name,
                command.CategoryId,
                command.Price,
                command.Amount,
                command.Description,
                command.ImageUrl);

            var integrationEvent =
                new ProductUpdatedIntegrationEvent(
                    MessageId: Guid.NewGuid(),
                    ProductId: product.Id,
                    Name: product.Name,
                    PriceAmount: product.Price.Amount,
                    Currency: product.Price.Currency,
                    Image: product.Image,
                    OccurredOnUtc: DateTimeOffset.UtcNow);

            var outboxMessage = new OutboxMessage(
                id: integrationEvent.MessageId,
                type: nameof(ProductUpdatedIntegrationEvent),
                content: JsonSerializer.Serialize(integrationEvent),
                occurredOnUtc: integrationEvent.OccurredOnUtc);

            _dbContext.OutboxMessages.Add(outboxMessage);

            // Product and OutboxMessage are committed in one DB transaction.
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
