using Atoll.DrawIo.Model;

namespace Atoll.DrawIo.Parsing;

/// <summary>
/// Represents a parsed draw.io file (<c>.drawio</c> or <c>.dio</c>).
/// Contains one or more diagram pages extracted from the <c>&lt;mxfile&gt;</c> envelope.
/// </summary>
public sealed class DrawioFile
{
    /// <summary>
    /// Initializes a new instance of <see cref="DrawioFile"/>.
    /// </summary>
    /// <param name="pages">The list of diagram pages contained in this file.</param>
    public DrawioFile(IReadOnlyList<DrawioPage> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        Pages = pages;
    }

    /// <summary>
    /// Gets the list of diagram pages in this file.
    /// </summary>
    public IReadOnlyList<DrawioPage> Pages { get; }
}
