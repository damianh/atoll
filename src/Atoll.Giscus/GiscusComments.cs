using Atoll.Components;
using Atoll.Islands;

namespace Atoll.Giscus;

/// <summary>
/// An island component that embeds a <a href="https://giscus.app/">giscus</a> comments widget
/// powered by GitHub Discussions. The component defers loading until the user scrolls near it,
/// then dynamically creates the giscus <c>&lt;script&gt;</c> tag with all configuration attributes.
/// </summary>
/// <remarks>
/// <para>
/// To configure giscus, visit <a href="https://giscus.app/">giscus.app</a> to obtain your
/// repository ID, category, and category ID values.
/// </para>
/// <para>
/// The component renders a <c>&lt;div class="giscus"&gt;</c> placeholder server-side.
/// All giscus configuration is encoded as <c>data-*</c> attributes on that div and read
/// by the client-side JavaScript when the island hydrates.
/// </para>
/// <para>
/// If the Atoll theme toggle is present on the page (setting <c>data-theme</c> on
/// <c>&lt;html&gt;</c>), this island will automatically sync the giscus theme via the
/// giscus <c>postMessage</c> API.
/// </para>
/// </remarks>
[ClientVisible(RootMargin = "300px")]
public sealed class GiscusComments : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-giscus.js";

    /// <summary>
    /// Gets or sets the GitHub repository in <c>owner/repo</c> format.
    /// The repository must have the giscus GitHub App installed.
    /// </summary>
    [Parameter(Required = true)]
    public string Repo { get; set; } = "";

    /// <summary>
    /// Gets or sets the Base64-encoded repository ID.
    /// Obtain this from the <a href="https://giscus.app/">giscus configuration page</a>
    /// or the GitHub GraphQL API.
    /// </summary>
    [Parameter(Required = true)]
    public string RepoId { get; set; } = "";

    /// <summary>
    /// Gets or sets the GitHub Discussions category name.
    /// Either <see cref="Category"/> or <see cref="CategoryId"/> must be provided.
    /// </summary>
    [Parameter]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the Base64-encoded discussions category ID.
    /// Obtain this from the <a href="https://giscus.app/">giscus configuration page</a>
    /// or the GitHub GraphQL API.
    /// </summary>
    [Parameter(Required = true)]
    public string CategoryId { get; set; } = "";

    /// <summary>
    /// Gets or sets how giscus maps pages to GitHub Discussions.
    /// Defaults to <see cref="GiscusMapping.Pathname"/>.
    /// </summary>
    [Parameter]
    public GiscusMapping Mapping { get; set; } = GiscusMapping.Pathname;

    /// <summary>
    /// Gets or sets the custom search term used when <see cref="Mapping"/> is
    /// <see cref="GiscusMapping.Specific"/> or <see cref="GiscusMapping.Number"/>.
    /// </summary>
    [Parameter]
    public string? Term { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether strict title matching is used.
    /// When <see langword="true"/>, giscus uses a SHA-1 hash of the title to prevent
    /// false matches. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool Strict { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether emoji reactions on the top-level post are shown.
    /// Defaults to <see langword="true"/>.
    /// </summary>
    [Parameter]
    public bool ReactionsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether giscus emits discussion metadata
    /// to the parent page via <c>postMessage</c>. Defaults to <see langword="false"/>.
    /// </summary>
    [Parameter]
    public bool EmitMetadata { get; set; }

    /// <summary>
    /// Gets or sets where the comment input box is placed relative to the comments list.
    /// Defaults to <see cref="GiscusInputPosition.Bottom"/>.
    /// </summary>
    [Parameter]
    public GiscusInputPosition InputPosition { get; set; } = GiscusInputPosition.Bottom;

    /// <summary>
    /// Gets or sets the visual theme for the giscus widget.
    /// Accepts a built-in theme name (e.g., <c>"light"</c>, <c>"dark"</c>,
    /// <c>"preferred_color_scheme"</c>) or a URL to a custom CSS file.
    /// Defaults to <c>"preferred_color_scheme"</c>.
    /// </summary>
    [Parameter]
    public string Theme { get; set; } = "preferred_color_scheme";

    /// <summary>
    /// Gets or sets the BCP 47 language tag for the giscus UI language.
    /// Defaults to <c>"en"</c> (English).
    /// </summary>
    [Parameter]
    public string Lang { get; set; } = "en";

    /// <summary>
    /// Gets or sets the loading strategy for the giscus iframe.
    /// Defaults to <see cref="GiscusLoading.Lazy"/> for better performance.
    /// </summary>
    [Parameter]
    public GiscusLoading Loading { get; set; } = GiscusLoading.Lazy;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        var strict = Strict ? "1" : "0";
        var reactionsEnabled = ReactionsEnabled ? "1" : "0";
        var emitMetadata = EmitMetadata ? "1" : "0";

        WriteHtml("<div class=\"giscus\"");
        WriteHtml($" data-repo=\"{Encode(Repo)}\"");
        WriteHtml($" data-repo-id=\"{Encode(RepoId)}\"");
        if (Category is not null)
        {
            WriteHtml($" data-category=\"{Encode(Category)}\"");
        }
        WriteHtml($" data-category-id=\"{Encode(CategoryId)}\"");
        WriteHtml($" data-mapping=\"{Encode(Mapping.ToDataValue())}\"");
        if (Term is not null)
        {
            WriteHtml($" data-term=\"{Encode(Term)}\"");
        }
        WriteHtml($" data-strict=\"{strict}\"");
        WriteHtml($" data-reactions-enabled=\"{reactionsEnabled}\"");
        WriteHtml($" data-emit-metadata=\"{emitMetadata}\"");
        WriteHtml($" data-input-position=\"{Encode(InputPosition.ToDataValue())}\"");
        WriteHtml($" data-theme=\"{Encode(Theme)}\"");
        WriteHtml($" data-lang=\"{Encode(Lang)}\"");
        WriteHtml($" data-loading=\"{Encode(Loading.ToDataValue())}\"");
        WriteHtml("></div>");

        return Task.CompletedTask;
    }

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
