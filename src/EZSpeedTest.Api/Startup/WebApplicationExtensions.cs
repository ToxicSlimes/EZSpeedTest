using Serilog;

namespace EZSpeedTest.Api.Startup;

public static class WebApplicationExtensions
{
    public static WebApplication UseApi(this WebApplication app, IWebHostEnvironment env)
    {
        app.UseSerilogRequestLogging();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("ElectronCors");
        app.MapControllers();
        app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

        return app;
    }
}


