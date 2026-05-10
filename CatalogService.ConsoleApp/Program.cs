using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CatalogService.Application;
using CatalogService.Infrastructure;
using CatalogService.Application.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
    })
    .Build();

using var scope = host.Services.CreateScope();

var categoryService = scope.ServiceProvider.GetRequiredService<CategoryService>();
var productService = scope.ServiceProvider.GetRequiredService<ProductService>();

try
{
    var guid = Guid.NewGuid();
    await categoryService.AddAsync($"category{guid}", $"https://url.com/category{guid}.png");
    var categories = await categoryService.GetAllAsync();

    var cat = categories.First(c => c.Name == $"category{guid}");

    await productService.AddAsync($"prod{guid}", cat.Id, 100.25m, "USD", 3, $"description prod{guid}", $"https://url.com/prod{guid}.png");

    var products = await productService.GetAllAsync();

    foreach (var product in products)
    {
        Console.WriteLine($"{product.Id} - {product.Name}");
    }
}
catch(Exception ex)
{
    Console.WriteLine(ex.ToString());
}
