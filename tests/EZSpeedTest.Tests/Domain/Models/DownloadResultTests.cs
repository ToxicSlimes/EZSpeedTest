using EZSpeedTest.Domain.Models;

namespace EZSpeedTest.Tests.Domain.Models;

public class DownloadResultTests
{
    [Fact]
    public void KBps_ShouldConvertFromMbpsCorrectly()
    {
        // Arrange
        var result = new DownloadResult
        {
            Mbps = 100.0, // 100 Mbps
            BytesDownloaded = 10_000_000,
            DurationSeconds = 1.0,
            ServerUrl = new Uri("https://example.com/test"),
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.Equal(12500.0, result.KBps); // 100 * 1000 / 8 = 12500 KB/s
    }

    [Fact]
    public void MBps_ShouldConvertFromMbpsCorrectly()
    {
        // Arrange
        var result = new DownloadResult
        {
            Mbps = 80.0, // 80 Mbps
            BytesDownloaded = 10_000_000,
            DurationSeconds = 1.0,
            ServerUrl = new Uri("https://example.com/test"),
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.Equal(10.0, result.MegaBytesPerSecond); // 80 / 8 = 10 MB/s
    }

    [Theory]
    [InlineData(1.0, 1.0, 8.0)] // 1 MB in 1 second = 8 Mbps
    [InlineData(10.0, 1.0, 80.0)] // 10 MB in 1 second = 80 Mbps
    [InlineData(5.0, 2.0, 20.0)] // 5 MB in 2 seconds = 20 Mbps
    public void Mbps_ShouldBeCalculatedCorrectly(double megabytes, double seconds, double expectedMbps)
    {
        // Arrange
        var bytes = (long)(megabytes * 1024 * 1024);
        var result = new DownloadResult
        {
            Mbps = expectedMbps,
            BytesDownloaded = bytes,
            DurationSeconds = seconds,
            ServerUrl = new Uri("https://example.com/test"),
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.Equal(expectedMbps, result.Mbps);
    }
}
