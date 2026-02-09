using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Web.Infrastructure;

namespace TradingApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly SettingsService _settingsService;

    public SettingsController(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("alpaca")]
    public async Task<ActionResult<AlpacaSettingsDto>> GetAlpaca(CancellationToken ct)
    {
        var dto = await _settingsService.GetAlpacaSettingsAsync(ct);
        return Ok(dto);
    }

    [HttpPost("alpaca")]
    public async Task<IActionResult> PostAlpaca([FromBody] SaveAlpacaRequest? body, CancellationToken ct)
    {
        if (body == null)
            return BadRequest(new ApiErrorResponse("Request body is required."));
        try
        {
            await _settingsService.SaveAlpacaSettingsAsync(body.ApiKey, body.ApiSecret, body.BaseUrl, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiErrorResponse(ex.Message));
        }
    }
}

public sealed class SaveAlpacaRequest
{
    [Required, MinLength(1)]
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    [Required, MinLength(1)]
    public string? BaseUrl { get; set; }
}
