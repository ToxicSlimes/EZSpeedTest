using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using ILogger = Serilog.ILogger;

namespace EZSpeedTest.Api.Startup;

internal sealed class ElectronDevHostedService : IHostedService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger _logger;
    private Process? _process;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IConfiguration _config;
    private readonly IEnumerable<string> _urls;

    public ElectronDevHostedService(IWebHostEnvironment env, IHostApplicationLifetime lifetime, IConfiguration config, IEnumerable<IServerAddressesFeature> addresses)
    {
        _env = env;
        _logger = Log.ForContext<ElectronDevHostedService>();
        _lifetime = lifetime;
        _config = config;
        _urls = addresses.SelectMany(a => a.Addresses);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return Task.CompletedTask;

        try
        {
            var backendUrl = _urls.FirstOrDefault(u => u.StartsWith("https://")) ?? "https://localhost:5299";

            var repoRoot = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", ".."));
            var electronDir = Path.Combine(repoRoot, "ui", "electron");

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command npm run dev",
                WorkingDirectory = electronDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.Environment["BACKEND_URL"] = backendUrl;
            _process = Process.Start(psi);
            _process!.OutputDataReceived += (_, e) => { if (e.Data is not null) _logger.Information("[ELECTRON] {Line}", e.Data); };
            _process!.ErrorDataReceived += (_, e) => { if (e.Data is not null) _logger.Warning("[ELECTRON] {Line}", e.Data); };
            _process!.BeginOutputReadLine();
            _process!.BeginErrorReadLine();

            _lifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    if (_process is { HasExited: false })
                    {
                        _process.Kill(true);
                        _process.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to stop Electron dev process");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start Electron dev process");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


