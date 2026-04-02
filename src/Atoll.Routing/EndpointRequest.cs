using System.Collections.ObjectModel;

namespace Atoll.Routing;

/// <summary>
/// Represents the incoming HTTP request information for an API endpoint handler.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EndpointRequest"/> abstracts away the underlying HTTP infrastructure
/// (ASP.NET Core <c>HttpRequest</c>) so that endpoint handlers remain decoupled
/// from the hosting layer. This enables endpoints to be tested and invoked
/// during SSG without a live HTTP connection.
/// </para>
/// </remarks>
public sealed class EndpointRequest
{
    /// <summary>
    /// Initializes a new <see cref="EndpointRequest"/> with the specified method, URL,
    /// headers, and body.
    /// </summary>
    /// <param name="method">The HTTP method (e.g., <c>GET</c>, <c>POST</c>).</param>
    /// <param name="url">The request URL.</param>
    /// <param name="headers">The request headers.</param>
    /// <param name="body">The request body stream, or <c>null</c> for bodyless requests.</param>
    public EndpointRequest(
        string method,
        Uri url,
        IReadOnlyDictionary<string, string> headers,
        Stream? body)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(headers);
        Method = method.ToUpperInvariant();
        Url = url;
        Headers = headers;
        Body = body;
    }

    /// <summary>
    /// Initializes a new <see cref="EndpointRequest"/> with the specified method and URL,
    /// empty headers, and no body.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The request URL.</param>
    public EndpointRequest(string method, Uri url)
        : this(method, url, EmptyHeaders, null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="EndpointRequest"/> with the specified method, URL,
    /// and headers, and no body.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="url">The request URL.</param>
    /// <param name="headers">The request headers.</param>
    public EndpointRequest(
        string method,
        Uri url,
        IReadOnlyDictionary<string, string> headers)
        : this(method, url, headers, null)
    {
    }

    /// <summary>
    /// Gets the HTTP method (always uppercase, e.g., <c>GET</c>, <c>POST</c>).
    /// </summary>
    public string Method { get; }

    /// <summary>
    /// Gets the request URL.
    /// </summary>
    public Uri Url { get; }

    /// <summary>
    /// Gets the request headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the request body stream, or <c>null</c> for bodyless requests (e.g., GET, HEAD).
    /// </summary>
    public Stream? Body { get; }

    private static readonly IReadOnlyDictionary<string, string> EmptyHeaders =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
}
