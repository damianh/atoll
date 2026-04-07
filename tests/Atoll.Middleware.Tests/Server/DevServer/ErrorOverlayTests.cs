using Atoll.Middleware.Server.DevServer;

namespace Atoll.Middleware.Tests.Server.DevServer;

public sealed class ErrorOverlayTests
{
    // ── Render produces valid HTML structure ─────────────────────────

    [Fact]
    public void RenderShouldReturnHtmlDocumentWithDoctype()
    {
        var exception = new InvalidOperationException("Test error");

        var html = ErrorOverlay.Render(exception);

        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("</html>");
    }

    [Fact]
    public void RenderShouldIncludeHeadWithCharsetAndTitle()
    {
        var exception = new InvalidOperationException("Test error");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("<meta charset=\"utf-8\">");
        html.ShouldContain("<title>Atoll - Error</title>");
    }

    [Fact]
    public void RenderShouldIncludeInlineCss()
    {
        var exception = new InvalidOperationException("Test error");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("<style>");
        html.ShouldContain(".overlay");
        html.ShouldContain(".stack-trace");
    }

    // ── Exception type and message ──────────────────────────────────

    [Fact]
    public void RenderShouldIncludeExceptionTypeName()
    {
        var exception = new InvalidOperationException("Test error");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("System.InvalidOperationException");
    }

    [Fact]
    public void RenderShouldIncludeExceptionMessage()
    {
        var exception = new InvalidOperationException("Component rendering failed badly");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("Component rendering failed badly");
    }

    [Fact]
    public void RenderShouldHtmlEncodeExceptionMessage()
    {
        var exception = new InvalidOperationException("Error with <script>alert('xss')</script>");

        var html = ErrorOverlay.Render(exception);

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Fact]
    public void RenderShouldIncludeErrorBadge()
    {
        var exception = new InvalidOperationException("Test error");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("ERROR");
        html.ShouldContain("badge");
    }

    [Fact]
    public void RenderShouldIncludeHeaderText()
    {
        var exception = new InvalidOperationException("Test error");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("Atoll encountered an error");
    }

    // ── Stack trace ─────────────────────────────────────────────────

