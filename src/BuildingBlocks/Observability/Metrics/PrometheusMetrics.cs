using System.Diagnostics.Metrics;

namespace BuildingBlocks.Observability.Metrics;

public static class MetricsRegistry
{
    private static readonly Meter Meter = new("Conference.Platform");
    public static readonly Counter<int> RequestsCounter = Meter.CreateCounter<int>("api_requests_total");
    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>("api_request_duration_seconds");
}
