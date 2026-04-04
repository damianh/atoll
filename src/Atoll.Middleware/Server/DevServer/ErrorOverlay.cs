using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Atoll.Middleware.Server.DevServer;

/// <summary>
/// Renders a styled HTML error overlay page for the development server.
/// When a component throws during rendering, this overlay shows the exception
/// type, message, stack trace, and source location in the browser.
/// </summary>
/// <remarks>
/// <para>
/// The overlay is intended for development only and should never be used in
/// production. It renders a self-contained HTML page with inline CSS styling
/// so no external resources are needed.
/// </para>
/// </remarks>
public static class ErrorOverlay
{
    /// <summary>
    /// Renders a full HTML error overlay page for the specified exception.
    /// </summary>
    /// <param name="exception">The exception to display.</param>
    /// <returns>A complete HTML page string with styled error information.</returns>
    public static string Render(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.AppendLine("  <title>Atoll - Error</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine(GetCss());
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"overlay\">");

        // Header
        sb.AppendLine("    <div class=\"header\">");
        sb.AppendLine("      <span class=\"badge\">ERROR</span>");
        sb.AppendLine("      <h1>Atoll encountered an error</h1>");
        sb.AppendLine("    </div>");

        // Exception type and message
        sb.AppendLine("    <div class=\"section\">");
        sb.Append("      <h2 class=\"exception-type\">");
        sb.Append(HtmlEncode(exception.GetType().FullName ?? exception.GetType().Name));
        sb.AppendLine("</h2>");
        sb.Append("      <p class=\"exception-message\">");
        sb.Append(HtmlEncode(exception.Message));
        sb.AppendLine("</p>");
        sb.AppendLine("    </div>");

        // Source location (if available from stack trace)
        var sourceLocation = ExtractSourceLocation(exception);
        if (sourceLocation is not null)
        {
            sb.AppendLine("    <div class=\"section\">");
            sb.AppendLine("      <h3>Source</h3>");
            sb.AppendLine("      <div class=\"source-location\">");
            sb.Append("        <span class=\"file-path\">");
            sb.Append(HtmlEncode(sourceLocation.FilePath));
            sb.AppendLine("</span>");
            if (sourceLocation.LineNumber > 0)
            {
                sb.Append("        <span class=\"line-number\">Line ");
                sb.Append(sourceLocation.LineNumber);
                sb.AppendLine("</span>");
            }
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
        }

        // Stack trace
        if (exception.StackTrace is not null)
        {
            sb.AppendLine("    <div class=\"section\">");
            sb.AppendLine("      <h3>Stack Trace</h3>");
            sb.AppendLine("      <pre class=\"stack-trace\">");
            sb.Append(FormatStackTrace(exception.StackTrace));
            sb.AppendLine("</pre>");
            sb.AppendLine("    </div>");
        }

        // Inner exception chain
        var inner = exception.InnerException;
        var depth = 0;
        while (inner is not null && depth < 5)
        {
            sb.AppendLine("    <div class=\"section inner-exception\">");
            sb.Append("      <h3>Inner Exception");
            if (depth > 0)
            {
                sb.Append(" (");
                sb.Append(depth + 1);
                sb.Append(')');
            }
            sb.AppendLine("</h3>");
            sb.Append("      <h4 class=\"exception-type\">");
            sb.Append(HtmlEncode(inner.GetType().FullName ?? inner.GetType().Name));
            sb.AppendLine("</h4>");
            sb.Append("      <p class=\"exception-message\">");
            sb.Append(HtmlEncode(inner.Message));
            sb.AppendLine("</p>");
            if (inner.StackTrace is not null)
            {
                sb.AppendLine("      <pre class=\"stack-trace\">");
                sb.Append(FormatStackTrace(inner.StackTrace));
                sb.AppendLine("</pre>");
            }
            sb.AppendLine("    </div>");
            inner = inner.InnerException;
            depth++;
        }

        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Extracts the source file path and line number from the exception's stack trace.
    /// Returns <c>null</c> if no source information is available.
    /// </summary>
    /// <param name="exception">The exception to extract source location from.</param>
    /// <returns>A <see cref="SourceLocation"/> with file path and line number, or <c>null</c>.</returns>
    public static SourceLocation? ExtractSourceLocation(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var trace = new StackTrace(exception, fNeedFileInfo: true);
        var frames = trace.GetFrames();

        if (frames is null || frames.Length == 0)
        {
            return ExtractSourceLocationFromText(exception.StackTrace);
        }

        foreach (var frame in frames)
        {
            var fileName = frame.GetFileName();
            var lineNumber = frame.GetFileLineNumber();

            if (fileName is not null && fileName.Length > 0)
            {
                return new SourceLocation(fileName, lineNumber);
            }
        }

        return ExtractSourceLocationFromText(exception.StackTrace);
    }

    /// <summary>
    /// Extracts source location from stack trace text using regex as a fallback
    /// when <see cref="StackTrace"/> frames don't have file info.
    /// </summary>
    private static SourceLocation? ExtractSourceLocationFromText(string? stackTraceText)
    {
        if (stackTraceText is null || stackTraceText.Length == 0)
        {
            return null;
        }

        // Match patterns like " in C:\path\file.cs:line 42" or " in /path/file.cs:line 42"
        var match = Regex.Match(
            stackTraceText,
            @" in (.+?):line (\d+)",
            RegexOptions.None,
            TimeSpan.FromMilliseconds(100));

        if (match.Success)
        {
            var filePath = match.Groups[1].Value;
            if (int.TryParse(match.Groups[2].Value, out var lineNumber))
            {
                return new SourceLocation(filePath, lineNumber);
            }
        }

        return null;
    }

    /// <summary>
    /// Formats a stack trace string with HTML highlighting for file references.
    /// </summary>
    private static string FormatStackTrace(string stackTrace)
    {
        var encoded = HtmlEncode(stackTrace);
        // Highlight file paths and line numbers in the stack trace
        encoded = Regex.Replace(
            encoded,
            @"( in )(.+?)((:line \d+)|$)",
            "$1<span class=\"highlight-path\">$2</span>$3",
            RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(200));
        return encoded;
    }

    /// <summary>
    /// HTML-encodes a string for safe inclusion in HTML content.
    /// </summary>
    private static string HtmlEncode(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&#39;", StringComparison.Ordinal);

    /// <summary>
    /// Returns the inline CSS for the error overlay.
    /// </summary>
    private static string GetCss() =>
        """
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
          font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
          background: #1a1a2e;
          color: #eee;
          min-height: 100vh;
          display: flex;
          align-items: flex-start;
          justify-content: center;
          padding: 2rem 1rem;
        }
        .overlay {
          max-width: 960px;
          width: 100%;
          background: #16213e;
          border: 1px solid #e94560;
          border-radius: 12px;
          overflow: hidden;
          box-shadow: 0 4px 24px rgba(233, 69, 96, 0.3);
        }
        .header {
          background: #e94560;
          padding: 1.5rem 2rem;
          display: flex;
          align-items: center;
          gap: 1rem;
        }
        .badge {
          background: #fff;
          color: #e94560;
          font-weight: 700;
          font-size: 0.75rem;
          padding: 0.25rem 0.75rem;
          border-radius: 4px;
          letter-spacing: 0.05em;
        }
        .header h1 {
          font-size: 1.25rem;
          font-weight: 600;
          color: #fff;
        }
        .section {
          padding: 1.5rem 2rem;
          border-top: 1px solid #233554;
        }
        .section h3, .section h4 {
          color: #a3b8d4;
          font-size: 0.85rem;
          font-weight: 500;
          text-transform: uppercase;
          letter-spacing: 0.05em;
          margin-bottom: 0.75rem;
        }
        .exception-type {
          color: #ff6b6b;
          font-family: 'Cascadia Code', 'Fira Code', 'JetBrains Mono', monospace;
          font-size: 1.1rem;
          font-weight: 600;
          margin-bottom: 0.5rem;
        }
        .exception-message {
          color: #eee;
          font-size: 1rem;
          line-height: 1.6;
          word-break: break-word;
        }
        .source-location {
          background: #0f3460;
          border-radius: 8px;
          padding: 1rem 1.5rem;
          display: flex;
          align-items: center;
          gap: 1rem;
          flex-wrap: wrap;
        }
        .file-path {
          color: #53c0f5;
          font-family: 'Cascadia Code', 'Fira Code', 'JetBrains Mono', monospace;
          font-size: 0.95rem;
          word-break: break-all;
        }
        .line-number {
          color: #ffd93d;
          font-family: 'Cascadia Code', 'Fira Code', 'JetBrains Mono', monospace;
          font-size: 0.95rem;
          font-weight: 600;
        }
        .stack-trace {
          background: #0d1b2a;
          border-radius: 8px;
          padding: 1.25rem;
          font-family: 'Cascadia Code', 'Fira Code', 'JetBrains Mono', monospace;
          font-size: 0.85rem;
          line-height: 1.7;
          overflow-x: auto;
          color: #8899aa;
          white-space: pre-wrap;
          word-break: break-word;
        }
        .stack-trace .highlight-path {
          color: #53c0f5;
        }
        .inner-exception {
          background: #111b2e;
        }
        .inner-exception .exception-type {
          font-size: 0.95rem;
        }
        """;
}

/// <summary>
/// Represents a source code location extracted from an exception stack trace.
/// </summary>
/// <param name="FilePath">The source file path.</param>
/// <param name="LineNumber">The line number in the source file. Zero if not available.</param>
public sealed record SourceLocation(string FilePath, int LineNumber);
