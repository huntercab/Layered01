using CartService.API.Middleware;
using CartService.Business.CatalogEvents;
using CartService.Business.Interfaces;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Messaging;
using CartService.DataAccess.Repositories;
using LiteDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Shared.Authorization;
using Shared.Messaging.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

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

// Authentication and Authorization
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        jwtOptions =>
        {
            builder.Configuration.Bind("AzureAd", jwtOptions);

            // Preserve Entra claim names such as "roles", "sub" and "oid".
            jwtOptions.MapInboundClaims = false;
            jwtOptions.TokenValidationParameters.RoleClaimType = "roles";
        },
        identityOptions =>
        {
            builder.Configuration.Bind("AzureAd", identityOptions);
        });

builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(StorePolicies.Read, policy =>
            policy.RequireAuthenticatedUser()
                    .RequireRole(
                        StoreRoles.Manager,
                        StoreRoles.StoreCustomer));

            options.AddPolicy(StorePolicies.Create, policy =>
            policy.RequireAuthenticatedUser()
                    .RequireRole(StoreRoles.Manager));

            options.AddPolicy(StorePolicies.Update, policy =>
            policy.RequireAuthenticatedUser()
                    .RequireRole(StoreRoles.Manager));

            options.AddPolicy(StorePolicies.Delete, policy =>
            policy.RequireAuthenticatedUser()
                    .RequireRole(StoreRoles.Manager));

            options.AddPolicy(StorePolicies.CartAccess, policy =>
            policy.RequireAuthenticatedUser()
                    .RequireRole(
                        StoreRoles.Manager,
                        StoreRoles.StoreCustomer));
        });

var app = builder.Build();

app.MapHealthChecks("/health")
    .AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart Service API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<IdentityAccessTokenLoggingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
