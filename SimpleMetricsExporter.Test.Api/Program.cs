using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SimpleMetricsExporter.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddOpenTelemetry()
    .WithMetrics(b =>
        b.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            //.AddProcessInstrumentation()
            .AddRuntimeInstrumentation()
            //.AddSqlClientInstrumentation()
            .AddBuiltInMeterics())
    .WithMetrics(b => b.AddPrometheusExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
