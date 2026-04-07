using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;

namespace Atoll.Build.Tests.Content.Markdown;

/// <summary>
/// Integration tests for documents containing both <c>&lt;Tag&gt;</c> syntax and
/// <c>:::</c> directive syntax. Exercises the full <see cref="MarkdownRenderer.Render()"/>
/// pipeline including preprocessor integration, placeholder renumbering, and reference merging.
/// </summary>
public sealed class MixedComponentSyntaxTests
{
    // ── Tag-only documents ──

    [Fact]
    public void ShouldProduceFragmentsForTagOnlyDocument()
    {
        // Type name "Aside" → alias "Aside" → matches <Aside ...>
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<Aside>("aside"),
        };

        var result = MarkdownRenderer.Render("<Aside Type=\"tip\">tip content</Aside>", options);

        result.Fragments.ShouldNotBeNull();
        var compFragments = result.Fragments.OfType<ComponentContentFragment>().ToList();
        compFragments.Count.ShouldBe(1);
        compFragments[0].Reference.ComponentType.ShouldBe(typeof(Aside));
    }

    [Fact]
    public void ShouldPlaceTagFragmentAtIndexZero()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<Aside>("aside"),
        };

        var result = MarkdownRenderer.Render("<Aside>content</Aside>", options);

        result.Html.ShouldContain("<!--atoll:0-->");
        result.Fragments.ShouldNotBeNull();
        result.Fragments.OfType<ComponentContentFragment>().Single()
            .Reference.ComponentType.ShouldBe(typeof(Aside));
    }

    // ── Directive-only documents (regression) ──

    [Fact]
    public void ShouldNotBreakDirectiveOnlyDocument()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<Counter>("counter"),
        };

        var result = MarkdownRenderer.Render(":::counter{initialCount=5}\n:::", options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments.OfType<ComponentContentFragment>().Single();
        compFragment.Reference.ComponentType.ShouldBe(typeof(Counter));
        compFragment.Reference.Props["initialCount"].ShouldBe("5");
    }

    // ── Mixed documents: tag first ──

    [Fact]
    public void ShouldProduceCorrectFragmentsWhenTagPrecedesDirective()
    {
        // Tag preprocessor runs first (regardless of document order), so the tag always
        // occupies the lower index(es), directive(s) the higher.
        // In this document the <Aside> tag appears before :::counter, and the preprocessor
        // also runs first → index 0 = Aside, index 1 = Counter.
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<Aside>("aside")
                .Add<Counter>("counter"),
        };
        var markdown = "<Aside Type=\"tip\">tip</Aside>\n\n:::counter\n:::";

        var result = MarkdownRenderer.Render(markdown, options);

        result.Fragments.ShouldNotBeNull();
        var compFragments = result.Fragments.OfType<ComponentContentFragment>().ToList();
        compFragments.Count.ShouldBe(2);
        compFragments[0].Reference.ComponentType.ShouldBe(typeof(Aside));
        compFragments[1].Reference.ComponentType.ShouldBe(typeof(Counter));
    }

    [Fact]
    public void ShouldPlaceholderIndicesBeCorrectWhenTagPrecedesDirective()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<Aside>("aside")
                .Add<Counter>("counter"),
        };
        var markdown = "<Aside>tip</Aside>\n\n:::counter\n:::";

        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldContain("<!--atoll:0-->");
        result.Html.ShouldContain("<!--atoll:1-->");
    }

    // ── Mixed documents: directive first in document ──

    [Fact]
    public void ShouldProduceCorrectFragmentsWhenDirectivePrecedesTagInDocument()
    {
        // When :::counter appears before <Aside> in the document, the preprocessor still
        // assigns the tag (Aside) the lower index. However, BuildFragments orders fragments
        // by their position in the rendered HTML — so Counter (first in document) appears
        // first in the fragment list, even though its placeholder index is higher.
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<Aside>("aside")
                .Add<Counter>("counter"),
        };
        var markdown = ":::counter\n:::\n\n<Aside Type=\"tip\">tip</Aside>";

        var result = MarkdownRenderer.Render(markdown, options);

        result.Fragments.ShouldNotBeNull();
        var compFragments = result.Fragments.OfType<ComponentContentFragment>().ToList();
        compFragments.Count.ShouldBe(2);
        // Document order: :::counter comes first → Counter at fragment index 0
        compFragments[0].Reference.ComponentType.ShouldBe(typeof(Counter));
        // <Aside> comes second → Aside at fragment index 1
        compFragments[1].Reference.ComponentType.ShouldBe(typeof(Aside));
    }

    // ── Mixed documents: interleaved HTML ──

    [Fact]
    public void ShouldPreserveInterleavedHtmlFragments()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<Aside>("aside"),
        };
        var markdown = "Before\n\n<Aside>content</Aside>\n\nAfter";

        var result = MarkdownRenderer.Render(markdown, options);

        result.Fragments.ShouldNotBeNull();
        var htmlFragments = result.Fragments.OfType<HtmlContentFragment>().ToList();
        htmlFragments.ShouldNotBeEmpty();
        var allHtml = string.Concat(htmlFragments.Select(f => f.Html));
        allHtml.ShouldContain("Before");
        allHtml.ShouldContain("After");
    }

    // ── Self-closing tag in full pipeline ──

    [Fact]
    public void ShouldHandleSelfClosingTagInFullPipeline()
    {
        // Type name "LinkCard" → alias "LinkCard" → matches <LinkCard ... />
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<LinkCard>("link-card"),
        };

        var result = MarkdownRenderer.Render(@"<LinkCard Title=""Docs"" Href=""/docs"" />", options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments.OfType<ComponentContentFragment>().Single();
        compFragment.Reference.ComponentType.ShouldBe(typeof(LinkCard));
        compFragment.Reference.Props["Title"].ShouldBe("Docs");
        compFragment.Reference.Props["Href"].ShouldBe("/docs");
        compFragment.Reference.ChildHtml.ShouldBeNull();
    }

    // ── No components → null Fragments (fast path) ──

    [Fact]
    public void ShouldReturnNullFragmentsWhenNoComponentsPresent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<Aside>("aside"),
        };

        var result = MarkdownRenderer.Render("Plain **markdown** text", options);

        result.Fragments.ShouldBeNull();
        result.Html.ShouldContain("<strong>markdown</strong>");
    }

    // ── Fixtures ──
    // Component class names match the expected PascalCase tag names so that
    // ComponentMap's auto-alias (typeof(T).Name) resolves them correctly.

    private sealed class Aside : IAtollComponent
    {
        [Parameter] public string? Type { get; set; }

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<aside class=\"{Type}\"></aside>");
            return Task.CompletedTask;
        }
    }

    private sealed class Counter : IAtollComponent
    {
        [Parameter] public int InitialCount { get; set; }

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<span>{InitialCount}</span>");
            return Task.CompletedTask;
        }
    }

    private sealed class LinkCard : IAtollComponent
    {
        [Parameter] public string? Title { get; set; }
        [Parameter] public string? Href { get; set; }

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<a href=\"{Href}\">{Title}</a>");
            return Task.CompletedTask;
        }
    }
}
