using System.Diagnostics;

namespace Atoll.Middleware.Server.DevServer;

/// <summary>
/// Logs incoming HTTP requests to the console during development.
/// Emits a single line per request: <c>METHOD /path STATUS Xms</c>.
/// WebSocket upgrade requests and internal <c>/__atoll-*</c> paths are excluded
/// to keep the output focused on application traffic.
/// </summary>
public sealed class DevRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DevRequestLoggingMiddleware> _logger;

    /// <summary>
    /// Initializes a new <see cref="DevRequestLoggingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public DevRequestLoggingMiddleware(RequestDelegate next, ILogger<DevRequestLoggingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, timing the downstream pipeline and logging the result.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var path = context.Request.Path.Value ?? "/";

        // Skip internal Atoll paths (live-reload WebSocket, atoll scripts) to reduce noise.
        if (path.StartsWith("/__atoll", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var method = context.Request.Method;
        var startTimestamp = Stopwatch.GetTimestamp();

        await _next(context);

        var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
        var statusCode = context.Response.StatusCode;

        _logger.LogInformation(
            "{Method} {Path} {StatusCode} {ElapsedMs}ms",
            method,
            path,
            statusCode,
            (int)elapsed.TotalMilliseconds);
    }
}
