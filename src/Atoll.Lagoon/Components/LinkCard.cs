using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// A prominent navigation card rendered as an anchor element, with a title,
/// optional description, and optional icon.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>LinkCardTemplate.cshtml</c>.
/// </remarks>
public sealed class LinkCard : AtollComponent
{
    /// <summary>Gets or sets the card title text.</summary>
    [Parameter(Required = true)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL the card links to.</summary>
    [Parameter(Required = true)]
    public string Href { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description shown beneath the title.</summary>
    [Parameter]
    public string? Description { get; set; }

    /// <summary>Gets or sets an optional icon displayed before the title.</summary>
    [Parameter]
    public IconName? IconName { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new LinkCardModel(Title, Href, Description, IconName);

        await ComponentRenderer.RenderSliceAsync<LinkCardTemplate, LinkCardModel>(
            context.Destination,
            model);
    }
}
