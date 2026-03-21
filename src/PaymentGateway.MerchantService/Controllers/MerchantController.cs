using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Infrastructure.Data;
using PaymentGateway.Domain.DTOs;
using System.Security.Cryptography;

namespace PaymentGateway.MerchantService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchantController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MerchantController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
    {
        var apiKey = GenerateApiKey();
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ApiKey = apiKey,
            WebhookUrl = request.WebhookUrl,
            Balance = 0
        };

        _context.Merchants.Add(merchant);
        await _context.SaveChangesAsync();

        return Ok(new CreateMerchantResponse(merchant.Id, merchant.ApiKey));
    }

    private string GenerateApiKey()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
