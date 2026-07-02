using CartService.API.Contracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using CartService.DataAccess.Interfaces;
using CartService.DataAccess.Repositories;

namespace CartService.IntegrationTests
{
    public sealed class CartApiIntegrationTests
    {
        [Fact]
        public async Task AddItem_ShouldReturnOk_WhenRequestIsValid()
        {
            var databasePath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}.db"
            );

            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var existingRepository = services
                            .SingleOrDefault(x => x.ServiceType == typeof(ICartRepository));

                        if (existingRepository is not null)
                        {
                            services.Remove(existingRepository);
                        }

                        services.AddScoped<ICartRepository>(_ =>
                            new LiteDbCartRepository(
                                $"Filename={databasePath};Connection=shared"
                            ));
                    });
                });

            var client = factory.CreateClient();

            var cartKey = Guid.NewGuid().ToString();

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
                request
            );

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
