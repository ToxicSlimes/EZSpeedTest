using EZSpeedTest.Application.SpeedTest.Dto;
using MediatR;

namespace EZSpeedTest.Application.SpeedTest.Queries;

public sealed record GetAvailableServersQuery : IRequest<IReadOnlyList<SpeedTestServerDto>>;
