using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Infrastructure.Data;
using MassTransit;

using PaymentGateway.Domain.Events;
using PaymentGateway.Domain.Interfaces;
using PaymentGateway.Domain.DTOs;
using System.Text.Json;

namespace PaymentGateway.PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IIdempotencyService _idempotencyService;

    public PaymentController(
        ApplicationDbContext context, 
        IPublishEndpoint publishEndpoint,
        IIdempotencyService idempotencyService)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _idempotencyService = idempotencyService;
    }

    [HttpPost]
    public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentRequest request)
    {
        var idempotencyKey = $"payment_{request.IdempotencyKey}";
        
        // Check idempotency
        var cachedResponse = await _idempotencyService.GetAsync(idempotencyKey);
        if (cachedResponse != null)
        {
            return Ok(JsonSerializer.Deserialize<InitiatePaymentResponse>(cachedResponse));
        }

        var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.ApiKey == request.ApiKey);
        if (merchant == null)
            return Unauthorized();

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            MerchantId = merchant.Id,
            Amount = request.Amount,
            Currency = request.Currency,
            CardNumberMasked = MaskCardNumber(request.CardNumber),
            Status = "Authorized", // Mocking authorization
            ExternalReference = request.Reference,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Publish to MQ for settlement
        await _publishEndpoint.Publish(new PaymentAuthorizedEvent
        {
            PaymentId = payment.Id,
            MerchantId = merchant.Id,
            Amount = payment.Amount
        });

        var response = new InitiatePaymentResponse(payment.Id, payment.Status);
        
        // Cache for idempotency
        await _idempotencyService.SetAsync(idempotencyKey, JsonSerializer.Serialize(response));

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    private string MaskCardNumber(string cardNumber)
    {
        return "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4);
    }
}
