using Serilog;
using MediatR;
using FluentValidation;
using System.Reflection;

namespace EZSpeedTest.Api.Startup;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddLoggingAndConfiguration(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

        services.AddCors(options =>
        {
            options.AddPolicy("ElectronCors", p =>
                p.SetIsOriginAllowed(_ => true)
                 .AllowAnyHeader()
                 .AllowAnyMethod());
        });

        // Optional Electron dev service (config-gated)
        var enableElectronDev = configuration.GetValue<bool>("Electron:AutoStartDev");
        if (enableElectronDev && env.IsDevelopment())
        {
            services.AddHostedService<ElectronDevHostedService>();
        }

        return services;
    }
}


