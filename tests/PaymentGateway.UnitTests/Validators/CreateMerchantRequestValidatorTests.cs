using Xunit;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Domain.DTOs;

namespace PaymentGateway.UnitTests.Validators;

public class CreateMerchantRequestValidatorTests
{
    private readonly CreateMerchantRequestValidator _validator;

    public CreateMerchantRequestValidatorTests()
    {
        _validator = new CreateMerchantRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var request = new CreateMerchantRequest("", "https://example.com/webhook");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_WebhookUrl_Is_Invalid()
    {
        var request = new CreateMerchantRequest("Test Merchant", "invalid-url");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WebhookUrl");
    }

    [Fact]
    public void Should_Be_Valid_When_Request_Is_Correct()
    {
        var request = new CreateMerchantRequest("Test Merchant", "https://example.com/webhook");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
