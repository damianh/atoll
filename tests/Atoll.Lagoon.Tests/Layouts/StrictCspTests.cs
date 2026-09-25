using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Lagoon.Configuration;
using Atoll.Lagoon.I18n;
using Atoll.Lagoon.Islands;
using Atoll.Lagoon.Layouts;
using Atoll.Lagoon.Markdown;
using Atoll.Lagoon.Navigation;
using Atoll.Lagoon.Versioning;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Tests.Shared;

namespace Atoll.Lagoon.Tests.Layouts;

/// <summary>
/// Verifies Lagoon output works under a strict Content-Security-Policy (<c>script-src 'self'</c>).
/// </summary>
public sealed class StrictCspTests
{
    private static DocsConfig MakeFullConfig() => new()
    {
        Title = "Docs",
        EnableMermaid = true,
        Banner = new BannerConfig { Content = "Announcement", Dismissible = true, LinkHref = "/news", LinkText = "Read" },
        Locales = new Dictionary<string, LocaleConfig>
        {
            ["root"] = new() { Label = "English", Lang = "en" },
            ["fr"] = new() { Label = "French", Lang = "fr" },
        },
        Versions = new Dictionary<string, VersionConfig>
        {
            ["current"] = new() { Label = "Latest", Slug = "current" },
            ["v1.0"] = new() { Label = "v1.0", Slug = "v1.0" },
        },
    };

    private static RenderFragment MarkdownBody()
    {
        var markdown = """
            # Title

            ## Install

            ```csharp title="Program.cs"
            var x = 1;
            ```

            ```
            plain block
            ```

            ```mermaid
            graph TD; A-->B;
            ```
            """;
        var html = DocsMarkdownRenderer.Render(
            markdown,
            new DocsMarkdownOptions { EnableSyntaxHighlighting = true, EnableMermaid = true }).Html;
        return RenderFragment.FromHtml(html);
    }

    private static async Task<string> RenderAsync<TLayout>(Dictionary<string, object?> props)
        where TLayout : IAtollComponent, new()
    {
        var destination = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TLayout>(
            destination, props, SlotCollection.FromDefault(MarkdownBody()));
        return destination.GetOutput();
    }

    [Fact]
    public async Task DocsLayoutWithAllFeaturesShouldHaveNoInlineScriptsOrHandlers()
    {
        var sidebar = new[]
        {
            new ResolvedSidebarItem("Introduction", "/intro/", true, null),
            new ResolvedSidebarItem("Guides", false, null, false,
            [
                new ResolvedSidebarItem("One", "/guides/one/", false, null),
            ]),
        };
        var html = await RenderAsync<DocsLayout>(new Dictionary<string, object?>
        {
            ["Config"] = MakeFullConfig(),
            ["PageTitle"] = "Intro",
            ["Headings"] = new[] { new MarkdownHeading(2, "Install", "install") },
            ["SidebarItems"] = sidebar,
            ["CurrentPath"] = "/fr/intro",
        });

        html.ShouldContain("class=\"language-picker\"");
        html.ShouldContain("class=\"version-picker\"");
        html.ShouldContain("docs-banner-dismiss");
        html.ShouldContain("data-atoll-copy");
        html.ShouldContain("data-atoll-navigate");
        html.ShouldBeStrictCspCompatible();
    }

    [Fact]
    public async Task DocsLayoutShouldLoadExternalReplacementScripts()
    {
        var html = await RenderAsync<DocsLayout>(new Dictionary<string, object?>
        {
            ["Config"] = MakeFullConfig(),
            ["Headings"] = new[] { new MarkdownHeading(2, "Install", "install") },
            ["CurrentPath"] = "/fr/intro",
        });

        html.ShouldContain("<script src=\"/scripts/atoll-theme-init.js\"></script>");
        html.ShouldContain("<script src=\"/scripts/atoll-docs-actions.js\" defer></script>");
        html.ShouldContain("<script src=\"/scripts/atoll-sidebar-restore.js\"></script>");
        html.ShouldContain("<script src=\"/scripts/atoll-sidebar-scroll-restore.js\"></script>");
        html.ShouldContain("<script src=\"/scripts/atoll-docs-banner.js\"></script>");
        html.ShouldContain("<script src=\"/scripts/atoll-docs-toc.js\" defer></script>");
    }

    [Fact]
    public async Task ThemeInitScriptShouldBeRenderBlockingInHead()
    {
        var html = await RenderAsync<DocsLayout>(new Dictionary<string, object?>
        {
            ["Config"] = new DocsConfig { Title = "Docs" },
        });

        var head = html[..html.IndexOf("</head>", StringComparison.Ordinal)];
        head.ShouldContain("<script src=\"/scripts/atoll-theme-init.js\"></script>");
    }

    [Fact]
    public async Task SplashLayoutWithBannerShouldHaveNoInlineScriptsOrHandlers()
    {
        var html = await RenderAsync<SplashLayout>(new Dictionary<string, object?>
        {
            ["Config"] = MakeFullConfig(),
            ["PageTitle"] = "Home",
            ["CurrentPath"] = "/",
        });

        html.ShouldContain("docs-banner-dismiss");
        html.ShouldContain("<script src=\"/scripts/atoll-docs-banner.js\"></script>");
        html.ShouldBeStrictCspCompatible();
    }

    [Theory]
    [InlineData("scripts/atoll-theme-init.js", "atoll-theme")]
    [InlineData("scripts/atoll-sidebar-restore.js", "sl-sidebar-restore")]
    [InlineData("scripts/atoll-sidebar-scroll-restore.js", "scrollTop")]
    [InlineData("scripts/atoll-docs-banner.js", "data-dismiss-key")]
    [InlineData("scripts/atoll-docs-toc.js", "aria-current")]
    [InlineData("scripts/atoll-docs-actions.js", "data-atoll-copy")]
    [InlineData("scripts/atoll-docs-actions.js", "data-atoll-navigate")]
    public void ReplacementAssetsShouldBeEmbedded(string outputPath, string expected)
    {
        var asset = new LagoonIslandAssetProvider().GetAssets().Single(a => a.OutputPath == outputPath);
        using var stream = asset.ResourceAssembly.GetManifestResourceStream(asset.ResourceName);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream);
        reader.ReadToEnd().ShouldContain(expected);
    }
}
