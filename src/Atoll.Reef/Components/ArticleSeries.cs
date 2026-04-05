using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a multi-part article series indicator showing the current part number,
/// the total part count, and links to all parts in the series.
/// </summary>
public sealed class ArticleSeries : AtollComponent
{
    /// <summary>Gets or sets the display name of the series.</summary>
    [Parameter(Required = true)]
    public string SeriesName { get; set; } = "";

    /// <summary>Gets or sets all parts in the series.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<SeriesPart> Parts { get; set; } = [];

    /// <summary>Gets or sets the 1-based index of the currently viewed part.</summary>
    [Parameter(Required = true)]
    public int CurrentPart { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<aside class=\"article-series\" aria-label=\"Article series\">");
        WriteHtml("<p class=\"series-header\">Part ");
        WriteText(CurrentPart.ToString());
        WriteHtml(" of ");
        WriteText(Parts.Count.ToString());
        WriteHtml(" in &#8220;");
        WriteText(SeriesName);
        WriteHtml("&#8221;</p>");
        WriteHtml("<ol class=\"series-parts\">");

        for (var i = 0; i < Parts.Count; i++)
        {
            var part = Parts[i];
            var isCurrent = i + 1 == CurrentPart;
            var ariaCurrent = isCurrent ? " aria-current=\"page\"" : "";
            WriteHtml($"<li class=\"series-part{(isCurrent ? " series-part--current" : "")}\">");
            WriteHtml($"<a href=\"{HtmlEncode(part.Href)}\"{ariaCurrent}>");
            WriteText(part.Title);
            WriteHtml("</a>");
            WriteHtml("</li>");
        }

        WriteHtml("</ol>");
        WriteHtml("</aside>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
