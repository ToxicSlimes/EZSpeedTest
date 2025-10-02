namespace EZSpeedTest.Application.SpeedTest.Dto;

public sealed record SpeedTestServerDto
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string Region { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
}
