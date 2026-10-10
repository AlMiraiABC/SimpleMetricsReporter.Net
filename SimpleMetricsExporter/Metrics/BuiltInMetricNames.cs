using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SimpleMetricsExporter.Metrics
{
    internal class BuiltInMetricNames
    {
        // https://learn.microsoft.com/zh-cn/dotnet/core/diagnostics/built-in-metrics-diagnostics
        internal readonly static string[] BuiltInMetricsDiagnostics = {
            "dotnet.health_check.reports",
            "dotnet.health_check.unhealthy_checks",
            "process.cpu.utilization",
            "dotnet.process.memory.virtual.utilization",
            "system.network.connections",
#if NET9_0_OR_GREATER
            "container.cpu.limit.utilization",
            "container.cpu.request.utilization",
            "container.cpu.time",
            "container.memory.limit.utilization",
            "container.memory.request.utilization",
            "container.memory.usage",
#endif
        };
        // https://learn.microsoft.com/zh-cn/dotnet/core/diagnostics/built-in-metrics-runtime
        internal readonly static string[] BuiltInMetricsRuntime = {
#if NET9_0_OR_GREATER
            "dotnet.process.cpu.time",
            "dotnet.process.memory.working_set",
            "dotnet.gc.collections",
            "dotnet.gc.heap.total_allocated",
            "dotnet.gc.last_collection.memory.committed_size",
            "dotnet.gc.last_collection.heap.size",
            "dotnet.gc.last_collection.heap.fragmentation.size",
            "dotnet.gc.pause.time",
            "dotnet.jit.compiled_il.size",
            "dotnet.jit.compiled_methods",
            "dotnet.jit.compilation.time",
            "dotnet.thread_pool.thread.count",
            "dotnet.thread_pool.work_item.count",
            "dotnet.thread_pool.queue.length",
            "dotnet.monitor.lock_contentions",
            "dotnet.timer.count",
            "dotnet.assembly.count",
            "dotnet.exceptions",
#endif
        };
        // https://learn.microsoft.com/zh-cn/dotnet/core/diagnostics/built-in-metrics-system-net
        internal readonly static string[] BuiltInSystemNet = {
            "http.client.open_connections",
            "http.client.connection.duration",
            "http.client.request.duration",
            "http.client.request.time_in_queue",
            "http.client.active_requests",
        };
        // https://learn.microsoft.com/zh-cn/aspnet/core/metrics/overview
        internal readonly static string[] BuiltInAspNetCore = {
#if NET10_0_OR_GREATER
            "http.server.request.duration",
            "http.server.active_requests",
            "aspnetcore.routing.match_attempts",
            "aspnetcore.rate_limiting.active_request_leases",
            "aspnetcore.rate_limiting.request_lease.duration",
            "aspnetcore.rate_limiting.queued_requests",
            "aspnetcore.rate_limiting.request.time_in_queue",
            "aspnetcore.rate_limiting.requests",
            "aspnetcore.header_parsing.parse_errors",
            "aspnetcore.header_parsing.cache_accesses",
            "kestrel.active_connections",
            "kestrel.connection.duration",
            "kestrel.rejected_connections",
            "kestrel.queued_connections",
            "kestrel.queued_requests",
            "kestrel.upgraded_connections",
            "kestrel.tls_handshake.duration",
            "kestrel.active_tls_handshakes",
            "signalr.server.connection.duration",
            "signalr.server.active_connections",
            "aspnetcore.diagnostics.exceptions",
#endif
    };
        // https://learn.microsoft.com/zh-cn/ef/core/logging-events-diagnostics/metrics
        internal readonly static string[] BuiltInEFCore = {
#if NET9_0_OR_GREATER
            "microsoft.entityframeworkcore.active_dbcontexts",
            "microsoft.entityframeworkcore.queries",
            "microsoft.entityframeworkcore.savechanges",
            "microsoft.entityframeworkcore.compiled_query_cache_hits",
            "microsoft.entityframeworkcore.compiled_query_cache_misses",
            "microsoft.entityframeworkcore.execution_strategy_operation_failures",
            "microsoft.entityframeworkcore.optimistic_concurrency_failures",
#endif
        };
    }
}
