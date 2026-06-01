using Atoll.Components;
using Atoll.Routing;
using Atoll.Swell.Components;

namespace Docs.Pages;

/// <summary>
/// Renders an example Swell slide deck at /swell/example-slides.
/// This page is embedded as an iframe via SwellDeck on the Swell overview docs page.
/// </summary>
[PageRoute("/swell/example-slides")]
public sealed class SwellExamplePage : AtollComponent, IAtollPage
{
    private const string ExampleMarkdown = """
        ---
        title: Swell Example Deck
        aspectRatio: 16/9
        transition: fade
        slideNumbers: true
        ---

        ---
        layout: cover
        ---

        # Swell Presentations

        Write slides in Markdown, present anywhere.

        <!-- This is an example deck showcasing Swell's built-in layouts. -->

        ---

        ## What is Swell?

        A presentation plugin for **Atoll** that turns Markdown into slide decks.

        - No JavaScript frameworks needed
        - Keyboard navigation & presenter mode
        - Export to PDF, PPTX, and ODP

        ---
        layout: center
        ---

        ## Centred Layout

        Content is centred both vertically and horizontally.

        Perfect for impactful statements.

        ---
        layout: two-cols
        ---

        ## Two Columns

        The left column contains explanatory text.

        Use `::right::` to split content between columns.

        ::right::

        ## Code Example

        ```csharp
        [PageRoute("/slides")]
        public class SlidesPage
            : AtollComponent, IAtollPage
        {
            // Render your deck here
        }
        ```

        ---
        layout: section
        ---

        ## Section Divider

        Group slides into logical sections.

        ---

        ## Click Reveal

        Content appears progressively:

        :::Click
        - First point appears on click
        :::

        :::Click
        - Second point appears on next click
        :::

        :::Click
        - Third point appears last
        :::

        ---
        layout: end
        ---

        # Thank You!

        Press **o** for overview · **p** for presenter mode · **d** to draw
        """;

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var props = new Dictionary<string, object?>
        {
            [nameof(SwellPage.MarkdownContent)] = ExampleMarkdown,
        };

        await ComponentRenderer.RenderComponentAsync<SwellPage>(
            context.Destination, props);
    }
}
