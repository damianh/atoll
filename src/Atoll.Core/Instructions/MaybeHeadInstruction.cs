using Atoll.Core.Rendering;

namespace Atoll.Core.Instructions;

/// <summary>
/// A render instruction representing head content that may or may not be included
/// in the final output. Unlike <see cref="HeadInstruction"/>, a maybe-head instruction
/// is provisional — it is only promoted to actual head content when its containing
/// component renders successfully and the <see cref="Propagate"/> flag is set.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's propagation mechanism for head content
/// in conditional render branches. When a component conditionally renders a child
/// that emits head instructions, those instructions start as "maybe" instructions.
/// The parent component promotes them to actual head instructions by setting
/// <see cref="Propagate"/> to <c>true</c> once it confirms the branch was rendered.
/// </para>
/// <para>
/// The <see cref="InstructionProcessor"/> treats a <see cref="MaybeHeadInstruction"/>
/// the same as any other instruction for deduplication (using the <see cref="RenderInstruction.Key"/>
/// from the wrapped <see cref="HeadInstruction"/>). Consumers should check
/// <see cref="Propagate"/> before including the instruction in the final output.
/// </para>
/// </remarks>
public sealed class MaybeHeadInstruction : RenderInstruction
{
    /// <summary>
    /// Initializes a new <see cref="MaybeHeadInstruction"/> wrapping the specified
    /// head instruction.
    /// </summary>
    /// <param name="headInstruction">The head instruction to conditionally include.</param>
    public MaybeHeadInstruction(HeadInstruction headInstruction)
        : base(headInstruction?.Key ?? throw new ArgumentNullException(nameof(headInstruction)))
    {
        HeadInstruction = headInstruction;
    }

    /// <summary>
    /// Gets the wrapped head instruction.
    /// </summary>
    public HeadInstruction HeadInstruction { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this instruction should be propagated
    /// to the final head output. Defaults to <c>false</c>.
    /// </summary>
    public bool Propagate { get; set; }

    /// <inheritdoc />
    /// <remarks>
    /// Delegates to the wrapped <see cref="HeadInstruction"/>'s render method.
    /// Callers should check <see cref="Propagate"/> before invoking this method.
    /// </remarks>
    public override ValueTask RenderAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return HeadInstruction.RenderAsync(destination);
    }
}
