namespace PaymentGateway.Domain.DTOs;

public record CreateMerchantRequest(string Name, string Email, string WebhookUrl);
public record UpdateMerchantRequest(string Name, string Email, string WebhookUrl, bool IsActive);
public record CreateMerchantResponse(Guid Id, string ApiKey);
public record MerchantResponse(Guid Id, string Name, string Email, string WebhookUrl, bool IsActive, decimal Balance);

public record InitiatePaymentRequest(string ApiKey, decimal Amount, string Currency, string CardNumber, string Reference, string IdempotencyKey);
public record InitiatePaymentResponse(Guid PaymentId, string Status);

