using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

public sealed class AlpacaSettingsRepository : IAlpacaSettingsRepository
{
    private const int SingletonId = 1;
    private readonly AppDbContext _db;
    private readonly SecretEncryption _encryption;
    private readonly IConfiguration? _config;

    public AlpacaSettingsRepository(AppDbContext db, SecretEncryption enc, IConfiguration? config = null)
    {
        _db = db;
        _encryption = enc;
        _config = config;
    }

    public async Task<AlpacaSettings?> GetAsync(CancellationToken ct = default)
    {
        var row = await _db.AlpacaSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (row != null)
        {
            var secret = string.Empty;
            if (!string.IsNullOrEmpty(row.ApiSecret))
            {
                try { secret = _encryption.Decrypt(row.ApiSecret); }
                catch (CryptographicException) { /* key ring changed or payload invalid; treat as empty so save can re-store */ }
            }
            return new AlpacaSettings
            {
                Id = row.Id,
                ApiKey = row.ApiKey,
                ApiSecret = secret,
                BaseUrl = row.BaseUrl
            };
        }
        if (_config != null)
        {
            var key = _config["Alpaca:ApiKey"];
            var secret = _config["Alpaca:ApiSecret"];
            var baseUrl = _config["Alpaca:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(secret))
                return new AlpacaSettings { ApiKey = key, ApiSecret = secret, BaseUrl = baseUrl ?? "https://paper-api.alpaca.markets" };
        }
        return null;
    }

    public async Task SaveAsync(AlpacaSettings settings, CancellationToken ct = default)
    {
        var existing = await _db.AlpacaSettings.FirstOrDefaultAsync(x => x.Id == SingletonId, ct);
        if (existing != null)
        {
            existing.ApiKey = settings.ApiKey;
            existing.BaseUrl = settings.BaseUrl ?? string.Empty;
            if (!string.IsNullOrEmpty(settings.ApiSecret))
            {
                try { existing.ApiSecret = _encryption.Encrypt(settings.ApiSecret); }
                catch (CryptographicException ex) { throw new InvalidOperationException("Unable to encrypt API secret. Ensure the application key storage is accessible.", ex); }
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(settings.ApiSecret))
                throw new ArgumentException("API Secret is required when saving credentials for the first time.");
            string encryptedSecret;
            try { encryptedSecret = _encryption.Encrypt(settings.ApiSecret!); }
            catch (CryptographicException ex) { throw new InvalidOperationException("Unable to encrypt API secret. Ensure the application key storage is accessible.", ex); }
            _db.AlpacaSettings.Add(new AlpacaSettings
            {
                Id = SingletonId,
                ApiKey = settings.ApiKey,
                ApiSecret = encryptedSecret,
                BaseUrl = settings.BaseUrl ?? string.Empty
            });
        }
        await _db.SaveChangesAsync(ct);
    }
}
