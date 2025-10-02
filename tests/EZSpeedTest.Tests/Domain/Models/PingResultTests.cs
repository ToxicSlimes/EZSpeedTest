using EZSpeedTest.Domain.Models;

namespace EZSpeedTest.Tests.Domain.Models;

public class PingResultTests
{
    [Fact]
    public void PacketLossPercent_ShouldCalculateCorrectly_WhenAllPingsSucceed()
    {
        // Arrange
        var result = new PingResult
        {
            AverageMs = 50.0,
            MinMs = 45.0,
            MaxMs = 55.0,
            MedianMs = 50.0,
            SuccessfulAttempts = 4,
            TotalAttempts = 4,
            Host = "google.com",
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.Equal(0.0, result.PacketLossPercent);
    }

    [Fact]
    public void PacketLossPercent_ShouldCalculateCorrectly_WhenSomePingsFail()
    {
        // Arrange
        var result = new PingResult
        {
            AverageMs = 50.0,
            MinMs = 45.0,
            MaxMs = 55.0,
            MedianMs = 50.0,
            SuccessfulAttempts = 3,
            TotalAttempts = 4,
            Host = "google.com",
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.Equal(25.0, result.PacketLossPercent);
    }

    [Fact]
    public void PacketLossPercent_ShouldCalculateCorrectly_WhenAllPingsFail()
    {
        // Arrange
        var result = new PingResult
        {
            AverageMs = 0.0,
            MinMs = 0.0,
            MaxMs = 0.0,
            MedianMs = 0.0,
            SuccessfulAttempts = 0,
            TotalAttempts = 4,
            Host = "invalid.host",
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.Equal(100.0, result.PacketLossPercent);
    }
}
