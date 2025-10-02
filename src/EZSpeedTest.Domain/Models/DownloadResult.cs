namespace EZSpeedTest.Domain.Models;

public sealed record DownloadResult
{
    public required double Mbps { get; init; }
    public required long BytesDownloaded { get; init; }
    public required double DurationSeconds { get; init; }
    public required Uri ServerUrl { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public double KBps => Mbps * 1000 / 8; // Convert Mbps to KB/s
    public double MegaBytesPerSecond => Mbps / 8; // Convert Mbps to MB/s
}
