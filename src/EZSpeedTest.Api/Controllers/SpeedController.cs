using EZSpeedTest.Application.SpeedTest.Commands;
using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Application.SpeedTest.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EZSpeedTest.Api.Controllers;

[ApiController]
[Route("api/v1/speed")]
[Produces("application/json")]
public class SpeedController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SpeedController> _logger;

    public SpeedController(IMediator mediator, ILogger<SpeedController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Measure ping latency to a specific host
    /// </summary>
    /// <param name="request">Ping request with host information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ping measurement results</returns>
    [HttpPost("ping")]
    [ProducesResponseType<PingResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PingResponseDto>> MeasurePing(
        [FromBody] PingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new MeasurePingCommand(request.Host);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            var problemDetails = new ValidationProblemDetails();
            var errorGroups = ex.Errors.GroupBy(e => e.PropertyName);
            foreach (var group in errorGroups)
            {
                problemDetails.Errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
            }
            return BadRequest(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to measure ping for host: {Host}", request.Host);
            return Problem("Failed to measure ping", statusCode: 500);
        }
    }

    /// <summary>
    /// Measure download speed from a specific server
    /// </summary>
    /// <param name="request">Download request with server URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download speed measurement results</returns>
    [HttpPost("download")]
    [ProducesResponseType<DownloadResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DownloadResponseDto>> MeasureDownload(
        [FromBody] DownloadRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new MeasureDownloadCommand(request.ServerUrl);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            var problemDetails = new ValidationProblemDetails();
            var errorGroups = ex.Errors.GroupBy(e => e.PropertyName);
            foreach (var group in errorGroups)
            {
                problemDetails.Errors[group.Key] = group.Select(e => e.ErrorMessage).ToArray();
            }
            return BadRequest(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to measure download speed from: {ServerUrl}", request.ServerUrl);
            return Problem("Failed to measure download speed", statusCode: 500);
        }
    }

    /// <summary>
    /// Get list of available speed test servers
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available servers</returns>
    [HttpGet("servers")]
    [ProducesResponseType<IReadOnlyList<SpeedTestServerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SpeedTestServerDto>>> GetServers(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAvailableServersQuery();
            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve available servers");
            return Problem("Failed to retrieve servers", statusCode: 500);
        }
    }

    /// <summary>
    /// Legacy ping endpoint for backward compatibility
    /// </summary>
    [HttpGet("ping")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult LegacyPing() => Ok(new { utc = DateTimeOffset.UtcNow });
}
