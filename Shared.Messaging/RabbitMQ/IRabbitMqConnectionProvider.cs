using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.RabbitMQ
{
    public interface IRabbitMqConnectionProvider : IAsyncDisposable
    {
        Task<IConnection> GetConnectionAsync(
            CancellationToken cancellationToken = default);
    }
}
