namespace Atoll.Build.Diagnostics;

/// <summary>
/// Represents the severity level of a build diagnostic.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// Informational message about build progress.
    /// </summary>
    Info,

    /// <summary>
    /// Warning that does not prevent the build from completing.
    /// </summary>
    Warning,

    /// <summary>
    /// Error that indicates a build failure.
    /// </summary>
    Error,
}

/// <summary>
/// Represents a single diagnostic message emitted during the build process.
/// </summary>
public sealed class BuildDiagnostic
{
    /// <summary>
    /// Initializes a new <see cref="BuildDiagnostic"/>.
    /// </summary>
    /// <param name="severity">The diagnostic severity.</param>
    /// <param name="message">The diagnostic message.</param>
    public BuildDiagnostic(DiagnosticSeverity severity, string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Severity = severity;
        Message = message;
    }

    /// <summary>
    /// Initializes a new <see cref="BuildDiagnostic"/> with source context.
    /// </summary>
    /// <param name="severity">The diagnostic severity.</param>
    /// <param name="message">The diagnostic message.</param>
    /// <param name="source">The source file or route that produced this diagnostic.</param>
    public BuildDiagnostic(DiagnosticSeverity severity, string message, string source)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(source);
        Severity = severity;
        Message = message;
        Source = source;
    }

    /// <summary>
    /// Gets the diagnostic severity level.
    /// </summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the diagnostic message text.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the source file or route that produced this diagnostic, or <c>null</c> if not applicable.
    /// </summary>
    public string? Source { get; }
}
