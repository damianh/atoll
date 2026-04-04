namespace Atoll.Lagoon.Navigation;

/// <summary>
/// Holds the resolved previous and next page links for a documentation page.
/// </summary>
public sealed class PaginationResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="PaginationResult"/>.
    /// </summary>
    /// <param name="previous">The previous page link, or <c>null</c> if this is the first page.</param>
    /// <param name="next">The next page link, or <c>null</c> if this is the last page.</param>
    public PaginationResult(PaginationLink? previous, PaginationLink? next)
    {
        Previous = previous;
        Next = next;
    }

    /// <summary>
    /// Gets the link to the previous page, or <c>null</c> if this is the first page.
    /// </summary>
    public PaginationLink? Previous { get; }

    /// <summary>
    /// Gets the link to the next page, or <c>null</c> if this is the last page.
    /// </summary>
    public PaginationLink? Next { get; }
}
