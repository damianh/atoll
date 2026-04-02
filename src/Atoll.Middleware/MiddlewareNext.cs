namespace Atoll.Middleware;

/// <summary>
/// Represents the function that invokes the next handler in the Atoll middleware pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Calling <see cref="MiddlewareNext"/> advances the pipeline to the next middleware,
/// or to the final route handler if there are no more middleware in the chain.
/// </para>
/// <para>
/// Middleware can pass a rewrite path to change the resolved route for subsequent
/// handlers in the pipeline. See <see cref="MiddlewareContext.Rewrite(string)"/>
/// for the higher-level rewrite API.
/// </para>
/// </remarks>
/// <returns>A task that resolves to the <see cref="Atoll.Routing.AtollResponse"/> from downstream handlers.</returns>
public delegate Task<Routing.AtollResponse> MiddlewareNext();
