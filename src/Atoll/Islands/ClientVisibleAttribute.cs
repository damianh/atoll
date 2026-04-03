using Atoll.Instructions;

namespace Atoll.Islands;

/// <summary>
/// Marks a component for client-side hydration when it enters the viewport.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>client:visible</c> directive. The component
/// is server-side rendered and hydrated when the user scrolls it into view, using
/// <c>IntersectionObserver</c>.
/// </para>
/// <para>
/// Use this for interactive components below the fold that don't need JavaScript
/// until the user actually sees them (e.g., image carousels, interactive charts,
/// comment sections).
/// </para>
/// <para>
/// An optional <see cref="RootMargin"/> can be specified to trigger hydration before
/// the element is fully visible (e.g., to start hydrating slightly before it scrolls into view).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ClientVisible]
/// public sealed class ImageCarousel : AtollComponent
/// {
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;div class=\"carousel\"&gt;Loading...&lt;/div&gt;");
///         return Task.CompletedTask;
///     }
/// }
///
/// // With root margin for early hydration:
/// [ClientVisible(RootMargin = "200px")]
/// public sealed class EarlyCarousel : AtollComponent
/// {
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;div class=\"carousel\"&gt;Loading...&lt;/div&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ClientVisibleAttribute : ClientDirectiveAttribute
{
    /// <summary>
    /// Initializes a new <see cref="ClientVisibleAttribute"/>.
    /// </summary>
    public ClientVisibleAttribute() : base(ClientDirectiveType.Visible)
    {
    }

    /// <summary>
    /// Gets or sets the root margin for the <c>IntersectionObserver</c>.
    /// This allows hydration to begin before the element is fully visible.
    /// Uses CSS margin syntax (e.g., <c>"200px"</c>, <c>"0px 0px 200px 0px"</c>).
    /// </summary>
    public string? RootMargin { get; set; }

    /// <inheritdoc />
    public override string? Value => RootMargin;
}
