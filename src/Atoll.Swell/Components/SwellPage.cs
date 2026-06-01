using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Mermaid;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Swell.Layouts;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Components;

/// <summary>
/// The main Swell page component. Parses a Swell Markdown file, renders each slide's
/// Markdown body, wraps each in the appropriate slide layout component, and passes the
/// assembled slides to <see cref="SwellDeckLayout"/> for the final full-page output.
/// </summary>
/// <remarks>
/// Wire this component up to a route in your Atoll project:
/// <code>
/// app.MapGet("/", () => new PageRenderResult(renderer.RenderAsync&lt;SwellPage&gt;(props)));
/// </code>
/// The <see cref="MarkdownContent"/> property must be provided with the raw Markdown text
/// of the slide deck file.
/// </remarks>
public sealed class SwellPage : AtollComponent
{
    /// <summary>
    /// Gets or sets the raw Markdown content of the slide deck file. Required.
    /// Use <see cref="System.IO.File.ReadAllText(string)"/> or the Atoll content pipeline to load this.
    /// </summary>
    [Parameter(Required = true)]
    public string MarkdownContent { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var deck = SlideParser.Parse(MarkdownContent);
        var markdownOptions = BuildMarkdownOptions();

        var entries = new List<RenderedSlideEntry>(deck.Slides.Count);
        var slideContents = new List<(SlideData Slide, ContentComponent Content)>(deck.Slides.Count);

        foreach (var slide in deck.Slides)
        {
            ContentComponent content;

            // For two-cols layout, split content at ::right:: separator
            if (string.Equals(slide.Config.Layout, "two-cols", StringComparison.OrdinalIgnoreCase)
                && slide.Body.Contains("::right::"))
            {
                var parts = slide.Body.Split("::right::", 2, StringSplitOptions.None);
                var leftResult = MarkdownRenderer.Render(parts[0].Trim(), markdownOptions);
                var rightResult = MarkdownRenderer.Render(parts[1].Trim(), markdownOptions);

                content = ContentComponent.FromHtml(
                    $"<div class=\"swell-col-left\">{leftResult.Html}</div><div class=\"swell-col-right\">{rightResult.Html}</div>",
                    leftResult.Headings);
            }
            else
            {
                var result = MarkdownRenderer.Render(slide.Body, markdownOptions);
                content = result.Fragments is not null
                    ? ContentComponent.FromRenderedContent(
                        new RenderedContent(result.Html, result.Headings, result.Fragments, result.AllReferences))
                    : ContentComponent.FromHtml(result.Html, result.Headings);
            }

            slideContents.Add((slide, content));
            entries.Add(new RenderedSlideEntry(slide.Index, slide.Config, slide.Notes));
        }

        var deckConfig = deck.Config;
        var totalSlides = deck.Slides.Count;

        // Build a render fragment that emits all <section> slide elements.
        var allSlidesFragment = RenderFragment.FromAsync(async dest =>
        {
            foreach (var (slide, content) in slideContents)
            {
                var showSlideNumber = ResolveShowSlideNumber(
                    slide.Config.SlideNumber, deckConfig.SlideNumbers);

                var slideProps = new Dictionary<string, object?>
                {
                    [nameof(SlideLayoutBase.Config)] = slide.Config,
                    [nameof(SlideLayoutBase.SlideIndex)] = slide.Index + 1,
                    [nameof(SlideLayoutBase.TotalSlides)] = totalSlides,
                    [nameof(SlideLayoutBase.ShowSlideNumber)] = showSlideNumber,
                };

                // Build a slot that renders the slide content (resolving component directives).
                var slideContentFragment = RenderFragment.FromAsync(async slotDest =>
                {
                    var slotContext = new RenderContext(slotDest);
                    await content.RenderAsync(slotContext);
                });
                var slideSlots = SlotCollection.FromDefault(slideContentFragment);

                // Resolve layout type and create instance.
                var layoutType = SlideLayoutResolver.Resolve(slide.Config.Layout);
                var layoutInstance = (IAtollComponent)Activator.CreateInstance(layoutType)!;

                // Wrap in <section data-slide-index="N">
                var sectionOpen = $"<section class=\"swell-slide\" role=\"group\" aria-roledescription=\"slide\" aria-label=\"Slide {slide.Index + 1}\" data-slide-index=\"{slide.Index}\"";
                dest.Write(RenderChunk.Html(sectionOpen));

                if (slide.Config.Transition.HasValue)
                {
                    dest.Write(RenderChunk.Html($" data-transition=\"{slide.Config.Transition.Value.ToString().ToLowerInvariant()}\""));
                }

                dest.Write(RenderChunk.Html(">"));

                await ComponentRenderer.RenderComponentAsync(layoutInstance, dest, slideProps, slideSlots);

                dest.Write(RenderChunk.Html("</section>"));
            }
        });

        var deckProps = new Dictionary<string, object?>
        {
            [nameof(SwellDeckLayout.Config)] = deckConfig,
            [nameof(SwellDeckLayout.Slides)] = (IReadOnlyList<RenderedSlideEntry>)entries,
        };

        var deckSlots = SlotCollection.FromDefault(allSlidesFragment);

        await ComponentRenderer.RenderComponentAsync<SwellDeckLayout>(
            context.Destination, deckProps, deckSlots);
    }

    private static MarkdownOptions BuildMarkdownOptions() =>
        new()
        {
            EnableTables = true,
            EnableAutoLinks = true,
            EnableTaskLists = true,
            EnableEmphasisExtras = true,
            EnableAutoIdentifiers = true,
            Extensions = [new MermaidExtension()],
            Components = new ComponentMap()
                .Add<Click>("Click"),
        };

    private static bool ResolveShowSlideNumber(bool? perSlideOverride, bool deckDefault) =>
        perSlideOverride ?? deckDefault;
}
