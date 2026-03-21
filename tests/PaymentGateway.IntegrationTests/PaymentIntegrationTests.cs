extern alias PaymentService;

using System.Net.Http.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PaymentGateway.Domain.DTOs;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Infrastructure.Data;
using StackExchange.Redis;
using Xunit;

namespace PaymentGateway.IntegrationTests;

public class PaymentIntegrationTests : IClassFixture<WebApplicationFactory<PaymentService::Program>>
{
    private readonly WebApplicationFactory<PaymentService::Program> _factory;
    private readonly Mock<IConnectionMultiplexer> _redisMock = new();
    private readonly Mock<IDatabase> _redisDbMock = new();

    public PaymentIntegrationTests(WebApplicationFactory<PaymentService::Program> factory)
    {
        _redisMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_redisDbMock.Object);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DB and replace with In-Memory
                var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextOptionsDescriptor != null) services.Remove(dbContextOptionsDescriptor);

                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
                if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                var dbServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("PaymentIntegrationTestDb");
                    options.UseInternalServiceProvider(dbServiceProvider);
                });

                // Mock Redis
                services.AddSingleton(_redisMock.Object);

                // Mock MassTransit (RabbitMQ)
                services.AddMassTransitTestHarness();
            });
        });
    }

    [Fact]
    public async Task InitiatePayment_ReturnsSuccess_When_RequestIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var apiKey = "valid-api-key";
        var merchantId = Guid.NewGuid();

        // Seed Merchant
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Merchants.Add(new Merchant { Id = merchantId, Name = "Test Merchant", ApiKey = apiKey });
            await db.SaveChangesAsync();
        }

        var request = new InitiatePaymentRequest(
            ApiKey: apiKey,
            Amount: 100.50m,
            Currency: "USD",
            CardNumber: "4111111111111111",
            Reference: "Order-123",
            IdempotencyKey: "unique-key-123"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/Payment", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<InitiatePaymentResponse>();
        Assert.NotNull(content);
        Assert.Equal("Authorized", content.Status);

        // Verify in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var payment = await db.Payments.FindAsync(content.PaymentId);
            Assert.NotNull(payment);
            Assert.Equal(100.50m, payment.Amount);
            Assert.Equal(merchantId, payment.MerchantId);
        }
    }
}
