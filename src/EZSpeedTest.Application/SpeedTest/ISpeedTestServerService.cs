using EZSpeedTest.Domain.Models;

namespace EZSpeedTest.Application.SpeedTest;

public interface ISpeedTestServerService
{
    Task<IReadOnlyList<SpeedTestServer>> GetAvailableServersAsync(CancellationToken cancellationToken = default);
    Task<SpeedTestServer?> GetBestServerAsync(CancellationToken cancellationToken = default);
    Task<SpeedTestServer?> GetServerByNameAsync(string name, CancellationToken cancellationToken = default);
}
