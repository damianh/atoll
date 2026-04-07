using Atoll.Routing.Matching;

namespace Atoll.Middleware.Pipeline;

/// <summary>
/// Provides middleware that applies rewrites within the pipeline by re-resolving
/// routes when <see cref="MiddlewareContext.Rewrite(string)"/> has been called.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RewriteHandler"/> integrates with the <see cref="RouteMatcher"/> to
/// re-resolve routes when middleware triggers a rewrite. It processes the rewrite
/// before continuing the pipeline, updating the context's route pattern and parameters.
/// </para>
/// <para>
/// Place this handler in the middleware pipeline wherever rewrite processing should occur.
/// Typically, this is handled automatically by <see cref="MiddlewareRunner"/>, but
/// <see cref="RewriteHandler"/> is available for custom pipeline compositions.
/// </para>
/// </remarks>
public sealed class RewriteHandler
{
    private readonly RouteMatcher _routeMatcher;

    /// <summary>
    /// Initializes a new <see cref="RewriteHandler"/> with the specified route matcher.
    /// </summary>
    /// <param name="routeMatcher">The route matcher used for re-resolving rewrite targets.</param>
    public RewriteHandler(RouteMatcher routeMatcher)
    {
        ArgumentNullException.ThrowIfNull(routeMatcher);
        _routeMatcher = routeMatcher;
    }

    /// <summary>
    /// Creates a <see cref="MiddlewareHandler"/> that applies pending rewrites
    /// before calling the next handler in the pipeline.
    /// </summary>
    /// <returns>A middleware handler that processes rewrites.</returns>
    public MiddlewareHandler CreateHandler() =>
        async (context, next) =>
        {
            if (context.HasRewrite)
            {
                ApplyRewrite(context);
            }

            var response = await next();

            return response;
        };

    /// <summary>
    /// Attempts to resolve the specified path against the route table.
    /// </summary>
    /// <param name="path">The URL path to resolve.</param>
    /// <returns>
    /// A <see cref="RouteMatchResult"/> if the path matches a route; otherwise, <c>null</c>.
    /// </returns>
    public RouteMatchResult? TryResolve(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return _routeMatcher.Match(path);
    }

    /// <summary>
    /// Resolves the specified path against the route table.
    /// </summary>
    /// <param name="path">The URL path to resolve.</param>
    /// <returns>The <see cref="RouteMatchResult"/> for the matched route.</returns>
    /// <exception cref="InvalidOperationException">The path does not match any route.</exception>
    public RouteMatchResult Resolve(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var match = _routeMatcher.Match(path);
        if (match is null)
        {
            throw new InvalidOperationException(
                $"Rewrite target '{path}' does not match any route.");
        }

        return match;
    }

    private void ApplyRewrite(MiddlewareContext context)
    {
        var rewritePath = context.RewriteTarget!;
        context.ClearRewrite();

        var newMatch = Resolve(rewritePath);
        context.RoutePattern = newMatch.RouteEntry.Pattern;
        context.Parameters = newMatch.Parameters;
    }
}
