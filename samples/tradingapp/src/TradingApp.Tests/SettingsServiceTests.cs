using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;
using System.Net.Http;
using System.Text;
using TradingApp.Application;
using TradingApp.Application.DTOs;
using TradingApp.Domain;
using Xunit;

namespace TradingApp.Tests;

public class SettingsServiceTests
{
    [Fact]
    public async Task GetAlpacaSettingsAsync_ShouldReturnMaskedSecret_WhenSettingsExist()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        
        var settings = new AlpacaSettings
        {
            Id = 1,
            ApiKey = "TEST_KEY",
            ApiSecret = "SECRET123456",
            BaseUrl = "https://api.alpaca.markets"
        };

        repoMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await service.GetAlpacaSettingsAsync(CancellationToken.None);

        // Assert
        result.ApiKey.Should().Be("TEST_KEY");
        result.ApiSecretMasked.Should().Be("****3456"); // Last 4 chars
        result.BaseUrl.Should().Be("https://api.alpaca.markets");
    }

    [Fact]
    public async Task GetAlpacaSettingsAsync_ShouldReturnEmptyDto_WhenSettingsDoNotExist()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        repoMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlpacaSettings?)null);

        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        var result = await service.GetAlpacaSettingsAsync(CancellationToken.None);

        // Assert
        result.ApiKey.Should().BeNull();
        result.ApiSecretMasked.Should().BeNull();
        result.BaseUrl.Should().BeNull();
    }

    [Fact]
    public async Task SaveAlpacaSettingsAsync_ShouldThrow_WhenApiKeyIsMissing()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        var act = async () => await service.SaveAlpacaSettingsAsync(null, "secret", "https://api.alpaca.markets", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ApiKey*");
    }

    [Fact]
    public async Task SaveAlpacaSettingsAsync_ShouldThrow_WhenBaseUrlIsMissing()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        var act = async () => await service.SaveAlpacaSettingsAsync("key", "secret", null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*BaseUrl*");
    }

    [Fact]
    public async Task SaveAlpacaSettingsAsync_ShouldTrimAndNormalizeUrl()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        httpClientFactoryMock.Setup(f => f.CreateClient("Alpaca")).Returns(httpClient);

        repoMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlpacaSettings?)null);

        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        await service.SaveAlpacaSettingsAsync("  key  ", "secret", "  https://api.alpaca.markets/  ", CancellationToken.None);

        // Assert
        repoMock.Verify(r => r.SaveAsync(It.Is<AlpacaSettings>(s => 
            s.ApiKey == "key" && 
            s.BaseUrl == "https://api.alpaca.markets"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAlpacaSettingsAsync_ShouldValidateCredentials_WhenSecretProvided()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Headers.Contains("APCA-API-KEY-ID") &&
                    req.Headers.Contains("APCA-API-SECRET-KEY")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        httpClientFactoryMock.Setup(f => f.CreateClient("Alpaca")).Returns(httpClient);

        repoMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlpacaSettings?)null);

        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        await service.SaveAlpacaSettingsAsync("key", "secret", "https://api.alpaca.markets", CancellationToken.None);

        // Assert
        httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SaveAlpacaSettingsAsync_ShouldThrow_WhenCredentialsInvalid()
    {
        // Arrange
        var repoMock = new Mock<IAlpacaSettingsRepository>();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        httpClientFactoryMock.Setup(f => f.CreateClient("Alpaca")).Returns(httpClient);

        repoMock
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AlpacaSettings?)null);

        var service = new SettingsService(repoMock.Object, httpClientFactoryMock.Object);

        // Act
        var act = async () => await service.SaveAlpacaSettingsAsync("key", "secret", "https://api.alpaca.markets", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invalid*");
    }
}
