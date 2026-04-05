namespace Atoll.Lagoon.Components;

/// <summary>
/// A mutable counter that assigns sequential zero-based indices to sidebar groups
/// during rendering, ensuring each <c>&lt;details&gt;</c> element gets a unique
/// <c>data-index</c> attribute that matches the client-side state array.
/// </summary>
public sealed class GroupIndexCounter
{
    private int _next;

    /// <summary>Returns the next index and increments the counter.</summary>
    public int Next() => _next++;
}
