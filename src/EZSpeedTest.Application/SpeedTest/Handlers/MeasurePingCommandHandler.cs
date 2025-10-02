using EZSpeedTest.Application.SpeedTest.Commands;
using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Application.SpeedTest.Mapping;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EZSpeedTest.Application.SpeedTest.Handlers;

public sealed class MeasurePingCommandHandler : IRequestHandler<MeasurePingCommand, PingResponseDto>
{
    private readonly ISpeedTestService _speedTestService;
    private readonly IValidator<PingRequestDto> _validator;
    private readonly ILogger<MeasurePingCommandHandler> _logger;

    public MeasurePingCommandHandler(
        ISpeedTestService speedTestService,
        IValidator<PingRequestDto> validator,
        ILogger<MeasurePingCommandHandler> logger)
    {
        _speedTestService = speedTestService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<PingResponseDto> Handle(MeasurePingCommand request, CancellationToken cancellationToken)
    {
        var dto = new PingRequestDto { Host = request.Host };
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        
        _logger.LogInformation("Processing ping command for host: {Host}", request.Host);
        
        var result = await _speedTestService.MeasurePingAsync(request.Host, cancellationToken);
        
        var response = result.ToDto();
        
        _logger.LogInformation("Ping measurement completed for {Host}: {AverageMs}ms", 
            request.Host, response.AverageMs);
        
        return response;
    }
}
