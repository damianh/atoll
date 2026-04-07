using Atoll.Build.Content.Markdown;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Markdown;

public sealed class MarkdownRendererTests
{
    [Fact]
    public void ShouldRenderBasicParagraph()
    {
        var result = MarkdownRenderer.Render("Hello World");

        result.Html.ShouldContain("<p>Hello World</p>");
    }

    [Fact]
    public void ShouldRenderHeadings()
    {
        var result = MarkdownRenderer.Render("# Title\n\n## Subtitle\n\n### Section");

        result.Html.ShouldContain("<h1");
        result.Html.ShouldContain("Title");
        result.Html.ShouldContain("<h2");
        result.Html.ShouldContain("Subtitle");
        result.Html.ShouldContain("<h3");
        result.Html.ShouldContain("Section");
    }

    [Fact]
    public void ShouldRenderFencedCodeBlock()
    {
        var markdown = "```csharp\nvar x = 42;\n```";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<pre>");
        result.Html.ShouldContain("<code");
        result.Html.ShouldContain("language-csharp");
        result.Html.ShouldContain("var x = 42;");
    }

    [Fact]
    public void ShouldRenderPipeTable()
    {
        var markdown = "| Name | Age |\n|------|-----|\n| Alice | 30 |\n| Bob | 25 |";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<table>");
        result.Html.ShouldContain("<th>Name</th>");
        result.Html.ShouldContain("<td>Alice</td>");
    }

    [Fact]
    public void ShouldNotRenderTableWhenDisabled()
    {
        var markdown = "| Name | Age |\n|------|-----|\n| Alice | 30 |";
        var options = new MarkdownOptions { EnableTables = false };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldNotContain("<table>");
    }

    [Fact]
    public void ShouldRenderTaskLists()
    {
        var markdown = "- [x] Done\n- [ ] Todo";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("type=\"checkbox\"");
        result.Html.ShouldContain("checked");
        result.Html.ShouldContain("Done");
        result.Html.ShouldContain("Todo");
    }

