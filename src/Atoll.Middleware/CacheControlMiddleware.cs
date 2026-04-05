using System.Collections.ObjectModel;
using Atoll.Build.Pipeline;
using Atoll.Routing;

namespace Atoll.Middleware;

/// <summary>
/// Options for <see cref="CacheControlMiddleware"/>.
/// </summary>
public sealed class CacheControlMiddlewareOptions
{
    /// <summary>
    /// Gets or sets whether to add a <c>Cache-Control</c> header to responses.
    /// When <c>true</c>, the <see cref="DefaultCacheControl"/> value is added to
    /// responses that do not already have a <c>Cache-Control</c> header.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IncludeCacheControlHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets the default <c>Cache-Control</c> value applied to dynamic endpoint responses.
    /// Defaults to <c>no-cache</c>.
    /// </summary>
    public string DefaultCacheControl { get; set; } = "no-cache";
}

/// <summary>
/// An Atoll middleware handler that adds ETag-based HTTP conditional request support
/// to endpoint responses. When a response has a body and a 200 status code, the
/// middleware computes a weak ETag from the body and:
/// <list type="bullet">
///   <item><description>Returns 304 Not Modified when the request's <c>If-None-Match</c> header matches.</description></item>
///   <item><description>Adds the <c>ETag</c> header to the response otherwise.</description></item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// <b>When to use:</b> Compose this middleware into your Atoll middleware pipeline for endpoint
/// responses that should support conditional requests. For page responses, ETag is handled
/// automatically by <c>AtollRequestHandler</c> — this middleware is for endpoint pipelines only.
/// </para>
/// <para>
/// <b>Double-ETag guard:</b> If the downstream response already contains an <c>ETag</c> header,
/// this middleware passes through unchanged to avoid overwriting a deliberately set ETag.
/// </para>
/// </remarks>
public static class CacheControlMiddleware
{
    /// <summary>
    /// Creates a new <see cref="MiddlewareHandler"/> that adds ETag/304 support
    /// with default options.
    /// </summary>
    public static MiddlewareHandler Create()
    {
        return Create(new CacheControlMiddlewareOptions());
    }

    /// <summary>
    /// Creates a new <see cref="MiddlewareHandler"/> that adds ETag/304 support
    /// with the specified options.
    /// </summary>
    /// <param name="options">Options controlling the middleware behaviour.</param>
    public static MiddlewareHandler Create(CacheControlMiddlewareOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return async (context, next) =>
        {
            var response = await next();

            // Only handle 200 responses with a body
            if (response.StatusCode != 200 || response.Body is null || response.Body.Length == 0)
            {
                return response;
            }

            // Double-ETag guard: skip if ETag already present
            if (response.Headers.ContainsKey("ETag"))
            {
                return response;
            }

            // Compute weak ETag using full SHA-256 (64 hex chars) to prevent collision risk
            var hash = AssetFingerprinter.ComputeHash(response.Body, 64);
            var etag = $"W/\"{hash}\"";

            // Check If-None-Match conditional request (RFC 7232 §3.2)
            context.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch);
            if (ifNoneMatch == "*" || ifNoneMatch == etag)
            {
                // Content not modified — return 304 with ETag but no body
                var notModifiedHeaders = new Dictionary<string, string>(response.Headers)
                {
                    ["ETag"] = etag
                };
                return new AtollResponse(
                    304,
                    new ReadOnlyDictionary<string, string>(notModifiedHeaders));
            }

            // Add ETag (and optional Cache-Control) to the response
            var newHeaders = new Dictionary<string, string>(response.Headers)
            {
                ["ETag"] = etag
            };

            if (options.IncludeCacheControlHeader && !response.Headers.ContainsKey("Cache-Control"))
            {
                newHeaders["Cache-Control"] = options.DefaultCacheControl;
            }

            return new AtollResponse(
                response.StatusCode,
                new ReadOnlyDictionary<string, string>(newHeaders),
                response.Body);
        };
    }
}
