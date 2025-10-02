using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Domain.Models;
using Mapster;

namespace EZSpeedTest.Application.SpeedTest.Mapping;

public static class SpeedTestMappingExtensions
{
    public static PingResponseDto ToDto(this PingResult result) => result.Adapt<PingResponseDto>();

    public static DownloadResponseDto ToDto(this DownloadResult result) => result.Adapt<DownloadResponseDto>();

    public static SpeedTestServerDto ToDto(this SpeedTestServer server) => server.Adapt<SpeedTestServerDto>();
}
