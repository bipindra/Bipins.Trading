using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Domain;
using Xunit;

namespace TradingApp.Tests;

public class WatchlistServiceTests
{
    [Fact]
    public async Task GetWatchlistAsync_ShouldReturnDtosWithAssetInfo()
    {
        // Arrange
        var repoMock = new Mock<IWatchlistRepository>();
        var alpacaMock = new Mock<IAlpacaService>();
        
        var watchlistItems = new List<WatchlistItem>
        {
            new WatchlistItem { Symbol = "AAPL", AddedAt = DateTime.UtcNow }
        };

        repoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(watchlistItems);

        alpacaMock
            .Setup(a => a.GetAssetAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockDetailDto("AAPL", "Apple Inc.", null, null, null));

        alpacaMock
            .Setup(a => a.GetLatestPriceAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(150.50m);

        var service = new WatchlistService(repoMock.Object, alpacaMock.Object);

        // Act
        var result = await service.GetWatchlistAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Symbol.Should().Be("AAPL");
        result[0].Name.Should().Be("Apple Inc.");
        result[0].LatestPrice.Should().Be(150.50m);
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNull_WhenSymbolIsEmpty()
    {
        // Arrange
        var repoMock = new Mock<IWatchlistRepository>();
        var alpacaMock = new Mock<IAlpacaService>();
        var service = new WatchlistService(repoMock.Object, alpacaMock.Object);

        // Act
        var result = await service.AddAsync("  ", CancellationToken.None);

        // Assert
        result.Should().BeNull();
        repoMock.Verify(r => r.AddAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddAsync_ShouldAddItemAndReturnDto()
    {
        // Arrange
        var repoMock = new Mock<IWatchlistRepository>();
        var alpacaMock = new Mock<IAlpacaService>();
        
        var addedItem = new WatchlistItem { Symbol = "MSFT", AddedAt = DateTime.UtcNow };

        repoMock
            .Setup(r => r.AddAsync("MSFT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(addedItem);

        alpacaMock
            .Setup(a => a.GetAssetAsync("MSFT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockDetailDto("MSFT", "Microsoft Corporation", null, null, null));

        alpacaMock
            .Setup(a => a.GetLatestPriceAsync("MSFT", It.IsAny<CancellationToken>()))
            .ReturnsAsync(300.25m);

        var service = new WatchlistService(repoMock.Object, alpacaMock.Object);

        // Act
        var result = await service.AddAsync("MSFT", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Symbol.Should().Be("MSFT");
        result.Name.Should().Be("Microsoft Corporation");
        result.LatestPrice.Should().Be(300.25m);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallRepository()
    {
        // Arrange
        var repoMock = new Mock<IWatchlistRepository>();
        var alpacaMock = new Mock<IAlpacaService>();
        
        repoMock
            .Setup(r => r.RemoveAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new WatchlistService(repoMock.Object, alpacaMock.Object);

        // Act
        var result = await service.RemoveAsync("AAPL", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        repoMock.Verify(r => r.RemoveAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldHandleNullSymbol()
    {
        // Arrange
        var repoMock = new Mock<IWatchlistRepository>();
        var alpacaMock = new Mock<IAlpacaService>();
        
        repoMock
            .Setup(r => r.RemoveAsync("", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new WatchlistService(repoMock.Object, alpacaMock.Object);

        // Act
        var result = await service.RemoveAsync(null, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
