namespace EZSpeedTest.Domain.Models;

public sealed record PingResult
{
    public required double AverageMs { get; init; }
    public required double MinMs { get; init; }
    public required double MaxMs { get; init; }
    public required double MedianMs { get; init; }
    public required int SuccessfulAttempts { get; init; }
    public required int TotalAttempts { get; init; }
    public required string Host { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public double PacketLossPercent => (TotalAttempts - SuccessfulAttempts) * 100.0 / TotalAttempts;
}
