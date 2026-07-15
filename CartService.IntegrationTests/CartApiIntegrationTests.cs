using CartService.API.Contracts;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Repositories;
using LiteDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace CartService.IntegrationTests
{
    public sealed class CartApiIntegrationTests
    {
        [Fact]
        public async Task AddItem_ShouldReturnOk_WhenRequestIsValid()
        {
            var databasePath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}.db");

            var connectionString =
                $"Filename={databasePath};Connection=shared";

            try
            {
                await using var factory =
                    new WebApplicationFactory<Program>()
                        .WithWebHostBuilder(builder =>
                        {
                            builder.ConfigureServices(services =>
                            {
                                services.RemoveAll<ILiteDatabase>();
                                services.RemoveAll<ICartRepository>();

                                services.AddSingleton<ILiteDatabase>(
                                    _ => new LiteDatabase(connectionString));

                                services.AddScoped<
                                    ICartRepository,
                                    LiteDbCartRepository>();
                            });
                        });

                using var client = factory.CreateClient();

                var cartKey = Guid.NewGuid();

                var request = new CartItemRequest
                {
                    Id = 1,
                    Name = "Keyboard",
                    PriceAmount = 50,
                    PriceCurrency = "USD",
                    Quantity = 2
                };

                var response = await client.PostAsJsonAsync(
                    $"/api/v1/carts/{cartKey}/items",
                    request);

                Assert.Equal(
                    HttpStatusCode.OK,
                    response.StatusCode);
            }
            finally
            {
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }

                var logFilePath = databasePath + "-log";

                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }
            }
        }
    }
}
