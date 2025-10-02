namespace EZSpeedTest.Domain.Models;

public sealed record SpeedTestSettings
{
    public TimeSpan PingTimeout { get; init; } = TimeSpan.FromSeconds(5);
    public int PingAttemptCount { get; init; } = 4;
    public TimeSpan DownloadTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan UploadTimeout { get; init; } = TimeSpan.FromSeconds(30);
    public int BufferSizeKb { get; init; } = 64;
    public int MaxRetryAttempts { get; init; } = 3;
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromSeconds(1);
    public long MinDownloadSizeBytes { get; init; } = 1024 * 1024; // 1MB
    public long MaxDownloadSizeBytes { get; init; } = 100 * 1024 * 1024; // 100MB
}
