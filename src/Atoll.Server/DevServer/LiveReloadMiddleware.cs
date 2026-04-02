using System.Text;
using Microsoft.AspNetCore.Http;

namespace Atoll.Server.DevServer;

/// <summary>
/// ASP.NET Core middleware that enables live reload functionality during development.
/// It handles two responsibilities:
/// </summary>
/// <remarks>
/// <para>
/// 1. Intercepts WebSocket upgrade requests at <c>/__atoll-live-reload</c> and delegates
///    them to <see cref="LiveReloadWebSocketHandler"/> for persistent connection tracking.
/// </para>
/// <para>
/// 2. For HTML responses, injects a small JavaScript snippet before the closing
///    <c>&lt;/body&gt;</c> tag. This script opens a WebSocket connection to the server
///    and listens for <c>reload</c> and <c>css-reload</c> messages.
/// </para>
/// </remarks>
public sealed class LiveReloadMiddleware
{
    /// <summary>
    /// The request path that the live reload WebSocket endpoint listens on.
    /// </summary>
    public const string WebSocketPath = "/__atoll-live-reload";

    private static readonly byte[] ClosingBodyTag = Encoding.UTF8.GetBytes("</body>");

    private readonly RequestDelegate _next;
    private readonly LiveReloadWebSocketHandler _handler;

    /// <summary>
    /// Initializes a new <see cref="LiveReloadMiddleware"/> with the next middleware
    /// delegate and the live reload WebSocket handler.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="handler">The WebSocket handler that manages live reload connections.</param>
    public LiveReloadMiddleware(RequestDelegate next, LiveReloadWebSocketHandler handler)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(handler);
        _next = next;
        _handler = handler;
    }

    /// <summary>
    /// Processes an incoming HTTP request. If the request is a WebSocket upgrade
    /// targeting <see cref="WebSocketPath"/>, accepts the connection. Otherwise,
    /// intercepts HTML responses and injects the live reload script.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Handle WebSocket upgrade requests at the live reload path
        if (context.Request.Path == WebSocketPath && context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await _handler.HandleConnectionAsync(webSocket, context.RequestAborted);
            return;
        }

        // For non-WebSocket requests, capture the response to inject the script
        var originalBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = memoryStream.ToArray();

            if (IsHtmlResponse(context.Response) && responseBody.Length > 0)
            {
                responseBody = InjectScript(responseBody);
            }

            context.Response.Body = originalBody;
            context.Response.ContentLength = responseBody.Length;
            await context.Response.Body.WriteAsync(responseBody);
        }
        catch
        {
            context.Response.Body = originalBody;
            throw;
        }
    }

    /// <summary>
    /// Determines whether the response has an HTML content type.
    /// </summary>
    private static bool IsHtmlResponse(HttpResponse response)
    {
        var contentType = response.ContentType;
        if (contentType is null)
        {
            return false;
        }

        return contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Injects the live reload JavaScript snippet before the closing
    /// <c>&lt;/body&gt;</c> tag in the response body. If no closing body tag
    /// is found, the response is returned unchanged.
    /// </summary>
    private static byte[] InjectScript(byte[] responseBody)
    {
        var bodyIndex = FindClosingBodyTag(responseBody);
        if (bodyIndex < 0)
        {
            return responseBody;
        }

        var scriptBytes = Encoding.UTF8.GetBytes(GetInjectedScript());
        var result = new byte[responseBody.Length + scriptBytes.Length];

        // Copy everything before </body>
        Array.Copy(responseBody, 0, result, 0, bodyIndex);
        // Insert script
        Array.Copy(scriptBytes, 0, result, bodyIndex, scriptBytes.Length);
        // Copy </body> and everything after
        Array.Copy(responseBody, bodyIndex, result, bodyIndex + scriptBytes.Length, responseBody.Length - bodyIndex);

        return result;
    }

    /// <summary>
    /// Finds the byte index of the closing <c>&lt;/body&gt;</c> tag in the
    /// response body using a case-insensitive search. Returns -1 if not found.
    /// </summary>
    private static int FindClosingBodyTag(byte[] responseBody)
    {
        if (responseBody.Length < ClosingBodyTag.Length)
        {
            return -1;
        }

        // Search for </body> case-insensitively by scanning bytes
        for (var i = 0; i <= responseBody.Length - ClosingBodyTag.Length; i++)
        {
            var found = true;
            for (var j = 0; j < ClosingBodyTag.Length; j++)
            {
                var b = responseBody[i + j];
                // Convert to lowercase for comparison
                if (b >= (byte)'A' && b <= (byte)'Z')
                {
                    b = (byte)(b + 32);
                }

                if (b != ClosingBodyTag[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the JavaScript snippet that establishes the WebSocket connection
    /// for live reload. The script automatically reconnects on disconnect and
    /// handles both full page reloads and CSS-only reloads.
    /// </summary>
    /// <summary>
    /// Gets the JavaScript snippet that is injected into HTML pages.
    /// Exposed for testing.
    /// </summary>
    public static string GetInjectedScript()
    {
        return """
            <script data-atoll-live-reload>
            (function(){
              var protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
              var url = protocol + '//' + location.host + '/__atoll-live-reload';
              var reconnectDelay = 1000;
              function connect(){
                var ws = new WebSocket(url);
                ws.onmessage = function(e){
                  var data = JSON.parse(e.data);
                  if(data.type === 'reload'){
                    location.reload();
                  } else if(data.type === 'css-reload'){
                    var links = document.querySelectorAll('link[rel="stylesheet"]');
                    links.forEach(function(link){
                      var href = link.getAttribute('href');
                      if(href){
                        var separator = href.indexOf('?') >= 0 ? '&' : '?';
                        link.setAttribute('href', href.split('?')[0] + separator + '_r=' + Date.now());
                      }
                    });
                  }
                };
                ws.onclose = function(){
                  setTimeout(connect, reconnectDelay);
                };
              }
              connect();
            })();
            </script>
            """;
    }
}
