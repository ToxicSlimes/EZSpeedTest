namespace EZSpeedTest.Application.SpeedTest.Dto;

public sealed record DownloadResponseDto
{
    public required double Mbps { get; init; }
    public required double MegaBytesPerSecond { get; init; }
    public required long BytesDownloaded { get; init; }
    public required double DurationSeconds { get; init; }
    public required string ServerUrl { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
