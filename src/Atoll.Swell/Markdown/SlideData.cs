namespace Atoll.Swell.Markdown;

/// <summary>
/// Represents a single parsed slide, holding its configuration, Markdown body,
/// presenter notes, and zero-based position in the deck.
/// </summary>
/// <param name="Index">Zero-based position of this slide in the deck.</param>
/// <param name="Config">Per-slide configuration parsed from frontmatter.</param>
/// <param name="Body">The Markdown content of the slide (frontmatter and notes stripped).</param>
/// <param name="Notes">Presenter notes extracted from HTML comments at the end of the slide, or empty.</param>
public sealed record SlideData(
    int Index,
    SlideConfig Config,
    string Body,
    string Notes);
