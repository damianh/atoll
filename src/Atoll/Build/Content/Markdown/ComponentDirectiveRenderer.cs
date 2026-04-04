using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Custom Markdig HTML renderer for <see cref="CustomContainer"/> blocks that represent
/// Atoll component directives (e.g., <c>:::counter{initialCount=5}</c>).
/// </summary>
/// <remarks>
/// <para>
/// When a <c>:::</c> directive is encountered whose name is registered in the
/// <see cref="ComponentMap"/>, this renderer:
/// </para>
/// <list type="number">
/// <item>Renders the container's child blocks to a separate HTML string (for the default slot).</item>
/// <item>Records a <see cref="ComponentReference"/> in the shared collection.</item>
/// <item>Emits an HTML placeholder comment (<c>&lt;!--atoll:N--&gt;</c>) into the main output.</item>
/// </list>
/// <para>
/// Unrecognized directive names fall back to the default <see cref="HtmlCustomContainerRenderer"/>
/// behavior (renders as a <c>&lt;div class="..."&gt;</c>).
/// </para>
/// </remarks>
internal sealed class ComponentDirectiveRenderer : HtmlObjectRenderer<CustomContainer>
{
    private readonly ComponentMap _componentMap;
    private readonly List<ComponentReference> _collected;
    private readonly HtmlCustomContainerRenderer _fallback = new();

    /// <summary>
    /// Initializes a new <see cref="ComponentDirectiveRenderer"/>.
    /// </summary>
    /// <param name="componentMap">The registry of directive name → component type.</param>
    /// <param name="collected">The shared list where collected component references are appended.</param>
    internal ComponentDirectiveRenderer(ComponentMap componentMap, List<ComponentReference> collected)
    {
        _componentMap = componentMap;
        _collected = collected;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CustomContainer block)
    {
        var name = block.Info?.Trim() ?? string.Empty;

        if (!_componentMap.TryResolve(name, out var componentType) || componentType is null)
        {
            // Not a registered component — fall back to default rendering.
            _fallback.Write(renderer, block);
            return;
        }

        // Parse props from generic attributes (e.g., {key=value key2="quoted"}).
        var props = ExtractProps(block);

        // Render child blocks to HTML for the default slot.
        var childHtml = RenderChildren(renderer, block);

        // Store the reference and emit a placeholder.
        var index = _collected.Count;
        _collected.Add(new ComponentReference(componentType, props, childHtml));

        renderer.Write($"<!--atoll:{index}-->");
    }

    private static IReadOnlyDictionary<string, object?> ExtractProps(CustomContainer block)
    {
        var attributes = block.TryGetAttributes();
        if (attributes?.Properties is not { Count: > 0 } properties)
        {
            return EmptyProps;
        }

        var dict = new Dictionary<string, object?>(properties.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var kv in properties)
        {
            dict[kv.Key] = kv.Value;
        }

        return dict;
    }

    private static string? RenderChildren(HtmlRenderer parentRenderer, CustomContainer block)
    {
        // If there are no child blocks, return null (no default slot).
        if (!block.Any())
        {
            return null;
        }

        // Use a separate writer+renderer to render just the child blocks.
        var writer = new StringWriter();
        var childRenderer = new HtmlRenderer(writer);

        // Copy relevant renderer settings so child content renders consistently.
        childRenderer.ImplicitParagraph = false;

        foreach (var child in block)
        {
            childRenderer.Render(child);
        }

        childRenderer.Writer.Flush();
        var html = writer.ToString();
        return string.IsNullOrEmpty(html) ? null : html;
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyProps =
        new Dictionary<string, object?>();
}