    [Fact]
    public void ShouldRenderAutoLinks()
    {
        var markdown = "Visit https://example.com for more.";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<a href=\"https://example.com\"");
    }

    [Fact]
    public void ShouldRenderStrikethrough()
    {
        var markdown = "This is ~~deleted~~ text.";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<del>deleted</del>");
    }

    [Fact]
    public void ShouldRenderBoldAndItalic()
    {
        var markdown = "**bold** and *italic*";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<strong>bold</strong>");
        result.Html.ShouldContain("<em>italic</em>");
    }

    [Fact]
    public void ShouldRenderUnorderedList()
    {
        var markdown = "- Item 1\n- Item 2\n- Item 3";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<ul>");
        result.Html.ShouldContain("<li>Item 1</li>");
    }

    [Fact]
    public void ShouldRenderOrderedList()
    {
        var markdown = "1. First\n2. Second\n3. Third";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<ol>");
        result.Html.ShouldContain("<li>First</li>");
    }

    [Fact]
    public void ShouldRenderBlockquote()
    {
        var markdown = "> This is a quote";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<blockquote>");
        result.Html.ShouldContain("This is a quote");
    }

    [Fact]
    public void ShouldRenderLinks()
    {
        var markdown = "[Atoll](https://atoll.dev)";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<a href=\"https://atoll.dev\"");
        result.Html.ShouldContain("Atoll</a>");
    }

    [Fact]
    public void ShouldRenderImages()
    {
        var markdown = "![Alt text](image.png)";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<img");
        result.Html.ShouldContain("src=\"image.png\"");
        result.Html.ShouldContain("alt=\"Alt text\"");
    }

    [Fact]
    public void ShouldRenderInlineCode()
    {
        var markdown = "Use `var x = 42;` in your code.";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<code>var x = 42;</code>");
    }

    [Fact]
    public void ShouldRenderHorizontalRule()
    {
        var markdown = "Before\n\n---\n\nAfter";
        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<hr");
    }

    // --- Heading extraction ---

    [Fact]
    public void ShouldExtractHeadingsWithDepth()
    {
        var markdown = "# Title\n\n## Subtitle\n\n### Section";
        var result = MarkdownRenderer.Render(markdown);

        result.Headings.Count.ShouldBe(3);
        result.Headings[0].Depth.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("Title");
        result.Headings[1].Depth.ShouldBe(2);
        result.Headings[1].Text.ShouldBe("Subtitle");
        result.Headings[2].Depth.ShouldBe(3);
        result.Headings[2].Text.ShouldBe("Section");
    }

    [Fact]
    public void ShouldExtractHeadingIdsWhenAutoIdentifiersEnabled()
    {
        var markdown = "# Hello World\n\n## Getting Started";
        var options = new MarkdownOptions { EnableAutoIdentifiers = true };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Headings[0].Id.ShouldNotBeNull();
        result.Headings[1].Id.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldNotExtractHeadingIdsWhenAutoIdentifiersDisabled()
    {
        var markdown = "# Hello World";
        var options = new MarkdownOptions { EnableAutoIdentifiers = false };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Headings[0].Id.ShouldBeNull();
    }

    [Fact]
    public void ShouldReturnEmptyHeadingsForNoHeadingContent()
    {
        var result = MarkdownRenderer.Render("Just a paragraph.");

        result.Headings.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldThrowOnNullMarkdown()
    {
        Should.Throw<ArgumentNullException>(() => MarkdownRenderer.Render(null!));
    }

    [Fact]
    public void ShouldThrowOnNullOptions()
    {
        Should.Throw<ArgumentNullException>(() => MarkdownRenderer.Render("# Hello", null!));
    }

    [Fact]
    public void ShouldRenderEmptyMarkdownToEmptyHtml()
    {
        var result = MarkdownRenderer.Render("");

        result.Html.ShouldBeEmpty();
        result.Headings.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderComplexDocument()
    {
        var markdown = """
            # Blog Post Title

            ## Introduction

            Welcome to this **amazing** blog post. You can find more at https://example.com.

            ## Features

            | Feature | Status |
            |---------|--------|
            | Tables  | ✅     |
            | Tasks   | ✅     |

            ### Task List

            - [x] Write content
            - [ ] Review content
            - [ ] Publish

            ## Code Example

            ```csharp
            public sealed class HelloWorld
            {
                public void Greet() => Console.WriteLine("Hello!");
            }
            ```

            ## Conclusion

            That's all folks!
            """;

        var result = MarkdownRenderer.Render(markdown);

        result.Html.ShouldContain("<h1");
        result.Html.ShouldContain("<table>");
        result.Html.ShouldContain("type=\"checkbox\"");
        result.Html.ShouldContain("<pre>");
        result.Headings.Count.ShouldBe(6);
    }

    [Fact]
    public void ShouldRenderFootnotesWhenEnabled()
    {
        var markdown = "Text with a footnote[^1].\n\n[^1]: This is the footnote.";
        var options = new MarkdownOptions { EnableFootnotes = true };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldContain("footnote");
    }

    [Fact]
    public void ShouldNotRenderAutoLinksWhenDisabled()
    {
        var markdown = "Visit https://example.com for more.";
        var options = new MarkdownOptions { EnableAutoLinks = false };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldNotContain("<a href=");
    }

    [Fact]
    public void ShouldNotRenderTaskListsWhenDisabled()
    {
        var markdown = "- [x] Done\n- [ ] Todo";
        var options = new MarkdownOptions { EnableTaskLists = false };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldNotContain("type=\"checkbox\"");
    }

    [Fact]
    public void ShouldNotRenderStrikethroughWhenEmphasisExtrasDisabled()
    {
        var markdown = "This is ~~deleted~~ text.";
        var options = new MarkdownOptions { EnableEmphasisExtras = false };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldNotContain("<del>");
    }

    [Fact]
    public void BuildPipelineShouldThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => MarkdownRenderer.BuildPipeline(null!));
    }

    [Fact]
    public void ShouldRenderAutoIdentifiersInHtml()
    {
        var markdown = "# Hello World";
        var options = new MarkdownOptions { EnableAutoIdentifiers = true };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldContain("id=");
    }

    [Fact]
    public void ShouldNotRenderAutoIdentifiersWhenDisabled()
    {
        var markdown = "# Hello World";
        var options = new MarkdownOptions { EnableAutoIdentifiers = false };
        var result = MarkdownRenderer.Render(markdown, options);

        result.Html.ShouldNotContain("id=");
    }

    [Fact]
    public void ShouldExtractInlineCodeInHeadingText()
    {
        var markdown = "## Grid view — `ArticleGrid` + `ArticleCard`";
        var result = MarkdownRenderer.Render(markdown);

        result.Headings.Count.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("Grid view");
    }

    [Fact]
    public void ShouldExtractSingleInlineCodeInHeadingText()
    {
        var markdown = "### List view — `ArticleList`";
        var result = MarkdownRenderer.Render(markdown);

        result.Headings.Count.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("List view");
    }

    [Fact]
    public void ShouldTruncateHeadingAtSpacedDash()
    {
        var markdown = "## Configuration - advanced options";
        var result = MarkdownRenderer.Render(markdown);

        result.Headings.Count.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("Configuration");
    }

    [Fact]
    public void ShouldNotTruncateHeadingWithoutDash()
    {
        var markdown = "## Pagination strategy";
        var result = MarkdownRenderer.Render(markdown);

        result.Headings.Count.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("Pagination strategy");
    }

    [Fact]
    public void ShouldNotTruncateHeadingWithHyphenatedWord()
    {
        var markdown = "## Auto-generated content";
        var result = MarkdownRenderer.Render(markdown);

        result.Headings.Count.ShouldBe(1);
        result.Headings[0].Text.ShouldBe("Auto-generated content");
    }
}
