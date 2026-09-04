using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Components;

public sealed class LinkButtonTests
{
    private static async Task<string> RenderLinkButtonAsync(
        string href,
        string label,
        LinkButtonVariant variant = LinkButtonVariant.Primary,
        IconName? iconName = null,
        IconPlacement iconPlacement = IconPlacement.Start,
        string? target = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Href"] = href,
            ["Label"] = label,
            ["Variant"] = variant,
            ["IconName"] = iconName,
            ["IconPlacement"] = iconPlacement,
            ["Target"] = target,
        };
        await ComponentRenderer.RenderComponentAsync<LinkButton>(destination, props);
        return destination.GetOutput();
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

    [Fact]
    public async Task ShouldOmitTargetAndRelByDefault()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go");

        html.ShouldNotContain("target=");
        html.ShouldNotContain("rel=");
    }

    [Fact]
    public async Task ShouldRenderTargetWhenProvided()
    {
        var html = await RenderLinkButtonAsync("https://example.com/", "Open", target: "_blank");

        html.ShouldContain("target=\"_blank\"");
    }

    [Fact]
    public async Task ShouldAddRelNoopenerNoreferrerForBlankTarget()
    {
        var html = await RenderLinkButtonAsync("https://example.com/", "Open", target: "_blank");

        html.ShouldContain("rel=\"noopener noreferrer\"");
    }

    [Fact]
    public async Task ShouldNotAddRelForNonBlankTarget()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", target: "myframe");

        html.ShouldContain("target=\"myframe\"");
        html.ShouldNotContain("rel=");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTarget()
    {
        var html = await RenderLinkButtonAsync("/docs", "Go", target: "\"><script>");

        html.ShouldNotContain("<script>");
        html.ShouldContain("target=\"&quot;&gt;&lt;script&gt;\"");
    }
}
