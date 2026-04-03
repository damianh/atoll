using Atoll.Rendering;

namespace Atoll.Instructions;

/// <summary>
/// A render instruction that represents a client directive for island hydration.
/// Directives determine when and how a component is hydrated on the client side.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>client:*</c> directives. Each directive
/// type corresponds to a different hydration strategy:
/// </para>
/// <list type="bullet">
/// <item><see cref="ClientDirectiveType.Load"/> — Hydrate immediately on page load</item>
/// <item><see cref="ClientDirectiveType.Idle"/> — Hydrate when the browser is idle</item>
/// <item><see cref="ClientDirectiveType.Visible"/> — Hydrate when the component enters the viewport</item>
/// <item><see cref="ClientDirectiveType.Media"/> — Hydrate when a CSS media query matches</item>
/// </list>
/// <para>
/// Directive instructions emit the hydration script required for their directive type.
/// The <see cref="InstructionProcessor"/> ensures each directive handler script is only
/// emitted once per page, regardless of how many islands use that directive.
/// </para>
/// </remarks>
public sealed class DirectiveInstruction : RenderInstruction
{
    /// <summary>
    /// Initializes a new <see cref="DirectiveInstruction"/> with the specified directive type.
    /// </summary>
    /// <param name="directiveType">The client directive type.</param>
    public DirectiveInstruction(ClientDirectiveType directiveType)
        : this(directiveType, null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="DirectiveInstruction"/> with the specified directive type
    /// and media query value.
    /// </summary>
    /// <param name="directiveType">The client directive type.</param>
    /// <param name="value">
    /// The directive value (e.g., the CSS media query for <see cref="ClientDirectiveType.Media"/>).
    /// </param>
    public DirectiveInstruction(ClientDirectiveType directiveType, string? value)
        : base($"directive:{directiveType}:{value ?? ""}")
    {
        DirectiveType = directiveType;
        Value = value;
    }

    /// <summary>
    /// Gets the client directive type.
    /// </summary>
    public ClientDirectiveType DirectiveType { get; }

    /// <summary>
    /// Gets the directive value, if any. For <see cref="ClientDirectiveType.Media"/>,
    /// this is the CSS media query string.
    /// </summary>
    public string? Value { get; }

    /// <inheritdoc />
    public override ValueTask RenderAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        // Directive instructions render a comment marker indicating the directive type.
        // The actual hydration script is managed by the island renderer and
        // HydrationTracker (Phase 3). For now, emit a marker that the page
        // renderer can use to track which directives are active.
        var marker = Value is not null
            ? $"<!--[atoll:directive:{DirectiveType}:{Value}]-->"
            : $"<!--[atoll:directive:{DirectiveType}]-->";

        destination.Write(RenderChunk.Html(marker));
        return default;
    }
}
