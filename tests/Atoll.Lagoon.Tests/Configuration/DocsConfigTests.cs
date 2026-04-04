using Atoll.Lagoon.Configuration;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Configuration;

public sealed class DocsConfigTests
{
    [Fact]
    public void DefaultConfigShouldHaveSensibleDefaults()
    {
        var config = new DocsConfig();

        config.Title.ShouldBe("");
        config.Description.ShouldBe("");
        config.LogoSrc.ShouldBeNull();
        config.LogoAlt.ShouldBe("");
        config.Sidebar.ShouldBeEmpty();
        config.Social.ShouldBeEmpty();
        config.CustomCss.ShouldBeEmpty();
        config.EnableMermaid.ShouldBeFalse();
        config.BasePath.ShouldBe("");
        config.TableOfContents.ShouldNotBeNull();
        config.TableOfContents.MinHeadingLevel.ShouldBe(2);
        config.TableOfContents.MaxHeadingLevel.ShouldBe(3);
    }

    [Fact]
    public void SidebarItemShouldSupportManualLink()
    {
        var item = new SidebarItem
        {
            Label = "Getting Started",
            Link = "/docs/getting-started/"
        };

        item.Label.ShouldBe("Getting Started");
        item.Link.ShouldBe("/docs/getting-started/");
        item.AutoGenerate.ShouldBeNull();
        item.Items.ShouldBeEmpty();
        item.Collapsed.ShouldBeFalse();
        item.Badge.ShouldBeNull();
    }

    [Fact]
    public void SidebarItemShouldSupportAutoGenerate()
    {
        var item = new SidebarItem
        {
            Label = "Guides",
            AutoGenerate = "guides"
        };

        item.AutoGenerate.ShouldBe("guides");
        item.Link.ShouldBeNull();
    }

    [Fact]
    public void SidebarItemShouldSupportNestedChildren()
    {
        var parent = new SidebarItem
        {
            Label = "Reference",
            Items =
            [
                new SidebarItem { Label = "API", Link = "/docs/api/" },
                new SidebarItem { Label = "CLI", Link = "/docs/cli/" }
            ]
        };

        parent.Items.Count.ShouldBe(2);
        parent.Items[0].Label.ShouldBe("API");
        parent.Items[1].Label.ShouldBe("CLI");
    }

    [Fact]
    public void SidebarItemShouldSupportBadge()
    {
        var item = new SidebarItem
        {
            Label = "New Feature",
            Link = "/docs/new/",
            Badge = "New"
        };

        item.Badge.ShouldBe("New");
    }

    [Fact]
    public void SidebarItemShouldSupportCollapsed()
    {
        var item = new SidebarItem
        {
            Label = "Advanced",
            Collapsed = true,
            Items = [new SidebarItem { Label = "Child", Link = "/docs/child/" }]
        };

        item.Collapsed.ShouldBeTrue();
    }

    [Fact]
    public void SocialLinkShouldStoreProperties()
    {
        var link = new SocialLink("GitHub", "https://github.com/example", SocialIcon.GitHub);

        link.Label.ShouldBe("GitHub");
        link.Url.ShouldBe("https://github.com/example");
        link.Icon.ShouldBe(SocialIcon.GitHub);
    }

    [Fact]
    public void SocialLinkShouldThrowOnNullLabel()
    {
        Should.Throw<ArgumentException>(() => new SocialLink(null!, "https://github.com", SocialIcon.GitHub));
    }

    [Fact]
    public void SocialLinkShouldThrowOnEmptyUrl()
    {
        Should.Throw<ArgumentException>(() => new SocialLink("GitHub", "", SocialIcon.GitHub));
    }

    [Fact]
    public void TableOfContentsConfigShouldAllowCustomLevels()
    {
        var toc = new TableOfContentsConfig
        {
            MinHeadingLevel = 2,
            MaxHeadingLevel = 4
        };

        toc.MinHeadingLevel.ShouldBe(2);
        toc.MaxHeadingLevel.ShouldBe(4);
    }

    [Fact]
    public void DocsConfigShouldRepresentStarlightEquivalentStructure()
    {
        // Verify we can represent the same structure as Starlight's starlight() config.
        var config = new DocsConfig
        {
            Title = "My Docs",
            Description = "Documentation site",
            LogoSrc = "/logo.svg",
            LogoAlt = "My Logo",
            Social =
            [
                new SocialLink("GitHub", "https://github.com/example", SocialIcon.GitHub),
                new SocialLink("Discord", "https://discord.gg/example", SocialIcon.Discord),
            ],
            Sidebar =
            [
                new SidebarItem
                {
                    Label = "Getting Started",
                    Items =
                    [
                        new SidebarItem { Label = "Introduction", Link = "/docs/intro/" },
                        new SidebarItem { Label = "Installation", Link = "/docs/install/" },
                    ]
                },
                new SidebarItem
                {
                    Label = "Guides",
                    AutoGenerate = "guides",
                    Collapsed = true
                }
            ],
            TableOfContents = new TableOfContentsConfig { MinHeadingLevel = 2, MaxHeadingLevel = 3 },
            EnableMermaid = true,
            BasePath = "/docs"
        };

        config.Title.ShouldBe("My Docs");
        config.Social.Count.ShouldBe(2);
        config.Sidebar.Count.ShouldBe(2);
        config.Sidebar[0].Items.Count.ShouldBe(2);
        config.Sidebar[1].AutoGenerate.ShouldBe("guides");
        config.Sidebar[1].Collapsed.ShouldBeTrue();
        config.EnableMermaid.ShouldBeTrue();
        config.BasePath.ShouldBe("/docs");
    }
}
