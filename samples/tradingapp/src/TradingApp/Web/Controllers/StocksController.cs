using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/stocks")]
public class StocksController : ControllerBase
{
    private readonly IAlpacaService _alpacaService;

    public StocksController(IAlpacaService alpacaService)
    {
        _alpacaService = alpacaService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<StockSearchResponse>> Search([FromQuery] string? q, CancellationToken ct)
    {
        var query = (q ?? "").Trim();
        var items = await _alpacaService.SearchAssetsAsync(query, ct);
        return Ok(new StockSearchResponse { Items = items.ToList() });
    }

    [HttpGet("{symbol:notsearch}")]
    public async Task<ActionResult<StockDetailDto>> GetBySymbol(string symbol, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest(new ApiErrorResponse("Symbol is required.", new[] { "symbol" }));
        var detail = await _alpacaService.GetAssetAsync(symbol.Trim(), ct);
        if (detail == null)
            return NotFound();
        return Ok(detail);
    }
}

public sealed class StockSearchResponse
{
    [JsonPropertyName("items")]
    public List<StockSearchResult> Items { get; set; } = new();
}
