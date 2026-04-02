using Atoll.Routing;

namespace Atoll.Middleware;

/// <summary>
/// Represents an Atoll middleware handler that can inspect, modify, short-circuit,
/// or pass through requests in the Atoll pipeline.
/// </summary>
/// <remarks>
/// <para>
/// This is Atoll's own middleware — distinct from ASP.NET Core middleware.
/// It runs within the Atoll request pipeline after ASP.NET Core has routed
/// the request to the Atoll middleware.
/// </para>
/// <para>
/// A handler can:
/// <list type="bullet">
/// <item><description><b>Pass through</b>: call <paramref name="next"/> and return its response.</description></item>
/// <item><description><b>Modify</b>: call <paramref name="next"/>, then modify the response before returning.</description></item>
/// <item><description><b>Short-circuit</b>: return an <see cref="AtollResponse"/> without calling <paramref name="next"/>.</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="context">The middleware context for the current request.</param>
/// <param name="next">
/// A delegate that invokes the next handler in the pipeline.
/// </param>
/// <returns>A task that resolves to the <see cref="AtollResponse"/>.</returns>
public delegate Task<AtollResponse> MiddlewareHandler(
    MiddlewareContext context,
    MiddlewareNext next);
