namespace EZSpeedTest.Domain.Models;

public sealed record SpeedTestServer
{
    public required string Name { get; init; }
    public required Uri Url { get; init; }
    public required string Region { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public bool IsActive { get; init; } = true;
    public int Priority { get; init; } = 0;
}
