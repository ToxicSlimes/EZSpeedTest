namespace EZSpeedTest.Application.SpeedTest;

public interface ISpeedTestService
{
    Task<double> MeasureDownloadMbpsAsync(CancellationToken ct = default);
    Task<double> MeasureUploadMbpsAsync(CancellationToken ct = default);
}