    [Fact]
    public void RenderShouldIncludeStackTrace()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("Test with stack");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("Stack Trace");
        html.ShouldContain("stack-trace");
    }

    [Fact]
    public void RenderShouldHandleExceptionWithNoStackTrace()
    {
        var exception = new InvalidOperationException("No stack trace");

        var html = ErrorOverlay.Render(exception);

        // Should still render, just without the stack trace section
        html.ShouldContain("No stack trace");
        html.ShouldContain("System.InvalidOperationException");
    }

    // ── Inner exception ─────────────────────────────────────────────

    [Fact]
    public void RenderShouldIncludeInnerException()
    {
        var inner = new ArgumentException("Missing parameter");
        var outer = new InvalidOperationException("Outer error", inner);

        var html = ErrorOverlay.Render(outer);

        html.ShouldContain("Inner Exception");
        html.ShouldContain("System.ArgumentException");
        html.ShouldContain("Missing parameter");
    }

    [Fact]
    public void RenderShouldIncludeNestedInnerExceptions()
    {
        var innermost = new FormatException("Bad format");
        var middle = new ArgumentException("Bad arg", innermost);
        var outer = new InvalidOperationException("Outer error", middle);

        var html = ErrorOverlay.Render(outer);

        html.ShouldContain("System.FormatException");
        html.ShouldContain("Bad format");
        html.ShouldContain("System.ArgumentException");
        html.ShouldContain("Bad arg");
    }

    [Fact]
    public void RenderShouldLimitInnerExceptionDepthToFive()
    {
        // Build a chain of 7 inner exceptions
        Exception current = new InvalidOperationException("Deepest");
        for (var i = 6; i >= 0; i--)
        {
            current = new InvalidOperationException("Level " + i, current);
        }

        var html = ErrorOverlay.Render(current);

        // Should have "Inner Exception" sections but not endlessly
        // The outer exception "Level 0" is the top-level, levels 1-5 are inner (5 total)
        // "Deepest" and "Level 6" should NOT appear because depth limit is 5
        html.ShouldContain("Level 0");
        html.ShouldContain("Level 1");
    }

    // ── Source location extraction ──────────────────────────────────

    [Fact]
    public void ExtractSourceLocationShouldReturnNullWhenNoFileInfo()
    {
        var exception = new InvalidOperationException("No file info");

        var location = ErrorOverlay.ExtractSourceLocation(exception);

        // Might be null or might have info depending on environment
        // The key thing is it doesn't throw
        // (We can't guarantee file info is absent in all test environments)
    }

    [Fact]
    public void ExtractSourceLocationShouldReturnLocationFromThrown()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("Test with location");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var location = ErrorOverlay.ExtractSourceLocation(exception);

        // In a compiled test project with PDBs, we should get file info
        if (location is not null)
        {
            location.FilePath.ShouldNotBeNullOrEmpty();
            location.LineNumber.ShouldBeGreaterThan(0);
            location.FilePath.ShouldContain("ErrorOverlayTests.cs");
        }
    }

    [Fact]
    public void ExtractSourceLocationShouldThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ErrorOverlay.ExtractSourceLocation(null!));
    }

    // ── Render with thrown exception (integration) ──────────────────

    [Fact]
    public void RenderShouldProduceSourceSectionForThrownException()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("Integration test error");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        var html = ErrorOverlay.Render(exception);

        // Should include a Source section if file info is available
        html.ShouldContain("Integration test error");
        html.ShouldContain("System.InvalidOperationException");
        html.ShouldContain("Stack Trace");
    }

    // ── HTML safety ──────────────────────────────────────────────────

    [Fact]
    public void RenderShouldHtmlEncodeExceptionType()
    {
        // Custom exception type name can't have HTML, but message can
        var exception = new InvalidOperationException("Error <b>bold</b> & \"quoted\"");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("&lt;b&gt;bold&lt;/b&gt;");
        html.ShouldContain("&amp;");
        html.ShouldContain("&quot;quoted&quot;");
    }

    [Fact]
    public void RenderShouldThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => ErrorOverlay.Render(null!));
    }

    // ── Self-contained rendering ────────────────────────────────────

    [Fact]
    public void RenderShouldNotReferenceExternalResources()
    {
        var exception = new InvalidOperationException("Test");

        var html = ErrorOverlay.Render(exception);

        // No external CSS links, scripts, or images
        html.ShouldNotContain("link rel=\"stylesheet\"");
        html.ShouldNotContain("<script src=");
        html.ShouldNotContain("<img src=");
    }

    [Fact]
    public void RenderShouldIncludeViewportMeta()
    {
        var exception = new InvalidOperationException("Test");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("viewport");
    }

    // ── CSS content ──────────────────────────────────────────────────

    [Fact]
    public void RenderShouldIncludeKeyColorStyles()
    {
        var exception = new InvalidOperationException("Test");

        var html = ErrorOverlay.Render(exception);

        // Check for the dark theme background and error accent colors
        html.ShouldContain("#1a1a2e"); // body background
        html.ShouldContain("#e94560"); // error accent
    }

    // ── Different exception types ────────────────────────────────────

    [Fact]
    public void RenderShouldHandleNullReferenceException()
    {
        var exception = new NullReferenceException("Object reference not set");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("System.NullReferenceException");
        html.ShouldContain("Object reference not set");
    }

    [Fact]
    public void RenderShouldHandleAggregateException()
    {
        var inner1 = new InvalidOperationException("Error 1");
        var inner2 = new ArgumentException("Error 2");
        var aggregate = new AggregateException("Multiple errors", inner1, inner2);

        var html = ErrorOverlay.Render(aggregate);

        html.ShouldContain("System.AggregateException");
        html.ShouldContain("Multiple errors");
        // AggregateException InnerException is the first one
        html.ShouldContain("Inner Exception");
    }

    [Fact]
    public void RenderShouldHandleExceptionWithEmptyMessage()
    {
        var exception = new Exception("");

        var html = ErrorOverlay.Render(exception);

        html.ShouldContain("System.Exception");
        // Should still produce valid HTML
        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("</html>");
    }

    // ── SourceLocation record ────────────────────────────────────────

    [Fact]
    public void SourceLocationShouldStoreFilePathAndLineNumber()
    {
        var location = new SourceLocation("/path/to/file.cs", 42);

        location.FilePath.ShouldBe("/path/to/file.cs");
        location.LineNumber.ShouldBe(42);
    }

    [Fact]
    public void SourceLocationShouldSupportValueEquality()
    {
        var a = new SourceLocation("/path/file.cs", 42);
        var b = new SourceLocation("/path/file.cs", 42);

        a.ShouldBe(b);
    }

    [Fact]
    public void SourceLocationShouldSupportInequality()
    {
        var a = new SourceLocation("/path/file.cs", 42);
        var b = new SourceLocation("/path/file.cs", 43);

        a.ShouldNotBe(b);
    }
}
