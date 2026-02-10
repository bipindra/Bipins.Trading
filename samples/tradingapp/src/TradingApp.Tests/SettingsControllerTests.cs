using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Web.Controllers;
using TradingApp.Web.Infrastructure;
using Xunit;

namespace TradingApp.Tests;

public class SettingsControllerTests
{
    private readonly SettingsService _service;
    private readonly Mock<IAlpacaSettingsRepository> _repoMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _repoMock = new Mock<IAlpacaSettingsRepository>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _service = new SettingsService(_repoMock.Object, _httpClientFactoryMock.Object);
        _controller = new SettingsController(_service);
    }

    [Fact]
    public async Task GetAlpaca_ShouldReturnSettings()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.AlpacaSettings { Id = 1, ApiKey = "key", ApiSecret = "SECRET123456", BaseUrl = "https://api.alpaca.markets" });

        // Act
        var result = await _controller.GetAlpaca(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var dto = okResult!.Value as AlpacaSettingsDto;
        dto.Should().NotBeNull();
        dto!.ApiKey.Should().Be("key");
    }

    [Fact]
    public async Task PostAlpaca_ShouldReturnBadRequest_WhenBodyIsNull()
    {
        // Act
        var result = await _controller.PostAlpaca(null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PostAlpaca_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var request = new SaveAlpacaRequest
        {
            ApiKey = "key",
            ApiSecret = "secret",
            BaseUrl = "https://api.alpaca.markets"
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}", Encoding.UTF8, "application/json") });
        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("Alpaca")).Returns(httpClient);
        _repoMock.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Domain.AlpacaSettings?)null);

        // Act
        var result = await _controller.PostAlpaca(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task PostAlpaca_ShouldReturnBadRequest_WhenArgumentException()
    {
        // Arrange
        var request = new SaveAlpacaRequest
        {
            ApiKey = "",
            BaseUrl = "https://api.alpaca.markets"
        };

        // Will throw ArgumentException when ApiKey is empty

        // Act
        var result = await _controller.PostAlpaca(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PostAlpaca_ShouldReturnBadRequest_WhenInvalidOperationException()
    {
        // Arrange
        var request = new SaveAlpacaRequest
        {
            ApiKey = "key",
            ApiSecret = "secret",
            BaseUrl = "https://api.alpaca.markets"
        };

        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized });
        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        _httpClientFactoryMock.Setup(f => f.CreateClient("Alpaca")).Returns(httpClient);
        _repoMock.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((Domain.AlpacaSettings?)null);

        // Act
        var result = await _controller.PostAlpaca(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
