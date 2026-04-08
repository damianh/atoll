using System.IO;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// A Markdig extension that generates heading auto-identifiers with leading digits preserved.
/// <para>
/// This replaces the built-in <c>AutoIdentifierExtension</c> to work around a Markdig bug
/// where <see cref="LinkHelper.Urilize(string, bool, bool)"/> is called with
/// <c>keepOpeningDigits: false</c>, causing headings like "3rd Party Cookies" to produce
/// <c>id="rd-party-cookies"</c> instead of the expected <c>id="3rd-party-cookies"</c>.
/// </para>
/// </summary>
internal sealed class AutoIdentifierFixExtension : IMarkdownExtension
{
    private const string AutoIdentifierKey = "AutoIdentifierFix";

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        var headingBlockParser = pipeline.BlockParsers.Find<HeadingBlockParser>();
        if (headingBlockParser is not null)
        {
            headingBlockParser.Closed -= HeadingBlockParserClosed;
            headingBlockParser.Closed += HeadingBlockParserClosed;
        }

        var paragraphBlockParser = pipeline.BlockParsers.FindExact<ParagraphBlockParser>();
        if (paragraphBlockParser is not null)
        {
            paragraphBlockParser.Closed -= HeadingBlockParserClosed;
            paragraphBlockParser.Closed += HeadingBlockParserClosed;
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
    }

    private static void HeadingBlockParserClosed(BlockProcessor processor, Block block)
    {
        if (block is not HeadingBlock headingBlock)
        {
            return;
        }

        headingBlock.ProcessInlinesEnd += HeadingBlockProcessInlinesEnd;
    }

    private static void HeadingBlockProcessInlinesEnd(InlineProcessor processor, Inline? inline)
    {
        var identifiers = processor.Document.GetData(AutoIdentifierKey) as HashSet<string>;
        if (identifiers is null)
        {
            identifiers = new HashSet<string>();
            processor.Document.SetData(AutoIdentifierKey, identifiers);
        }

        var headingBlock = (HeadingBlock)processor.Block!;
        if (headingBlock.Inline is null)
        {
            return;
        }

        // If id is already set (e.g. via generic attributes), don't override it.
        var attributes = headingBlock.GetAttributes();
        if (attributes.Id is not null)
        {
            return;
        }

        // Strip links/formatting from the heading text using a temporary HtmlRenderer
        // configured to emit plain text only.
        var rawHeadingText = StripInlineFormatting(headingBlock.Inline);

        // The fix: pass keepOpeningDigits: true so "3rd Party Cookies" → "3rd-party-cookies".
        var headingText = LinkHelper.Urilize(rawHeadingText, allowOnlyAscii: true, keepOpeningDigits: true);

        var baseHeadingId = string.IsNullOrEmpty(headingText) ? "section" : headingText;

        // Handle collisions by appending -1, -2, etc.
        var headingId = baseHeadingId;
        if (!identifiers.Add(headingId))
        {
            var index = 0u;
            do
            {
                index++;
                headingId = $"{baseHeadingId}-{index}";
            }
            while (!identifiers.Add(headingId));
        }

        attributes.Id = headingId;
    }

    private static string StripInlineFormatting(ContainerInline containerInline)
    {
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer)
        {
            EnableHtmlForInline = false,
            EnableHtmlEscape = false,
        };
        renderer.Render(containerInline);
        writer.Flush();
        return writer.ToString();
    }
}
