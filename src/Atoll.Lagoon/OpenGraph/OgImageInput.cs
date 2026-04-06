namespace Atoll.Lagoon.OpenGraph;

/// <summary>
/// Represents a single documentation page to generate an OpenGraph image for.
/// </summary>
/// <param name="Title">The page title rendered on the OG image.</param>
/// <param name="Slug">
/// The URL path of the page (e.g. <c>/identityserver/overview/big-picture</c>).
/// Used to determine the output image path and auto-detect the category from the first URL segment.
/// </param>
/// <param name="Description">Optional page description rendered on the OG image.</param>
/// <param name="Category">
/// Optional explicit category label. When <c>null</c>, the category is auto-detected from the
/// first segment of <paramref name="Slug"/> using the <c>Categories</c> map in
/// <see cref="Atoll.Lagoon.Configuration.OpenGraphConfig"/>.
/// </param>
public sealed record OgImageInput(
    string Title,
    string Slug,
    string? Description,
    string? Category);
