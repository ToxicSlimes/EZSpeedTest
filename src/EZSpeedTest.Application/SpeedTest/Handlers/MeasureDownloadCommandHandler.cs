using EZSpeedTest.Application.SpeedTest.Commands;
using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Application.SpeedTest.Mapping;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EZSpeedTest.Application.SpeedTest.Handlers;

public sealed class MeasureDownloadCommandHandler : IRequestHandler<MeasureDownloadCommand, DownloadResponseDto>
{
    private readonly ISpeedTestService _speedTestService;
    private readonly IValidator<DownloadRequestDto> _validator;
    private readonly ILogger<MeasureDownloadCommandHandler> _logger;

    public MeasureDownloadCommandHandler(
        ISpeedTestService speedTestService,
        IValidator<DownloadRequestDto> validator,
        ILogger<MeasureDownloadCommandHandler> logger)
    {
        _speedTestService = speedTestService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<DownloadResponseDto> Handle(MeasureDownloadCommand request, CancellationToken cancellationToken)
    {
        var dto = new DownloadRequestDto { ServerUrl = request.ServerUrl };
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        
        _logger.LogInformation("Processing download command for URL: {ServerUrl}", request.ServerUrl);
        
        var serverUri = new Uri(request.ServerUrl);
        var result = await _speedTestService.MeasureDownloadAsync(serverUri, cancellationToken);
        
        var response = result.ToDto();
        
        _logger.LogInformation("Download measurement completed: {Mbps} Mbps from {ServerUrl}", 
            response.Mbps, request.ServerUrl);
        
        return response;
    }
}
