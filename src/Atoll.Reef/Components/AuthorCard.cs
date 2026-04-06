using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders an author bio card showing avatar, name, bio text, and an optional profile link.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>AuthorCardTemplate.cshtml</c>.
/// </remarks>
public sealed class AuthorCard : AtollComponent
{
    /// <summary>Gets or sets the author's display name.</summary>
    [Parameter(Required = true)]
    public string Name { get; set; } = "";

    /// <summary>Gets or sets the URL of the author's avatar image. Optional.</summary>
    [Parameter]
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the author's short biography text. Optional.</summary>
    [Parameter]
    public string? Bio { get; set; }

    /// <summary>Gets or sets the URL of the author's profile page. Optional.</summary>
    [Parameter]
    public string? Url { get; set; }

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new AuthorCardModel(Name, AvatarUrl, Bio, Url);

        await ComponentRenderer.RenderSliceAsync<AuthorCardTemplate, AuthorCardModel>(
            context.Destination,
            model);
    }
}
