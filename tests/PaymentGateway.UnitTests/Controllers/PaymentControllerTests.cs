using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.PaymentService.Controllers;
using PaymentGateway.Domain.DTOs;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.UnitTests.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IIdempotencyService> _idempotencyServiceMock;
    private readonly ApplicationDbContext _context;

    public PaymentControllerTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _idempotencyServiceMock = new Mock<IIdempotencyService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task InitiatePayment_ReturnsUnauthorized_When_Merchant_Not_Found()
    {
        // Arrange
        var controller = new PaymentController(_context, _publishEndpointMock.Object, _idempotencyServiceMock.Object);
        var request = new InitiatePaymentRequest("wrong-key", 100, "USD", "4111111111111111", "ref", "idemp");

        // Act
        var result = await controller.InitiatePayment(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task InitiatePayment_ReturnsOk_When_Payment_Is_Successful()
    {
        // Arrange
        var apiKey = "valid-key";
        var merchant = new Merchant { Id = Guid.NewGuid(), ApiKey = apiKey, Name = "Test Merchant" };
        _context.Merchants.Add(merchant);
        await _context.SaveChangesAsync();

        var controller = new PaymentController(_context, _publishEndpointMock.Object, _idempotencyServiceMock.Object);
        var request = new InitiatePaymentRequest(apiKey, 100, "USD", "4111111111111111", "ref", "idemp");

        // Act
        var result = await controller.InitiatePayment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<InitiatePaymentResponse>(okResult.Value);
        Assert.Equal("Authorized", response.Status);
    }

    [Fact]
    public async Task InitiatePayment_ReturnsCachedResponse_When_IdempotencyKey_Exists()
    {
        // Arrange
        var idempotencyKey = "duplicate-key";
        var cachedResponse = new InitiatePaymentResponse(Guid.NewGuid(), "Authorized");
        _idempotencyServiceMock.Setup(s => s.GetAsync($"payment_{idempotencyKey}"))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(cachedResponse));

        var controller = new PaymentController(_context, _publishEndpointMock.Object, _idempotencyServiceMock.Object);
        var request = new InitiatePaymentRequest("any-key", 100, "USD", "4111111111111111", "ref", idempotencyKey);

        // Act
        var result = await controller.InitiatePayment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<InitiatePaymentResponse>(okResult.Value);
        Assert.Equal(cachedResponse.PaymentId, response.PaymentId);
    }
}
