using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CatalogService.IntegrationTests
{
    public class ProductsApiTests
    {
        [Fact]
        public async Task GetProducts_ShouldReturnOk()
        {
            await using var application =
                new WebApplicationFactory<Program>();

            var client = application.CreateClient();

            var response = await client.GetAsync(
                "/api/v1.0/products?pageNumber=1&pageSize=10");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
