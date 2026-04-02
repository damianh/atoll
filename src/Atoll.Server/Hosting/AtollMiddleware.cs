using Microsoft.AspNetCore.Http;

namespace Atoll.Server.Hosting;

/// <summary>
/// ASP.NET Core middleware that intercepts incoming HTTP requests and dispatches them
/// to Atoll page components or API endpoints based on the route table.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AtollMiddleware"/> is registered in the ASP.NET Core pipeline via
/// <see cref="AtollApplicationBuilderExtensions.UseAtoll"/>. It sits in the middleware
/// chain and delegates to <see cref="AtollRequestHandler"/> for route matching and
/// request handling.
/// </para>
/// <para>
/// If no matching route is found, the middleware passes the request to the next
/// middleware in the pipeline (e.g., static files or a 404 handler).
/// </para>
/// </remarks>
public sealed class AtollMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AtollRequestHandler _handler;

    /// <summary>
    /// Initializes a new <see cref="AtollMiddleware"/> with the next middleware delegate
    /// and the Atoll request handler.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="handler">The Atoll request handler.</param>
    public AtollMiddleware(RequestDelegate next, AtollRequestHandler handler)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(handler);
        _next = next;
        _handler = handler;
    }

    /// <summary>
    /// Processes an incoming HTTP request. If the request matches an Atoll route,
    /// renders the page or dispatches the endpoint. Otherwise, passes the request
    /// to the next middleware.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var handled = await _handler.TryHandleAsync(context);
        if (!handled)
        {
            await _next(context);
        }
    }
}
