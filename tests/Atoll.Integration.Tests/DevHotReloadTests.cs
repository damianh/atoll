using System.Collections.ObjectModel;
using System.Net;
using System.Net.WebSockets;
using Atoll.Cli.Commands.Dev;
using Atoll.Middleware.Server.DevServer;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing.FileSystem;
using Atoll.Routing.Matching;
using Atoll.Samples.Blog.Pages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Atoll.Integration.Tests;

/// <summary>
/// Integration tests for the <c>atoll dev</c> hot-reload infrastructure:
/// <see cref="DevAtollRequestHandler"/>, <see cref="DevFileWatcher"/>,
/// and <see cref="DevServerState"/>.
/// </summary>
public sealed class DevHotReloadTests : IDisposable
{
    private readonly List<string> _tempDirs = new();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning));

    private static readonly IReadOnlyDictionary<string, byte[]> EmptyAssets =
        new ReadOnlyDictionary<string, byte[]>(new Dictionary<string, byte[]>());

    /// <summary>
    /// Creates a <see cref="DevServerState"/> with the given routes using
    /// blog sample page types (available at compile time without building a separate project).
    /// </summary>
    private static DevServerState CreateStateWithRoutes(
        params (string RelativeFile, Type PageType)[] routes)
    {
        var routeEntries = RouteDiscovery.DiscoverRoutesFromEntries(routes);
        var matcher = new RouteMatcher(routeEntries);
        var options = new AtollOptions();
        return new DevServerState(matcher, options, null, null, "", EmptyAssets, null, null);
    }

    /// <summary>
    /// Builds a <see cref="HttpClient"/> backed by a test host that wires
    /// <see cref="DevAtollRequestHandler"/> into the ASP.NET Core pipeline.
    /// </summary>
    private static (HttpClient Client, DevAtollRequestHandler Handler, IHost Host) CreateDevTestHost(
        DevServerState initialState,
        ILoggerFactory loggerFactory)
    {
        var handlerLogger = loggerFactory.CreateLogger<DevAtollRequestHandler>();
        var handler = new DevAtollRequestHandler(initialState, handlerLogger);

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => services.AddLogging());
                webHost.Configure(app =>
                {
                    app.Use(async (context, next) =>
                    {
                        if (!await handler.TryHandleAsync(context))
                        {
                            await next(context);
                        }
                    });
                });
            });

        var host = hostBuilder.Start();
        return (host.GetTestClient(), handler, host);
    }

    // ── DevAtollRequestHandler tests ─────────────────────────────────────────

    [Fact]
    public async Task DevAtollRequestHandlerShouldServeRoutesFromInitialState()
    {
        using var loggerFactory = CreateLoggerFactory();
        var state = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));

        var (client, _, host) = CreateDevTestHost(state, loggerFactory);
        using var _ = host;
        using var __ = client;

        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task DevAtollRequestHandlerShouldPickUpNewStateAfterUpdateState()
    {
        using var loggerFactory = CreateLoggerFactory();

        // State A: only index route.
        var stateA = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var (client, handler, host) = CreateDevTestHost(stateA, loggerFactory);
        using var _ = host;
        using var __ = client;

        // /about should 404 with state A.
        var beforeResponse = await client.GetAsync("/about");
        beforeResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // Swap to state B: index + about routes.
        var stateB = CreateStateWithRoutes(
            ("index.cs", typeof(IndexPage)),
            ("about.cs", typeof(AboutPage)));
        handler.UpdateState(stateB);

        // /about should 200 with state B.
        var afterResponse = await client.GetAsync("/about");
        afterResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DevAtollRequestHandlerShouldHandleEmptyStateGracefully()
    {
        using var loggerFactory = CreateLoggerFactory();
        var emptyState = DevServerState.Empty;

        var (client, _, host) = CreateDevTestHost(emptyState, loggerFactory);
        using var _ = host;
        using var __ = client;

        // All requests should fall through to the next middleware (404).
        var response = await client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DevAtollRequestHandlerShouldReturnHtmlContentType()
    {
        using var loggerFactory = CreateLoggerFactory();
        var state = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));

        var (client, _, host) = CreateDevTestHost(state, loggerFactory);
        using var _ = host;
        using var __ = client;

        var response = await client.GetAsync("/");
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
    }

    [Fact]
    public async Task DevAtollRequestHandlerStateSwapShouldBeThreadSafe()
    {
        using var loggerFactory = CreateLoggerFactory();

        var stateA = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var stateB = CreateStateWithRoutes(
            ("index.cs", typeof(IndexPage)),
            ("about.cs", typeof(AboutPage)));

        var (client, handler, host) = CreateDevTestHost(stateA, loggerFactory);
        using var _ = host;
        using var __ = client;

        // Interleave concurrent requests with state swaps — all should complete without exceptions.
        var tasks = new List<Task<HttpResponseMessage>>();
        for (var i = 0; i < 20; i++)
        {
            tasks.Add(client.GetAsync("/"));
            if (i % 5 == 0)
            {
                handler.UpdateState(i % 2 == 0 ? stateA : stateB);
            }
        }

        var responses = await Task.WhenAll(tasks);
        foreach (var response in responses)
        {
            // Each request should have completed — not thrown.
            response.StatusCode.ShouldBeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    // ── DevFileWatcher tests ──────────────────────────────────────────────────

    private string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "AtollDevTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    [Fact]
    public async Task DevFileWatcherShouldClassifyMdChangesAsContentOnly()
    {
        var dir = CreateTempDir();
        FileChangeKind? observed = null;
        var tcs = new TaskCompletionSource<FileChangeKind>();

        using var watcher = new DevFileWatcher(dir, debounceMs: 50);
        watcher.OnChange += kind =>
        {
            observed = kind;
            tcs.TrySetResult(kind);
            return Task.CompletedTask;
        };
        watcher.Start();

        await File.WriteAllTextAsync(Path.Combine(dir, "test.md"), "hello");

        var result = await Task.WhenAny(tcs.Task, Task.Delay(3000));
        result.ShouldBe(tcs.Task, "Watcher should have fired within 3s");
        observed.ShouldBe(FileChangeKind.ContentOnly);
    }

    [Fact]
    public async Task DevFileWatcherShouldClassifyCsChangesAsCodeChange()
    {
        var dir = CreateTempDir();
        var tcs = new TaskCompletionSource<FileChangeKind>();

        using var watcher = new DevFileWatcher(dir, debounceMs: 50);
        watcher.OnChange += kind => { tcs.TrySetResult(kind); return Task.CompletedTask; };
        watcher.Start();

        await File.WriteAllTextAsync(Path.Combine(dir, "MyPage.cs"), "// code");

        var result = await Task.WhenAny(tcs.Task, Task.Delay(3000));
        result.ShouldBe(tcs.Task, "Watcher should have fired within 3s");
        var observedKind = await tcs.Task;
        observedKind.ShouldBe(FileChangeKind.CodeChange);
    }

    [Fact]
    public async Task DevFileWatcherShouldDebounceRapidChanges()
    {
        var dir = CreateTempDir();
        var fireCount = 0;
        var tcs = new TaskCompletionSource<bool>();

        // Use a generous debounce window (500ms) so that filesystem notification
        // delays on slow CI runners don't cause the timer to fire prematurely.
        using var watcher = new DevFileWatcher(dir, debounceMs: 500);
        watcher.OnChange += _ =>
        {
            Interlocked.Increment(ref fireCount);
            tcs.TrySetResult(true);
            return Task.CompletedTask;
        };
        watcher.Start();

        // Rapidly write 5 files with no artificial delay between writes so they
        // are as close together as possible within a single debounce window.
        for (var i = 0; i < 5; i++)
        {
            await File.WriteAllTextAsync(Path.Combine(dir, $"doc{i}.md"), "content");
        }

        // Wait for the debounce to fire (up to 5s for slow runners).
        await Task.WhenAny(tcs.Task, Task.Delay(5000));

        // Allow generous settle time for any additional (spurious) fires.
        await Task.Delay(1000);

        fireCount.ShouldBe(1, "Rapid changes should be debounced into a single callback");
    }

    [Fact]
    public async Task DevFileWatcherShouldEscalateToCodeChangeWhenBothKindsChange()
    {
        var dir = CreateTempDir();
        FileChangeKind? lastObservedKind = null;
        var fireCount = 0;
        var firedEvent = new ManualResetEventSlim(false);

        // Use a generous debounce window (500ms) so that both filesystem
        // notifications are aggregated into a single debounce window, even
        // on slow CI runners where FSW delivery can be delayed.
        using var watcher = new DevFileWatcher(dir, debounceMs: 500);
        watcher.OnChange += kind =>
        {
            lastObservedKind = kind;
            Interlocked.Increment(ref fireCount);
            firedEvent.Set();
            return Task.CompletedTask;
        };
        watcher.Start();

        // Write both files as fast as possible so they land within the
        // same debounce window regardless of FSW notification timing.
        await File.WriteAllTextAsync(Path.Combine(dir, "doc.md"), "markdown");
        await File.WriteAllTextAsync(Path.Combine(dir, "page.cs"), "// code");

        // Wait for at least one debounce fire (up to 5s for slow runners).
        firedEvent.Wait(TimeSpan.FromSeconds(5))
            .ShouldBeTrue("Watcher should have fired within 5s");

        // Allow settle time for any additional fires.
        await Task.Delay(1500);

        lastObservedKind.ShouldBe(FileChangeKind.CodeChange,
            "Mixed .md + .cs changes should escalate to CodeChange");
    }

    // ── DevServerState tests ──────────────────────────────────────────────────

    [Fact]
    public void DevServerStateEmptyShouldHaveNoRoutes()
    {
        var state = DevServerState.Empty;
        state.RouteMatcher.ShouldNotBeNull();
        state.RouteMatcher.SortedRoutes.Count.ShouldBe(0);
        state.LoadContext.ShouldBeNull();
        state.UserAssembly.ShouldBeNull();
    }

    [Fact]
    public void ContentOnlyReloadShouldNotScheduleAlcUnload()
    {
        // Both states share the same (null) ALC — typical of content-only reloads.
        using var loggerFactory = CreateLoggerFactory();
        var handlerLogger = loggerFactory.CreateLogger<DevAtollRequestHandler>();

        var stateA = DevServerState.Empty; // LoadContext = null
        var handler = new DevAtollRequestHandler(stateA, handlerLogger);

        // Content-only new state reuses the same null ALC.
        var options = new AtollOptions();
        var stateB = new DevServerState(new RouteMatcher([]), options, null, null, "", EmptyAssets, null, null);

        // Should not throw — must not try to unload null ALC.
        var exception = Record.Exception(() => handler.UpdateState(stateB));
        exception.ShouldBeNull();

        // Handler should now use the new state.
        handler.CurrentState.ShouldBeSameAs(stateB);
    }

    // ── Live-reload WebSocket tests ───────────────────────────────────────────

    /// <summary>
    /// Builds a test host that wires <see cref="LiveReloadMiddleware"/> and
    /// <see cref="DevAtollRequestHandler"/> into the pipeline, returning all
    /// three objects needed to drive live-reload assertions.
    /// </summary>
    private static (HttpClient Client, DevAtollRequestHandler Handler, LiveReloadWebSocketHandler LiveReloadHandler, IHost Host)
        CreateDevTestHostWithLiveReload(DevServerState initialState, ILoggerFactory loggerFactory)
    {
        var liveReloadHandler = new LiveReloadWebSocketHandler();
        var handlerLogger = loggerFactory.CreateLogger<DevAtollRequestHandler>();
        var handler = new DevAtollRequestHandler(initialState, handlerLogger);

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton(liveReloadHandler);
                });
                webHost.Configure(app =>
                {
                    app.UseWebSockets();
                    app.UseMiddleware<LiveReloadMiddleware>();
                    app.Use(async (context, next) =>
                    {
                        if (!await handler.TryHandleAsync(context))
                        {
                            await next(context);
                        }
                    });
                });
            });

        var host = hostBuilder.Start();
        return (host.GetTestClient(), handler, liveReloadHandler, host);
    }

    private static async Task<string> ReceiveMessageAsync(WebSocket webSocket)
    {
        var buffer = new byte[4096];
        var result = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None);
        return System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    [Fact]
    public async Task LiveReloadShouldNotifyAfterCodeChangeStateSwap()
    {
        using var loggerFactory = CreateLoggerFactory();
        var stateA = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var (client, handler, liveReloadHandler, host) =
            CreateDevTestHostWithLiveReload(stateA, loggerFactory);
        using var _ = host;
        using var __ = client;
        using var ___ = liveReloadHandler;

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        // Give handler time to register the connection.
        await Task.Delay(50);

        // Simulate a code-change reload: swap state then notify.
        var stateB = CreateStateWithRoutes(
            ("index.cs", typeof(IndexPage)),
            ("about.cs", typeof(AboutPage)));
        handler.UpdateState(stateB);
        await liveReloadHandler.NotifyReloadAsync();

        var message = await ReceiveMessageAsync(ws);
        message.ShouldBe("{\"type\":\"reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task LiveReloadShouldNotifyAfterContentOnlyStateSwap()
    {
        using var loggerFactory = CreateLoggerFactory();
        var stateA = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var (client, handler, liveReloadHandler, host) =
            CreateDevTestHostWithLiveReload(stateA, loggerFactory);
        using var _ = host;
        using var __ = client;
        using var ___ = liveReloadHandler;

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        // Content-only reload: reuse same state instance (no ALC change), then notify.
        handler.UpdateState(stateA);
        await liveReloadHandler.NotifyReloadAsync();

        var message = await ReceiveMessageAsync(ws);
        // Content changes alter rendered HTML so the full-page reload message is used,
        // not the css-reload message.
        message.ShouldBe("{\"type\":\"reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task LiveReloadShouldInjectScriptIntoHtmlResponses()
    {
        using var loggerFactory = CreateLoggerFactory();
        var state = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var (client, _, liveReloadHandler, host) =
            CreateDevTestHostWithLiveReload(state, loggerFactory);
        using var _ = host;
        using var __ = client;
        using var ___ = liveReloadHandler;

        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("data-atoll-live-reload");
        html.ShouldContain("__atoll-live-reload");
    }

    [Fact]
    public async Task LiveReloadShouldSendBuildErrorOnFailedReload()
    {
        using var loggerFactory = CreateLoggerFactory();
        var state = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var (client, handler, liveReloadHandler, host) =
            CreateDevTestHostWithLiveReload(state, loggerFactory);
        using var _ = host;
        using var __ = client;
        using var ___ = liveReloadHandler;

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        // Simulate a build failure notification.
        await liveReloadHandler.NotifyBuildErrorAsync("error CS1002: ; expected");

        var message = await ReceiveMessageAsync(ws);
        message.ShouldContain("\"type\":\"build-error\"");
        message.ShouldContain("error CS1002");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task LiveReloadShouldDismissOverlayOnSuccessfulReload()
    {
        using var loggerFactory = CreateLoggerFactory();
        var state = CreateStateWithRoutes(("index.cs", typeof(IndexPage)));
        var (client, handler, liveReloadHandler, host) =
            CreateDevTestHostWithLiveReload(state, loggerFactory);
        using var _ = host;
        using var __ = client;
        using var ___ = liveReloadHandler;

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        // First: build error.
        await liveReloadHandler.NotifyBuildErrorAsync("error CS1002: ; expected");
        var errorMsg = await ReceiveMessageAsync(ws);
        errorMsg.ShouldContain("\"type\":\"build-error\"");

        // Then: successful reload (dismisses overlay via page reload).
        await liveReloadHandler.NotifyReloadAsync();
        var reloadMsg = await ReceiveMessageAsync(ws);
        reloadMsg.ShouldBe("{\"type\":\"reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            try { Directory.Delete(dir, recursive: true); }
            catch { /* best effort */ }
        }
    }
}
