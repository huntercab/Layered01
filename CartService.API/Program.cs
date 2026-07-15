using CartService.Business.CatalogEvents;
using CartService.Business.Interfaces;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Messaging;
using CartService.DataAccess.Repositories;
using LiteDB;
using Microsoft.OpenApi;
using Shared.Messaging.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<ICartService, CartService.Business.Services.CartService>();

var connectionString =
    builder.Configuration.GetConnectionString("CartDatabase")
    ?? "Filename=cart.db;Connection=shared";

Console.WriteLine($"LiteDB connection: {connectionString}");
Console.WriteLine($"LiteDB file path: {Path.GetFullPath("cart.db")}");

builder.Services.AddSingleton<ILiteDatabase>(
    _ => new LiteDatabase(connectionString));


// RabbitMQ configuration
builder.Services.AddRabbitMqMessaging(
    builder.Configuration);

builder.Services.AddScoped<ProductUpdatedEventHandler>();

builder.Services.AddScoped<ICartRepository,
    LiteDbCartRepository>();

builder.Services.AddScoped<IInboxRepository,
    LiteDbInboxRepository>();

builder.Services.AddHostedService<ProductUpdatedConsumer>();
//////////////////

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cart Service API",
        Version = "v1"
    });

    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart Service API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;