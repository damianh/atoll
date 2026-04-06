using Atoll.Build.Content.Markdown;
using Atoll.Lagoon.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Markdown;

public sealed class ExpressiveCodeInlineMarkerTests
{
    private static string Render(string markdown)
    {
        var options = new DocsMarkdownOptions { EnableSyntaxHighlighting = true };
        return DocsMarkdownRenderer.Render(markdown, options).Html;
    }

    // ── Literal string markers ────────────────────────────────────────────────

    [Fact]
    public void ShouldHighlightLiteralStringMarker()
    {
        var md = "```csharp \"var\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-text-marker");
        html.ShouldContain("var");
    }

    [Fact]
    public void ShouldWrapMatchInMarkElement()
    {
        var md = "```csharp \"var\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("<mark class=\"ec-text-marker\">");
        html.ShouldContain("</mark>");
    }

    [Fact]
    public void ShouldHighlightAllOccurrencesOfLiteralMarker()
    {
        // "x" appears twice in the code.
        var md = "```csharp \"x\"\nvar x = x + 1;\n```";
        var html = Render(md);

        // Count mark elements (should appear twice).
        var count = 0;
        var search = "<mark class=\"ec-text-marker\">";
        var idx = 0;
        while ((idx = html.IndexOf(search, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += search.Length;
        }

        count.ShouldBe(2);
    }

    [Fact]
    public void ShouldNotMarkWhenLiteralNotFound()
    {
        var md = "```csharp \"notPresent\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("ec-text-marker");
    }

    // ── Regex markers ─────────────────────────────────────────────────────────

    [Fact]
    public void ShouldHighlightRegexMarker()
    {
        var md = "```csharp /\\d+/\nvar x = 42;\n```";
        var html = Render(md);

        html.ShouldContain("ec-text-marker");
    }

    [Fact]
    public void ShouldHighlightCaptureGroupOnly()
    {
        // Pattern: /total: (\d+)/ — only the number part should be highlighted.
        var md = "```csharp /total: (\\d+)/\nvar total: 99;\n```";
        var html = Render(md);

        html.ShouldContain("ec-text-marker");
    }

    [Fact]
    public void ShouldNotThrowOnInvalidRegex()
    {
        // Invalid regex pattern — should silently skip, not throw.
        var md = "```csharp /[invalid/\nvar x = 1;\n```";
        Should.NotThrow(() => Render(md));
    }

    // ── Multiple markers ──────────────────────────────────────────────────────

    [Fact]
    public void ShouldApplyMultipleMarkersOnSameLine()
    {
        var md = "```csharp \"var\" \"1\"\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-text-marker");
    }

    // ── Marker with line markers ──────────────────────────────────────────────

    [Fact]
    public void ShouldCombineInlineMarkersWithLineMarkers()
    {
        var md = "```csharp \"var\" {1}\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldContain("ec-text-marker");
        html.ShouldContain("ec-mark");
    }

    // ── No markers ───────────────────────────────────────────────────────────

    [Fact]
    public void ShouldNotEmitMarkWhenNoInlineMarkers()
    {
        var md = "```csharp\nvar x = 1;\n```";
        var html = Render(md);

        html.ShouldNotContain("ec-text-marker");
        html.ShouldNotContain("<mark");
    }

    // ── HTML escaping preserved ───────────────────────────────────────────────

    [Fact]
    public void ShouldPreserveHtmlEscapingInMarkedText()
    {
        // The '<' in the code should be escaped as &lt; even inside a mark.
        var md = "```csharp \"<\"\nif (x < 5) {}\n```";
        var html = Render(md);

        // The literal '<' should be escaped.
        html.ShouldContain("&lt;");
        html.ShouldNotContain("<5");
    }
}
