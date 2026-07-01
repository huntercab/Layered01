using CartService.Business.Interfaces;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Repositories;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<ICartService, CartService.Business.Services.CartService>();

builder.Services.AddScoped<ICartRepository>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("CartDatabase")
        ?? "Filename=cart.db;Connection=shared";

    return new LiteDbCartRepository(connectionString);
});

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