using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace SimpleMetricsExporter.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddSimpleMetericsExporter(IServiceCollection services, EnabledMeterics enabledMeterics = EnabledMeterics.All, string endpoint = "/meterics")
        {
            AddSimpleMetericsExporter(services, c =>
            {
                if ((enabledMeterics & EnabledMeterics.AspNetCore) == EnabledMeterics.AspNetCore)
                {
                    c.AddAspNetCoreInstrumentation();
                }
                if ((enabledMeterics & EnabledMeterics.HttpClient) == EnabledMeterics.HttpClient)
                {
                    c.AddHttpClientInstrumentation();
                }
                if ((enabledMeterics & EnabledMeterics.Runtime) == EnabledMeterics.Runtime)
                {
                    c.AddRuntimeInstrumentation();
                }
#if NET8_0_OR_GREATER
                if ((enabledMeterics & EnabledMeterics.Process) == EnabledMeterics.Process)
                {
                    c.AddProcessInstrumentation();
                }
                if ((enabledMeterics & EnabledMeterics.SqlClient) == EnabledMeterics.SqlClient)
                {
                    c.AddSqlClientInstrumentation();
                }
#endif
                if ((enabledMeterics & EnabledMeterics.BuiltIn) == EnabledMeterics.BuiltIn)
                {
                    c.AddBuiltInMeterics();
                }
                c.AddPrometheusExporter(c => c.ScrapeEndpointPath = endpoint);
            });
            return services;
        }

        public static IServiceCollection AddSimpleMetericsExporter(IServiceCollection services, Action<MeterProviderBuilder> configure)
        {
            services.AddOpenTelemetry().WithMetrics(c => configure(c));
            return services;
        }
    }

    [Flags]
    public enum EnabledMeterics
    {
        All = 0xff,
        AspNetCore = 1,
        HttpClient = 1 << 1,
        Runtime = 1 << 2,
#if NET8_0_OR_GREATER
        Process = 1 << 3,
        SqlClient = 1 << 4,
#endif
        BuiltIn = 1 << 5,
    }
}
