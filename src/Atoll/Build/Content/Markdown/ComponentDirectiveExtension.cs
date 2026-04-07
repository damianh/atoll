using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers;

namespace Atoll.Build.Content.Markdown;

/// <summary>
/// Markdig extension that enables <c>:::</c> component directive syntax in Markdown content.
/// </summary>
/// <remarks>
/// <para>
/// When added to the Markdig pipeline, this extension enables:
/// </para>
/// <list type="bullet">
/// <item><c>UseCustomContainers()</c> — parses <c>:::name</c> / <c>:::</c> blocks.</item>
/// <item><c>UseGenericAttributes()</c> — parses <c>{key=value}</c> prop syntax.</item>
/// <item>A custom <see cref="ComponentDirectiveRenderer"/> — intercepts recognized directive
/// names and emits placeholder comments, collecting <see cref="ComponentReference"/> data.</item>
/// </list>
/// <para>
/// After rendering, retrieve the collected references via <see cref="CollectedReferences"/>.
/// The list is populated during the call to <c>document.ToHtml(pipeline)</c>.
/// </para>
/// <para>
/// Usage:
/// <code>
/// var extension = new ComponentDirectiveExtension(componentMap);
/// var builder = new MarkdownPipelineBuilder();
/// extension.Setup(builder);  // or add to builder.Extensions
/// var pipeline = builder.Build();
/// var html = document.ToHtml(pipeline);
/// var references = extension.CollectedReferences;
/// </code>
/// </para>
/// </remarks>
internal sealed class ComponentDirectiveExtension : IMarkdownExtension
{
    private readonly ComponentMap _componentMap;
    private readonly List<ComponentReference> _collected = [];

    /// <summary>
    /// Initializes a new <see cref="ComponentDirectiveExtension"/> with the specified component map.
    /// </summary>
    /// <param name="componentMap">The registry mapping directive names to component types.</param>
    internal ComponentDirectiveExtension(ComponentMap componentMap)
    {
        _componentMap = componentMap;
    }

    /// <summary>
    /// Gets the component references collected during the most recent render pass.
    /// This list is populated when <c>document.ToHtml(pipeline)</c> is called.
    /// </summary>
    internal IReadOnlyList<ComponentReference> CollectedReferences => _collected;

    /// <inheritdoc />
    void IMarkdownExtension.Setup(MarkdownPipelineBuilder pipeline)
    {
        // CustomContainers and GenericAttributes are registered by MarkdownRenderer
        // before this extension is added to the pipeline. Nothing to do here.
    }

    /// <inheritdoc />
    void IMarkdownExtension.Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is not HtmlRenderer htmlRenderer)
        {
            return;
        }

        // Reset collected references for each render pass.
        _collected.Clear();

        // Replace the default HtmlCustomContainerRenderer with our component-aware version.
        var existing = htmlRenderer.ObjectRenderers.FindExact<HtmlCustomContainerRenderer>();
        if (existing is not null)
        {
            htmlRenderer.ObjectRenderers.Remove(existing);
        }

        htmlRenderer.ObjectRenderers.Insert(0, new ComponentDirectiveRenderer(_componentMap, _collected));
    }
}
