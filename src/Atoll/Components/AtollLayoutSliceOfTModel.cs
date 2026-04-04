namespace Atoll.Components;

/// <summary>
/// Abstract base class for Atoll layouts authored as Razor templates with a strongly-typed model.
/// Extends <see cref="AtollSlice{TModel}"/> with a <see cref="RenderBodyAsync"/> helper that
/// renders the page body (the default slot) at the appropriate position in the layout template.
/// </summary>
/// <typeparam name="TModel">The model type, which serves as the strongly-typed props for the layout.</typeparam>
/// <remarks>
/// <para>
/// Razor layout files should declare <c>@inherits AtollLayoutSlice&lt;TModel&gt;</c> and call
/// <c>@{ await RenderBodyAsync(); }</c> where the page content should appear.
/// </para>
/// <para>
/// This class does NOT extend <c>RazorLayoutSlice</c> from the RazorSlices library.
/// Atoll uses its own slot-based layout mechanism rather than RazorSlices sections.
/// </para>
/// </remarks>
public abstract class AtollLayoutSlice<TModel> : AtollSlice<TModel>
{
    /// <summary>
    /// Renders the page body (default slot) to the current destination.
    /// Call this from <c>@{ await RenderBodyAsync(); }</c> in the Razor layout template
    /// to inject the wrapped page content.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    protected ValueTask RenderBodyAsync()
    {
        return RenderSlotAsync();
    }
}
