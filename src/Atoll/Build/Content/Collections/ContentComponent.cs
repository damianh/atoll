using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using System.Text.RegularExpressions;

namespace Atoll.Build.Content.Collections;

/// <summary>
/// An <see cref="IAtollComponent"/> that renders pre-rendered Markdown content (HTML) into the
/// component tree. This enables content entries to be used inside layouts and composed
/// with other Atoll components.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>Content</c> component returned by
/// <c>render(entry)</c>. The component outputs the pre-rendered HTML directly
/// to the render destination. It also exposes the heading metadata for
/// table-of-contents generation.
/// </para>
/// <para>
/// When the Markdown content contains embedded component directives (<c>:::</c> syntax),
/// the component iterates the fragment sequence, rendering HTML chunks directly and
/// instantiating Atoll components (or islands) at directive sites.
/// </para>
/// <para>
/// Usage:
/// <code>
/// var entry = query.GetEntry&lt;BlogPost&gt;("blog", "my-post");
/// var rendered = query.Render(entry);
/// var component = ContentComponent.FromRenderedContent(rendered);
/// await componentRenderer.RenderComponentAsync(component, context);
/// </code>
/// </para>
/// </remarks>
public sealed class ContentComponent : IAtollComponent
{
    // Matches <!--atoll-tag:N--> placeholders embedded in component ChildHtml strings.
    // These are the original (pre-renumbering) placeholders left by the tag preprocessor
    // inside nested component content. Index N maps directly into AllReferences.
    private static readonly Regex ChildPlaceholderPattern =
        new(@"<!--atoll-tag:(\d+)-->", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private readonly string _html;
    private readonly IReadOnlyList<ContentFragment>? _fragments;
    private readonly IReadOnlyList<ComponentReference> _allReferences;

    private ContentComponent(
        string html,
        IReadOnlyList<MarkdownHeading> headings,
        IReadOnlyList<ContentFragment>? fragments,
        IReadOnlyList<ComponentReference> allReferences)
    {
        _html = html;
        Headings = headings;
        _fragments = fragments;
        _allReferences = allReferences;
    }

    /// <summary>
    /// Gets the headings extracted from the rendered Markdown content.
    /// </summary>
    public IReadOnlyList<MarkdownHeading> Headings { get; }

    /// <summary>
    /// Creates a <see cref="ContentComponent"/> from a <see cref="RenderedContent"/> instance.
    /// </summary>
    /// <param name="renderedContent">The rendered content.</param>
    /// <returns>A new <see cref="ContentComponent"/> that renders the content's HTML.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="renderedContent"/> is <c>null</c>.</exception>
    public static ContentComponent FromRenderedContent(RenderedContent renderedContent)
    {
        ArgumentNullException.ThrowIfNull(renderedContent);
        return new ContentComponent(
            renderedContent.Html,
            renderedContent.Headings,
            renderedContent.Fragments,
            renderedContent.AllReferences);
    }

    /// <summary>
    /// Creates a <see cref="ContentComponent"/> from raw HTML and heading metadata.
    /// </summary>
    /// <param name="html">The HTML content to render.</param>
    /// <param name="headings">The heading metadata.</param>
    /// <returns>A new <see cref="ContentComponent"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="html"/> or <paramref name="headings"/> is <c>null</c>.
    /// </exception>
    public static ContentComponent FromHtml(string html, IReadOnlyList<MarkdownHeading> headings)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(headings);
        return new ContentComponent(html, headings, fragments: null, allReferences: []);
    }

