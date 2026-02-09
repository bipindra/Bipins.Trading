using Microsoft.AspNetCore.DataProtection;

namespace TradingApp.Infrastructure;

public sealed class SecretEncryption
{
    private const string Purpose = "TradingApp.AlpacaSettings.ApiSecret";
    private readonly IDataProtector _protector;

    public SecretEncryption(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(Purpose);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        return _protector.Unprotect(cipherText);
    }
}
