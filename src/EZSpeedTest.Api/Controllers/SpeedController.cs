using Microsoft.AspNetCore.Mvc;

namespace EZSpeedTest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpeedController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { utc = DateTimeOffset.UtcNow });

    // TODO Sprint 1: /download, /upload, /result, /history
}
