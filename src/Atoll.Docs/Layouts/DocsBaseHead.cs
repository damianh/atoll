using Atoll.Components;
using Atoll.Docs.Configuration;

namespace Atoll.Docs.Layouts;

/// <summary>
/// Renders the <c>&lt;head&gt;</c> section for documentation pages, including meta tags,
/// viewport settings, title template, custom CSS, and the theme FOUC-prevention inline script.
/// </summary>
public sealed class DocsBaseHead : AtollComponent
{
    /// <summary>Gets or sets the docs site configuration.</summary>
    [Parameter(Required = true)]
    public DocsConfig Config { get; set; } = null!;

    /// <summary>Gets or sets the page-specific title. Appended to the site title.</summary>
    [Parameter]
    public string PageTitle { get; set; } = "";

    /// <summary>Gets or sets the page-specific description for the meta description tag.</summary>
    [Parameter]
    public string? PageDescription { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<head>");
        WriteHtml("<meta charset=\"utf-8\" />");
        WriteHtml("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");

        // Title
        WriteHtml("<title>");
        if (!string.IsNullOrEmpty(PageTitle))
        {
            WriteText(PageTitle);
            WriteHtml(" | ");
        }

        WriteText(Config.Title);
        WriteHtml("</title>");

        // Description meta
        var description = PageDescription ?? Config.Description;
        if (!string.IsNullOrEmpty(description))
        {
            WriteHtml($"<meta name=\"description\" content=\"{HtmlEncode(description)}\" />");
        }

        // Theme FOUC prevention — must run before page renders
        WriteHtml("""
            <script>
            (function(){var s=localStorage.getItem('atoll-docs-theme');if(s==='dark'||s==='light'){document.documentElement.setAttribute('data-theme',s);}else if(window.matchMedia('(prefers-color-scheme: dark)').matches){document.documentElement.setAttribute('data-theme','dark');}})();
            </script>
            """);

        // Custom CSS
        foreach (var css in Config.CustomCss)
        {
            WriteHtml($"<link rel=\"stylesheet\" href=\"{HtmlEncode(css)}\" />");
        }

        WriteHtml("</head>");
        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
