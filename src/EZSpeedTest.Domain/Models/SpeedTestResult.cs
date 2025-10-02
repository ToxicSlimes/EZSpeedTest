namespace EZSpeedTest.Domain.Models;

public sealed record SpeedTestResult
{
    public required double PingMs { get; init; }
    public required double DownloadMbps { get; init; }
    public double? UploadMbps { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string Server { get; init; }
    public string? Region { get; init; }
    public required long BytesDownloaded { get; init; }
    public required double DownloadDurationSeconds { get; init; }
    public long? BytesUploaded { get; init; }
    public double? UploadDurationSeconds { get; init; }
}
