using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Domain;
using Xunit;

namespace TradingApp.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task GetRecentAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var repoMock = new Mock<INotificationRepository>();
        var notifications = new List<Notification>
        {
            new Notification { Id = 1, AlertId = 1, Symbol = "AAPL", Message = "Test 1", TriggeredAt = DateTime.UtcNow },
            new Notification { Id = 2, AlertId = 2, Symbol = "MSFT", Message = "Test 2", TriggeredAt = DateTime.UtcNow, ReadAt = DateTime.UtcNow }
        };

        repoMock
            .Setup(r => r.GetRecentAsync(50, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var service = new NotificationService(repoMock.Object);

        // Act
        var result = await service.GetRecentAsync(50, false, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].Symbol.Should().Be("AAPL");
        result[1].Id.Should().Be(2);
        result[1].Symbol.Should().Be("MSFT");
        result[1].ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRecentAsync_ShouldFilterUnreadOnly_WhenRequested()
    {
        // Arrange
        var repoMock = new Mock<INotificationRepository>();
        var notifications = new List<Notification>
        {
            new Notification { Id = 1, AlertId = 1, Symbol = "AAPL", Message = "Test 1", TriggeredAt = DateTime.UtcNow }
        };

        repoMock
            .Setup(r => r.GetRecentAsync(50, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var service = new NotificationService(repoMock.Object);

        // Act
        var result = await service.GetRecentAsync(50, true, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        repoMock.Verify(r => r.GetRecentAsync(50, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkReadAsync_ShouldCallRepository()
    {
        // Arrange
        var repoMock = new Mock<INotificationRepository>();
        var service = new NotificationService(repoMock.Object);

        // Act
        await service.MarkReadAsync(42, CancellationToken.None);

        // Assert
        repoMock.Verify(r => r.MarkReadAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }
}
