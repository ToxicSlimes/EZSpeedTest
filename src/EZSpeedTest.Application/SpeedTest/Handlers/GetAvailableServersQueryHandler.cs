using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Application.SpeedTest.Mapping;
using EZSpeedTest.Application.SpeedTest.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EZSpeedTest.Application.SpeedTest.Handlers;

public sealed class GetAvailableServersQueryHandler : IRequestHandler<GetAvailableServersQuery, IReadOnlyList<SpeedTestServerDto>>
{
    private readonly ISpeedTestServerService _serverService;
    private readonly ILogger<GetAvailableServersQueryHandler> _logger;

    public GetAvailableServersQueryHandler(
        ISpeedTestServerService serverService,
        ILogger<GetAvailableServersQueryHandler> logger)
    {
        _serverService = serverService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SpeedTestServerDto>> Handle(GetAvailableServersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving available speed test servers");
        
        var servers = await _serverService.GetAvailableServersAsync(cancellationToken);
        
        var result = servers.Select(s => s.ToDto()).ToList().AsReadOnly();
        
        _logger.LogInformation("Retrieved {Count} available servers", result.Count);
        
        return result;
    }
}
