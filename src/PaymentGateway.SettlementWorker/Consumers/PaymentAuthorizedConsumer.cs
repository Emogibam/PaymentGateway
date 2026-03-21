using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Infrastructure.Data;

using PaymentGateway.Domain.Events;

namespace PaymentGateway.SettlementWorker.Consumers;

public class PaymentAuthorizedConsumer : IConsumer<PaymentAuthorizedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentAuthorizedConsumer> _logger;

    public PaymentAuthorizedConsumer(ApplicationDbContext context, ILogger<PaymentAuthorizedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentAuthorizedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing settlement for PaymentId: {PaymentId}", message.PaymentId);

        var merchant = await _context.Merchants.FindAsync(message.MerchantId);
        if (merchant == null)
        {
            _logger.LogError("Merchant not found: {MerchantId}", message.MerchantId);
            return;
        }

        // Add to Transaction Ledger
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            PaymentId = message.PaymentId,
            MerchantId = message.MerchantId,
            Amount = message.Amount,
            Type = "Payment",
            CreatedAt = DateTime.UtcNow
        };

        // Update Merchant Balance
        merchant.Balance += message.Amount;

        _context.Transactions.Add(transaction);
        _context.Merchants.Update(merchant);

        // Update Payment status to Captured
        var payment = await _context.Payments.FindAsync(message.PaymentId);
        if (payment != null)
        {
            payment.Status = "Captured";
            payment.ProcessedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);
        }

        await _context.SaveChangesAsync();

        // Publish to Webhook Service
        await context.Publish(new PaymentStatusChangedEvent
        {
            PaymentId = message.PaymentId,
            MerchantId = message.MerchantId,
            NewStatus = "Captured",
            Reference = payment?.ExternalReference ?? string.Empty
        });

        _logger.LogInformation("Settlement completed for PaymentId: {PaymentId}. Merchant balance updated.", message.PaymentId);
    }
}
