using System.Collections.Concurrent;

namespace ApiGateway.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _requests = new();
    private readonly int _limit = 100;
    private readonly int _windowSeconds = 60;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        if (_requests.TryGetValue(ip, out var entry))
        {
            if (now - entry.WindowStart > TimeSpan.FromSeconds(_windowSeconds))
            {
                _requests[ip] = (1, now);
            }
            else if (entry.Count >= _limit)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Too many requests. Try again later.");
                return;
            }
            else
            {
                _requests[ip] = (entry.Count + 1, entry.WindowStart);
            }
        }
        else
        {
            _requests[ip] = (1, now);
        }

        await _next(context);
    }
}
