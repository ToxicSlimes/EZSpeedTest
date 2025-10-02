using EZSpeedTest.Api.Middleware;
using Serilog;

namespace EZSpeedTest.Api.Startup;

public static class WebApplicationExtensions
{
    public static WebApplication UseApi(this WebApplication app, IWebHostEnvironment env)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "[{CorrelationId}] HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var correlationId = httpContext.Response.Headers["X-Correlation-ID"].FirstOrDefault() ?? "unknown";
                diagnosticContext.Set("CorrelationId", correlationId);
            };
        });

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


