using Atoll.Components;

namespace Atoll.Docs.Components;

/// <summary>
/// A single sidebar navigation link item.
/// </summary>
public sealed class NavItem : AtollComponent
{
    /// <summary>
    /// Gets or sets the display title of the nav link.
    /// </summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the URL slug for the docs page (e.g., "getting-started").
    /// </summary>
    [Parameter(Required = true)]
    public string Slug { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether this item represents the current page.
    /// </summary>
    [Parameter]
    public bool IsActive { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var cssClass = IsActive ? " class=\"active\"" : "";
        var ariaCurrent = IsActive ? " aria-current=\"page\"" : "";

        WriteHtml("<li><a href=\"/docs/");
        WriteText(Slug);
        WriteHtml("\"");
        WriteHtml(cssClass);
        WriteHtml(ariaCurrent);
        WriteHtml(">");
        WriteText(Title);
        WriteHtml("</a></li>");

        return Task.CompletedTask;
    }
}
