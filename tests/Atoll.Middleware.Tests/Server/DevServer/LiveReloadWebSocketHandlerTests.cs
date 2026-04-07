using System.Net.WebSockets;
using System.Text.Json;
using Atoll.Middleware.Server.DevServer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atoll.Middleware.Tests.Server.DevServer;

public sealed class LiveReloadWebSocketHandlerTests : IDisposable
{
    private readonly LiveReloadWebSocketHandler _handler = new();

    public void Dispose()
    {
        _handler.Dispose();
    }

    // ── ConnectionCount ─────────────────────────────────────────────

    [Fact]
    public void ConnectionCountShouldBeZeroInitially()
    {
        _handler.ConnectionCount.ShouldBe(0);
    }

    // ── HandleConnectionAsync ───────────────────────────────────────

    [Fact]
    public async Task HandleConnectionAsyncShouldThrowOnNullWebSocket()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => _handler.HandleConnectionAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task HandleConnectionAsyncShouldTrackConnection()
    {
        using var host = CreateHost();
        host.Start();

        var wsClient = host.GetTestServer().CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        // Give the handler time to register
        await Task.Delay(50);
        _handler.ConnectionCount.ShouldBe(1);

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);

        // Wait for cleanup
        await Task.Delay(100);
        _handler.ConnectionCount.ShouldBe(0);
    }

    [Fact]
    public async Task HandleConnectionAsyncShouldTrackMultipleConnections()
    {
        using var host = CreateHost();
        host.Start();

        var server = host.GetTestServer();
        var ws1 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);
        var ws2 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);
        _handler.ConnectionCount.ShouldBe(2);

        await ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        await Task.Delay(100);
        _handler.ConnectionCount.ShouldBe(1);

        await ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        await Task.Delay(100);
        _handler.ConnectionCount.ShouldBe(0);
    }

    [Fact]
    public void HandleConnectionAsyncShouldNotAcceptNewConnectionsAfterDispose()
    {
        var handler = new LiveReloadWebSocketHandler();
        handler.ConnectionCount.ShouldBe(0);

        handler.Dispose();

        // After disposal, ConnectionCount should still be zero
        handler.ConnectionCount.ShouldBe(0);

        // All notification methods should throw
        Should.Throw<ObjectDisposedException>(() =>
            handler.NotifyReloadAsync().GetAwaiter().GetResult());
        Should.Throw<ObjectDisposedException>(() =>
            handler.NotifyCssReloadAsync().GetAwaiter().GetResult());
    }

    // ── NotifyReloadAsync ───────────────────────────────────────────

    [Fact]
    public async Task NotifyReloadAsyncShouldSendReloadMessage()
    {
        using var host = CreateHost();
        host.Start();

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        await _handler.NotifyReloadAsync();

        var message = await ReceiveMessageAsync(ws);
        message.ShouldBe("{\"type\":\"reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task NotifyReloadAsyncShouldThrowWhenDisposed()
    {
        var handler = new LiveReloadWebSocketHandler();
        handler.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(
            () => handler.NotifyReloadAsync());
    }

    // ── NotifyCssReloadAsync ────────────────────────────────────────

    [Fact]
    public async Task NotifyCssReloadAsyncShouldSendCssReloadMessage()
    {
        using var host = CreateHost();
        host.Start();

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        await _handler.NotifyCssReloadAsync();

        var message = await ReceiveMessageAsync(ws);
        message.ShouldBe("{\"type\":\"css-reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task NotifyCssReloadAsyncShouldThrowWhenDisposed()
    {
        var handler = new LiveReloadWebSocketHandler();
        handler.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(
            () => handler.NotifyCssReloadAsync());
    }

    // ── Broadcast to multiple ───────────────────────────────────────

    [Fact]
    public async Task ShouldBroadcastReloadToAllConnections()
    {
        using var host = CreateHost();
        host.Start();

        var server = host.GetTestServer();
        var ws1 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);
        var ws2 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);
        _handler.ConnectionCount.ShouldBe(2);

        await _handler.NotifyReloadAsync();

        var msg1 = await ReceiveMessageAsync(ws1);
        var msg2 = await ReceiveMessageAsync(ws2);

        msg1.ShouldBe("{\"type\":\"reload\"}");
        msg2.ShouldBe("{\"type\":\"reload\"}");

        await ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        await ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task ShouldBroadcastCssReloadToAllConnections()
    {
        using var host = CreateHost();
        host.Start();

        var server = host.GetTestServer();
        var ws1 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);
        var ws2 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        await _handler.NotifyCssReloadAsync();

        var msg1 = await ReceiveMessageAsync(ws1);
        var msg2 = await ReceiveMessageAsync(ws2);

        msg1.ShouldBe("{\"type\":\"css-reload\"}");
        msg2.ShouldBe("{\"type\":\"css-reload\"}");

        await ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        await ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task ShouldNotFailWhenBroadcastingWithNoConnections()
    {
        // Should not throw
        await _handler.NotifyReloadAsync();
        await _handler.NotifyCssReloadAsync();
    }

    // ── Multiple messages ───────────────────────────────────────────

    [Fact]
    public async Task ShouldSendMultipleMessagesInSequence()
    {
        using var host = CreateHost();
        host.Start();

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        await _handler.NotifyReloadAsync();
        await _handler.NotifyCssReloadAsync();
        await _handler.NotifyReloadAsync();

        var msg1 = await ReceiveMessageAsync(ws);
        var msg2 = await ReceiveMessageAsync(ws);
        var msg3 = await ReceiveMessageAsync(ws);

        msg1.ShouldBe("{\"type\":\"reload\"}");
        msg2.ShouldBe("{\"type\":\"css-reload\"}");
        msg3.ShouldBe("{\"type\":\"reload\"}");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    // ── Dispose ─────────────────────────────────────────────────────

    [Fact]
    public void DisposeShouldBeIdempotent()
    {
        var handler = new LiveReloadWebSocketHandler();

        handler.Dispose();
        handler.Dispose(); // Should not throw
    }

    // ── NotifyBuildErrorAsync ────────────────────────────────────────

    [Fact]
    public async Task NotifyBuildErrorAsyncShouldSendBuildErrorMessage()
    {
        using var host = CreateHost();
        host.Start();

        var ws = await host.GetTestServer().CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);

        await _handler.NotifyBuildErrorAsync("error CS1002: ; expected");

        var message = await ReceiveMessageAsync(ws);
        using var doc = JsonDocument.Parse(message);
        doc.RootElement.GetProperty("type").GetString().ShouldBe("build-error");
        doc.RootElement.GetProperty("errors").GetString().ShouldNotBeNull().ShouldContain("error CS1002");

        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    [Fact]
    public async Task NotifyBuildErrorAsyncShouldThrowWhenDisposed()
    {
        var handler = new LiveReloadWebSocketHandler();
        handler.Dispose();

        await Should.ThrowAsync<ObjectDisposedException>(
            () => handler.NotifyBuildErrorAsync("error"));
    }

    [Fact]
    public async Task NotifyBuildErrorAsyncShouldThrowOnNullErrors()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => _handler.NotifyBuildErrorAsync(null!));
    }

    [Fact]
    public async Task NotifyBuildErrorAsyncShouldBroadcastToAllConnections()
    {
        using var host = CreateHost();
        host.Start();

        var server = host.GetTestServer();
        var ws1 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);
        var ws2 = await server.CreateWebSocketClient().ConnectAsync(
            new Uri("ws://localhost" + LiveReloadMiddleware.WebSocketPath),
            CancellationToken.None);

        await Task.Delay(50);
        _handler.ConnectionCount.ShouldBe(2);

        await _handler.NotifyBuildErrorAsync("error CS0246: type not found");

        var msg1 = await ReceiveMessageAsync(ws1);
        var msg2 = await ReceiveMessageAsync(ws2);

        using var doc1 = JsonDocument.Parse(msg1);
        using var doc2 = JsonDocument.Parse(msg2);
        doc1.RootElement.GetProperty("type").GetString().ShouldBe("build-error");
        doc2.RootElement.GetProperty("type").GetString().ShouldBe("build-error");
        doc1.RootElement.GetProperty("errors").GetString().ShouldNotBeNull().ShouldContain("CS0246");
        doc2.RootElement.GetProperty("errors").GetString().ShouldNotBeNull().ShouldContain("CS0246");

        await ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        await ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private IHost CreateHost()
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
                        await ctx.Response.WriteAsync("OK");
                    });
                });
            })
            .Build();
    }

    private static async Task<string> ReceiveMessageAsync(WebSocket webSocket)
    {
        var buffer = new byte[4096];
        var result = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None);
        return System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
    }
}
