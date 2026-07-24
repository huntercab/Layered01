using System.Globalization;
using System.Text;
using System.Text.Json;
using CartService.Business.CatalogEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging.Configuration;
using Shared.Messaging.Contracts;
using Shared.Messaging.RabbitMQ;

namespace CartService.DataAccess.Messaging;

public sealed class ProductUpdatedConsumer : BackgroundService
{
    private const string RetryHeader = "x-retry-count";

    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly RabbitMqTopology _topology;
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProductUpdatedConsumer> _logger;

    private IChannel? _channel;

    public ProductUpdatedConsumer(
        IRabbitMqConnectionProvider connectionProvider,
        RabbitMqTopology topology,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<ProductUpdatedConsumer> logger)
    {
        _connectionProvider = connectionProvider;
        _topology = topology;
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        IConnection connection =
            await _connectionProvider.GetConnectionAsync(
                stoppingToken);

        _channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await _topology.DeclareAsync(
            _channel,
            stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 10,
            global: false,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: _options.CartProductUpdatedQueue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task HandleMessageAsync(
        object sender,
        BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException(
                "RabbitMQ channel has not been initialized.");
        }

        try
        {
            ProductUpdatedIntegrationEvent integrationEvent =
                Deserialize(eventArgs.Body);

            using IServiceScope scope =
                _scopeFactory.CreateScope();

            var handler =
                scope.ServiceProvider
                    .GetRequiredService<ProductUpdatedEventHandler>();

            await handler.HandleAsync(
                integrationEvent,
                eventArgs.CancellationToken);

            await _channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken: eventArgs.CancellationToken);

            _logger.LogInformation(
                "Processed product update {MessageId} for product {ProductId}.",
                integrationEvent.MessageId,
                integrationEvent.ProductId);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Invalid product update message. Sending it to dead-letter queue.");

            await PublishToDeadLetterQueueAsync(
                eventArgs,
                "Invalid JSON message");

            await AcknowledgeOriginalAsync(eventArgs);
        }
        catch (Exception exception)
        {
            int retryCount = GetRetryCount(
                eventArgs.BasicProperties);

            _logger.LogWarning(
                exception,
                "Product update processing failed. Retry {RetryCount}/{MaximumRetryCount}.",
                retryCount,
                _options.MaximumRetryCount);

            if (retryCount < _options.MaximumRetryCount)
            {
                await PublishToRetryQueueAsync(
                    eventArgs,
                    retryCount + 1);
            }
            else
            {
                await PublishToDeadLetterQueueAsync(
                    eventArgs,
                    exception.Message);
            }

            /*
             * ACK only after the retry/DLQ publication succeeds.
             * If publication throws, the original delivery remains
             * unacknowledged and can be redelivered.
             */
            await AcknowledgeOriginalAsync(eventArgs);
        }
    }

    private static ProductUpdatedIntegrationEvent Deserialize(
        ReadOnlyMemory<byte> body)
    {
        ProductUpdatedIntegrationEvent? integrationEvent =
            JsonSerializer.Deserialize<ProductUpdatedIntegrationEvent>(
                body.Span);

        return integrationEvent
            ?? throw new JsonException(
                "Product update message was null.");
    }

    private async Task PublishToRetryQueueAsync(
        BasicDeliverEventArgs eventArgs,
        int retryCount)
    {
        ArgumentNullException.ThrowIfNull(_channel);

        BasicProperties properties =
            CopyProperties(eventArgs.BasicProperties);

        properties.Headers ??=
            new Dictionary<string, object?>();

        properties.Headers[RetryHeader] = retryCount;
        properties.DeliveryMode = DeliveryModes.Persistent;

        await _channel.BasicPublishAsync(
            exchange: _options.CartRetryExchange,
            routingKey: _options.CartRetryQueue,
            mandatory: true,
            basicProperties: properties,
            body: eventArgs.Body,
            cancellationToken: eventArgs.CancellationToken);
    }

    private async Task PublishToDeadLetterQueueAsync(
        BasicDeliverEventArgs eventArgs,
        string reason)
    {
        ArgumentNullException.ThrowIfNull(_channel);

        BasicProperties properties =
            CopyProperties(eventArgs.BasicProperties);

        properties.Headers ??=
            new Dictionary<string, object?>();

        properties.Headers["x-failure-reason"] = reason;
        properties.Headers["x-failed-on-utc"] =
            DateTimeOffset.UtcNow.ToString(
                "O",
                CultureInfo.InvariantCulture);

        properties.DeliveryMode = DeliveryModes.Persistent;

        await _channel.BasicPublishAsync(
            exchange: _options.CartDeadLetterExchange,
            routingKey: _options.CartDeadLetterQueue,
            mandatory: true,
            basicProperties: properties,
            body: eventArgs.Body,
            cancellationToken: eventArgs.CancellationToken);
    }

    private async Task AcknowledgeOriginalAsync(
        BasicDeliverEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(_channel);

        await _channel.BasicAckAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false,
            cancellationToken: eventArgs.CancellationToken);
    }

    private static int GetRetryCount(
        IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null ||
            !properties.Headers.TryGetValue(
                RetryHeader,
                out object? value) ||
            value is null)
        {
            return 0;
        }

        return value switch
        {
            byte number => number,
            short number => number,
            int number => number,
            long number => checked((int)number),

            byte[] bytes when
                int.TryParse(
                    Encoding.UTF8.GetString(bytes),
                    out int number) =>
                number,

            _ => 0
        };
    }

    private static BasicProperties CopyProperties(
        IReadOnlyBasicProperties source)
    {
        return new BasicProperties
        {
            MessageId = source.MessageId,
            ContentType = source.ContentType,
            ContentEncoding = source.ContentEncoding,
            CorrelationId = source.CorrelationId,
            Type = source.Type,
            AppId = source.AppId,
            Timestamp = source.Timestamp,
            DeliveryMode = DeliveryModes.Persistent,

            Headers = source.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(
                    source.Headers)
        };
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
