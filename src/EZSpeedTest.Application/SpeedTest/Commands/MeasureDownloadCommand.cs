using EZSpeedTest.Application.SpeedTest.Dto;
using MediatR;

namespace EZSpeedTest.Application.SpeedTest.Commands;

public sealed record MeasureDownloadCommand(string ServerUrl) : IRequest<DownloadResponseDto>;
