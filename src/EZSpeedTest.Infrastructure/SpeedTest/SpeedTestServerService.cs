using EZSpeedTest.Application.SpeedTest;
using EZSpeedTest.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EZSpeedTest.Infrastructure.SpeedTest;

public sealed class SpeedTestServerService : ISpeedTestServerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SpeedTestServerService> _logger;
    private readonly Lazy<IReadOnlyList<SpeedTestServer>> _servers;

    public SpeedTestServerService(IConfiguration configuration, ILogger<SpeedTestServerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _servers = new Lazy<IReadOnlyList<SpeedTestServer>>(LoadServersFromConfiguration);
    }

    public Task<IReadOnlyList<SpeedTestServer>> GetAvailableServersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_servers.Value);
    }

    public async Task<SpeedTestServer?> GetBestServerAsync(CancellationToken cancellationToken = default)
    {
        var servers = await GetAvailableServersAsync(cancellationToken);
        
        // For now, return the first active server with highest priority
        // In the future, we can implement ping-based selection
        return servers
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.Priority)
            .FirstOrDefault();
    }

    public async Task<SpeedTestServer?> GetServerByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var servers = await GetAvailableServersAsync(cancellationToken);
        return servers.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private IReadOnlyList<SpeedTestServer> LoadServersFromConfiguration()
    {
        var servers = new List<SpeedTestServer>();
        
        try
        {
            var serverSection = _configuration.GetSection("SpeedTest:Servers");
            
            foreach (var serverConfig in serverSection.GetChildren())
            {
                var name = serverConfig["Name"];
                var url = serverConfig["Url"];
                var region = serverConfig["Region"];
                
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(region))
                {
                    _logger.LogWarning("Skipping invalid server configuration: Name={Name}, Url={Url}, Region={Region}", 
                        name, url, region);
                    continue;
                }

                if (!Uri.TryCreate(url, UriKind.Absolute, out var serverUri))
                {
                    _logger.LogWarning("Invalid URL for server {Name}: {Url}", name, url);
                    continue;
                }

                var server = new SpeedTestServer
                {
                    Name = name,
                    Url = serverUri,
                    Region = region,
                    Country = serverConfig["Country"],
                    City = serverConfig["City"],
                    IsActive = serverConfig.GetValue("IsActive", true),
                    Priority = serverConfig.GetValue("Priority", 0)
                };

                servers.Add(server);
            }

            _logger.LogInformation("Loaded {Count} speed test servers from configuration", servers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load speed test servers from configuration");
        }

        // Add default servers if none configured
        if (servers.Count == 0)
        {
            servers.AddRange(GetDefaultServers());
            _logger.LogInformation("Using {Count} default speed test servers", servers.Count);
        }

        return servers.AsReadOnly();
    }

    private static IEnumerable<SpeedTestServer> GetDefaultServers()
    {
        return new[]
        {
            new SpeedTestServer
            {
                Name = "Cloudflare Test File",
                Url = new Uri("https://speed.cloudflare.com/__down?bytes=10000000"),
                Region = "Global",
                Country = "Global",
                City = "CDN",
                IsActive = true,
                Priority = 100
            },
            new SpeedTestServer
            {
                Name = "Fast.com Test File",
                Url = new Uri("https://api.fast.com/netflix/speedtest/v2/download"),
                Region = "Global", 
                Country = "Global",
                City = "CDN",
                IsActive = true,
                Priority = 90
            },
            new SpeedTestServer
            {
                Name = "Google Test File",
                Url = new Uri("https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png"),
                Region = "Global",
                Country = "Global", 
                City = "CDN",
                IsActive = true,
                Priority = 80
            }
        };
    }
}
