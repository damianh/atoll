namespace Atoll.Lagoon.Validation;

/// <summary>
/// Describes the category of a link validation failure.
/// </summary>
public enum LinkErrorKind
{
    /// <summary>The link target page does not exist in the site.</summary>
    BrokenLink,

    /// <summary>The link target page exists but the specified fragment (anchor) does not.</summary>
    InvalidFragment,
}

/// <summary>
/// Describes a single link validation failure detected during build.
/// </summary>
public sealed class LinkValidationError
{
    /// <summary>
    /// Initializes a new instance of <see cref="LinkValidationError"/>.
    /// </summary>
    /// <param name="sourcePage">The URL path of the page where the broken link was found.</param>
    /// <param name="targetHref">The raw <c>href</c> value of the broken link.</param>
    /// <param name="errorKind">The kind of validation failure.</param>
    /// <param name="message">A human-readable description of the failure.</param>
    public LinkValidationError(
        string sourcePage,
        string targetHref,
        LinkErrorKind errorKind,
        string message)
    {
        ArgumentNullException.ThrowIfNull(sourcePage);
        ArgumentNullException.ThrowIfNull(targetHref);
        ArgumentNullException.ThrowIfNull(message);
        SourcePage = sourcePage;
        TargetHref = targetHref;
        ErrorKind = errorKind;
        Message = message;
    }

    /// <summary>Gets the URL path of the page where the broken link was found.</summary>
    public string SourcePage { get; }

    /// <summary>Gets the raw <c>href</c> value of the broken link.</summary>
    public string TargetHref { get; }

    /// <summary>Gets the kind of validation failure.</summary>
    public LinkErrorKind ErrorKind { get; }

    /// <summary>Gets a human-readable description of the failure.</summary>
    public string Message { get; }
}
