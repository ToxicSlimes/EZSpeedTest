using EZSpeedTest.Application.SpeedTest.Dto;
using EZSpeedTest.Domain.Models;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EZSpeedTest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        var config = TypeAdapterConfig.GlobalSettings;
        
        ConfigureMappings(config);
        
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static void ConfigureMappings(TypeAdapterConfig config)
    {
        config.NewConfig<DownloadResult, DownloadResponseDto>()
            .Map(dest => dest.ServerUrl, src => src.ServerUrl.ToString());

        config.NewConfig<SpeedTestServer, SpeedTestServerDto>()
            .Map(dest => dest.Url, src => src.Url.ToString());

    }
}
