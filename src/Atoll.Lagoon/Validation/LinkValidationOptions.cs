namespace Atoll.Lagoon.Validation;

/// <summary>
/// Controls the behaviour of the <see cref="LagoonLinkValidator"/>.
/// </summary>
public sealed class LinkValidationOptions
{
    /// <summary>
    /// Initializes a new instance of <see cref="LinkValidationOptions"/> with default values.
    /// </summary>
    public LinkValidationOptions()
    {
        ValidateFragments = true;
        ExcludePatterns = [];
        TreatAsErrors = true;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LinkValidationOptions"/> with explicit values.
    /// </summary>
    /// <param name="validateFragments">Whether to validate fragment (anchor) targets.</param>
    /// <param name="excludePatterns">URL path prefix patterns to exclude from validation.</param>
    /// <param name="treatAsErrors">
    /// When <c>true</c>, broken links are reported as errors; when <c>false</c>, as warnings.
    /// </param>
    public LinkValidationOptions(
        bool validateFragments,
        IReadOnlyList<string> excludePatterns,
        bool treatAsErrors)
    {
        ArgumentNullException.ThrowIfNull(excludePatterns);
        ValidateFragments = validateFragments;
        ExcludePatterns = excludePatterns;
        TreatAsErrors = treatAsErrors;
    }

    /// <summary>
    /// Gets a value indicating whether to validate that fragment targets (e.g. <c>#section</c>)
    /// correspond to actual heading anchors on the target page. Defaults to <c>true</c>.
    /// </summary>
    public bool ValidateFragments { get; }

    /// <summary>
    /// Gets URL path prefix patterns for links to skip during validation.
    /// Any internal link whose path starts with one of these patterns is excluded.
    /// Example: <c>["/api/", "/changelog/"]</c>.
    /// </summary>
    public IReadOnlyList<string> ExcludePatterns { get; }

    /// <summary>
    /// Gets a value indicating whether broken links should be treated as build errors
    /// (<c>true</c>) or warnings (<c>false</c>). Defaults to <c>true</c>.
    /// </summary>
    public bool TreatAsErrors { get; }
}
