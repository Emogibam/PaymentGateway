using Xunit;
using PaymentGateway.Domain.Validators;
using PaymentGateway.Domain.DTOs;

namespace PaymentGateway.UnitTests.Validators;

public class InitiatePaymentRequestValidatorTests
{
    private readonly InitiatePaymentRequestValidator _validator;

    public InitiatePaymentRequestValidatorTests()
    {
        _validator = new InitiatePaymentRequestValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Amount_Is_Negative()
    {
        var request = new InitiatePaymentRequest("apiKey", -10, "USD", "4111111111111111", "ref", "idemp");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Amount");
    }

    [Fact]
    public void Should_Have_Error_When_Currency_Is_Invalid()
    {
        var request = new InitiatePaymentRequest("apiKey", 100, "INVALID", "4111111111111111", "ref", "idemp");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Currency");
    }

    [Fact]
    public void Should_Have_Error_When_CardNumber_Is_Invalid()
    {
        var request = new InitiatePaymentRequest("apiKey", 100, "USD", "12345678", "ref", "idemp");
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CardNumber");
    }

    [Fact]
    public void Should_Be_Valid_When_Request_Is_Correct()
    {
        // 4111111111111111 is a valid test Visa card number
        var request = new InitiatePaymentRequest("apiKey", 100, "USD", "4111111111111111", "ref", "idemp");
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
