using System.Net;
using System.Net.WebSockets;
using Atoll.Middleware.Server.DevServer;
using Microsoft.AspNetCore.TestHost;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests.Server.DevServer;

public sealed class LiveReloadMiddlewareTests : IDisposable
{
    private readonly LiveReloadWebSocketHandler _handler = new();

    public void Dispose()
    {
        _handler.Dispose();
    }

    // ── Script injection into HTML responses ────────────────────────

    [Fact]
    public async Task ShouldInjectScriptIntoHtmlResponse()
    {
        using var client = CreateTestClient(
            "<html><head><title>Test</title></head><body><h1>Hello</h1></body></html>");

        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("data-atoll-live-reload");
        body.ShouldContain("__atoll-live-reload");
        body.ShouldContain("</body>");
    }

    [Fact]
    public async Task ShouldInjectScriptBeforeClosingBodyTag()
    {
        using var client = CreateTestClient(
            "<html><body><p>Content</p></body></html>");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        // The script should appear before </body>
        var scriptIndex = body.IndexOf("data-atoll-live-reload", StringComparison.Ordinal);
        var bodyCloseIndex = body.IndexOf("</body>", StringComparison.Ordinal);

        scriptIndex.ShouldBeGreaterThan(-1);
        bodyCloseIndex.ShouldBeGreaterThan(scriptIndex);
    }

    [Fact]
    public async Task ShouldNotModifyNonHtmlResponses()
    {
        using var client = CreateTestClient(
            "{\"key\":\"value\"}", "application/json");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldBe("{\"key\":\"value\"}");
        body.ShouldNotContain("data-atoll-live-reload");
    }

    [Fact]
    public async Task ShouldNotModifyHtmlWithoutBodyTag()
    {
        using var client = CreateTestClient(
            "<html><head><title>Headless</title></head></html>");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldNotContain("data-atoll-live-reload");
        body.ShouldBe("<html><head><title>Headless</title></head></html>");
    }

    [Fact]
    public async Task ShouldHandleEmptyResponse()
    {
        using var client = CreateTestClient("", "text/html");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldHandleCaseInsensitiveBodyTag()
    {
        using var client = CreateTestClient(
            "<html><BODY><p>Content</p></BODY></html>");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldContain("data-atoll-live-reload");
    }

    [Fact]
    public async Task ShouldNotModifyTextPlainResponses()
    {
        using var client = CreateTestClient(
            "Just plain text", "text/plain");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldBe("Just plain text");
        body.ShouldNotContain("data-atoll-live-reload");
    }

    [Fact]
    public async Task ShouldPreserveOriginalHtmlContent()
    {
        var html = "<html><head><title>My Page</title></head><body><h1>Welcome</h1><p>Content here</p></body></html>";
        using var client = CreateTestClient(html);

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldContain("<h1>Welcome</h1>");
        body.ShouldContain("<p>Content here</p>");
        body.ShouldContain("<title>My Page</title>");
    }

    [Fact]
    public async Task ShouldNotInjectScriptTwice()
    {
        using var client = CreateTestClient(
            "<html><body><p>Hello</p></body></html>");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        // Count occurrences of the script marker
        var count = CountOccurrences(body, "data-atoll-live-reload");
        count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldHandleHtmlWithCharsetInContentType()
    {
        using var client = CreateTestClient(
            "<html><body><p>Charset test</p></body></html>",
            "text/html; charset=utf-8");

        var response = await client.GetAsync("/");
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldContain("data-atoll-live-reload");
    }

    // ── Script content ──────────────────────────────────────────────

    [Fact]
    public void GetInjectedScriptShouldContainWebSocketConnection()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("new WebSocket");
        script.ShouldContain("__atoll-live-reload");
    }

    [Fact]
    public void GetInjectedScriptShouldHandleReloadMessage()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("location.reload()");
        script.ShouldContain("'reload'");
    }

