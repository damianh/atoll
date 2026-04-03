namespace Atoll.Middleware.Server.Hosting;

/// <summary>
/// Extension methods for adding Atoll middleware to the ASP.NET Core request pipeline.
/// </summary>
public static class AtollApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Atoll middleware to the ASP.NET Core request pipeline.
    /// The middleware matches incoming requests against the Atoll route table and
    /// dispatches to page components or API endpoints.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <remarks>
    /// <para>
    /// Call <see cref="AtollServiceCollectionExtensions.AddAtoll"/> in
    /// <c>ConfigureServices</c> before calling this method.
    /// </para>
    /// <para>
    /// The middleware should be placed after authentication/authorization middleware
    /// but before any catch-all handlers (e.g., <c>MapFallback</c>).
    /// </para>
    /// </remarks>
    public static IApplicationBuilder UseAtoll(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<AtollMiddleware>();
    }
}
