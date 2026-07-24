namespace Shared.Messaging.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishProductUpdatedAsync(
        string body,
        Guid messageId,
        CancellationToken cancellationToken = default);
}
