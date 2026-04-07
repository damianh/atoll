namespace Atoll.Lagoon.LlmsTxt;

/// <summary>
/// Metadata about the documentation site, rendered as the header of the <c>llms.txt</c> file.
/// </summary>
/// <param name="Title">
/// The site/project name, rendered as the H1 heading (e.g., <c>"Duende IdentityServer"</c>).
/// </param>
/// <param name="Description">
/// An optional summary rendered as a blockquote below the title.
/// When <c>null</c>, the blockquote section is omitted.
/// </param>
public sealed record LlmsTxtSiteInfo(string Title, string? Description);
