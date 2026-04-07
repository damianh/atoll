using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Markdown;

/// <summary>
/// Tests for <see cref="ComponentTagPreprocessor"/> — placeholder emission, attribute parsing,
/// child content rendering, nesting, and fenced code block exclusion.
/// </summary>
public sealed class ComponentTagPreprocessorTests
{
    // ── Self-closing tags ──

    [Fact]
    public void ShouldEmitPlaceholderForSelfClosingTag()
    {
        // Type name "LinkCard" → alias "LinkCard" → matches <LinkCard ... />
        var map = new ComponentMap().Add<LinkCard>("link-card");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (markdown, references) = preprocessor.Process(@"<LinkCard Title=""Config"" Href=""./config"" />");

        markdown.ShouldBe("<!--atoll-tag:0-->");
        references.Count.ShouldBe(1);
        references[0].ComponentType.ShouldBe(typeof(LinkCard));
    }

    [Fact]
    public void ShouldParseSelfClosingTagAttributes()
    {
        var map = new ComponentMap().Add<LinkCard>("link-card");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process(@"<LinkCard Title=""Config"" Href=""./config"" />");

        references[0].Props.ShouldContainKey("Title");
        references[0].Props["Title"].ShouldBe("Config");
        references[0].Props.ShouldContainKey("Href");
        references[0].Props["Href"].ShouldBe("./config");
        references[0].ChildHtml.ShouldBeNull();
    }

    [Fact]
    public void ShouldEmitPlaceholderForSelfClosingTagWithNoAttributes()
    {
        // Type name "Aside" → alias "Aside" → matches <Aside />
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (markdown, references) = preprocessor.Process("<Aside />");

        markdown.ShouldBe("<!--atoll-tag:0-->");
        references.Count.ShouldBe(1);
        references[0].ChildHtml.ShouldBeNull();
    }

    // ── Tag with children ──

    [Fact]
    public void ShouldCapturePlainTextChildrenAsChildHtml()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process("<Aside Type=\"tip\">Hello world</Aside>");

        references.Count.ShouldBe(1);
        references[0].ChildHtml.ShouldNotBeNull();
        references[0].ChildHtml!.ShouldContain("Hello world");
    }

    [Fact]
    public void ShouldRenderMarkdownChildrenToHtml()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process("<Aside Type=\"tip\">**bold** content</Aside>");

        references[0].ChildHtml.ShouldNotBeNull();
        references[0].ChildHtml!.ShouldContain("<strong>bold</strong>");
    }

    [Fact]
    public void ShouldProduceNullChildHtmlForEmptyTag()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process("<Aside></Aside>");

        references.Count.ShouldBe(1);
        references[0].ChildHtml.ShouldBeNull();
    }

    [Fact]
    public void ShouldCaptureMultilineChildContent()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());
        var input = "<Aside>\nLine one\n\nLine two\n</Aside>";

        var (_, references) = preprocessor.Process(input);

        references[0].ChildHtml.ShouldNotBeNull();
        references[0].ChildHtml!.ShouldContain("Line one");
        references[0].ChildHtml!.ShouldContain("Line two");
    }

    // ── Attribute parsing ──

    [Fact]
    public void ShouldParseDoubleQuotedAttributes()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process(@"<Aside Type=""tip"">content</Aside>");

        references[0].Props["Type"].ShouldBe("tip");
    }

    [Fact]
    public void ShouldParseSingleQuotedAttributes()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process("<Aside Type='warning'>content</Aside>");

        references[0].Props["Type"].ShouldBe("warning");
    }

    [Fact]
    public void ShouldParseUnquotedAttributes()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process("<Aside Type=note>content</Aside>");

        references[0].Props["Type"].ShouldBe("note");
    }

    [Fact]
    public void ShouldParseBooleanAttributes()
    {
        // Type name "CardGrid" → alias "CardGrid" → matches <CardGrid ...>
        var map = new ComponentMap().Add<CardGrid>("card-grid");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (_, references) = preprocessor.Process("<CardGrid Stagger>content</CardGrid>");

        references[0].Props["Stagger"].ShouldBe("true");
    }

    // ── Nesting ──

    [Fact]
    public void ShouldProcessNestedTagsInsideOut()
    {
        var map = new ComponentMap()
            .Add<CardGrid>("card-grid")
            .Add<Card>("card");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (markdown, references) = preprocessor.Process("<CardGrid><Card Title=\"A\">content</Card></CardGrid>");

        // Inner Card at index 0, outer CardGrid at index 1
        references.Count.ShouldBe(2);
        references[0].ComponentType.ShouldBe(typeof(Card));
        references[1].ComponentType.ShouldBe(typeof(CardGrid));

        // Outer's ChildHtml contains the inner placeholder rendered through Markdown
        references[1].ChildHtml.ShouldNotBeNull();
        references[1].ChildHtml!.ShouldContain("<!--atoll-tag:0-->");

        // Overall markdown becomes the outer placeholder
        markdown.ShouldBe("<!--atoll-tag:1-->");
    }

    // ── Multiple sibling tags ──

    [Fact]
    public void ShouldProcessMultipleSiblingTags()
    {
        var map = new ComponentMap().Add<Card>("card");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());
        var input = "<Card Title=\"A\">one</Card>\n\n<Card Title=\"B\">two</Card>";

        var (markdown, references) = preprocessor.Process(input);

        references.Count.ShouldBe(2);
        references[0].Props["Title"].ShouldBe("A");
        references[1].Props["Title"].ShouldBe("B");
        markdown.ShouldContain("<!--atoll-tag:0-->");
        markdown.ShouldContain("<!--atoll-tag:1-->");
    }

    // ── Unregistered and standard HTML tags ──

    [Fact]
    public void ShouldLeaveUnregisteredPascalCaseTagUnchanged()
    {
        var map = new ComponentMap(); // nothing registered
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (markdown, references) = preprocessor.Process("<Unknown>stuff</Unknown>");

        markdown.ShouldBe("<Unknown>stuff</Unknown>");
        references.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldLeaveStandardLowercaseHtmlTagUnchanged()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());

        var (markdown, references) = preprocessor.Process("<div>content</div>");

        markdown.ShouldBe("<div>content</div>");
        references.Count.ShouldBe(0);
    }

    // ── Fenced code blocks ──

    [Fact]
    public void ShouldNotProcessTagInsideBacktickFencedCodeBlock()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());
        var input = "```\n<Aside>content</Aside>\n```";

        var (markdown, references) = preprocessor.Process(input);

        // The tag inside the fence must be left untouched
        markdown.ShouldContain("<Aside>content</Aside>");
        references.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldNotProcessTagInsideTildeFencedCodeBlock()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());
        var input = "~~~\n<Aside>content</Aside>\n~~~";

        var (markdown, references) = preprocessor.Process(input);

        markdown.ShouldContain("<Aside>content</Aside>");
        references.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldProcessTagOutsideFencedCodeBlock()
    {
        var map = new ComponentMap().Add<Aside>("aside");
        var preprocessor = new ComponentTagPreprocessor(map, new MarkdownOptions());
        var input = "```\ncode here\n```\n\n<Aside>tip content</Aside>";

        var (_, references) = preprocessor.Process(input);

        references.Count.ShouldBe(1);
        references[0].ComponentType.ShouldBe(typeof(Aside));
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

    private sealed class Card : IAtollComponent
    {
        [Parameter] public string? Title { get; set; }

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<div class=\"card\">{Title}</div>");
            return Task.CompletedTask;
        }
    }

    private sealed class CardGrid : IAtollComponent
    {
        [Parameter] public bool Stagger { get; set; }

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"card-grid\"></div>");
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
