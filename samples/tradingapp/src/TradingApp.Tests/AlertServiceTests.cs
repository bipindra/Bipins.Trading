using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Domain;
using Xunit;

namespace TradingApp.Tests;

public class AlertServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldNormalizeSymbolAndReturnDto()
    {
        // Arrange
        var repoMock = new Mock<IAlertRepository>();
        var activityRepoMock = new Mock<IActivityLogRepository>();

        var savedAlert = new Alert
        {
            Id = 42,
            Symbol = "AAPL",
            AlertType = AlertType.PriceAbove,
            Payload = "100",
        };

        repoMock
            .Setup(r => r.AddAsync(It.IsAny<Alert>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Alert a, CancellationToken _) =>
            {
                a.Id = savedAlert.Id;
                a.CreatedAt = savedAlert.CreatedAt;
                return a;
            });

        var service = new AlertService(repoMock.Object, activityRepoMock.Object);

        var request = new AlertRequest(
            Symbol: "aapl",
            AlertType: AlertType.PriceAbove,
            Payload: "100"
        );

        // Act
        AlertDto result = await service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Symbol.Should().Be("AAPL");
        result.Id.Should().Be(savedAlert.Id);

        repoMock.Verify(r => r.AddAsync(It.Is<Alert>(a =>
            a.Symbol == "AAPL" &&
            a.AlertType == request.AlertType &&
            a.Payload == request.Payload
        ), It.IsAny<CancellationToken>()), Times.Once);

        activityRepoMock.Verify(r => r.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenSymbolMissing()
    {
        var repoMock = new Mock<IAlertRepository>();
        var service = new AlertService(repoMock.Object);

        var request = new AlertRequest(
            Symbol: "  ", // invalid
            AlertType: AlertType.PriceAbove,
            Payload: "100"
        );

        var act = async () => await service.CreateAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Symbol is required*");
    }
}

