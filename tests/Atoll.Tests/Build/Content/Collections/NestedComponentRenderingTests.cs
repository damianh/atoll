using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Build.Tests.Content.Collections;

/// <summary>
/// Integration tests proving that nested component tags (<c>&lt;Outer&gt;&lt;Inner&gt;</c>)
/// render correctly end-to-end through the full markdown → fragment → component pipeline.
///
/// These tests guard against the regression where inner component placeholders
/// (<c>&lt;!--atoll-tag:N--&gt;</c>) in a parent component's <c>ChildHtml</c>
/// were written verbatim to the output instead of being resolved to rendered components.
/// </summary>
public sealed class NestedComponentRenderingTests
{
    // ── Simple nesting: Outer wraps Inner ──

    [Fact]
    public async Task ShouldRenderInnerComponentInsideOuterSlot()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<OuterComponent>("outer")
                .Add<InnerComponent>("inner"),
        };

        var result = MarkdownRenderer.Render(
            "<Outer><Inner>hello</Inner></Outer>",
            options);

        var output = await RenderFragmentsAsync(result);

        output.ShouldContain("<div class=\"outer\">");
        // Markdig wraps inline content in <p> tags when rendered as a block.
        output.ShouldContain("<span class=\"inner\">");
        output.ShouldContain("hello");
        // No raw placeholder comments should appear in the output.
        output.ShouldNotContain("<!--atoll");
    }

    [Fact]
    public async Task ShouldRenderMultipleSiblingInnerComponentsInsideOuter()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<OuterComponent>("outer")
                .Add<InnerComponent>("inner"),
        };

        var result = MarkdownRenderer.Render(
            "<Outer><Inner>first</Inner><Inner>second</Inner></Outer>",
            options);

        var output = await RenderFragmentsAsync(result);

        output.ShouldContain("<div class=\"outer\">");
        // Markdig wraps inline content in <p> tags when rendered as a block.
        output.ShouldContain("<span class=\"inner\">");
        output.ShouldContain("first");
        output.ShouldContain("second");
        output.ShouldNotContain("<!--atoll");
    }

    [Fact]
    public async Task ShouldPreserveInnerMarkdownContent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<OuterComponent>("outer")
                .Add<InnerComponent>("inner"),
        };

        // Inner content has markdown (bold) that should be rendered.
        var result = MarkdownRenderer.Render(
            "<Outer><Inner>**bold text**</Inner></Outer>",
            options);

        var output = await RenderFragmentsAsync(result);

        output.ShouldContain("<strong>bold text</strong>");
        output.ShouldNotContain("<!--atoll");
    }

    [Fact]
    public async Task ShouldRenderOuterPropsAndInnerContent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<TitledOuter>("titled-outer")
                .Add<InnerComponent>("inner"),
        };

        var result = MarkdownRenderer.Render(
            "<TitledOuter Title=\"My Title\"><Inner>content</Inner></TitledOuter>",
            options);

        var output = await RenderFragmentsAsync(result);

        output.ShouldContain("data-title=\"My Title\"");
        output.ShouldContain("<span class=\"inner\">");
        output.ShouldContain("content");
        output.ShouldNotContain("<!--atoll");
    }

    [Fact]
    public async Task ShouldRenderTextBeforeAndAfterNestedComponent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<OuterComponent>("outer")
                .Add<InnerComponent>("inner"),
        };

        var result = MarkdownRenderer.Render(
            "Before\n\n<Outer><Inner>inside</Inner></Outer>\n\nAfter",
            options);

        var output = await RenderFragmentsAsync(result);

        output.ShouldContain("Before");
        output.ShouldContain("<div class=\"outer\">");
        output.ShouldContain("<span class=\"inner\">");
        output.ShouldContain("inside");
        output.ShouldContain("After");
        output.ShouldNotContain("<!--atoll");
    }

    // ── Helper: render MarkdownRenderResult fragments to a string ──

    private static async Task<string> RenderFragmentsAsync(MarkdownRenderResult result)
    {
        if (result.Fragments is null)
        {
            return result.Html;
        }

        var rendered = new RenderedContent(
            result.Html,
            result.Headings,
            result.Fragments,
            result.AllReferences);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        await component.RenderAsync(new RenderContext(dest));
        return dest.GetOutput();
    }

    // ── Component fixtures ──

    /// <summary>
    /// A simple wrapper component that renders its slot inside a div.
    /// </summary>
    private sealed class OuterComponent : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"outer\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    /// <summary>
    /// A wrapper component that also renders a Title prop as a data attribute.
    /// </summary>
    private sealed class TitledOuter : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = string.Empty;

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<div class=\"outer\" data-title=\"{Title}\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    /// <summary>
    /// A simple inner component that renders its slot inside a span.
    /// </summary>
    private sealed class InnerComponent : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<span class=\"inner\">");
            await RenderSlotAsync();
            WriteHtml("</span>");
        }
    }
}
