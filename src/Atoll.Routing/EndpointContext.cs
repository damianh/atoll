using System.Collections.ObjectModel;

namespace Atoll.Routing;

/// <summary>
/// Provides the request context for an Atoll API endpoint handler method.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EndpointContext"/> is a lightweight context object passed to
/// <see cref="IAtollEndpoint"/> handler methods. It provides access to route
/// parameters, request metadata, and locals set by middleware — without
/// exposing the full rendering pipeline available to page components.
/// </para>
/// <para>
/// For SSR scenarios, <see cref="EndpointContext"/> wraps request data from
/// ASP.NET Core. For SSG scenarios (pre-rendered endpoints returning JSON files),
/// the context is populated from <see cref="StaticPath"/> data.
/// </para>
/// </remarks>
public sealed class EndpointContext
{
    /// <summary>
    /// Initializes a new <see cref="EndpointContext"/> with the specified parameters,
    /// request information, and locals.
    /// </summary>
    /// <param name="parameters">
    /// The route parameter values extracted from the matched URL
    /// (e.g., <c>{ "slug": "hello-world" }</c> for <c>/api/posts/[slug]</c>).
    /// </param>
    /// <param name="request">The incoming request information.</param>
    /// <param name="locals">
    /// Data set by middleware for consumption by the endpoint.
    /// </param>
    public EndpointContext(
        IReadOnlyDictionary<string, string> parameters,
        EndpointRequest request,
        IDictionary<string, object?> locals)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(locals);
        Parameters = parameters;
        Request = request;
        Locals = locals;
    }

    /// <summary>
    /// Initializes a new <see cref="EndpointContext"/> with the specified parameters
    /// and request, using empty locals.
    /// </summary>
    /// <param name="parameters">The route parameter values.</param>
    /// <param name="request">The incoming request information.</param>
    public EndpointContext(
        IReadOnlyDictionary<string, string> parameters,
        EndpointRequest request)
        : this(parameters, request, new Dictionary<string, object?>())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="EndpointContext"/> with the specified request
    /// and no route parameters or locals.
    /// </summary>
    /// <param name="request">The incoming request information.</param>
    public EndpointContext(EndpointRequest request)
        : this(EmptyParameters, request, new Dictionary<string, object?>())
    {
    }

    /// <summary>
    /// Gets the route parameter values extracted from the matched URL.
    /// </summary>
    /// <example>
    /// For a route <c>/api/posts/[slug]</c> matching <c>/api/posts/hello-world</c>,
    /// this contains <c>{ "slug": "hello-world" }</c>.
    /// </example>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>
    /// Gets the incoming request information.
    /// </summary>
    public EndpointRequest Request { get; }

    /// <summary>
    /// Gets the mutable locals dictionary. Middleware can set values here for
    /// consumption by endpoint handlers.
    /// </summary>
    public IDictionary<string, object?> Locals { get; }

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
    /// <param name="defaultValue">The default value to return if the key is not found.</param>
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

    private static readonly IReadOnlyDictionary<string, string> EmptyParameters =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
}
