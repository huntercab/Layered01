using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Configuration;

namespace Shared.Messaging.RabbitMQ;

public sealed class RabbitMqTopology
{
    private readonly RabbitMqOptions _options;

    public RabbitMqTopology(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task DeclareAsync(
        IChannel channel,
        CancellationToken cancellationToken = default)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.ProductExchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.CartRetryExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.CartDeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var mainQueueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] =
                _options.CartDeadLetterExchange,

            ["x-dead-letter-routing-key"] =
                _options.CartDeadLetterQueue
        };

        await channel.QueueDeclareAsync(
            queue: _options.CartProductUpdatedQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.CartProductUpdatedQueue,
            exchange: _options.ProductExchange,
            routingKey: _options.ProductUpdatedRoutingKey,
            cancellationToken: cancellationToken);

        var retryQueueArguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] =
                _options.RetryDelayMilliseconds,

            ["x-dead-letter-exchange"] =
                _options.ProductExchange,

            ["x-dead-letter-routing-key"] =
                _options.ProductUpdatedRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: _options.CartRetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryQueueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.CartRetryQueue,
            exchange: _options.CartRetryExchange,
            routingKey: _options.CartRetryQueue,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.CartDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.CartDeadLetterQueue,
            exchange: _options.CartDeadLetterExchange,
            routingKey: _options.CartDeadLetterQueue,
            cancellationToken: cancellationToken);
    }
}
