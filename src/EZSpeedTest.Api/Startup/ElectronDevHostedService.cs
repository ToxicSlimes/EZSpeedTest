using System.Diagnostics;
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

    public ElectronDevHostedService(IWebHostEnvironment env, IHostApplicationLifetime lifetime, IConfiguration config)
    {
        _env = env;
        _logger = Log.ForContext<ElectronDevHostedService>();
        _lifetime = lifetime;
        _config = config;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment()) return Task.CompletedTask;

        var autoStart = _config.GetValue<bool>("Electron:AutoStartDev");
        if (!autoStart)
        {
            _logger.Information("Electron auto-start is disabled");
            return Task.CompletedTask;
        }

        try
        {
            var backendUrl = _config["Urls"] ?? "http://localhost:5299";
            _logger.Information("Starting Electron with backend URL: {BackendUrl}", backendUrl);

            var repoRoot = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", ".."));
            var electronDir = Path.Combine(repoRoot, "ui", "electron");

            if (!Directory.Exists(electronDir))
            {
                _logger.Error("Electron directory not found: {ElectronDir}", electronDir);
                return Task.CompletedTask;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c npm run dev",
                WorkingDirectory = electronDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment =
                {
                    ["BACKEND_URL"] = backendUrl,
                    ["START_API"] = "false"
                }
            };

            _logger.Information("Starting Electron process in: {WorkingDir}", electronDir);
            _process = Process.Start(psi);

            if (_process == null)
            {
                _logger.Error("Failed to start Electron process");
                return Task.CompletedTask;
            }

            _process.OutputDataReceived += (_, e) => {
                if (e.Data is not null)
                    _logger.Information("[ELECTRON] {Line}", e.Data);
            };
            _process.ErrorDataReceived += (_, e) => {
                if (e.Data is not null)
                    _logger.Warning("[ELECTRON] {Line}", e.Data);
            };

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            _logger.Information("Electron process started with PID: {ProcessId}", _process.Id);

            _lifetime.ApplicationStopping.Register(() =>
            {
                try
                {
                    if (_process is not { HasExited: false })
                        return;

                    _logger.Information("Stopping Electron process...");
                    _process.Kill(true);
                    _process.Dispose();
                    _logger.Information("Electron process stopped");
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


