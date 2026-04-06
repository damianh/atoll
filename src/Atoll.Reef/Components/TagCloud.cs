using Atoll.Components;

namespace Atoll.Reef.Components;

/// <summary>
/// Renders a tag cloud: a navigation region containing tag pills with article counts and links.
/// </summary>
/// <remarks>
/// Rendering is delegated to <c>TagCloudTemplate.cshtml</c>.
/// </remarks>
public sealed class TagCloud : AtollComponent
{
    /// <summary>Gets or sets the tags with counts to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<TagCount> Tags { get; set; } = [];

    /// <summary>
    /// Gets or sets the base URL path prefix for the articles site (e.g. <c>"/blog"</c>).
    /// Used to construct <c>/tag/{slug}</c> links.
    /// </summary>
    [Parameter]
    public string BasePath { get; set; } = "";

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        var model = new TagCloudModel(Tags, BasePath.TrimEnd('/'));

        await ComponentRenderer.RenderSliceAsync<TagCloudTemplate, TagCloudModel>(
            context.Destination,
            model);
    }
}
