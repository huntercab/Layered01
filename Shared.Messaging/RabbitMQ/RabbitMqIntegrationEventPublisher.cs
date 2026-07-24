using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Abstractions;
using Shared.Messaging.Configuration;

namespace Shared.Messaging.RabbitMQ;

public sealed class RabbitMqIntegrationEventPublisher
: IIntegrationEventPublisher
{
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly RabbitMqTopology _topology;
    private readonly RabbitMqOptions _options;

    public RabbitMqIntegrationEventPublisher(
        IRabbitMqConnectionProvider connectionProvider,
        RabbitMqTopology topology,
        IOptions<RabbitMqOptions> options)
    {
        _connectionProvider = connectionProvider;
        _topology = topology;
        _options = options.Value;
    }

    public async Task PublishProductUpdatedAsync(
        string body,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        IConnection connection =
            await _connectionProvider.GetConnectionAsync(
                cancellationToken);

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using IChannel channel =
            await connection.CreateChannelAsync(
                channelOptions,
                cancellationToken);

        await _topology.DeclareAsync(
            channel,
            cancellationToken);

        var properties = new BasicProperties
        {
            MessageId = messageId.ToString(),
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            Type = "catalog.product.updated",
            Timestamp = new AmqpTimestamp(
                DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        byte[] payload = Encoding.UTF8.GetBytes(body);

        await channel.BasicPublishAsync(
            exchange: _options.ProductExchange,
            routingKey: _options.ProductUpdatedRoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: payload,
            cancellationToken: cancellationToken);
    }
}
