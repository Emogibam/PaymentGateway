namespace PaymentGateway.Domain.Interfaces;

public interface IIdempotencyService
{
    Task<bool> IsDuplicateAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetAsync(string key);
}
