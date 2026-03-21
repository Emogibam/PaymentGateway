namespace PaymentGateway.Domain.Entities;

public class Merchant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Balance { get; set; }
}

public class Payment
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CardNumberMasked { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Card";
    public string Status { get; set; } = "Pending";
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string ExternalReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Type { get; set; } = "Payment";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

