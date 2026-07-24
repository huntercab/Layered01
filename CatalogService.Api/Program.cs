using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CatalogService.Application;
using CatalogService.Infrastructure;
using CatalogService.Infrastructure.Outbox;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Shared.Authorization;
using Shared.Messaging.Abstractions;
using Shared.Messaging.DependencyInjection;
using Shared.Messaging.RabbitMQ;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// RabbitMQ configuration
builder.Services.AddRabbitMqMessaging(
    builder.Configuration);

builder.Services.AddScoped<IIntegrationEventPublisher,
    RabbitMqIntegrationEventPublisher>();

builder.Services.AddHostedService<OutboxPublisherWorker>();
//////////////////

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen();

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

var apiVersionDescriptionProvider =
    app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Catalog Service API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = "Catalog Service API",
                    Version = description.ApiVersion.ToString(),
                    Description = "REST API for Catalog Service"
                });
        }
    }
}
