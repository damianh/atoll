using Atoll.Swell.Configuration;
using YamlDotNet.Serialization;

namespace Atoll.Swell.Markdown;

/// <summary>
/// Per-slide configuration parsed from the YAML frontmatter at the top of each slide chunk.
/// </summary>
public sealed class SlideConfig
{
    /// <summary>
    /// Gets or sets the layout name for this slide.
    /// Valid values: "default", "cover", "center", "two-cols", "image-right", "image-left",
    /// "section", "end". Default: "default".
    /// </summary>
    [YamlMember(Alias = "layout")]
    public string Layout { get; set; } = "default";

    /// <summary>
    /// Gets or sets the background image URL or CSS colour for this slide.
    /// Applied as a CSS <c>background</c> property on the slide element.
    /// </summary>
    [YamlMember(Alias = "background")]
    public string? Background { get; set; }

    /// <summary>
    /// Gets or sets additional CSS classes to apply to the slide element.
    /// </summary>
    [YamlMember(Alias = "class")]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets the transition override for this slide.
    /// When <c>null</c> the deck-level default is used.
    /// </summary>
    [YamlMember(Alias = "transition")]
    public TransitionType? Transition { get; set; }

    /// <summary>
    /// Gets or sets whether to show the slide number on this specific slide.
    /// When <c>null</c>, the deck-level <see cref="DeckConfig.SlideNumbers"/> setting is used.
    /// </summary>
    [YamlMember(Alias = "slideNumber")]
    public bool? SlideNumber { get; set; }
}
