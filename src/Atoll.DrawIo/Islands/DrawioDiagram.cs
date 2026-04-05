using System.Text.Json;
using Atoll.Components;
using Atoll.Css;
using Atoll.DrawIo.Parsing;
using Atoll.Islands;

namespace Atoll.DrawIo.Islands;

/// <summary>
/// An Atoll island component that renders a draw.io diagram using the official
/// draw.io <c>viewer-static.min.js</c> for client-side rendering.
/// Emits a <c>&lt;div data-mxgraph="..."&gt;</c> element; the viewer script
/// processes it and renders the diagram with pan, zoom, layers, and toolbar.
/// </summary>
/// <remarks>
/// JavaScript is required to display the diagram. A <c>&lt;noscript&gt;</c>
/// fallback message is included for environments with JS disabled.
/// </remarks>
[ClientVisible]
[Styles(".drawio-diagram { display: block; width: 100%; }")]
public sealed class DrawioDiagram : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-drawio-viewer-init.js";

    /// <summary>Gets or sets the path to the <c>.drawio</c> file to render.</summary>
    [Parameter(Required = true)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the zero-based index of the page to render. Defaults to <c>0</c>.</summary>
    [Parameter]
    public int? Page { get; set; }

    /// <summary>Gets or sets the name of the page to render. Used when <see cref="Page"/> is not set.</summary>
    [Parameter]
    public string? PageName { get; set; }

    /// <summary>Gets or sets the accessible label for the diagram (used as <c>aria-label</c>).</summary>
    [Parameter]
    public string? Alt { get; set; }

    /// <summary>Gets or sets whether to show the toolbar. Defaults to <c>true</c>.</summary>
    [Parameter]
    public bool Toolbar { get; set; } = true;

    /// <summary>Gets or sets whether to enable the lightbox (full-screen view). Defaults to <c>false</c>.</summary>
    [Parameter]
    public bool Lightbox { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        // Read the raw .drawio file content (mxfile XML envelope) — the viewer handles decompression.
        var rawXml = File.ReadAllText(FilePath);

        // Resolve page index: use Page if set, otherwise look up PageName in the parsed file.
        var pageIndex = ResolvePageIndex(rawXml);

        // Build the data-mxgraph JSON config.
        var config = new Dictionary<string, object?>
        {
            ["xml"]           = rawXml,
            ["page"]          = pageIndex,
            ["auto-fit"]      = true,
            ["resize"]        = true,
            ["nav"]           = true,
            ["toolbar"]       = Toolbar ? "zoom layers lightbox" : null,
            ["toolbar-nohide"] = Toolbar,
            ["lightbox"]      = Lightbox,
            ["highlight"]     = "#0000ff",
            ["edit"]          = false,
        };

        // Remove null values (e.g. toolbar when disabled).
        var filteredConfig = config
            .Where(kv => kv.Value is not null)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var configJson = JsonSerializer.Serialize(filteredConfig);

        var ariaLabel = string.IsNullOrEmpty(Alt)
            ? string.Empty
            : $" aria-label=\"{System.Web.HttpUtility.HtmlAttributeEncode(Alt)}\"";
        var role = string.IsNullOrEmpty(Alt) ? string.Empty : " role=\"img\"";

        WriteHtml($"<div class=\"drawio-diagram\"{role}{ariaLabel} data-mxgraph=\"{System.Web.HttpUtility.HtmlAttributeEncode(configJson)}\">");
        WriteHtml("<noscript>Enable JavaScript to view this diagram.</noscript>");
        WriteHtml("</div>");

        return Task.CompletedTask;
    }

    private int ResolvePageIndex(string rawXml)
    {
        if (Page.HasValue)
        {
            return Page.Value;
        }

        if (!string.IsNullOrEmpty(PageName))
        {
            try
            {
                var file = DrawioFileParser.Parse(rawXml);
                for (var i = 0; i < file.Pages.Count; i++)
                {
                    if (string.Equals(file.Pages[i].Name, PageName, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        return 0;
    }
}
