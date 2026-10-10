using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTelemetry.Metrics;
using SimpleMetricsExporter.Helper;
using SimpleMetricsExporter.Metrics;

namespace SimpleMetricsExporter.Extensions
{
    public static class MeterProviderBuilderExtensions
    {
        public static MeterProviderBuilder AddBuiltInMeterics(this MeterProviderBuilder builder, IEnumerable<string>? includes = null, IEnumerable<string>? excludes = null)
        {
            includes = includes ?? Array.Empty<string>();
            excludes = excludes ?? Array.Empty<string>();
            var meters = BuiltInMetricNames.BuiltInMetricsDiagnostics
                .Concat(BuiltInMetricNames.BuiltInMetricsRuntime)
                .Concat(BuiltInMetricNames.BuiltInSystemNet)
                .Concat(BuiltInMetricNames.BuiltInAspNetCore)
                .Concat(BuiltInMetricNames.BuiltInEFCore)
                .Distinct()
                .ToArray();
            var matches = Matcher.Execute(meters, includes, excludes).ToArray();
            builder.AddMeter(matches);
            return builder;
        }
    }
}
