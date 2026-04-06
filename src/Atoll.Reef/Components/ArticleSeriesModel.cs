namespace Atoll.Reef.Components;

/// <summary>
/// Model for the <c>ArticleSeriesTemplate</c> Razor slice.
/// </summary>
/// <param name="SeriesName">The display name of the series.</param>
/// <param name="Parts">All parts in the series.</param>
/// <param name="CurrentPart">The 1-based index of the currently viewed part.</param>
public sealed record ArticleSeriesModel(
    string SeriesName,
    IReadOnlyList<SeriesPart> Parts,
    int CurrentPart);
