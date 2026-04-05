namespace Atoll.Reef.Components;

/// <summary>
/// Represents a single part in a multi-article series with a title and URL.
/// </summary>
public sealed class SeriesPart
{
    /// <summary>Initialises a new <see cref="SeriesPart"/> with title and href.</summary>
    public SeriesPart(string title, string href)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        Title = title;
        Href = href;
    }

    /// <summary>Gets the display title of this series part.</summary>
    public string Title { get; }

    /// <summary>Gets the URL of this series part.</summary>
    public string Href { get; }
}
