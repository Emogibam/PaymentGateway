using FluentValidation;
using PaymentGateway.Domain.DTOs;

namespace PaymentGateway.Domain.Validators;

public class InitiatePaymentRequestValidator : AbstractValidator<InitiatePaymentRequest>
{
    public InitiatePaymentRequestValidator()
    {
        RuleFor(x => x.ApiKey).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CardNumber).NotEmpty().CreditCard();
        RuleFor(x => x.Reference).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
