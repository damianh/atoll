using Atoll.Components;
using Atoll.Head;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Slots;

namespace Atoll.Rendering;

/// <summary>
/// Orchestrates rendering of a full HTML page from a page component. Handles
/// DOCTYPE auto-insertion, head content collection and injection, and script placement.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's page rendering pipeline. The renderer:
/// </para>
/// <list type="number">
/// <item>Renders the page component to a buffer</item>
/// <item>Collects head content from the <see cref="HeadManager"/> and <see cref="InstructionProcessor"/></item>
/// <item>Injects head content before <c>&lt;/head&gt;</c> if present</item>
/// <item>Injects script instructions before <c>&lt;/body&gt;</c> if present</item>
/// <item>Prepends <c>&lt;!DOCTYPE html&gt;</c> if not already present</item>
/// </list>
/// </remarks>
public sealed class PageRenderer
{
    private const string Doctype = "<!DOCTYPE html>";
    private const string HeadCloseTag = "</head>";
    private const string BodyCloseTag = "</body>";

    private readonly HeadManager _headManager = new();
    private readonly InstructionProcessor _instructionProcessor = new();

    /// <summary>
    /// Gets the <see cref="HeadManager"/> for collecting head elements during rendering.
    /// </summary>
    public HeadManager HeadManager => _headManager;

    /// <summary>
    /// Gets the <see cref="InstructionProcessor"/> for collecting render instructions during rendering.
    /// </summary>
    public InstructionProcessor InstructionProcessor => _instructionProcessor;

    /// <summary>
    /// Renders a page component and returns the complete HTML page.
    /// </summary>
    /// <typeparam name="TComponent">The page component type.</typeparam>
    /// <returns>A <see cref="PageRenderResult"/> containing the final HTML.</returns>
    public async Task<PageRenderResult> RenderPageAsync<TComponent>()
        where TComponent : IAtollComponent, new()
    {
        var bodyDest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TComponent>(bodyDest);

        return BuildResult(bodyDest.GetOutput());
    }

    /// <summary>
    /// Renders a page component with the specified props and returns the complete HTML page.
    /// </summary>
    /// <typeparam name="TComponent">The page component type.</typeparam>
    /// <param name="props">The props dictionary for the page component.</param>
    /// <returns>A <see cref="PageRenderResult"/> containing the final HTML.</returns>
    public async Task<PageRenderResult> RenderPageAsync<TComponent>(
        IReadOnlyDictionary<string, object?> props)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(props);

        var bodyDest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TComponent>(bodyDest, props);

