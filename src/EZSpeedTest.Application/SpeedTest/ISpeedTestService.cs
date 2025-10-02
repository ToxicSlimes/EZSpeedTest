using EZSpeedTest.Domain.Models;

namespace EZSpeedTest.Application.SpeedTest;

public interface ISpeedTestService
{
    Task<PingResult> MeasurePingAsync(string host, CancellationToken cancellationToken = default);
    Task<DownloadResult> MeasureDownloadAsync(Uri url, CancellationToken cancellationToken = default);
    Task<SpeedTestResult> RunFullTestAsync(SpeedTestServer server, CancellationToken cancellationToken = default);
}
