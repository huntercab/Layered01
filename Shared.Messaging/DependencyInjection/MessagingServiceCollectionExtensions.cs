using Shared.Messaging.Abstractions;
using Shared.Messaging.Configuration;
using Shared.Messaging.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Messaging.DependencyInjection
{
    public static class MessagingServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMqMessaging(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<RabbitMqOptions>()
                .Bind(
                    configuration.GetSection(
                        RabbitMqOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IRabbitMqConnectionProvider,
                RabbitMqConnectionProvider>();

            services.AddSingleton<RabbitMqTopology>();

            return services;
        }
    }
}
