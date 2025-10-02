namespace EZSpeedTest.Application.SpeedTest.Dto;

public sealed record DownloadRequestDto
{
    public required string ServerUrl { get; init; }
}
