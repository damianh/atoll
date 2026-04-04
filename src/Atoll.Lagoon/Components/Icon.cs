using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders an inline SVG icon from the built-in <see cref="IconSet"/>.
/// </summary>
public sealed class Icon : AtollComponent
{
    /// <summary>Gets or sets the icon name to render.</summary>
    [Parameter(Required = true)]
    public IconName Name { get; set; }

    /// <summary>Gets or sets an accessibility label for screen readers. When <c>null</c>, the icon is hidden from screen readers.</summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>Gets or sets the CSS size value (width and height). Defaults to <c>"1em"</c>.</summary>
    [Parameter]
    public string Size { get; set; } = "1em";

    /// <summary>Gets or sets the CSS color value. When <c>null</c>, inherits <c>currentColor</c>.</summary>
    [Parameter]
    public string? Color { get; set; }

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context)
    {
        if (!IconSet.TryGetSvgContent(Name, out var svgContent))
        {
            return Task.CompletedTask;
        }

        var fill = Color is not null ? HtmlEncode(Color) : "currentColor";
        var size = HtmlEncode(Size);

        if (Label is not null)
        {
            WriteHtml(
                $"<svg class=\"icon\" role=\"img\" aria-label=\"{HtmlEncode(Label)}\" " +
                $"width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" " +
                $"fill=\"{fill}\" stroke=\"none\" xmlns=\"http://www.w3.org/2000/svg\">");
        }
        else
        {
            WriteHtml(
                $"<svg class=\"icon\" role=\"img\" aria-hidden=\"true\" " +
                $"width=\"{size}\" height=\"{size}\" viewBox=\"0 0 24 24\" " +
                $"fill=\"{fill}\" stroke=\"none\" xmlns=\"http://www.w3.org/2000/svg\">");
        }

        WriteHtml(svgContent!);
        WriteHtml("</svg>");

        return Task.CompletedTask;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
