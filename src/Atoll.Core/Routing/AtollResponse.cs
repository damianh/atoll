using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace Atoll.Routing;

/// <summary>
/// Represents an HTTP response from an Atoll API endpoint.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AtollResponse"/> wraps HTTP response semantics (status code, headers, body)
/// without coupling to ASP.NET Core's <c>HttpResponse</c>. This allows endpoints to be
/// tested, composed, and used in both SSR and SSG contexts.
/// </para>
/// <para>
/// Use the static factory methods (<see cref="Json{T}(T,int)"/>,
/// <see cref="Text(string,int)"/>, <see cref="Redirect(string,int)"/>, etc.)
/// for common response patterns.
/// </para>
/// </remarks>
public sealed class AtollResponse
{
    /// <summary>
    /// Initializes a new <see cref="AtollResponse"/> with the specified status code,
    /// headers, and body.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="headers">The response headers.</param>
    /// <param name="body">The response body bytes, or <c>null</c> for bodyless responses.</param>
    public AtollResponse(
        int statusCode,
        IReadOnlyDictionary<string, string> headers,
        byte[]? body)
    {
        ArgumentNullException.ThrowIfNull(headers);
        StatusCode = statusCode;
        Headers = headers;
        Body = body;
    }

    /// <summary>
    /// Initializes a new <see cref="AtollResponse"/> with the specified status code
    /// and headers, and no body.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="headers">The response headers.</param>
    public AtollResponse(
        int statusCode,
        IReadOnlyDictionary<string, string> headers)
        : this(statusCode, headers, null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="AtollResponse"/> with the specified status code,
    /// no headers, and no body.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    public AtollResponse(int statusCode)
        : this(statusCode, EmptyHeaders, null)
    {
    }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the response body bytes, or <c>null</c> for bodyless responses.
    /// </summary>
    public byte[]? Body { get; }

    /// <summary>
    /// Gets the body as a UTF-8 string, or <c>null</c> if there is no body.
    /// </summary>
    /// <returns>The body string, or <c>null</c>.</returns>
    public string? GetBodyAsString()
    {
        return Body is null ? null : Encoding.UTF8.GetString(Body);
    }

    /// <summary>
    /// Creates a JSON response with the specified value serialized using
    /// <see cref="JsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize as JSON.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to 200.</param>
    /// <returns>A new <see cref="AtollResponse"/> with JSON content.</returns>
    public static AtollResponse Json<T>(T value, int statusCode)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "application/json; charset=utf-8"
        };
        return new AtollResponse(statusCode, new ReadOnlyDictionary<string, string>(headers), json);
    }

    /// <summary>
    /// Creates a JSON response with the specified value and a 200 status code.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize as JSON.</param>
    /// <returns>A new <see cref="AtollResponse"/> with JSON content.</returns>
    public static AtollResponse Json<T>(T value)
    {
        return Json(value, 200);
    }

    /// <summary>
    /// Creates a plain text response.
    /// </summary>
    /// <param name="content">The text content.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A new <see cref="AtollResponse"/> with text content.</returns>
    public static AtollResponse Text(string content, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(content);
        var body = Encoding.UTF8.GetBytes(content);
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "text/plain; charset=utf-8"
        };
        return new AtollResponse(statusCode, new ReadOnlyDictionary<string, string>(headers), body);
    }

    /// <summary>
    /// Creates a plain text response with a 200 status code.
    /// </summary>
    /// <param name="content">The text content.</param>
    /// <returns>A new <see cref="AtollResponse"/> with text content.</returns>
    public static AtollResponse Text(string content)
    {
        return Text(content, 200);
    }

    /// <summary>
    /// Creates an HTML response.
    /// </summary>
    /// <param name="html">The HTML content.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A new <see cref="AtollResponse"/> with HTML content.</returns>
    public static AtollResponse Html(string html, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(html);
        var body = Encoding.UTF8.GetBytes(html);
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "text/html; charset=utf-8"
        };
        return new AtollResponse(statusCode, new ReadOnlyDictionary<string, string>(headers), body);
    }

    /// <summary>
    /// Creates an HTML response with a 200 status code.
    /// </summary>
    /// <param name="html">The HTML content.</param>
    /// <returns>A new <see cref="AtollResponse"/> with HTML content.</returns>
    public static AtollResponse Html(string html)
    {
        return Html(html, 200);
    }

    /// <summary>
    /// Creates a redirect response.
    /// </summary>
    /// <param name="location">The redirect target URL.</param>
    /// <param name="statusCode">The HTTP redirect status code (302 by default).</param>
    /// <returns>A new <see cref="AtollResponse"/> with a <c>Location</c> header.</returns>
    public static AtollResponse Redirect(string location, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(location);
        var headers = new Dictionary<string, string>
        {
            ["Location"] = location
        };
        return new AtollResponse(statusCode, new ReadOnlyDictionary<string, string>(headers));
    }

    /// <summary>
    /// Creates a redirect response with a 302 status code.
    /// </summary>
    /// <param name="location">The redirect target URL.</param>
    /// <returns>A new <see cref="AtollResponse"/> with a <c>Location</c> header.</returns>
    public static AtollResponse Redirect(string location)
    {
        return Redirect(location, 302);
    }

    /// <summary>
    /// Creates a response with no body.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A new bodyless <see cref="AtollResponse"/>.</returns>
    public static AtollResponse Empty(int statusCode)
    {
        return new AtollResponse(statusCode);
    }

    /// <summary>
    /// Creates a <c>405 Method Not Allowed</c> response with an <c>Allow</c> header
    /// listing the permitted HTTP methods.
    /// </summary>
    /// <param name="allowedMethods">The HTTP methods that the endpoint supports.</param>
    /// <returns>A new 405 <see cref="AtollResponse"/>.</returns>
    public static AtollResponse MethodNotAllowed(IReadOnlyList<string> allowedMethods)
    {
        ArgumentNullException.ThrowIfNull(allowedMethods);
        var headers = new Dictionary<string, string>
        {
            ["Allow"] = string.Join(", ", allowedMethods)
        };
        return new AtollResponse(405, new ReadOnlyDictionary<string, string>(headers));
    }

    /// <summary>
    /// Creates a <c>404 Not Found</c> response with no body.
    /// </summary>
    /// <returns>A new 404 <see cref="AtollResponse"/>.</returns>
    public static AtollResponse NotFound()
    {
        return new AtollResponse(404);
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyHeaders =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };
}
