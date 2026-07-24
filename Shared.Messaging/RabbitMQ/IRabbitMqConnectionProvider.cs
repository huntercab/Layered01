using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMQ;

public interface IRabbitMqConnectionProvider : IAsyncDisposable
{
    Task<IConnection> GetConnectionAsync(
        CancellationToken cancellationToken = default);
}
