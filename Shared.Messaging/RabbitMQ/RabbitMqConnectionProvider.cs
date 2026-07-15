using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.RabbitMQ
{
    public sealed class RabbitMqConnectionProvider
    : IRabbitMqConnectionProvider
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private IConnection? _connection;

        public RabbitMqConnectionProvider(
            IOptions<RabbitMqOptions> options)
        {
            RabbitMqOptions settings = options.Value;

            _connectionFactory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                VirtualHost = settings.VirtualHost,

                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,

                ClientProvidedName =
                    $"{Environment.MachineName}-{AppDomain.CurrentDomain.FriendlyName}"
            };
        }

        public async Task<IConnection> GetConnectionAsync(
            CancellationToken cancellationToken = default)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            await _lock.WaitAsync(cancellationToken);

            try
            {
                if (_connection is { IsOpen: true })
                {
                    return _connection;
                }

                if (_connection is not null)
                {
                    await _connection.DisposeAsync();
                }

                _connection =
                    await _connectionFactory.CreateConnectionAsync(
                        cancellationToken);

                return _connection;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }

            _lock.Dispose();
        }
    }
}
