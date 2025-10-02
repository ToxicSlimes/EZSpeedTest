namespace EZSpeedTest.Application.SpeedTest.Dto;

public sealed record PingResponseDto
{
    public required double AverageMs { get; init; }
    public required double MinMs { get; init; }
    public required double MaxMs { get; init; }
    public required double MedianMs { get; init; }
    public required double PacketLossPercent { get; init; }
    public required string Host { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
