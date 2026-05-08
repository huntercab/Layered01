using Microsoft.Extensions.DependencyInjection;
using CatalogService.Application.Services;

namespace CatalogService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CategoryService>();
            services.AddScoped<ProductService>();

            return services;
        }
    }
}
