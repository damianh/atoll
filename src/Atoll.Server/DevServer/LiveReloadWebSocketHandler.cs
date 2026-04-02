using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Atoll.Server.DevServer;

/// <summary>
/// Manages WebSocket connections for the live reload system. Tracks connected
/// clients and broadcasts reload messages when source files change.
/// </summary>
/// <remarks>
/// <para>
/// The handler maintains a thread-safe collection of active WebSocket connections.
/// When <see cref="NotifyReloadAsync"/> or <see cref="NotifyCssReloadAsync"/> is called,
/// a JSON message is sent to all connected clients, instructing them to either
/// perform a full page reload or just re-fetch CSS.
/// </para>
/// </remarks>
public sealed class LiveReloadWebSocketHandler : IDisposable
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private bool _disposed;

    /// <summary>
    /// Gets the number of currently connected WebSocket clients.
    /// </summary>
    public int ConnectionCount => _connections.Count;

    /// <summary>
    /// Accepts and tracks a new WebSocket connection. Blocks until the client
    /// disconnects or the cancellation token is triggered.
    /// </summary>
    /// <param name="webSocket">The accepted WebSocket.</param>
    /// <param name="cancellationToken">Cancellation token for the connection lifetime.</param>
    /// <returns>A task that completes when the WebSocket closes.</returns>
    public async Task HandleConnectionAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(webSocket);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var connectionId = Guid.NewGuid().ToString("N");
        _connections.TryAdd(connectionId, webSocket);

        try
        {
            // Keep the connection alive by reading (and discarding) incoming messages
            var buffer = new byte[256];
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        catch (WebSocketException)
        {
            // Client disconnected unexpectedly
        }
        catch (OperationCanceledException)
        {
            // Server shutting down
        }
        finally
        {
            _connections.TryRemove(connectionId, out _);

            if (webSocket.State == WebSocketState.Open ||
                webSocket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Server shutting down",
                        CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    // Already closed
                }
            }
        }
    }

    /// <summary>
    /// Sends a full page reload message to all connected clients.
    /// </summary>
    /// <returns>A task that completes when all messages have been sent.</returns>
    public async Task NotifyReloadAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await BroadcastAsync("{\"type\":\"reload\"}");
    }

    /// <summary>
    /// Sends a CSS-only reload message to all connected clients.
    /// This allows browsers to re-fetch stylesheets without a full page refresh.
    /// </summary>
    /// <returns>A task that completes when all messages have been sent.</returns>
    public async Task NotifyCssReloadAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await BroadcastAsync("{\"type\":\"css-reload\"}");
    }

    /// <summary>
    /// Broadcasts a text message to all connected WebSocket clients.
    /// </summary>
    private async Task BroadcastAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(messageBytes);

        var deadConnections = new List<string>();

        foreach (var kvp in _connections)
        {
            try
            {
                if (kvp.Value.State == WebSocketState.Open)
                {
                    await kvp.Value.SendAsync(
                        segment,
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        CancellationToken.None);
                }
                else
                {
                    deadConnections.Add(kvp.Key);
                }
            }
            catch (WebSocketException)
            {
                deadConnections.Add(kvp.Key);
            }
        }

        // Clean up dead connections
        foreach (var id in deadConnections)
        {
            _connections.TryRemove(id, out _);
        }
    }

    /// <summary>
    /// Disposes of all managed resources and closes active connections.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var kvp in _connections)
        {
            try
            {
                if (kvp.Value.State == WebSocketState.Open)
                {
                    kvp.Value.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Server shutting down",
                        CancellationToken.None).GetAwaiter().GetResult();
                }
            }
            catch (WebSocketException)
            {
                // Ignore errors during shutdown
            }
        }

        _connections.Clear();
    }
}
