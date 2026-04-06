using Atoll.Redirects;
using Atoll.Routing;

namespace Atoll.Middleware;

/// <summary>
/// An Atoll middleware handler that short-circuits with an HTTP 301 (or custom status)
/// redirect response when the request path matches a configured redirect source.
/// </summary>
/// <remarks>
/// <para>
/// This middleware operates in the Atoll pipeline (distinct from ASP.NET Core middleware).
/// It must be composed into the middleware sequence via <c>MiddlewareSequencer.Sequence()</c>
/// and runs after <c>AtollMiddleware</c> has accepted the request.
/// </para>
/// <para>
/// Path matching is performed after normalizing the request path to lowercase and stripping
/// any trailing slash, consistent with <see cref="RedirectMap"/> normalization.
/// The original query string (if any) is preserved and appended to the redirect target.
/// </para>
/// <para>
/// For redirect source paths that do not correspond to any registered Atoll route
/// (which is the common case), use the ASP.NET Core-level
/// <c>RedirectMiddlewareAspNet</c> / <c>UseRedirects()</c> instead, so that the redirect
/// is issued before Atoll routing even begins.
/// </para>
/// </remarks>
public static class RedirectMiddleware
{
    /// <summary>
    /// Creates a <see cref="MiddlewareHandler"/> that issues HTTP 301 permanent redirects
    /// for paths present in <paramref name="redirectMap"/>.
    /// </summary>
    /// <param name="redirectMap">The redirect map to look up source paths against.</param>
    /// <returns>A <see cref="MiddlewareHandler"/> that short-circuits with a 301 on a match.</returns>
    public static MiddlewareHandler Create(RedirectMap redirectMap)
    {
        return Create(redirectMap, 301);
    }

    /// <summary>
    /// Creates a <see cref="MiddlewareHandler"/> that issues redirects with the specified
    /// HTTP status code for paths present in <paramref name="redirectMap"/>.
    /// </summary>
    /// <param name="redirectMap">The redirect map to look up source paths against.</param>
    /// <param name="statusCode">
    /// The HTTP redirect status code to use (e.g. 301, 302, 307, 308).
    /// </param>
    /// <returns>A <see cref="MiddlewareHandler"/> that short-circuits with the given status on a match.</returns>
    public static MiddlewareHandler Create(RedirectMap redirectMap, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(redirectMap);

        return async (context, next) =>
        {
            var path = context.Url.AbsolutePath;
            if (redirectMap.TryGetRedirect(path, out var target))
            {
                var query = context.Url.Query;
                var location = string.IsNullOrEmpty(query) ? target : target + query;
                return AtollResponse.Redirect(location, statusCode);
            }

            return await next();
        };
    }
}
