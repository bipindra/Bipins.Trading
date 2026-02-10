using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TradingApp.Application;
using TradingApp.Domain;
using TradingApp.Web.Controllers;
using TradingApp.Web.Infrastructure;
using Xunit;

namespace TradingApp.Tests;

public class ActivityLogsControllerTests
{
    private readonly Mock<IActivityLogRepository> _repositoryMock;
    private readonly Mock<ILogger<ActivityLogsController>> _loggerMock;
    private readonly ActivityLogsController _controller;

    public ActivityLogsControllerTests()
    {
        _repositoryMock = new Mock<IActivityLogRepository>();
        _loggerMock = new Mock<ILogger<ActivityLogsController>>();
        _controller = new ActivityLogsController(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetLogs_ShouldReturnLogs_WhenNoCategoryFilter()
    {
        // Arrange
        var logs = new List<ActivityLog>
        {
            new ActivityLog { Id = 1, Level = "Info", Category = "AlertWatch", Message = "Test" }
        };

        _repositoryMock
            .Setup(r => r.GetRecentAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetLogs(100, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as ActivityLogsResponse;
        response!.Logs.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetLogs_ShouldFilterByCategory_WhenCategoryProvided()
    {
        // Arrange
        var logs = new List<ActivityLog>
        {
            new ActivityLog { Id = 1, Level = "Info", Category = "AlertWatch", Message = "Test" }
        };

        _repositoryMock
            .Setup(r => r.GetByCategoryAsync("AlertWatch", 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetLogs(100, "AlertWatch", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _repositoryMock.Verify(r => r.GetByCategoryAsync("AlertWatch", 100, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLogs_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetRecentAsync(100, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetLogs(100, null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteAll_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _repositoryMock.Verify(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAll_ShouldReturn500_WhenExceptionOccurs()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
