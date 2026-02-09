using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly WatchlistService _watchlistService;

    public WatchlistController(WatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet]
    public async Task<ActionResult<WatchlistResponse>> Get(CancellationToken ct)
    {
        var items = await _watchlistService.GetWatchlistAsync(ct);
        return Ok(new WatchlistResponse(items.ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<WatchlistItemDto>> Post([FromBody] AddWatchlistRequest body, CancellationToken ct)
    {
        if (body == null)
            return BadRequest(new ApiErrorResponse("Request body is required."));
        try
        {
            var item = await _watchlistService.AddAsync((body.Symbol ?? "").Trim(), ct);
            if (item == null)
                return BadRequest(new ApiErrorResponse("Invalid symbol."));
            return StatusCode(201, item);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{symbol}")]
    public async Task<IActionResult> Delete(string symbol, CancellationToken ct)
    {
        var removed = await _watchlistService.RemoveAsync(symbol ?? "", ct);
        if (!removed)
            return NotFound();
        return NoContent();
    }
}

public sealed class AddWatchlistRequest
{
    [Required, MinLength(1)]
    public string? Symbol { get; set; }
}
public record WatchlistResponse(List<WatchlistItemDto> Items);
