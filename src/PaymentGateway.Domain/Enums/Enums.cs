namespace PaymentGateway.Domain.Enums;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded,
    PartiallyRefunded
}

public enum TransactionType
{
    Payment,
    Refund,
    Settlement,
    Chargeback
}
