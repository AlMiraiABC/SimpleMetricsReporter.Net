using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SimpleMetricsExporter.Extensions;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "DemoDashboard";

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSimpleMetericsExporter();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseCors(CorsPolicyName);
app.UseAuthorization();

app.MapControllers();

app.UseSimpleMetricsExporter();

app.Run();
