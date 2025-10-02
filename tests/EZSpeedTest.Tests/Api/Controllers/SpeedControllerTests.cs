using EZSpeedTest.Application.SpeedTest.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

namespace EZSpeedTest.Tests.Api.Controllers;

public class SpeedControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SpeedControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetServers_ShouldReturnServerList()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/speed/servers");

        // Assert
        response.EnsureSuccessStatusCode();
        var servers = await response.Content.ReadFromJsonAsync<SpeedTestServerDto[]>();
        
        Assert.NotNull(servers);
        Assert.NotEmpty(servers);
        Assert.All(servers, server =>
        {
            Assert.NotNull(server.Name);
            Assert.NotNull(server.Url);
            Assert.NotNull(server.Region);
        });
    }

    [Fact]
    public async Task LegacyPing_ShouldReturnUtcTime()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/speed/ping");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        Assert.True(json.RootElement.TryGetProperty("utc", out var utcProperty));
        Assert.True(DateTime.TryParse(utcProperty.GetString(), out _));
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/healthz");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        Assert.True(json.RootElement.TryGetProperty("status", out var statusProperty));
        Assert.Equal("ok", statusProperty.GetString());
    }

    [Fact]
    public async Task MeasurePing_ShouldReturnBadRequest_WhenHostIsEmpty()
    {
        // Arrange
        var request = new PingRequestDto { Host = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/speed/ping", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MeasurePing_ShouldReturnBadRequest_WhenHostIsInvalid()
    {
        // Arrange
        var request = new PingRequestDto { Host = "invalid..host" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/speed/ping", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MeasureDownload_ShouldReturnBadRequest_WhenUrlIsEmpty()
    {
        // Arrange
        var request = new DownloadRequestDto { ServerUrl = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/speed/download", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MeasureDownload_ShouldReturnBadRequest_WhenUrlIsInvalid()
    {
        // Arrange
        var request = new DownloadRequestDto { ServerUrl = "not-a-url" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/speed/download", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