    /// <summary>
    /// Creates a <see cref="ContentComponent"/> from raw HTML with no heading metadata.
    /// </summary>
    /// <param name="html">The HTML content to render.</param>
    /// <returns>A new <see cref="ContentComponent"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="html"/> is <c>null</c>.</exception>
    public static ContentComponent FromHtml(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        return new ContentComponent(html, [], fragments: null, allReferences: []);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from this component's HTML content.
    /// Useful when you need to inject content into a slot or compose with other fragments.
    /// </summary>
    /// <returns>A <see cref="RenderFragment"/> containing the content's HTML.</returns>
    public RenderFragment ToRenderFragment()
    {
        return RenderFragment.FromHtml(_html);
    }

    /// <inheritdoc />
    public async Task RenderAsync(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Fast path: no component directives — write HTML directly.
        if (_fragments is null)
        {
            context.WriteHtml(_html);
            return;
        }

        // Fragment path: iterate HTML chunks and component references in order.
        foreach (var fragment in _fragments)
        {
            switch (fragment)
            {
                case HtmlContentFragment htmlFragment:
                    context.WriteHtml(htmlFragment.Html);
                    break;

                case ComponentContentFragment componentFragment:
                    await RenderComponentFragmentAsync(
                        context.Destination,
                        componentFragment.Reference,
                        _allReferences);
                    break;
            }
        }
    }

    private static async Task RenderComponentFragmentAsync(
        IRenderDestination destination,
        ComponentReference reference,
        IReadOnlyList<ComponentReference> allReferences)
    {
        var componentType = reference.ComponentType;

        // Build props dictionary.
        var props = reference.Props;

        // Build slot collection from child HTML, if any.
        // If the child HTML contains <!--atoll-tag:N--> placeholders from nested component
        // tags, build a composite RenderFragment that interleaves HTML chunks with recursive
        // component renders rather than emitting the placeholder comments verbatim.
        var slots = reference.ChildHtml is not null
            ? SlotCollection.FromDefault(BuildSlotFragment(reference.ChildHtml, allReferences))
            : SlotCollection.Empty;

        // Check if this component is an island (has a client directive attribute).
        var directiveInfo = DirectiveExtractor.GetDirective(componentType);

        if (directiveInfo is not null &&
            typeof(IClientComponent).IsAssignableFrom(componentType))
        {
            // Island path: wrap in <atoll-island>.
            var instance = (IClientComponent)Activator.CreateInstance(componentType)!;

            var metadata = new IslandMetadata(instance.ClientModuleUrl, directiveInfo.DirectiveType)
            {
                ComponentExport = instance.ClientExportName,
                DirectiveValue = directiveInfo.Value,
                DisplayName = componentType.Name,
            };

            await IslandRenderer.RenderIslandAsync(destination, metadata, componentType, props, slots);
        }
        else
        {
            // Regular component path.
            var instance = (IAtollComponent)Activator.CreateInstance(componentType)!;
            await ComponentRenderer.RenderComponentAsync(instance, destination, props, slots);
        }
    }

    /// <summary>
    /// Builds a <see cref="RenderFragment"/> from a child HTML string that may contain
    /// <c>&lt;!--atoll-tag:N--&gt;</c> placeholders for nested components.
    /// Placeholders are resolved by looking up the corresponding reference in
    /// <paramref name="allReferences"/> and rendering it recursively.
    /// </summary>
    private static RenderFragment BuildSlotFragment(
        string childHtml,
        IReadOnlyList<ComponentReference> allReferences)
    {
        // Fast path: no nested placeholders — wrap as plain HTML.
        if (!childHtml.Contains("<!--atoll-tag:", StringComparison.Ordinal))
        {
            return RenderFragment.FromHtml(childHtml);
        }

        // Split on <!--atoll-tag:N--> placeholders.
        // Regex.Split with a capture group interleaves: [html, index, html, index, html, ...]
        var parts = ChildPlaceholderPattern.Split(childHtml);
        var fragmentParts = new List<RenderFragment>(parts.Length);

        for (var i = 0; i < parts.Length; i++)
        {
            if (i % 2 == 0)
            {
                // Even positions are HTML chunks.
                if (parts[i].Length > 0)
                {
                    fragmentParts.Add(RenderFragment.FromHtml(parts[i]));
                }
            }
            else
            {
                // Odd positions are captured index values.
                if (int.TryParse(parts[i], out var index) && index < allReferences.Count)
                {
                    var nestedRef = allReferences[index];
                    // Capture for closure.
                    var capturedRef = nestedRef;
                    var capturedAllRefs = allReferences;
                    fragmentParts.Add(RenderFragment.FromAsync(async dest =>
                        await RenderComponentFragmentAsync(dest, capturedRef, capturedAllRefs)));
                }
            }
        }

        return fragmentParts.Count == 0
            ? RenderFragment.Empty
            : RenderFragment.Concat([.. fragmentParts]);
    }
}
