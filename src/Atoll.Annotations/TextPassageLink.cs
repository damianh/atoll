using Atoll.Components;
using Atoll.Islands;

namespace Atoll.Annotations;

/// <summary>
/// Enables readers to copy a link to selected text without configuring GitHub feedback.
/// </summary>
[ClientIdle]
public sealed class TextPassageLink : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-annotations.js";

    /// <summary>
    /// Gets or sets the CSS selector identifying the content area. Defaults to <c>article</c>.
    /// </summary>
    [Parameter]
    public string ContentSelector { get; set; } = "article";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"atoll-passage-link\"");
        WriteHtml($" data-content-selector=\"{System.Net.WebUtility.HtmlEncode(ContentSelector)}\"");
        WriteHtml("></div>");
        return Task.CompletedTask;
    }
}
