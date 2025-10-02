using EZSpeedTest.Application.SpeedTest.Dto;
using MediatR;

namespace EZSpeedTest.Application.SpeedTest.Commands;

public sealed record MeasurePingCommand(string Host) : IRequest<PingResponseDto>;
