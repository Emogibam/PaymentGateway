namespace PaymentGateway.Domain.Events;

public class PaymentAuthorizedEvent
{
    public Guid PaymentId { get; set; }
    public Guid MerchantId { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentStatusChangedEvent
{
    public Guid PaymentId { get; set; }
    public Guid MerchantId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}
