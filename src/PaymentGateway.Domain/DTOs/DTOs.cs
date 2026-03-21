namespace PaymentGateway.Domain.DTOs;

public record CreateMerchantRequest(string Name, string WebhookUrl);
public record CreateMerchantResponse(Guid Id, string ApiKey);

public record InitiatePaymentRequest(string ApiKey, decimal Amount, string Currency, string CardNumber, string Reference, string IdempotencyKey);
public record InitiatePaymentResponse(Guid PaymentId, string Status);
