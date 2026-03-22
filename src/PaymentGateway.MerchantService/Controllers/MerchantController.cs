using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpGet]
    public async Task<IActionResult> GetMerchants()
    {
        var merchants = await _context.Merchants
            .Select(m => new MerchantResponse(m.Id, m.Name, m.Email, m.WebhookUrl, m.IsActive, m.Balance))
            .ToListAsync();
        return Ok(merchants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMerchant(Guid id)
    {
        var merchant = await _context.Merchants.FindAsync(id);
        if (merchant == null) return NotFound();

        return Ok(new MerchantResponse(merchant.Id, merchant.Name, merchant.Email, merchant.WebhookUrl, merchant.IsActive, merchant.Balance));
    }

    [HttpPost]
    public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
    {
        var apiKey = GenerateApiKey();
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            ApiKey = apiKey, // In a real system, this would be hashed
            WebhookUrl = request.WebhookUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Balance = 0
        };

        _context.Merchants.Add(merchant);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMerchant), new { id = merchant.Id }, new CreateMerchantResponse(merchant.Id, merchant.ApiKey));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMerchant(Guid id, [FromBody] UpdateMerchantRequest request)
    {
        var merchant = await _context.Merchants.FindAsync(id);
        if (merchant == null) return NotFound();

        merchant.Name = request.Name;
        merchant.Email = request.Email;
        merchant.WebhookUrl = request.WebhookUrl;
        merchant.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMerchant(Guid id)
    {
        var merchant = await _context.Merchants.FindAsync(id);
        if (merchant == null) return NotFound();

        _context.Merchants.Remove(merchant);
        await _context.SaveChangesAsync();
        return NoContent();
    }


    private string GenerateApiKey()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
