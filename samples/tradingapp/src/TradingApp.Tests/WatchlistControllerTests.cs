using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Web.Controllers;
using TradingApp.Web.Infrastructure;
using Xunit;

namespace TradingApp.Tests;

public class WatchlistControllerTests
{
    private readonly WatchlistService _service;
    private readonly Mock<IWatchlistRepository> _repoMock;
    private readonly Mock<IAlpacaService> _alpacaMock;
    private readonly WatchlistController _controller;

    public WatchlistControllerTests()
    {
        _repoMock = new Mock<IWatchlistRepository>();
        _alpacaMock = new Mock<IAlpacaService>();
        _service = new WatchlistService(_repoMock.Object, _alpacaMock.Object);
        _controller = new WatchlistController(_service);
    }

    [Fact]
    public async Task Get_ShouldReturnWatchlist()
    {
        // Arrange
        var items = new List<WatchlistItemDto>
        {
            new WatchlistItemDto("AAPL", "Apple Inc.", DateTime.UtcNow, 150.50m)
        };

        _repoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.WatchlistItem> { new Domain.WatchlistItem { Symbol = "AAPL", AddedAt = DateTime.UtcNow } });
        _alpacaMock.Setup(a => a.GetAssetAsync("AAPL", It.IsAny<CancellationToken>())).ReturnsAsync(new StockDetailDto("AAPL", "Apple Inc.", null, null, null));
        _alpacaMock.Setup(a => a.GetLatestPriceAsync("AAPL", It.IsAny<CancellationToken>())).ReturnsAsync(150.50m);

        // Act
        var result = await _controller.Get(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as WatchlistResponse;
        response!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenBodyIsNull()
    {
        // Act
        var result = await _controller.Post(null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Post_ShouldReturnBadRequest_WhenSymbolIsInvalid()
    {
        // Arrange
        var request = new AddWatchlistRequest { Symbol = "  " };

        // Will return null when symbol is empty

        // Act
        var result = await _controller.Post(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Post_ShouldReturnCreated_WhenItemAdded()
    {
        // Arrange
        var request = new AddWatchlistRequest { Symbol = "AAPL" };
        var item = new WatchlistItemDto("AAPL", "Apple Inc.", DateTime.UtcNow, 150.50m);

        _repoMock
            .Setup(r => r.AddAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.WatchlistItem { Symbol = "AAPL", AddedAt = DateTime.UtcNow });
        _alpacaMock.Setup(a => a.GetAssetAsync("AAPL", It.IsAny<CancellationToken>())).ReturnsAsync(new StockDetailDto("AAPL", "Apple Inc.", null, null, null));
        _alpacaMock.Setup(a => a.GetLatestPriceAsync("AAPL", It.IsAny<CancellationToken>())).ReturnsAsync(150.50m);

        // Act
        var result = await _controller.Post(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(201);
        var resultItem = objectResult.Value as WatchlistItemDto;
        resultItem.Should().NotBeNull();
        resultItem!.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        _repoMock
            .Setup(r => r.RemoveAsync("INVALID", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete("INVALID", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenItemRemoved()
    {
        // Arrange
        _repoMock
            .Setup(r => r.RemoveAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete("AAPL", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
