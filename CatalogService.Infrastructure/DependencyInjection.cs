using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;

namespace CatalogService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CatalogDb"));
            });

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }

    }
}
