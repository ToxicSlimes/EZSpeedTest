namespace EZSpeedTest.Application.SpeedTest.Dto;

public sealed record PingRequestDto
{
    public required string Host { get; init; }
}
