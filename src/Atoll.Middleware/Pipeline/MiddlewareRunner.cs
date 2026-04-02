using Atoll.Routing;
using Atoll.Routing.Matching;

namespace Atoll.Middleware.Pipeline;

/// <summary>
/// Executes a middleware pipeline against a request, handling route resolution
/// and rewrite processing.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MiddlewareRunner"/> orchestrates the execution of the middleware pipeline.
/// It creates a <see cref="MiddlewareContext"/>, runs the composed middleware chain,
/// and handles rewrites by re-resolving routes when <see cref="MiddlewareContext.Rewrite(string)"/>
/// is called.
/// </para>
/// <para>
/// The terminal handler (the actual route handler) is provided by the caller and is
/// invoked after all middleware have called <c>next()</c>.
/// </para>
/// </remarks>
public sealed class MiddlewareRunner
{
    private readonly MiddlewareHandler _pipeline;
    private readonly RouteMatcher? _routeMatcher;

    /// <summary>
    /// Initializes a new <see cref="MiddlewareRunner"/> with the specified middleware pipeline
    /// and route matcher.
    /// </summary>
    /// <param name="pipeline">The composed middleware handler (from <see cref="MiddlewareSequencer.Sequence(MiddlewareHandler[])"/>).</param>
    /// <param name="routeMatcher">The route matcher used for rewrite re-resolution.</param>
    public MiddlewareRunner(MiddlewareHandler pipeline, RouteMatcher routeMatcher)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(routeMatcher);
        _pipeline = pipeline;
        _routeMatcher = routeMatcher;
    }

    /// <summary>
    /// Initializes a new <see cref="MiddlewareRunner"/> with the specified middleware pipeline
    /// and no route matcher (rewrites will throw).
    /// </summary>
    /// <param name="pipeline">The composed middleware handler.</param>
    public MiddlewareRunner(MiddlewareHandler pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        _pipeline = pipeline;
        _routeMatcher = null;
    }

    /// <summary>
    /// Executes the middleware pipeline for the given context with the specified
    /// terminal handler.
    /// </summary>
    /// <param name="context">The middleware context.</param>
    /// <param name="terminalHandler">
    /// The terminal handler that produces the response for the matched route.
    /// This is called after all middleware have invoked <c>next()</c>.
    /// </param>
    /// <returns>A task that resolves to the final <see cref="AtollResponse"/>.</returns>
    public Task<AtollResponse> RunAsync(
        MiddlewareContext context,
        Func<MiddlewareContext, Task<AtollResponse>> terminalHandler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(terminalHandler);

        MiddlewareNext terminal = async () =>
        {
            // If middleware requested a rewrite, apply it before terminal handler
            if (context.HasRewrite)
            {
                ApplyRewrite(context);
            }

            return await terminalHandler(context);
        };

        return _pipeline(context, terminal);
    }

    /// <summary>
    /// Executes the middleware pipeline for the given context and route match,
    /// with the specified terminal handler.
    /// </summary>
    /// <param name="routeMatch">The initial route match result.</param>
    /// <param name="request">The incoming request.</param>
    /// <param name="terminalHandler">The terminal handler.</param>
    /// <returns>A task that resolves to the final <see cref="AtollResponse"/>.</returns>
    public Task<AtollResponse> RunAsync(
        RouteMatchResult routeMatch,
        EndpointRequest request,
        Func<MiddlewareContext, Task<AtollResponse>> terminalHandler)
    {
        ArgumentNullException.ThrowIfNull(routeMatch);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(terminalHandler);

        var context = new MiddlewareContext(
            routeMatch.RouteEntry.Pattern,
            routeMatch.Parameters,
            request);

        return RunAsync(context, terminalHandler);
    }

    /// <summary>
    /// Applies a pending rewrite by re-resolving the route using the <see cref="RouteMatcher"/>.
    /// Updates the context's <see cref="MiddlewareContext.RoutePattern"/> and
    /// <see cref="MiddlewareContext.Parameters"/>.
    /// </summary>
    private void ApplyRewrite(MiddlewareContext context)
    {
        if (_routeMatcher is null)
        {
            throw new InvalidOperationException(
                "Cannot apply rewrite: no RouteMatcher was provided. " +
                "Use the constructor overload that accepts a RouteMatcher to enable rewrite support.");
        }

        var rewritePath = context.RewriteTarget!;
        context.ClearRewrite();

        var newMatch = _routeMatcher.Match(rewritePath);
        if (newMatch is null)
        {
            throw new InvalidOperationException(
                $"Rewrite target '{rewritePath}' does not match any route.");
        }

        context.RoutePattern = newMatch.RouteEntry.Pattern;
        context.Parameters = newMatch.Parameters;
    }
}
