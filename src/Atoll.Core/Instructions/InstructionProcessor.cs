using Atoll.Core.Rendering;

namespace Atoll.Core.Instructions;

/// <summary>
/// Collects and deduplicates <see cref="RenderInstruction"/> instances emitted during
/// component tree rendering. The processor maintains insertion order while ensuring
/// each unique instruction (by <see cref="RenderInstruction.Key"/>) appears only once.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="InstructionProcessor"/> is typically created once per page render and
/// passed through the component tree via <see cref="Components.RenderContext"/>. Components
/// emit instructions by calling <see cref="Add"/>, and the page-level orchestrator
/// retrieves the collected instructions via <see cref="GetInstructions"/> or renders
/// them via <see cref="RenderAllAsync"/>.
/// </para>
/// <para>
/// Deduplication uses the instruction's <see cref="RenderInstruction.Key"/> property.
/// If two instructions with the same key are added, only the first one is kept.
/// This ensures that, for example, multiple components referencing the same stylesheet
/// result in only one <c>&lt;link&gt;</c> element in the output.
/// </para>
/// </remarks>
public sealed class InstructionProcessor
{
    private readonly List<RenderInstruction> _instructions = [];
    private readonly HashSet<string> _seenKeys = [];

    /// <summary>
    /// Adds an instruction to the processor. If an instruction with the same key
    /// has already been added, this call is a no-op.
    /// </summary>
    /// <param name="instruction">The instruction to add.</param>
    /// <returns><c>true</c> if the instruction was added; <c>false</c> if it was a duplicate.</returns>
    public bool Add(RenderInstruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        if (!_seenKeys.Add(instruction.Key))
        {
            return false;
        }

        _instructions.Add(instruction);
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether an instruction with the specified key has been added.
    /// </summary>
    /// <param name="key">The instruction key to check.</param>
    /// <returns><c>true</c> if the key has been seen; otherwise, <c>false</c>.</returns>
    public bool HasInstruction(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _seenKeys.Contains(key);
    }

    /// <summary>
    /// Gets the number of unique instructions that have been collected.
    /// </summary>
    public int Count => _instructions.Count;

    /// <summary>
    /// Gets all collected instructions in insertion order.
    /// </summary>
    /// <returns>A read-only list of instructions.</returns>
    public IReadOnlyList<RenderInstruction> GetInstructions()
    {
        return _instructions;
    }

    /// <summary>
    /// Gets all collected instructions of a specific type in insertion order.
    /// </summary>
    /// <typeparam name="T">The instruction type to filter by.</typeparam>
    /// <returns>An enumerable of matching instructions.</returns>
    public IEnumerable<T> GetInstructions<T>() where T : RenderInstruction
    {
        foreach (var instruction in _instructions)
        {
            if (instruction is T typed)
            {
                yield return typed;
            }
        }
    }

    /// <summary>
    /// Renders all collected instructions of the specified type to the destination.
    /// Instructions are rendered in insertion order, each followed by a newline.
    /// </summary>
    /// <typeparam name="T">The instruction type to render.</typeparam>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public async ValueTask RenderAllAsync<T>(IRenderDestination destination) where T : RenderInstruction
    {
        ArgumentNullException.ThrowIfNull(destination);

        foreach (var instruction in _instructions)
        {
            if (instruction is T typed)
            {
                await typed.RenderAsync(destination);
                destination.Write(RenderChunk.Html("\n"));
            }
        }
    }

    /// <summary>
    /// Renders all collected instructions to the destination.
    /// Instructions are rendered in insertion order, each followed by a newline.
    /// </summary>
    /// <param name="destination">The render destination.</param>
    /// <returns>A <see cref="ValueTask"/> representing the render operation.</returns>
    public async ValueTask RenderAllAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        foreach (var instruction in _instructions)
        {
            await instruction.RenderAsync(destination);
            destination.Write(RenderChunk.Html("\n"));
        }
    }

    /// <summary>
    /// Clears all collected instructions.
    /// </summary>
    public void Clear()
    {
        _instructions.Clear();
        _seenKeys.Clear();
    }
}
