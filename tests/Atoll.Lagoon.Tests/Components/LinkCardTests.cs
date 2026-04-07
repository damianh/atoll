using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Components;

public sealed class LinkCardTests
{
    private static async Task<string> RenderLinkCardAsync(
        string title,
        string href,
        string? description = null,
        IconName? iconName = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Title"] = title,
            ["Href"] = href,
            ["Description"] = description,
            ["IconName"] = iconName,
        };
        await ComponentRenderer.RenderComponentAsync<LinkCard>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderDescriptionWhenProvided()
    {
        var html = await RenderLinkCardAsync("Guide", "/guide", description: "Learn the basics");

        html.ShouldContain("Learn the basics");
        html.ShouldContain("link-card-description");
    }

    [Fact]
    public async Task ShouldOmitDescriptionWhenNull()
    {
        var html = await RenderLinkCardAsync("Guide", "/guide");

        html.ShouldNotContain("link-card-description");
    }

    [Fact]
    public async Task ShouldRenderIconWhenProvided()
    {
        var html = await RenderLinkCardAsync("Guide", "/guide", iconName: IconName.Document);

        html.ShouldContain("<svg");
    }

    [Fact]
    public async Task ShouldOmitIconWhenNull()
    {
        var html = await RenderLinkCardAsync("Guide", "/guide");

        html.ShouldNotContain("<svg");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTitleText()
    {
        var html = await RenderLinkCardAsync("A <script>evil</script> title", "/guide");

        html.ShouldContain("A &lt;script&gt;evil&lt;/script&gt; title");
    }

    [Fact]
    public async Task ShouldHtmlEncodeDescriptionText()
    {
        var html = await RenderLinkCardAsync("Guide", "/guide", description: "Use & enjoy");

        html.ShouldContain("Use &amp; enjoy");
    }

    [Fact]
    public async Task ShouldHtmlEncodeHref()
    {
        var html = await RenderLinkCardAsync("Guide", "/guide?a=1&b=2");

        html.ShouldContain("href=\"/guide?a=1&amp;b=2\"");
    }
}
