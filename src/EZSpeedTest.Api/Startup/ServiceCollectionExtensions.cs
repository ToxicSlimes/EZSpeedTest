using EZSpeedTest.Application;
using EZSpeedTest.Infrastructure;
using FluentValidation;
using MediatR;
using Serilog;
using System.Reflection;

namespace EZSpeedTest.Api.Startup;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddLoggingAndConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "EZSpeedTest.Api")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();


        services.AddCors(options =>
        {
            options.AddPolicy("ElectronCors", p =>
                p.SetIsOriginAllowed(_ => true)
                 .AllowAnyHeader()
                 .AllowAnyMethod());
        });

        services.AddApplication();
        services.AddInfrastructure(configuration);

        var enableElectronDev = configuration.GetValue<bool>("Electron:AutoStartDev");
        if (enableElectronDev && env.IsDevelopment())
        {
            services.AddHostedService<ElectronDevHostedService>();
        }

        return services;
    }
}


