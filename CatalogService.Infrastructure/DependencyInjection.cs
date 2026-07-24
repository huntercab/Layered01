using CatalogService.Application.Interfaces;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection String CatalogDb was not found", nameof(connectionString));
        }

        var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var databaseFolder = Path.Combine(programDataPath, "CatalogService", "Data");

        Directory.CreateDirectory(databaseFolder);

        //var databaseFilePath = Path.Combine(databaseFolder, "CatalogServiceDb.mdf");

        //connectionString.Replace("{DbFilePath}", databaseFilePath);//it's not working for migrations

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });


        services.AddScoped<ICatalogDbContext>(provider =>
            provider.GetRequiredService<CatalogDbContext>());

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }

}