        return BuildResult(bodyDest.GetOutput());
    }

    /// <summary>
    /// Renders a page component with the specified props and slots, and returns the complete HTML page.
    /// </summary>
    /// <typeparam name="TComponent">The page component type.</typeparam>
    /// <param name="props">The props dictionary for the page component.</param>
    /// <param name="slots">The slot collection for the page component.</param>
    /// <returns>A <see cref="PageRenderResult"/> containing the final HTML.</returns>
    public async Task<PageRenderResult> RenderPageAsync<TComponent>(
        IReadOnlyDictionary<string, object?> props,
        SlotCollection slots)
        where TComponent : IAtollComponent, new()
    {
        ArgumentNullException.ThrowIfNull(props);
        ArgumentNullException.ThrowIfNull(slots);

        var bodyDest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TComponent>(bodyDest, props, slots);

        return BuildResult(bodyDest.GetOutput());
    }

    /// <summary>
    /// Renders a pre-existing page component instance and returns the complete HTML page.
    /// </summary>
    /// <param name="component">The page component instance.</param>
    /// <returns>A <see cref="PageRenderResult"/> containing the final HTML.</returns>
    public async Task<PageRenderResult> RenderPageAsync(IAtollComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var bodyDest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(component, bodyDest);

        return BuildResult(bodyDest.GetOutput());
    }

    /// <summary>
    /// Renders a pre-existing page component instance with the specified props
    /// and returns the complete HTML page.
    /// </summary>
    /// <param name="component">The page component instance.</param>
    /// <param name="props">The props dictionary for the page component.</param>
    /// <returns>A <see cref="PageRenderResult"/> containing the final HTML.</returns>
    public async Task<PageRenderResult> RenderPageAsync(
        IAtollComponent component,
        IReadOnlyDictionary<string, object?> props)
    {
        ArgumentNullException.ThrowIfNull(component);
        ArgumentNullException.ThrowIfNull(props);

        var bodyDest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync(component, bodyDest, props);

        return BuildResult(bodyDest.GetOutput());
    }

    /// <summary>
    /// Renders a functional component delegate and returns the complete HTML page.
    /// </summary>
    /// <param name="componentDelegate">The page component delegate.</param>
    /// <returns>A <see cref="PageRenderResult"/> containing the final HTML.</returns>
    public async Task<PageRenderResult> RenderPageAsync(ComponentDelegate componentDelegate)
    {
        ArgumentNullException.ThrowIfNull(componentDelegate);

        var bodyDest = new StringRenderDestination();
        await ComponentRenderer.RenderDelegateAsync(componentDelegate, bodyDest);

        return BuildResult(bodyDest.GetOutput());
    }

    private PageRenderResult BuildResult(string componentOutput)
    {
        EmitHydrationScripts(componentOutput);
        var html = InjectHeadContent(componentOutput);
        html = InjectScripts(html);
        html = EnsureDoctype(html);
        return new PageRenderResult(html);
    }

    private string InjectHeadContent(string html)
    {
        // Collect head content from both HeadManager and HeadInstruction instructions
        var headDest = new StringRenderDestination();

        // Render HeadManager elements
        _headManager.RenderAllHeadContentAsync(headDest).AsTask().GetAwaiter().GetResult();

        // Render HeadInstruction instructions from the processor
        _instructionProcessor.RenderAllAsync<HeadInstruction>(headDest).AsTask().GetAwaiter().GetResult();

        var headContent = headDest.GetOutput();
        if (headContent.Length == 0)
        {
            return html;
        }

        // Find </head> and inject before it
        var headCloseIndex = html.IndexOf(HeadCloseTag, StringComparison.OrdinalIgnoreCase);
        if (headCloseIndex >= 0)
        {
            return string.Concat(
                html.AsSpan(0, headCloseIndex),
                headContent.AsSpan(),
                html.AsSpan(headCloseIndex));
        }

        // No </head> found — append to end
        return html + headContent;
    }

    private string InjectScripts(string html)
    {
        // Render ScriptInstruction instructions from the processor
        var scriptDest = new StringRenderDestination();
        _instructionProcessor.RenderAllAsync<ScriptInstruction>(scriptDest).AsTask().GetAwaiter().GetResult();

        var scriptContent = scriptDest.GetOutput();
        if (scriptContent.Length == 0)
        {
            return html;
        }

        // Find </body> and inject before it
        var bodyCloseIndex = html.IndexOf(BodyCloseTag, StringComparison.OrdinalIgnoreCase);
        if (bodyCloseIndex >= 0)
        {
            return string.Concat(
                html.AsSpan(0, bodyCloseIndex),
                scriptContent.AsSpan(),
                html.AsSpan(bodyCloseIndex));
        }

        // No </body> found — append to end
        return html + scriptContent;
    }

    private static string EnsureDoctype(string html)
    {
        if (html.StartsWith(Doctype, StringComparison.OrdinalIgnoreCase))
        {
            return html;
        }

        return Doctype + "\n" + html;
    }

    /// <summary>
    /// Scans rendered HTML for <c>&lt;atoll-island&gt;</c> elements and, when found,
    /// adds the island bootstrap and directives scripts to the <see cref="InstructionProcessor"/>
    /// so they are injected before <c>&lt;/body&gt;</c>.
    /// </summary>
    private void EmitHydrationScripts(string html)
    {
        if (!IslandDirectiveScanner.ContainsIslands(html))
        {
            return;
        }

        _instructionProcessor.Add(ScriptInstruction.Module(IslandDirectiveScanner.IslandScriptUrl));
        _instructionProcessor.Add(ScriptInstruction.Module(IslandDirectiveScanner.DirectivesScriptUrl));
    }
}
