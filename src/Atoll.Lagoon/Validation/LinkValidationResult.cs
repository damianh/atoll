namespace Atoll.Lagoon.Validation;

/// <summary>
/// Aggregates the results of a link validation run.
/// </summary>
public sealed class LinkValidationResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="LinkValidationResult"/>.
    /// </summary>
    /// <param name="errors">The list of validation errors found.</param>
    /// <param name="pagesScanned">The number of pages scanned.</param>
    /// <param name="linksChecked">The total number of internal links checked.</param>
    /// <param name="elapsed">The time taken to complete validation.</param>
    public LinkValidationResult(
        IReadOnlyList<LinkValidationError> errors,
        int pagesScanned,
        int linksChecked,
        TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Errors = errors;
        PagesScanned = pagesScanned;
        LinksChecked = linksChecked;
        Elapsed = elapsed;
    }

    /// <summary>Gets the list of validation errors found. Empty when all links are valid.</summary>
    public IReadOnlyList<LinkValidationError> Errors { get; }

    /// <summary>Gets the number of pages that were scanned.</summary>
    public int PagesScanned { get; }

    /// <summary>Gets the total number of internal links that were checked.</summary>
    public int LinksChecked { get; }

    /// <summary>Gets the time taken to complete the validation run.</summary>
    public TimeSpan Elapsed { get; }

    /// <summary>Gets a value indicating whether the validation found no errors.</summary>
    public bool IsValid => Errors.Count == 0;
}
