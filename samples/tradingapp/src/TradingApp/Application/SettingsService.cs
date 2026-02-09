using System.Net;
using TradingApp.Application.DTOs;
using TradingApp.Domain;

namespace TradingApp.Application;

public sealed class SettingsService
{
    private readonly IAlpacaSettingsRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;

    public SettingsService(IAlpacaSettingsRepository repository, IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AlpacaSettingsDto> GetAlpacaSettingsAsync(CancellationToken ct = default)
    {
        var s = await _repository.GetAsync(ct);
        if (s == null)
            return new AlpacaSettingsDto(null, null, null);

        var secretMasked = string.IsNullOrEmpty(s.ApiSecret)
            ? null
            : "****" + (s.ApiSecret.Length >= 4 ? s.ApiSecret[^4..] : "");
        return new AlpacaSettingsDto(s.ApiKey, secretMasked, s.BaseUrl);
    }

    public async Task SaveAlpacaSettingsAsync(string? apiKey, string? apiSecret, string? baseUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("ApiKey and BaseUrl are required.");
        var key = apiKey.Trim();
        var secret = (apiSecret ?? "").Trim();
        var url = baseUrl.Trim().TrimEnd('/');

        if (!string.IsNullOrEmpty(secret))
        {
            await ValidateAlpacaCredentialsAsync(key, secret, url, ct);
        }

        var s = await _repository.GetAsync(ct);
        var settings = new AlpacaSettings
        {
            Id = s?.Id ?? 1,
            ApiKey = key,
            ApiSecret = secret,
            BaseUrl = url
        };
        await _repository.SaveAsync(settings, ct);
    }

    /// <summary>
    /// Calls Alpaca GET /v2/account to verify the credentials work before saving.
    /// </summary>
    private async Task ValidateAlpacaCredentialsAsync(string apiKey, string apiSecret, string baseUrl, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("Alpaca");
        client.Timeout = TimeSpan.FromSeconds(10);
        var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v2/account");
        request.Headers.Add("APCA-API-KEY-ID", apiKey);
        request.Headers.Add("APCA-API-SECRET-KEY", apiSecret);

        var response = await client.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
            return;

        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Alpaca API key or secret is invalid.",
            HttpStatusCode.Forbidden => "Alpaca credentials are not allowed to access this resource.",
            HttpStatusCode.NotFound => "Alpaca API URL may be incorrect (e.g. use https://paper-api.alpaca.markets for paper).",
            _ => response.ReasonPhrase ?? "Alpaca API returned an error. Check your credentials and base URL."
        };
        throw new InvalidOperationException(message);
    }
}