    [Fact]
    public void GetInjectedScriptShouldHandleCssReloadMessage()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("'css-reload'");
        script.ShouldContain("stylesheet");
    }

    [Fact]
    public void GetInjectedScriptShouldSupportAutoReconnect()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("onclose");
        script.ShouldContain("reconnectDelay");
        script.ShouldContain("setTimeout");
    }

    [Fact]
    public void GetInjectedScriptShouldHandleBuildErrorMessage()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("'build-error'");
        script.ShouldContain("atoll-build-error-overlay");
    }

    [Fact]
    public void GetInjectedScriptShouldContainOverlayCloseButton()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("closeBtn");
        script.ShouldContain("overlay.remove()");
    }

    [Fact]
    public void GetInjectedScriptShouldSupportBothProtocols()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("wss:");
        script.ShouldContain("ws:");
    }

    [Fact]
    public void GetInjectedScriptShouldBeWrappedInScriptTag()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("<script");
        script.ShouldContain("</script>");
    }

    [Fact]
    public void GetInjectedScriptShouldHaveDataAttribute()
    {
        var script = LiveReloadMiddleware.GetInjectedScript();

        script.ShouldContain("data-atoll-live-reload");
    }

    // ── WebSocket path constant ─────────────────────────────────────

    [Fact]
    public void WebSocketPathShouldBeCorrect()
    {
        LiveReloadMiddleware.WebSocketPath.ShouldBe("/__atoll-live-reload");
    }

    // ── WebSocket upgrade handling ──────────────────────────────────

    [Fact]
    public async Task ShouldReturnOkForNonWebSocketRequestToWebSocketPath()
    {
        // A regular (non-upgrade) HTTP request to the WebSocket path should
        // pass through to the next middleware
        using var client = CreateTestClient(
            "Fallback response", "text/plain");

        var response = await client.GetAsync(LiveReloadMiddleware.WebSocketPath);
        var body = await response.Content.ReadAsStringAsync();

        // The request passes through because it's not a WebSocket upgrade
        body.ShouldBe("Fallback response");
    }

    [Fact]
    public async Task ShouldAcceptWebSocketConnection()
    {
        using var host = CreateWebSocketHost();
        host.Start();

        var wsClient = host.GetTestServer().CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        ws.State.ShouldBe(WebSocketState.Open);
        _handler.ConnectionCount.ShouldBe(1);

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task ShouldReceiveReloadMessageViaWebSocket()
    {
        using var host = CreateWebSocketHost();
        host.Start();

        var wsClient = host.GetTestServer().CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50); // Let the handler register the connection

        await _handler.NotifyReloadAsync();

        var buffer = new byte[4096];
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

        message.ShouldBe("{\"type\":\"reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task ShouldReceiveCssReloadMessageViaWebSocket()
    {
        using var host = CreateWebSocketHost();
        host.Start();

        var wsClient = host.GetTestServer().CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        await _handler.NotifyCssReloadAsync();

        var buffer = new byte[4096];
        var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

        message.ShouldBe("{\"type\":\"css-reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    // ── Integration: middleware pipeline ─────────────────────────────

    [Fact]
    public async Task ShouldWorkInMiddlewarePipeline()
    {
        using var host = CreateFullPipelineHost();
        host.Start();

        using var client = host.GetTestClient();
        var response = await client.GetAsync("/page");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();

        body.ShouldContain("<h1>My Page</h1>");
        body.ShouldContain("data-atoll-live-reload");
        body.ShouldContain("</body>");
        body.ShouldContain("</html>");
    }

    [Fact]
    public async Task ShouldNotInjectIntoApiResponse()
    {
        using var host = CreateFullPipelineHost();
        host.Start();

        using var client = host.GetTestClient();
        var response = await client.GetAsync("/api/data");

        var body = await response.Content.ReadAsStringAsync();

        body.ShouldBe("{\"ok\":true}");
        body.ShouldNotContain("data-atoll-live-reload");
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private HttpClient CreateTestClient(string responseContent, string contentType = "text/html")
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton(_handler);
                });
                webHost.Configure(app =>
                {
                    app.UseWebSockets();
                    app.UseMiddleware<LiveReloadMiddleware>();
                    app.Run(async ctx =>
                    {
                        ctx.Response.ContentType = contentType;
                        await ctx.Response.WriteAsync(responseContent);
                    });
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    private IHost CreateWebSocketHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton(_handler);
                });
                webHost.Configure(app =>
                {
                    app.UseWebSockets();
                    app.UseMiddleware<LiveReloadMiddleware>();
                    app.Run(async ctx =>
                    {
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.WriteAsync("Fallback");
                    });
                });
            })
            .Build();
    }

    private IHost CreateFullPipelineHost()
    {
        return new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddSingleton(_handler);
                });
                webHost.Configure(app =>
                {
                    app.UseWebSockets();
                    app.UseMiddleware<LiveReloadMiddleware>();
                    app.Run(async ctx =>
                    {
                        if (ctx.Request.Path == "/page")
                        {
                            ctx.Response.ContentType = "text/html; charset=utf-8";
                            await ctx.Response.WriteAsync(
                                "<html><head><title>Test</title></head><body><h1>My Page</h1></body></html>");
                        }
                        else if (ctx.Request.Path == "/api/data")
                        {
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync("{\"ok\":true}");
                        }
                        else
                        {
                            ctx.Response.StatusCode = 404;
                        }
                    });
                });
            })
            .Build();
    }

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
