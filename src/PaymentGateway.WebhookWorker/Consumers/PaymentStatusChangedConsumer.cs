using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Events;
using PaymentGateway.Infrastructure.Data;
using System.Text.Json;
using System.Net.Http.Json;

namespace PaymentGateway.WebhookWorker.Consumers;

public class PaymentStatusChangedConsumer : IConsumer<PaymentStatusChangedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentStatusChangedConsumer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentStatusChangedConsumer(
        ApplicationDbContext context, 
        ILogger<PaymentStatusChangedConsumer> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task Consume(ConsumeContext<PaymentStatusChangedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Sending webhook for PaymentId: {PaymentId}", message.PaymentId);

        var merchant = await _context.Merchants.FindAsync(message.MerchantId);
        if (merchant == null || string.IsNullOrEmpty(merchant.WebhookUrl))
        {
            _logger.LogWarning("Merchant not found or no Webhook URL for MerchantId: {MerchantId}", message.MerchantId);
            return;
        }

        var client = _httpClientFactory.CreateClient();
        var payload = new
        {
            event_type = "payment.status_changed",
            payment_id = message.PaymentId,
            status = message.NewStatus,
            reference = message.Reference,
            timestamp = DateTime.UtcNow
        };

        try
        {
            var response = await client.PostAsJsonAsync(merchant.WebhookUrl, payload);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook sent successfully for PaymentId: {PaymentId}", message.PaymentId);
            }
            else
            {
                _logger.LogError("Failed to send webhook for PaymentId: {PaymentId}. Status: {Status}", message.PaymentId, response.StatusCode);
                // In a real system, you'd implement a retry policy here (e.g., using Polly or MassTransit's retry)
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook for PaymentId: {PaymentId}", message.PaymentId);
        }
    }
}
