using Atoll.Redirects;
using Microsoft.AspNetCore.Http;

namespace Atoll.Middleware.Server.Hosting;

/// <summary>
/// ASP.NET Core middleware that intercepts requests before Atoll routing and issues
/// HTTP redirects for paths present in a <see cref="RedirectMap"/>.
/// </summary>
/// <remarks>
/// <para>
/// Register this middleware before <see cref="AtollApplicationBuilderExtensions.UseAtoll"/>
/// via the <see cref="RedirectApplicationBuilderExtensions.UseRedirects(IApplicationBuilder, RedirectMap)"/>
/// extension method. This ensures that redirect source paths — which do NOT have
/// registered Atoll routes — are intercepted before Atoll routing returns a 404.
/// </para>
/// <para>
/// Path matching normalizes the request path to lowercase and strips any trailing slash,
/// consistent with <see cref="RedirectMap"/> normalization.
/// The original query string (if any) is preserved and appended to the redirect target.
/// </para>
/// </remarks>
public sealed class RedirectMiddlewareAspNet
{
    private readonly RequestDelegate _next;
    private readonly RedirectMap _redirectMap;
    private readonly int _statusCode;

    /// <summary>
    /// Initializes a new <see cref="RedirectMiddlewareAspNet"/> with the specified redirect map
    /// and a default 301 permanent redirect status code.
    /// </summary>
    /// <param name="next">The next middleware in the ASP.NET Core pipeline.</param>
    /// <param name="redirectMap">The redirect map to look up source paths against.</param>
    public RedirectMiddlewareAspNet(RequestDelegate next, RedirectMap redirectMap)
        : this(next, redirectMap, 301)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="RedirectMiddlewareAspNet"/> with the specified redirect map
    /// and status code.
    /// </summary>
    /// <param name="next">The next middleware in the ASP.NET Core pipeline.</param>
    /// <param name="redirectMap">The redirect map to look up source paths against.</param>
    /// <param name="statusCode">The HTTP redirect status code (e.g. 301, 302, 307, 308).</param>
    public RedirectMiddlewareAspNet(RequestDelegate next, RedirectMap redirectMap, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(redirectMap);
        _next = next;
        _redirectMap = redirectMap;
        _statusCode = statusCode;
    }

    /// <summary>
    /// Processes an HTTP request. If the request path matches a redirect source,
    /// issues an HTTP redirect response. Otherwise, passes the request to the next middleware.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var path = context.Request.Path.Value ?? "/";
        if (_redirectMap.TryGetRedirect(path, out var target))
        {
            var query = context.Request.QueryString.Value;
            var location = string.IsNullOrEmpty(query) ? target : target + query;
            context.Response.StatusCode = _statusCode;
            context.Response.Headers.Location = location;
            return;
        }

        await _next(context);
    }
}
