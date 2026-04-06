using Atoll.Components;
using Atoll.Islands;

namespace Atoll.Annotations;

/// <summary>
/// An island component that enables contextual text-selection feedback on any Atoll-powered page.
/// When a user selects text within the configured content area, a floating button appears.
/// Clicking the button opens a small form where the user can type a comment.
/// On submit, a new browser tab opens with a pre-populated GitHub Issue or Discussion URL
/// containing the quoted text, user comment, and a text-fragment link back to the selection.
/// </summary>
/// <remarks>
/// <para>
/// No backend, database, or authentication is required on the Atoll side.
/// GitHub handles authentication, spam prevention, and storage.
/// </para>
/// <para>
/// When <see cref="Target"/> is <see cref="AnnotationTarget.Discussion"/>, the
/// <see cref="Category"/> parameter should be set to match an existing GitHub Discussions
/// category name in the target repository.
/// </para>
/// <para>
/// When <see cref="Target"/> is <see cref="AnnotationTarget.Issue"/>, the optional
/// <see cref="Labels"/> parameter accepts a comma-separated list of label names to
/// pre-populate on the new issue.
/// </para>
/// </remarks>
[ClientIdle]
public sealed class TextAnnotation : VanillaJsIsland
{
    /// <inheritdoc />
    public override string ClientModuleUrl => "/scripts/atoll-annotations.js";

    /// <summary>
    /// Gets or sets the GitHub repository in <c>owner/repo</c> format.
    /// The repository must be accessible to the user submitting feedback.
    /// </summary>
    [Parameter(Required = true)]
    public string Repo { get; set; } = "";

    /// <summary>
    /// Gets or sets where the annotation feedback is submitted.
    /// Defaults to <see cref="AnnotationTarget.Issue"/>.
    /// </summary>
    [Parameter(Required = true)]
    public AnnotationTarget Target { get; set; } = AnnotationTarget.Issue;

    /// <summary>
    /// Gets or sets the GitHub Discussions category name used when
    /// <see cref="Target"/> is <see cref="AnnotationTarget.Discussion"/>.
    /// Must match an existing category name in the target repository.
    /// </summary>
    [Parameter]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets a comma-separated list of label names to pre-populate on new issues.
    /// Only applies when <see cref="Target"/> is <see cref="AnnotationTarget.Issue"/>.
    /// </summary>
    [Parameter]
    public string? Labels { get; set; }

    /// <summary>
    /// Gets or sets the prefix prepended to the page title when constructing the
    /// GitHub Issue or Discussion title. Defaults to <c>"Feedback:"</c>.
    /// </summary>
    [Parameter]
    public string TitlePrefix { get; set; } = "Feedback:";

    /// <summary>
    /// Gets or sets the CSS selector used to identify the content area where
    /// text selection is enabled. Defaults to <c>"article"</c>.
    /// </summary>
    [Parameter]
    public string ContentSelector { get; set; } = "article";

    /// <summary>
    /// Gets or sets the text or icon displayed in the floating trigger button
    /// that appears when the user selects text. Defaults to <c>"💬"</c>.
    /// </summary>
    [Parameter]
    public string ButtonText { get; set; } = "💬";

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"atoll-annotations\"");
        WriteHtml($" data-repo=\"{Encode(Repo)}\"");
        WriteHtml($" data-target=\"{Encode(Target.ToDataValue())}\"");
        if (Category is not null)
        {
            WriteHtml($" data-category=\"{Encode(Category)}\"");
        }
        if (Labels is not null)
        {
            WriteHtml($" data-labels=\"{Encode(Labels)}\"");
        }
        WriteHtml($" data-title-prefix=\"{Encode(TitlePrefix)}\"");
        WriteHtml($" data-content-selector=\"{Encode(ContentSelector)}\"");
        WriteHtml($" data-button-text=\"{Encode(ButtonText)}\"");
        WriteHtml("></div>");

        return Task.CompletedTask;
    }

    private static string Encode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
