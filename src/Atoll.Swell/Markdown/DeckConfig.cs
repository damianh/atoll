using Atoll.Swell.Configuration;
using YamlDotNet.Serialization;

namespace Atoll.Swell.Markdown;

/// <summary>
/// Deck-wide configuration parsed from the headmatter (the first YAML frontmatter block)
/// of a Swell Markdown file.
/// </summary>
public sealed class DeckConfig
{
    /// <summary>Gets or sets the deck title shown in the browser tab and cover slide.</summary>
    [YamlMember(Alias = "title")]
    public string Title { get; set; } = "";

    /// <summary>Gets or sets the aspect ratio of the slide container. Default: 16:9.</summary>
    [YamlMember(Alias = "aspectRatio")]
    public AspectRatio AspectRatio { get; set; } = AspectRatio.Ratio16x9;

    /// <summary>Gets or sets the default transition between slides. Default: None.</summary>
    [YamlMember(Alias = "transition")]
    public TransitionType Transition { get; set; } = TransitionType.None;

    /// <summary>
    /// Gets or sets whether slide numbers (current / total) are shown globally.
    /// Default: <c>true</c>.
    /// </summary>
    [YamlMember(Alias = "slideNumbers")]
    public bool SlideNumbers { get; set; } = true;

    /// <summary>
    /// Gets or sets the export formats to generate at build time.
    /// Example: <c>["pdf", "pptx"]</c>. Default: empty (no export).
    /// </summary>
    [YamlMember(Alias = "export")]
    public IReadOnlyList<string> Export { get; set; } = [];

    /// <summary>Gets or sets the theme name. Default: "default".</summary>
    [YamlMember(Alias = "theme")]
    public string Theme { get; set; } = "default";
}
