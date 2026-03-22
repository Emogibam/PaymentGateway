using FluentValidation;
using PaymentGateway.Domain.DTOs;

namespace PaymentGateway.Domain.Validators;

public class UpdateMerchantRequestValidator : AbstractValidator<UpdateMerchantRequest>
{
    public UpdateMerchantRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.WebhookUrl).NotEmpty().Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("A valid absolute Webhook URL is required.");
    }
}
