using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;

namespace Atoll.Lagoon.Tests.Components;

public sealed class HeroTests
{
    private static async Task<string> RenderHeroAsync(
        string title,
        string? tagline = null,
        string? imageSrc = null,
        string imageAlt = "",
        IReadOnlyList<HeroAction>? actions = null)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Title"] = title,
            ["Tagline"] = tagline,
            ["ImageSrc"] = imageSrc,
            ["ImageAlt"] = imageAlt,
            ["Actions"] = actions ?? []
        };
        await ComponentRenderer.RenderComponentAsync<Hero>(destination, props);
        return destination.GetOutput();
    }

    // --- Title ---

    [Fact]
    public async Task ShouldHtmlEncodeTitleText()
    {
        var html = await RenderHeroAsync("<script>xss</script>");

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    // --- Tagline ---

    [Fact]
    public async Task ShouldRenderTaglineWhenProvided()
    {
        var html = await RenderHeroAsync("Title", tagline: "The fastest docs framework");

        html.ShouldContain("<p class=\"hero-tagline\">");
        html.ShouldContain("The fastest docs framework");
    }

    [Fact]
    public async Task ShouldNotRenderTaglineWhenNull()
    {
        var html = await RenderHeroAsync("Title", tagline: null);

        html.ShouldNotContain("hero-tagline");
    }

    // --- Image ---

    [Fact]
    public async Task ShouldRenderImageWhenSrcProvided()
    {
        var html = await RenderHeroAsync("Title", imageSrc: "/images/hero.png", imageAlt: "Hero illustration");

        html.ShouldContain("<img src=\"/images/hero.png\"");
        html.ShouldContain("alt=\"Hero illustration\"");
    }

    [Fact]
    public async Task ShouldNotRenderImageWhenSrcIsNull()
    {
        var html = await RenderHeroAsync("Title");

        html.ShouldNotContain("<img");
        html.ShouldNotContain("hero-image");
    }

    [Fact]
    public async Task ShouldHtmlEncodeImageSrcAndAlt()
    {
        var html = await RenderHeroAsync("Title", imageSrc: "/img?a=1&b=2", imageAlt: "A \"quoted\" alt");

        html.ShouldContain("src=\"/img?a=1&amp;b=2\"");
        html.ShouldContain("alt=\"A &quot;quoted&quot; alt\"");
    }

    // --- Actions ---

    [Fact]
    public async Task ShouldRenderPrimaryAction()
    {
        var actions = new[] { new HeroAction("Get Started", "/docs/start/") };
        var html = await RenderHeroAsync("Title", actions: actions);

        html.ShouldContain("hero-action-primary");
        html.ShouldContain("href=\"/docs/start/\"");
        html.ShouldContain("Get Started");
    }

    [Fact]
    public async Task ShouldRenderSecondaryAction()
    {
        var actions = new[] { new HeroAction("Learn More", "/docs/about/", HeroActionVariant.Secondary) };
        var html = await RenderHeroAsync("Title", actions: actions);

        html.ShouldContain("hero-action-secondary");
        html.ShouldContain("href=\"/docs/about/\"");
        html.ShouldContain("Learn More");
    }

    [Fact]
    public async Task ShouldRenderMultipleActions()
    {
        var actions = new[]
        {
            new HeroAction("Get Started", "/start/"),
            new HeroAction("View on GitHub", "/github/", HeroActionVariant.Secondary)
        };
        var html = await RenderHeroAsync("Title", actions: actions);

        html.ShouldContain("hero-action-primary");
        html.ShouldContain("hero-action-secondary");
        html.ShouldContain("Get Started");
        html.ShouldContain("View on GitHub");
    }

    [Fact]
    public async Task ShouldNotRenderActionsContainerWhenEmpty()
    {
        var html = await RenderHeroAsync("Title", actions: []);

        html.ShouldNotContain("hero-actions");
    }

    [Fact]
    public async Task ShouldHtmlEncodeActionLabel()
    {
        var actions = new[] { new HeroAction("<b>Start</b>", "/start/") };
        var html = await RenderHeroAsync("Title", actions: actions);

        html.ShouldNotContain("<b>Start</b>");
        html.ShouldContain("&lt;b&gt;");
    }
}
