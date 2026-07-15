using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Messaging.Configuration
{
    public sealed class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public required string HostName { get; init; }
        public int Port { get; init; } = 5672;
        public required string UserName { get; init; }
        public required string Password { get; init; }
        public string VirtualHost { get; init; } = "/";

        public string ProductExchange { get; init; } = "catalog.product.events";
        public string ProductUpdatedRoutingKey { get; init; } = "catalog.product.updated";

        public string CartProductUpdatedQueue { get; init; } =
            "cart.catalog.product-updated";

        public string CartRetryExchange { get; init; } =
            "cart.catalog.retry";

        public string CartRetryQueue { get; init; } =
            "cart.catalog.product-updated.retry";

        public string CartDeadLetterExchange { get; init; } =
            "cart.catalog.dead-letter";

        public string CartDeadLetterQueue { get; init; } =
            "cart.catalog.product-updated.dead-letter";

        public int RetryDelayMilliseconds { get; init; } = 30_000;
        public int MaximumRetryCount { get; init; } = 5;
    }
}
