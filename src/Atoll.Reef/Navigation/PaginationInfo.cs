namespace Atoll.Reef.Navigation;

/// <summary>
/// Holds pagination state: the current page, total pages, and a base URL used to
/// generate page links. Page 1 uses the base URL directly; subsequent pages append
/// <c>/page/{n}</c>.
/// </summary>
public sealed class PaginationInfo
{
    /// <summary>
    /// Initialises a new instance of <see cref="PaginationInfo"/>.
    /// </summary>
    /// <param name="currentPage">The 1-based current page number.</param>
    /// <param name="totalPages">The total number of pages.</param>
    /// <param name="baseUrl">
    /// The canonical base URL for the first page (e.g. <c>"/articles"</c>).
    /// Subsequent pages are <c>"{baseUrl}/page/{n}"</c>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="currentPage"/> or <paramref name="totalPages"/> are less than 1,
    /// or <paramref name="currentPage"/> exceeds <paramref name="totalPages"/>.
    /// </exception>
    public PaginationInfo(int currentPage, int totalPages, string baseUrl)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(currentPage, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(totalPages, 1);
        if (currentPage > totalPages)
        {
            throw new ArgumentOutOfRangeException(nameof(currentPage),
                "Current page cannot exceed total pages.");
        }

        CurrentPage = currentPage;
        TotalPages = totalPages;
        BaseUrl = baseUrl ?? "";
    }

    /// <summary>Gets the 1-based current page number.</summary>
    public int CurrentPage { get; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages { get; }

    /// <summary>Gets the base URL for the first page.</summary>
    public string BaseUrl { get; }

    /// <summary>Gets a value indicating whether there is a previous page.</summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>Gets a value indicating whether there is a next page.</summary>
    public bool HasNext => CurrentPage < TotalPages;

    /// <summary>
    /// Returns the URL for the given page number.
    /// Page 1 returns <see cref="BaseUrl"/>; all other pages append <c>/page/{page}</c>.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    public string GetPageUrl(int page) =>
        page == 1 ? BaseUrl : $"{BaseUrl.TrimEnd('/')}/page/{page}";
}
