using System.Diagnostics;
using System.Net.NetworkInformation;
using EZSpeedTest.Application.SpeedTest;
using EZSpeedTest.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EZSpeedTest.Infrastructure.SpeedTest;

public sealed class SpeedTestService : ISpeedTestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpeedTestService> _logger;
    private readonly SpeedTestSettings _settings;

    public SpeedTestService(
        HttpClient httpClient,
        ILogger<SpeedTestService> logger,
        IOptions<SpeedTestSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<PingResult> MeasurePingAsync(string host, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ping measurement for host: {Host}", host);
        
        var ping = new Ping();
        var results = new List<double>();
        var successCount = 0;

        try
        {
            for (int i = 0; i < _settings.PingAttemptCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                try
                {
                    var reply = await ping.SendPingAsync(host, (int)_settings.PingTimeout.TotalMilliseconds);
                    
                    if (reply.Status == IPStatus.Success)
                    {
                        results.Add(reply.RoundtripTime);
                        successCount++;
                        _logger.LogDebug("Ping {Attempt}/{Total} to {Host}: {RoundtripTime}ms", 
                            i + 1, _settings.PingAttemptCount, host, reply.RoundtripTime);
                    }
                    else
                    {
                        _logger.LogDebug("Ping {Attempt}/{Total} to {Host} failed: {Status}", 
                            i + 1, _settings.PingAttemptCount, host, reply.Status);
                    }
                }
                catch (PingException ex)
                {
                    _logger.LogDebug(ex, "Ping {Attempt}/{Total} to {Host} threw exception", 
                        i + 1, _settings.PingAttemptCount, host);
                }

                // Small delay between pings
                if (i < _settings.PingAttemptCount - 1)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }

            if (results.Count == 0)
            {
                throw new InvalidOperationException($"All ping attempts to {host} failed");
            }

            results.Sort();
            
            var result = new PingResult
            {
                AverageMs = results.Average(),
                MinMs = results.Min(),
                MaxMs = results.Max(),
                MedianMs = GetMedian(results),
                SuccessfulAttempts = successCount,
                TotalAttempts = _settings.PingAttemptCount,
                Host = host,
                Timestamp = DateTimeOffset.UtcNow
            };

            _logger.LogInformation("Ping measurement completed for {Host}: {AverageMs}ms average, {PacketLoss}% packet loss", 
                host, result.AverageMs, result.PacketLossPercent);

            return result;
        }
        finally
        {
            ping.Dispose();
        }
    }

    public async Task<DownloadResult> MeasureDownloadAsync(Uri url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting download measurement from: {Url}", url);
        
        var stopwatch = Stopwatch.StartNew();
        long totalBytes = 0;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_settings.DownloadTimeout);

            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;
            _logger.LogDebug("Download response received, Content-Length: {ContentLength}", contentLength);

            using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            var buffer = new byte[_settings.BufferSizeKb * 1024];
            
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, cts.Token)) > 0)
            {
                totalBytes += bytesRead;
                cts.Token.ThrowIfCancellationRequested();
            }

            stopwatch.Stop();
            
            if (totalBytes == 0)
            {
                throw new InvalidOperationException("No data was downloaded");
            }

            var durationSeconds = stopwatch.Elapsed.TotalSeconds;
            var mbps = (totalBytes * 8.0) / (durationSeconds * 1_000_000);

            var result = new DownloadResult
            {
                Mbps = mbps,
                BytesDownloaded = totalBytes,
                DurationSeconds = durationSeconds,
                ServerUrl = url,
                Timestamp = DateTimeOffset.UtcNow
            };

            _logger.LogInformation("Download measurement completed: {Mbps:F2} Mbps, {BytesDownloaded} bytes in {DurationSeconds:F2}s", 
                result.Mbps, result.BytesDownloaded, result.DurationSeconds);

            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Download measurement was cancelled by user");
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Download measurement timed out after {Timeout}s", _settings.DownloadTimeout.TotalSeconds);
            throw new TimeoutException($"Download measurement timed out after {_settings.DownloadTimeout.TotalSeconds}s");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download measurement failed for URL: {Url}", url);
            throw;
        }
    }

    public async Task<SpeedTestResult> RunFullTestAsync(SpeedTestServer server, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full speed test for server: {ServerName}", server.Name);

        try
        {
            // First, ping the server host
            var pingResult = await MeasurePingAsync(server.Url.Host, cancellationToken);
            
            // Then, measure download speed
            var downloadResult = await MeasureDownloadAsync(server.Url, cancellationToken);

            var result = new SpeedTestResult
            {
                PingMs = pingResult.AverageMs,
                DownloadMbps = downloadResult.Mbps,
                UploadMbps = null, // Upload not implemented in S1
                Timestamp = DateTimeOffset.UtcNow,
                Server = server.Name,
                Region = server.Region,
                BytesDownloaded = downloadResult.BytesDownloaded,
                DownloadDurationSeconds = downloadResult.DurationSeconds,
                BytesUploaded = null,
                UploadDurationSeconds = null
            };

            _logger.LogInformation("Full speed test completed for {ServerName}: {PingMs}ms ping, {DownloadMbps:F2} Mbps download", 
                server.Name, result.PingMs, result.DownloadMbps);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Full speed test failed for server: {ServerName}", server.Name);
            throw;
        }
    }

    private static double GetMedian(List<double> sortedValues)
    {
        var count = sortedValues.Count;
        if (count % 2 == 0)
        {
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2.0;
        }
        return sortedValues[count / 2];
    }
}
