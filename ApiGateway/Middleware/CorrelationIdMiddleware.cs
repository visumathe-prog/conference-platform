namespace ApiGateway.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString();
        context.Request.Headers.TryAdd("X-Correlation-Id", correlationId);
        context.Response.Headers.TryAdd("X-Correlation-Id", correlationId);
        await _next(context);
    }
}
