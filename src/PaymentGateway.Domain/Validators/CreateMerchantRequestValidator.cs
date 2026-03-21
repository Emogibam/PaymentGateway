using FluentValidation;
using PaymentGateway.Domain.DTOs;

namespace PaymentGateway.Domain.Validators;

public class CreateMerchantRequestValidator : AbstractValidator<CreateMerchantRequest>
{
    public CreateMerchantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.WebhookUrl).NotEmpty().Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("A valid absolute Webhook URL is required.");
    }
}
