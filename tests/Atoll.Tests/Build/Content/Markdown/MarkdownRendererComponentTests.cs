using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Markdown;

/// <summary>
/// Tests for fragment-based <see cref="MarkdownRenderer.Render"/> output:
/// fragment types and ordering, backward compatibility with plain markdown.
/// </summary>
public sealed class MarkdownRendererComponentTests
{
    // ── Backward compatibility (no components) ──

    [Fact]
    public void ShouldProduceNullFragmentsForPlainMarkdown()
    {
        var result = MarkdownRenderer.Render("# Hello\n\nWorld");

        result.Fragments.ShouldBeNull();
        result.Html.ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldProduceNullFragmentsWhenComponentsOptionIsNull()
    {
        var options = new MarkdownOptions(); // Components = null

        var result = MarkdownRenderer.Render("# Hello\n\nWorld", options);

        result.Fragments.ShouldBeNull();
        result.Html.ShouldContain("<h1");
    }

    [Fact]
    public void ShouldProduceNullFragmentsWhenNoDirectivesPresent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<TinyComponent>("comp"),
        };

        var result = MarkdownRenderer.Render("# Hello\n\nNo directives here.", options);

        // No directives → no fragments
        result.Fragments.ShouldBeNull();
        result.Html.ShouldContain("<h1");
        result.Html.ShouldContain("No directives here.");
    }

    // ── Fragment splitting with one component ──

    [Fact]
    public void ShouldProduceFragmentsWhenComponentDirectivePresent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<TinyComponent>("comp"),
        };

        var result = MarkdownRenderer.Render(":::comp\n:::", options);

        result.Fragments.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldProduceCorrectFragmentCountForDocumentWithOneDirective()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<TinyComponent>("comp"),
        };

        // Text before + component + text after → multiple fragments
        var result = MarkdownRenderer.Render(
            "Before\n\n:::comp\n:::\n\nAfter",
            options);

        result.Fragments.ShouldNotBeNull();
        result.Fragments!.Count.ShouldBeGreaterThanOrEqualTo(1);
        result.Fragments.OfType<ComponentContentFragment>().Count().ShouldBe(1);
    }

    [Fact]
    public void ShouldInterleaveHtmlAndComponentFragments()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<TinyComponent>("comp"),
        };

        var result = MarkdownRenderer.Render(
            "Before\n\n:::comp\n:::\n\nAfter",
            options);

        result.Fragments.ShouldNotBeNull();

        // The sequence should contain at least one html fragment and one component fragment
        result.Fragments!.OfType<HtmlContentFragment>().ShouldNotBeEmpty();
        result.Fragments!.OfType<ComponentContentFragment>().ShouldNotBeEmpty();
    }

    [Fact]
    public void ShouldOrderFragmentsCorrectly()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<TinyComponent>("comp"),
        };

        var result = MarkdownRenderer.Render(
            "Before\n\n:::comp\n:::\n\nAfter",
            options);

        result.Fragments.ShouldNotBeNull();
        var fragments = result.Fragments!.ToList();

        // First non-empty fragment should be HTML, then component
        var firstHtml = fragments.OfType<HtmlContentFragment>().First();
        var compFragment = fragments.OfType<ComponentContentFragment>().First();

        var htmlIndex = fragments.IndexOf(firstHtml);
        var compIndex = fragments.IndexOf(compFragment);

        htmlIndex.ShouldBeLessThan(compIndex);
    }

    // ── Fragment splitting with multiple components ──

    [Fact]
    public void ShouldProduceCorrectFragmentsForTwoDirectives()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<TinyComponent>("comp")
                .Add<TinyComponent2>("comp2"),
        };

        var result = MarkdownRenderer.Render(
            ":::comp\n:::\n\n:::comp2\n:::",
            options);

        result.Fragments.ShouldNotBeNull();
        result.Fragments!.OfType<ComponentContentFragment>().Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldPreserveComponentTypesInFragmentOrder()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<TinyComponent>("first")
                .Add<TinyComponent2>("second"),
        };

        var result = MarkdownRenderer.Render(
            ":::first\n:::\n\n:::second\n:::",
            options);

        result.Fragments.ShouldNotBeNull();
        var compFragments = result.Fragments!
            .OfType<ComponentContentFragment>()
            .ToList();

        compFragments[0].Reference.ComponentType.ShouldBe(typeof(TinyComponent));
        compFragments[1].Reference.ComponentType.ShouldBe(typeof(TinyComponent2));
    }

    // ── Heading extraction still works ──

    [Fact]
    public void ShouldExtractHeadingsAlongsideFragments()
    {
        var options = new MarkdownOptions
        {
            EnableAutoIdentifiers = true,
            Components = new ComponentMap().Add<TinyComponent>("comp"),
        };

        var result = MarkdownRenderer.Render(
            "# My Heading\n\n:::comp\n:::",
            options);

        result.Headings.Count.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("My Heading");
        result.Fragments.ShouldNotBeNull();
    }

    // ── Fixtures ──

    private sealed class TinyComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<span>tiny</span>");
            return Task.CompletedTask;
        }
    }

    private sealed class TinyComponent2 : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<span>tiny2</span>");
            return Task.CompletedTask;
        }
    }
}
