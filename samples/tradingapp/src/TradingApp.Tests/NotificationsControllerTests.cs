using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Domain;
using TradingApp.Web.Controllers;
using Xunit;

namespace TradingApp.Tests;

public class NotificationsControllerTests
{
    private readonly NotificationService _service;
    private readonly Mock<INotificationRepository> _repoMock;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _repoMock = new Mock<INotificationRepository>();
        _service = new NotificationService(_repoMock.Object);
        _controller = new NotificationsController(_service);
    }

    [Fact]
    public async Task Get_ShouldReturnNotifications()
    {
        // Arrange
        var notifications = new List<NotificationDto>
        {
            new NotificationDto(1, 1, "AAPL", "Test", DateTime.UtcNow, null)
        };

        _repoMock
            .Setup(r => r.GetRecentAsync(50, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Notification> { new Domain.Notification { Id = 1, AlertId = 1, Symbol = "AAPL", Message = "Test", TriggeredAt = DateTime.UtcNow } });

        // Act
        var result = await _controller.Get(false, 50, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as NotificationsListResponse;
        response!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Get_ShouldClampLimit()
    {
        // Arrange
        var notifications = new List<NotificationDto>();

        _repoMock
            .Setup(r => r.GetRecentAsync(100, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Notification>());

        // Act
        var result = await _controller.Get(false, 200, CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.GetRecentAsync(100, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkRead_ShouldReturnNoContent()
    {
        // Arrange
        _repoMock
            .Setup(r => r.MarkReadAsync(42, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.MarkRead(42, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _repoMock.Verify(r => r.MarkReadAsync(42, It.IsAny<CancellationToken>()), Times.Once);
    }
}
