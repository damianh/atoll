using System.Collections.ObjectModel;
using Atoll.Routing;

namespace Atoll.Middleware;

/// <summary>
/// Provides request context to Atoll middleware handlers.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="MiddlewareContext"/> is the Atoll-level context passed to
/// <see cref="MiddlewareHandler"/> delegates. It provides access to the
/// matched route, request information, extracted parameters, and a mutable
/// <see cref="Locals"/> dictionary for sharing data between middleware and
/// downstream handlers.
/// </para>
/// <para>
/// Middleware can mutate <see cref="Locals"/> to pass data to subsequent
/// middleware and to page/endpoint handlers.
/// </para>
/// </remarks>
public sealed class MiddlewareContext
{
    /// <summary>
    /// Initializes a new <see cref="MiddlewareContext"/> with the specified route pattern,
    /// parameters, request, and locals.
    /// </summary>
    /// <param name="routePattern">The matched route pattern (e.g., <c>/blog/[slug]</c>).</param>
    /// <param name="parameters">The extracted route parameter values.</param>
    /// <param name="request">The incoming request information.</param>
    /// <param name="locals">The mutable locals dictionary for sharing data between middleware.</param>
    public MiddlewareContext(
        string routePattern,
        IReadOnlyDictionary<string, string> parameters,
        EndpointRequest request,
        IDictionary<string, object?> locals)
    {
        ArgumentNullException.ThrowIfNull(routePattern);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(locals);
        RoutePattern = routePattern;
        Parameters = parameters;
        Request = request;
        Locals = locals;
    }

    /// <summary>
    /// Initializes a new <see cref="MiddlewareContext"/> with the specified route pattern,
    /// parameters, and request, using empty locals.
    /// </summary>
    /// <param name="routePattern">The matched route pattern.</param>
    /// <param name="parameters">The extracted route parameter values.</param>
    /// <param name="request">The incoming request information.</param>
    public MiddlewareContext(
        string routePattern,
        IReadOnlyDictionary<string, string> parameters,
        EndpointRequest request)
        : this(routePattern, parameters, request, new Dictionary<string, object?>())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="MiddlewareContext"/> with the specified request
    /// and no route or parameters (for use before route matching).
    /// </summary>
    /// <param name="request">The incoming request information.</param>
    public MiddlewareContext(EndpointRequest request)
        : this(string.Empty, EmptyParameters, request, new Dictionary<string, object?>())
    {
    }

    /// <summary>
    /// Gets the matched route pattern (e.g., <c>/blog/[slug]</c>).
    /// </summary>
    /// <remarks>
    /// This value may be updated by a rewrite operation.
    /// </remarks>
    public string RoutePattern { get; internal set; }

    /// <summary>
    /// Gets the extracted route parameter values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For a route <c>/blog/[slug]</c> matching <c>/blog/hello-world</c>,
    /// this would contain <c>{ "slug": "hello-world" }</c>.
    /// </para>
    /// <para>
    /// This value may be updated by a rewrite operation.
    /// </para>
    /// </remarks>
    public IReadOnlyDictionary<string, string> Parameters { get; internal set; }

    /// <summary>
    /// Gets the incoming request information.
    /// </summary>
    public EndpointRequest Request { get; }

    /// <summary>
    /// Gets the mutable locals dictionary for sharing data between middleware
    /// and downstream handlers.
    /// </summary>
    /// <remarks>
    /// Middleware can set values here for consumption by subsequent middleware,
    /// page components, or endpoint handlers.
    /// </remarks>
    public IDictionary<string, object?> Locals { get; }

    /// <summary>
    /// Gets the request URL.
    /// </summary>
    public Uri Url => Request.Url;

    /// <summary>
    /// Gets or sets the rewrite target path, or <c>null</c> if no rewrite was requested.
    /// </summary>
    /// <remarks>
    /// When set, the pipeline will re-resolve the route against the new path
    /// before continuing to the next middleware or the final route handler.
    /// </remarks>
    public string? RewriteTarget { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this context has a pending rewrite.
    /// </summary>
    public bool HasRewrite => RewriteTarget is not null;

    /// <summary>
    /// Requests that the pipeline re-resolve the route for the specified path.
    /// </summary>
    /// <param name="newPath">The new URL path to route to (e.g., <c>/new-page</c>).</param>
    public void Rewrite(string newPath)
    {
        ArgumentNullException.ThrowIfNull(newPath);
        RewriteTarget = newPath;
    }

    /// <summary>
    /// Clears the pending rewrite target.
    /// </summary>
    internal void ClearRewrite()
    {
        RewriteTarget = null;
    }

    /// <summary>
    /// Gets a typed local value by key.
    /// </summary>
    /// <typeparam name="T">The expected type of the local value.</typeparam>
    /// <param name="key">The local key.</param>
    /// <returns>The typed local value.</returns>
    /// <exception cref="KeyNotFoundException">The local key does not exist.</exception>
    /// <exception cref="InvalidCastException">The local value cannot be cast to <typeparamref name="T"/>.</exception>
    public T GetLocal<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (!Locals.TryGetValue(key, out var value))
        {
            throw new KeyNotFoundException($"Local '{key}' not found.");
        }

        return (T)value!;
    }

    /// <summary>
    /// Gets a typed local value by key, or the specified default if the key does not exist.
    /// </summary>
    /// <typeparam name="T">The expected type of the local value.</typeparam>
    /// <param name="key">The local key.</param>
    /// <param name="defaultValue">The default value if the key is not found.</param>
    /// <returns>The typed local value, or <paramref name="defaultValue"/> if not found.</returns>
    public T GetLocal<T>(string key, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (Locals.TryGetValue(key, out var value) && value is T typed)
        {
            return typed;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets a route parameter value by name.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <returns>The parameter value.</returns>
    /// <exception cref="KeyNotFoundException">The parameter name does not exist.</exception>
    public string GetParameter(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!Parameters.TryGetValue(name, out var value))
        {
            throw new KeyNotFoundException($"Route parameter '{name}' not found.");
        }

        return value;
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyParameters =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
}
