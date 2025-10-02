using EZSpeedTest.Application.SpeedTest;
using EZSpeedTest.Domain.Models;
using EZSpeedTest.Infrastructure.SpeedTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;

namespace EZSpeedTest.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SpeedTestSettings>(configuration.GetSection("SpeedTest:Settings"));

        services.AddScoped<ISpeedTestService, SpeedTestService>();
        services.AddSingleton<ISpeedTestServerService, SpeedTestServerService>();

        services.AddHttpClient<SpeedTestService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(2); 
            client.DefaultRequestHeaders.Add("User-Agent", "EZSpeedTest/1.0");
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetTimeoutPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (_, timespan, retryCount, _) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(30);
    }
}
