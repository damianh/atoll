using Atoll.Redirects;

namespace Atoll.Middleware.Server.Hosting;

/// <summary>
/// Extension methods for adding the Atoll redirect middleware to the ASP.NET Core pipeline.
/// </summary>
public static class RedirectApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Atoll redirect middleware to the ASP.NET Core pipeline using a 301 permanent
    /// redirect status code.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="redirectMap">The redirect map containing source-to-target path mappings.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <remarks>
    /// Call this method <b>before</b> <see cref="AtollApplicationBuilderExtensions.UseAtoll"/>
    /// so that redirect source paths are intercepted before Atoll routing begins.
    /// </remarks>
    public static IApplicationBuilder UseRedirects(
        this IApplicationBuilder app,
        RedirectMap redirectMap)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(redirectMap);
        return app.UseMiddleware<RedirectMiddlewareAspNet>(redirectMap);
    }

    /// <summary>
    /// Adds the Atoll redirect middleware to the ASP.NET Core pipeline using the specified
    /// HTTP status code.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="redirectMap">The redirect map containing source-to-target path mappings.</param>
    /// <param name="statusCode">The HTTP redirect status code (e.g. 301, 302, 307, 308).</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseRedirects(
        this IApplicationBuilder app,
        RedirectMap redirectMap,
        int statusCode)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(redirectMap);
        return app.UseMiddleware<RedirectMiddlewareAspNet>(redirectMap, statusCode);
    }
}
