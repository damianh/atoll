using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class IconTests
{
    private static async Task<string> RenderIconAsync(
        IconName name,
        string? label = null,
        string size = "1em",
        string? color = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Name"] = name,
            ["Label"] = label,
            ["Size"] = size,
            ["Color"] = color,
        };
        await ComponentRenderer.RenderComponentAsync<Icon>(destination, props);
        return destination.GetOutput();
    }

    // --- Structure ---

    [Fact]
    public async Task ShouldRenderSvgElement()
    {
        var html = await RenderIconAsync(IconName.Star);

        html.ShouldContain("<svg");
        html.ShouldContain("</svg>");
    }

    [Fact]
    public async Task ShouldRenderIconClass()
    {
        var html = await RenderIconAsync(IconName.Star);

        html.ShouldContain("class=\"icon\"");
    }

    [Fact]
    public async Task ShouldRenderViewBox()
    {
        var html = await RenderIconAsync(IconName.Star);

        html.ShouldContain("viewBox=\"0 0 24 24\"");
    }

    // --- Accessibility ---

    [Fact]
    public async Task ShouldRenderAriaHiddenWhenNoLabel()
    {
        var html = await RenderIconAsync(IconName.Star);

        html.ShouldContain("aria-hidden=\"true\"");
        html.ShouldNotContain("aria-label");
    }

    [Fact]
    public async Task ShouldRenderAriaLabelWhenLabelProvided()
    {
        var html = await RenderIconAsync(IconName.Star, label: "Star icon");

        html.ShouldContain("aria-label=\"Star icon\"");
        html.ShouldNotContain("aria-hidden");
    }

    [Fact]
    public async Task ShouldHtmlEncodeLabelText()
    {
        var html = await RenderIconAsync(IconName.Star, label: "A \"quoted\" label");

        html.ShouldContain("aria-label=\"A &quot;quoted&quot; label\"");
    }

    // --- Size ---

    [Fact]
    public async Task ShouldApplyDefaultSize()
    {
        var html = await RenderIconAsync(IconName.Star);

        html.ShouldContain("width=\"1em\"");
        html.ShouldContain("height=\"1em\"");
    }

    [Fact]
    public async Task ShouldApplyCustomSize()
    {
        var html = await RenderIconAsync(IconName.Star, size: "2rem");

        html.ShouldContain("width=\"2rem\"");
        html.ShouldContain("height=\"2rem\"");
    }

    // --- Color ---

    [Fact]
    public async Task ShouldUseCurrentColorWhenNoColorProvided()
    {
        var html = await RenderIconAsync(IconName.Star);

        html.ShouldContain("fill=\"currentColor\"");
    }

    [Fact]
    public async Task ShouldApplyCustomColor()
    {
        var html = await RenderIconAsync(IconName.Star, color: "goldenrod");

        html.ShouldContain("fill=\"goldenrod\"");
    }

    // --- Icon content ---

    [Fact]
    public async Task ShouldRenderSvgPathDataForKnownIcon()
    {
        var html = await RenderIconAsync(IconName.Check);

        // Check icon has a polyline
        html.ShouldContain("<polyline");
    }

    [Fact]
    public async Task ShouldRenderEmptyForUnknownIconValue()
    {
        // Cast an out-of-range int to simulate an unknown enum value
        var unknownIcon = (IconName)999;
        var html = await RenderIconAsync(unknownIcon);

        html.ShouldBeEmpty();
    }

    // --- All enum values have entries ---

    [Fact]
    public async Task ShouldRenderSvgForAllDefinedIconNames()
    {
        foreach (var name in Enum.GetValues<IconName>())
        {
            var html = await RenderIconAsync(name);
            html.ShouldNotBeEmpty($"Expected SVG for IconName.{name} but got empty output");
            html.ShouldContain("<svg");
        }
    }
}
