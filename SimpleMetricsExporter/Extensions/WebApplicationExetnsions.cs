using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace SimpleMetricsExporter.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSimpleMetricsExporter(this WebApplication app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        var options = app.Services.GetRequiredService<SimpleMetricsExporterOptions>();
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == $"{options.Endpoint}/")
            {
                context.Request.Path = $"{options.Endpoint}/{StaticRouteConstants.IndexHtml}";
            }
            await next();
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(StaticRouteConstants.StaticPath),
            RequestPath = options.Endpoint,
        });

        return app;
    }
}
