using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability.Tracing;

public static class OpenTelemetryConfig
{
    public static TracerProvider Configure()
    {
        return Sdk.CreateTracerProviderBuilder()
            .AddSource("Conference.Platform")
            .AddConsoleExporter()
            .Build();
    }
}
