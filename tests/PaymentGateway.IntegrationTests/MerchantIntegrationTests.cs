extern alias MerchantService;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Domain.DTOs;
using PaymentGateway.Infrastructure.Data;
using Xunit;

namespace PaymentGateway.IntegrationTests;

public class MerchantIntegrationTests : IClassFixture<WebApplicationFactory<MerchantService::Program>>
{
    private readonly WebApplicationFactory<MerchantService::Program> _factory;

    public MerchantIntegrationTests(WebApplicationFactory<MerchantService::Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration and all its internal services
                var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextOptionsDescriptor != null) services.Remove(dbContextOptionsDescriptor);

                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                // Add In-Memory Database for testing with a fresh service provider
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("IntegrationTestDb");
                    options.UseInternalServiceProvider(serviceProvider);
                });
            });
        });
    }

    [Fact]
    public async Task CreateMerchant_ReturnsSuccess_And_PersistsToDatabase()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateMerchantRequest("Integration Test Merchant", "https://webhook.com");

        // Act
        var response = await client.PostAsJsonAsync("/api/Merchant", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<CreateMerchantResponse>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content.Id);
        Assert.False(string.IsNullOrEmpty(content.ApiKey));

        // Verify in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var merchant = await db.Merchants.FindAsync(content.Id);
        Assert.NotNull(merchant);
        Assert.Equal("Integration Test Merchant", merchant.Name);
    }
}
