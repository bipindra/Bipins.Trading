using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Domain;
using TradingApp.Web.Controllers;
using TradingApp.Web.Infrastructure;
using Xunit;

namespace TradingApp.Tests;

public class AlertsControllerTests
{
    private readonly AlertService _alertService;
    private readonly Mock<ILogger<AlertsController>> _loggerMock;
    private readonly AlertsController _controller;
    private readonly Mock<IAlertRepository> _alertRepoMock;

    public AlertsControllerTests()
    {
        _alertRepoMock = new Mock<IAlertRepository>();
        _alertService = new AlertService(_alertRepoMock.Object, Mock.Of<IActivityLogRepository>());
        _loggerMock = new Mock<ILogger<AlertsController>>();
        _controller = new AlertsController(_alertService, _loggerMock.Object);
    }

    [Fact]
    public async Task Get_ShouldReturnBadRequest_WhenSymbolIsMissing()
    {
        // Act
        var result = await _controller.Get(null, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result.Result as BadRequestObjectResult;
        var error = badRequest!.Value as ApiErrorResponse;
        error!.Message.Should().Contain("Symbol");
    }

    [Fact]
    public async Task Get_ShouldReturnAlerts_WhenSymbolProvided()
    {
        // Arrange
        var alerts = new List<AlertDto>
        {
            new AlertDto(1, "AAPL", AlertType.PriceAbove, "100", DateTime.UtcNow)
        };

        _alertRepoMock
            .Setup(r => r.GetBySymbolAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Alert> { new Domain.Alert { Id = 1, Symbol = "AAPL", AlertType = Domain.AlertType.PriceAbove, Payload = "100", CreatedAt = DateTime.UtcNow } });

        // Act
        var result = await _controller.Get("AAPL", CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AlertsListResponse;
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
    public async Task Post_ShouldReturnBadRequest_WhenSymbolIsMissing()
    {
        // Arrange
        var request = new AlertRequest("", AlertType.PriceAbove);

        // Act
        var result = await _controller.Post(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Post_ShouldReturnCreated_WhenAlertCreated()
    {
        // Arrange
        var request = new AlertRequest("AAPL", AlertType.PriceAbove, "100");
        var savedAlert = new Domain.Alert { Id = 1, Symbol = "AAPL", AlertType = Domain.AlertType.PriceAbove, Payload = "100", CreatedAt = DateTime.UtcNow };
        
        _alertRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Domain.Alert>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Alert a, CancellationToken _) => { a.Id = savedAlert.Id; return a; });

        // Act
        var result = await _controller.Post(request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(201);
        var resultDto = objectResult.Value as AlertDto;
        resultDto.Should().NotBeNull();
        resultDto!.Symbol.Should().Be("AAPL");
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenAlertDoesNotExist()
    {
        // Arrange
        _alertRepoMock
            .Setup(r => r.DeleteAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(42, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenAlertDeleted()
    {
        // Arrange
        _alertRepoMock
            .Setup(r => r.DeleteAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(42, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
