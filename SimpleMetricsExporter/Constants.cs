namespace SimpleMetricsExporter
{
    internal static class StaticRouteConstants
    {
        public const string StaticFolder = "Static";
        public static string StaticPath = Path.Combine(AppContext.BaseDirectory, StaticFolder);
        public const string DefaultPrefix = "/metrics";
        public const string DefaultPrometheusEndpoint = "prometheus";
        public const string IndexHtml = "index.html";
    }
}
