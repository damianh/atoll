using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class LinkButtonTests
{
    private static async Task<string> RenderLinkButtonAsync(
        string href,
        string label,
        LinkButtonVariant variant = LinkButtonVariant.Primary,
        IconName? iconName = null,
        IconPlacement iconPlacement = IconPlacement.Start)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Href"] = href,
            ["Label"] = label,
            ["Variant"] = variant,
            ["IconName"] = iconName,
            ["IconPlacement"] = iconPlacement,
        };
        await ComponentRenderer.RenderComponentAsync<LinkButton>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderAnchorElement()
    {
        var html = await RenderLinkButtonAsync("/docs", "Get started");

        html.ShouldContain("<a ");
        html.ShouldContain("</a>");
    }

    [Fact]
    public async Task ShouldRenderHref()
    {
        var html = await RenderLinkButtonAsync("/docs", "Get started");

        html.ShouldContain("href=\"/docs\"");
    }

    [Fact]
    public async Task ShouldRenderLabelText()
    {
        var html = await RenderLinkButtonAsync("/docs", "Get started");

        html.ShouldContain("Get started");
    }

    [Fact]
    public async Task ShouldApplyPrimaryVariantClass()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", LinkButtonVariant.Primary);

        html.ShouldContain("link-button-primary");
    }

    [Fact]
    public async Task ShouldApplySecondaryVariantClass()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", LinkButtonVariant.Secondary);

        html.ShouldContain("link-button-secondary");
    }

    [Fact]
    public async Task ShouldApplyMinimalVariantClass()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", LinkButtonVariant.Minimal);

        html.ShouldContain("link-button-minimal");
    }

    [Fact]
    public async Task ShouldRenderIconAtStartByDefault()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", iconName: IconName.Star, iconPlacement: IconPlacement.Start);

        var iconIndex = html.IndexOf("<svg", StringComparison.Ordinal);
        var textIndex = html.IndexOf("Go", StringComparison.Ordinal);
        iconIndex.ShouldBeLessThan(textIndex);
    }

    [Fact]
    public async Task ShouldRenderIconAtEnd()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", iconName: IconName.ArrowRight, iconPlacement: IconPlacement.End);

        var textIndex = html.IndexOf("Go", StringComparison.Ordinal);
        var iconIndex = html.IndexOf("<svg", StringComparison.Ordinal);
        textIndex.ShouldBeLessThan(iconIndex);
    }

    [Fact]
    public async Task ShouldOmitIconWhenNotProvided()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go");

        html.ShouldNotContain("<svg");
    }

    [Fact]
    public async Task ShouldHtmlEncodeHref()
    {
        var html = await RenderLinkButtonAsync("/docs?a=1&b=2", "Go");

        html.ShouldContain("href=\"/docs?a=1&amp;b=2\"");
    }

    [Fact]
    public async Task ShouldHtmlEncodeLabelText()
    {
        var html = await RenderLinkButtonAsync("/docs", "A <b>bold</b> label");

        html.ShouldContain("A &lt;b&gt;bold&lt;/b&gt; label");
    }
}
