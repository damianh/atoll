using Atoll.Rendering;

namespace Atoll.Instructions;

/// <summary>
/// Base type for render instructions — side-channel metadata that bubbles up through
/// the component tree during rendering. Instructions are collected by the page-level
/// <see cref="InstructionProcessor"/> and processed after the component tree has been rendered.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's render instruction system. Components emit
/// instructions for things like head content (styles, scripts, links) and client directives
/// (island hydration). These are collected during rendering and injected at the appropriate
/// point in the final HTML output.
/// </para>
/// <para>
/// Each instruction has a <see cref="Key"/> used for deduplication. Two instructions with
/// the same key are considered identical, and only one will be emitted in the final output.
/// </para>
/// </remarks>
public abstract class RenderInstruction
{
    /// <summary>
    /// Initializes a new <see cref="RenderInstruction"/> with the specified key.
    /// </summary>
    /// <param name="key">The deduplication key for this instruction.</param>
    protected RenderInstruction(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        Key = key;
    }

    /// <summary>
    /// Gets the deduplication key for this instruction. Two instructions with the same
    /// key are considered identical, and only one will be emitted.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Renders this instruction's content to the specified destination.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public abstract ValueTask RenderAsync(IRenderDestination destination);
}
