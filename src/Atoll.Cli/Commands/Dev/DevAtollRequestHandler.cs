using System.Collections.ObjectModel;
using Atoll.Components;
using Atoll.Css;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Routing.Matching;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Atoll.Cli.Commands.Dev;

/// <summary>
/// Dev-server request handler that reads from a <see langword="volatile"/>
/// <see cref="DevServerState"/> snapshot on each request, enabling atomic hot-reload
/// without restarting the Kestrel listener.
/// </summary>
internal sealed class DevAtollRequestHandler
{
    private volatile DevServerState _state;
    private readonly ILogger<DevAtollRequestHandler> _logger;

    /// <summary>
    /// Initializes a new <see cref="DevAtollRequestHandler"/> with the given initial state.
    /// </summary>
    /// <param name="initialState">The initial server state to serve requests from.</param>
    /// <param name="logger">The logger instance.</param>
    public DevAtollRequestHandler(
        DevServerState initialState,
        ILogger<DevAtollRequestHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(initialState);
        ArgumentNullException.ThrowIfNull(logger);
        _state = initialState;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current server state snapshot.
    /// </summary>
    public DevServerState CurrentState => _state;

    /// <summary>
    /// Atomically swaps the server state. Schedules a delayed unload of the old
    /// <see cref="System.Runtime.Loader.AssemblyLoadContext"/> if it changed (i.e.,
    /// a code-change reload), giving in-flight requests time to drain.
    /// </summary>
    /// <param name="newState">The new state snapshot to begin serving.</param>
    public void UpdateState(DevServerState newState)
    {
        ArgumentNullException.ThrowIfNull(newState);

        var oldState = _state;
        _state = newState;

        // Only schedule unload when the ALC actually changed (code-change reload).
        // Content-only reloads reuse the same ALC — unloading it would destroy the
        // new state's types.
        if (oldState.LoadContext is not null &&
            !ReferenceEquals(oldState.LoadContext, newState.LoadContext))
        {
            var alcToUnload = oldState.LoadContext;
            // Best-effort drain: wait 5 s for in-flight renders to complete before
            // requesting unload. Not a production-grade reference count.
            _ = Task.Delay(TimeSpan.FromSeconds(5))
                    .ContinueWith(_ =>
                    {
                        try { alcToUnload.Unload(); }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "ALC unload failed — this is expected if references remain.");
                        }
                    }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Attempts to handle the incoming HTTP request using the current state snapshot.
    /// Returns <c>true</c> if a matching route was found; <c>false</c> otherwise.
    /// </summary>
    /// <param name="context">The ASP.NET Core HTTP context.</param>
    public async Task<bool> TryHandleAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Capture state once — all request processing uses this consistent snapshot.
        var state = _state;

        var requestPath = ExtractRoutePath(context.Request.Path, state.Options.BasePath);
        if (requestPath is null)
        {
            return false;
        }

        var match = state.RouteMatcher.Match(requestPath);
        if (match is null)
        {
            _logger.LogDebug("No route matched for path '{Path}'", requestPath);
            return false;
        }

        _logger.LogDebug(
            "Route matched: '{Path}' -> {ComponentType} (pattern: {Pattern})",
            requestPath,
            match.RouteEntry.ComponentType.Name,
            match.RouteEntry.Pattern);

        if (typeof(IAtollEndpoint).IsAssignableFrom(match.RouteEntry.ComponentType))
        {
            await HandleEndpointAsync(context, match, state);
        }
        else if (typeof(IAtollComponent).IsAssignableFrom(match.RouteEntry.ComponentType))
        {
            await HandlePageAsync(context, match, state);
        }
        else
        {
            _logger.LogWarning(
                "Matched type '{Type}' is neither IAtollComponent nor IAtollEndpoint",
                match.RouteEntry.ComponentType.FullName);
            return false;
        }

        return true;
    }

    // ── Private helpers ─────────────────────────────────────────────────────────

    private static string? ExtractRoutePath(PathString requestPath, string basePath)
    {
        var trimmedBase = basePath.TrimEnd('/');
        var path = requestPath.Value ?? "/";

        if (trimmedBase.Length > 1)
        {
            if (!path.StartsWith(trimmedBase, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            path = path[trimmedBase.Length..];
            if (path.Length == 0)
            {
                path = "/";
            }
        }

        return path;
    }

    private async Task HandleEndpointAsync(
        HttpContext httpContext,
        RouteMatchResult match,
        DevServerState state)
    {
        var endpoint = (IAtollEndpoint)Activator.CreateInstance(match.RouteEntry.ComponentType)!;
        var endpointRequest = BuildEndpointRequest(httpContext);
        var endpointContext = new EndpointContext(match.Parameters, endpointRequest);
        var response = await EndpointDispatcher.DispatchAsync(endpoint, endpointContext);
        await WriteAtollResponseAsync(httpContext.Response, response);
    }

    private async Task HandlePageAsync(
        HttpContext httpContext,
        RouteMatchResult match,
        DevServerState state)
    {
        var componentType = match.RouteEntry.ComponentType;
        var props = BuildPageProps(match.Parameters, state.Options);

        ComponentDelegate renderDelegate = async context =>
        {
            var component = (IAtollComponent)Activator.CreateInstance(componentType)!;

            var pageFragment = RenderFragment.FromAsync(async destination =>
            {
                await ComponentRenderer.RenderComponentAsync(component, destination, props);
            });

            // Pass props to layouts so they receive Query, Slug, etc.
            var wrappedFragment = LayoutResolver.WrapWithLayouts(componentType, pageFragment, props);

            await context.RenderAsync(wrappedFragment);
        };

        // PageRenderer must be instantiated fresh per request — it has instance fields.
        var renderer = new PageRenderer();

        // Inject global CSS (from [GlobalStyle] components) into <head>.
        // In SSG builds the asset pipeline writes CSS to a file and injects a <link> tag;
        // in dev mode we inline it as a <style> element for simplicity.
        if (state.GlobalCss.Length > 0)
        {
            renderer.HeadManager.Add(CssInjector.CreateInlineStyle(state.GlobalCss));
        }

        var result = await renderer.RenderPageAsync(renderDelegate);

        httpContext.Response.StatusCode = 200;
        httpContext.Response.ContentType = "text/html; charset=utf-8";
        await result.WriteToStreamAsync(httpContext.Response.Body);
    }

    private static IReadOnlyDictionary<string, object?> BuildPageProps(
        IReadOnlyDictionary<string, string> parameters,
        Atoll.Middleware.Server.Hosting.AtollOptions options)
    {
        var props = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in options.ServiceProps)
        {
            props[kvp.Key] = kvp.Value;
        }

        // Route parameters override service props
        foreach (var kvp in parameters)
        {
            props[kvp.Key] = kvp.Value;
        }

        return new ReadOnlyDictionary<string, object?>(props);
    }

    private static EndpointRequest BuildEndpointRequest(HttpContext httpContext)
    {
        var request = httpContext.Request;
        var url = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}");

        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        return new EndpointRequest(
            request.Method,
            url,
            new ReadOnlyDictionary<string, string>(headers),
            request.Body);
    }

    private static async Task WriteAtollResponseAsync(HttpResponse httpResponse, AtollResponse atollResponse)
    {
        httpResponse.StatusCode = atollResponse.StatusCode;

        foreach (var header in atollResponse.Headers)
        {
            httpResponse.Headers[header.Key] = header.Value;
        }

        if (atollResponse.Body is not null)
        {
            await httpResponse.Body.WriteAsync(atollResponse.Body);
        }
    }
}
