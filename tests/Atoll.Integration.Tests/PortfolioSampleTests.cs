using System.Net;
using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing;
using Atoll.Samples.Portfolio.Components;
using Atoll.Samples.Portfolio.Islands;
using Atoll.Samples.Portfolio.Layouts;
using Atoll.Samples.Portfolio.Pages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Atoll.Integration.Tests;

/// <summary>
/// End-to-end integration tests for the portfolio sample site.
/// Verifies that all pages, components, layouts, and islands with different
/// client directives (client:load, client:visible, client:media) work correctly.
/// </summary>
public sealed class PortfolioSampleTests
{
    // ── Helper to render a page to HTML string ──

    private static async Task<string> RenderPageAsync<TPage>(
        IReadOnlyDictionary<string, object?>? props = null)
        where TPage : IAtollComponent, new()
    {
        var renderer = new PageRenderer();
        var pageProps = props ?? new Dictionary<string, object?>();

        var pageType = typeof(TPage);
        if (LayoutResolver.HasLayout(pageType))
        {
            var pageFragment = RenderFragment.FromAsync(async dest =>
            {
                await ComponentRenderer.RenderComponentAsync<TPage>(dest, pageProps);
            });
            var wrappedFragment = LayoutResolver.WrapWithLayouts(pageType, pageFragment, pageProps);
            var result = await renderer.RenderPageAsync(ctx =>
            {
                return ctx.RenderAsync(wrappedFragment).AsTask();
            });
            return result.Html;
        }
        else
        {
            var result = await renderer.RenderPageAsync<TPage>(pageProps);
            return result.Html;
        }
    }

    private static async Task<string> RenderComponentAsync<TComponent>(
        IReadOnlyDictionary<string, object?>? props = null)
        where TComponent : IAtollComponent, new()
    {
        var dest = new StringRenderDestination();
        var componentProps = props ?? new Dictionary<string, object?>();
        await ComponentRenderer.RenderComponentAsync<TComponent>(dest, componentProps);
        return dest.GetOutput();
    }

    // ── Layout rendering tests ──

    [Fact]
    public async Task PortfolioLayoutShouldRenderHtmlStructure()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("<!DOCTYPE html>");
        html.ShouldContain("<html lang=\"en\">");
        html.ShouldContain("<meta charset=\"utf-8\"");
        html.ShouldContain("<meta name=\"viewport\"");
        html.ShouldContain("<header");
        html.ShouldContain("<main>");
        html.ShouldContain("<footer");
        html.ShouldContain("</html>");
    }

    [Fact]
    public async Task PortfolioLayoutShouldContainNavigationLinks()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("href=\"/projects\"");
        html.ShouldContain("href=\"/about\"");
        html.ShouldContain("href=\"/contact\"");
    }

    [Fact]
    public async Task PortfolioLayoutShouldRenderSiteNameInHeader()
    {
        var html = await RenderPageAsync<IndexPage>();
        html.ShouldContain("&lt;dev /&gt;");
    }

    [Fact]
    public async Task PortfolioLayoutShouldRenderFooterWithSocialLinks()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("Built with Atoll");
        html.ShouldContain("href=\"https://github.com\"");
        html.ShouldContain("href=\"https://linkedin.com\"");
        html.ShouldContain("href=\"https://twitter.com\"");
    }

    [Fact]
    public async Task PortfolioLayoutShouldIncludeDarkThemeStyles()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("--color-bg: #0f172a");
        html.ShouldContain("--color-primary: #38bdf8");
        html.ShouldContain("--color-accent: #a78bfa");
    }

    // ── Index page tests ──

    [Fact]
    public async Task IndexPageShouldRenderHeroSection()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("Hello, I'm");
        html.ShouldContain("Alex Chen");
        html.ShouldContain("Full-Stack .NET Developer");
    }

    [Fact]
    public async Task IndexPageShouldRenderHeroIntroText()
    {
        var html = await RenderPageAsync<IndexPage>();
        html.ShouldContain("modern web applications");
    }

    [Fact]
    public async Task IndexPageShouldRenderCallToActionButtons()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("View My Work");
        html.ShouldContain("href=\"/projects\"");
        html.ShouldContain("Get in Touch");
        html.ShouldContain("href=\"/contact\"");
    }

    [Fact]
    public async Task IndexPageShouldRenderFeaturedProjectsSection()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("Featured Projects");
        html.ShouldContain("A selection of my recent work");
    }

    [Fact]
    public async Task IndexPageShouldRenderFeaturedProjectCards()
    {
        var html = await RenderPageAsync<IndexPage>();

        html.ShouldContain("Atoll Framework");
        html.ShouldContain("Cloud Dashboard");
    }

    [Fact]
    public async Task IndexPageShouldRenderViewAllProjectsLink()
    {
        var html = await RenderPageAsync<IndexPage>();
        html.ShouldContain("View All Projects");
    }

    // ── Projects page tests ──

    [Fact]
    public async Task ProjectsPageShouldRenderHeading()
    {
        var html = await RenderPageAsync<ProjectsPage>();

        html.ShouldContain("<h1");
        html.ShouldContain("Projects");
    }

    [Fact]
    public async Task ProjectsPageShouldRenderAllSixProjects()
    {
        var html = await RenderPageAsync<ProjectsPage>();

        html.ShouldContain("Atoll Framework");
        html.ShouldContain("Cloud Dashboard");
        html.ShouldContain("E-Commerce API");
        html.ShouldContain("DevOps Toolkit");
        html.ShouldContain("Weather Station");
        html.ShouldContain("Markdown Editor");
    }

    [Fact]
    public async Task ProjectsPageShouldRenderProjectDescriptions()
    {
        var html = await RenderPageAsync<ProjectsPage>();

        html.ShouldContain(".NET-native web framework");
        html.ShouldContain("Real-time monitoring dashboard");
        html.ShouldContain("High-performance REST API");
    }

    [Fact]
    public async Task ProjectsPageShouldRenderTechnologyTags()
    {
        var html = await RenderPageAsync<ProjectsPage>();

        html.ShouldContain("C#");
        html.ShouldContain("ASP.NET Core");
        html.ShouldContain("Blazor WASM");
        html.ShouldContain("Docker");
        html.ShouldContain("PostgreSQL");
    }

    [Fact]
    public async Task ProjectsPageShouldRenderProjectLinks()
    {
        var html = await RenderPageAsync<ProjectsPage>();

        html.ShouldContain("Live Demo");
        html.ShouldContain("Source Code");
        html.ShouldContain("href=\"https://atoll.dev\"");
        html.ShouldContain("href=\"https://github.com/example/atoll\"");
    }

    [Fact]
    public async Task ProjectsPageShouldBeWrappedInLayout()
    {
        var html = await RenderPageAsync<ProjectsPage>();

        html.ShouldContain("<header");
        html.ShouldContain("<footer");
        html.ShouldContain("&lt;dev /&gt;");
    }

    // ── About page tests ──

    [Fact]
    public async Task AboutPageShouldRenderBiography()
    {
        var html = await RenderPageAsync<AboutPage>();

        html.ShouldContain("About Me");
        html.ShouldContain("full-stack developer");
        html.ShouldContain("8 years of experience");
    }

    [Fact]
    public async Task AboutPageShouldRenderSkillsBadges()
    {
        var html = await RenderPageAsync<AboutPage>();

        html.ShouldContain("Skills");
        html.ShouldContain("C#");
        html.ShouldContain("ASP.NET Core");
        html.ShouldContain("Blazor");
        html.ShouldContain("Docker");
        html.ShouldContain("TypeScript");
    }

    [Fact]
    public async Task AboutPageShouldRenderSkillLevels()
    {
        var html = await RenderPageAsync<AboutPage>();

        html.ShouldContain("Expert");
        html.ShouldContain("Advanced");
        html.ShouldContain("Intermediate");
    }

    [Fact]
    public async Task AboutPageShouldRenderImageGallerySection()
    {
        var html = await RenderPageAsync<AboutPage>();

        html.ShouldContain("Photo Gallery");
        html.ShouldContain("image-gallery");
    }

    [Fact]
    public async Task AboutPageShouldRenderGalleryPlaceholders()
    {
        var html = await RenderPageAsync<AboutPage>();

        // When no image URLs are provided, gallery renders 6 placeholder items
        html.ShouldContain("gallery-item");
    }

    [Fact]
    public async Task AboutPageShouldRenderGalleryLightboxContainer()
    {
        var html = await RenderPageAsync<AboutPage>();
        html.ShouldContain("gallery-lightbox");
    }

    // ── Contact page tests ──

    [Fact]
    public async Task ContactPageShouldRenderHeading()
    {
        var html = await RenderPageAsync<ContactPage>();

        html.ShouldContain("Get in Touch");
        html.ShouldContain("Fill out the form below");
    }

    [Fact]
    public async Task ContactPageShouldRenderContactForm()
    {
        var html = await RenderPageAsync<ContactPage>();

        html.ShouldContain("<form");
        html.ShouldContain("id=\"contact-form\"");
        html.ShouldContain("contact-name");
        html.ShouldContain("contact-email");
        html.ShouldContain("contact-subject");
        html.ShouldContain("contact-message");
        html.ShouldContain("Send Message");
    }

    [Fact]
    public async Task ContactPageShouldRenderFormInputsWithPlaceholders()
    {
        var html = await RenderPageAsync<ContactPage>();

        html.ShouldContain("placeholder=\"Your name\"");
        html.ShouldContain("placeholder=\"you@example.com\"");
        html.ShouldContain("placeholder=\"Project inquiry\"");
        html.ShouldContain("placeholder=\"Tell me about your project...\"");
    }

    [Fact]
    public async Task ContactPageShouldRenderRequiredFields()
    {
        var html = await RenderPageAsync<ContactPage>();

        // Name, email, and message are required
        var formSection = html;
        formSection.ShouldContain("required");
    }

    [Fact]
    public async Task ContactPageShouldRenderAlternateContactMethods()
    {
        var html = await RenderPageAsync<ContactPage>();

        html.ShouldContain("Other Ways to Reach Me");
        html.ShouldContain("alex@example.com");
        html.ShouldContain("github.com/example");
        html.ShouldContain("linkedin.com/in/example");
    }

    [Fact]
    public async Task ContactPageShouldRenderFormStatusContainer()
    {
        var html = await RenderPageAsync<ContactPage>();
        html.ShouldContain("form-status");
    }

    // ── HeroSection component tests ──

    [Fact]
    public async Task HeroSectionShouldRenderNameAndTagline()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "Test User",
            ["Tagline"] = "Software Engineer",
        };
        var html = await RenderComponentAsync<HeroSection>(props);

        html.ShouldContain("Test User");
        html.ShouldContain("Software Engineer");
        html.ShouldContain("Hello, I'm");
    }

    [Fact]
    public async Task HeroSectionShouldRenderIntroWhenProvided()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "Test",
            ["Tagline"] = "Dev",
            ["Intro"] = "I build cool things.",
        };
        var html = await RenderComponentAsync<HeroSection>(props);

        html.ShouldContain("I build cool things.");
    }

    [Fact]
    public async Task HeroSectionShouldSkipIntroWhenEmpty()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "Test",
            ["Tagline"] = "Dev",
            ["Intro"] = "",
        };
        var html = await RenderComponentAsync<HeroSection>(props);

        // Hero has "Hello, I'm" and tagline paragraphs (2 total).
        // When intro is empty, no third paragraph should be rendered.
        var pTagCount = CountOccurrences(html, "<p ");
        pTagCount.ShouldBe(2); // "Hello, I'm" and tagline only, no intro paragraph
    }

    [Fact]
    public async Task HeroSectionShouldRenderCallToActionButtons()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "Test",
            ["Tagline"] = "Dev",
        };
        var html = await RenderComponentAsync<HeroSection>(props);

        html.ShouldContain("View My Work");
        html.ShouldContain("href=\"/projects\"");
        html.ShouldContain("Get in Touch");
        html.ShouldContain("href=\"/contact\"");
    }

    // ── ProjectCard component tests ──

    [Fact]
    public async Task ProjectCardShouldRenderTitleAndDescription()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "My Project",
            ["Description"] = "A great project.",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldContain("<article");
        html.ShouldContain("My Project");
        html.ShouldContain("A great project.");
    }

    [Fact]
    public async Task ProjectCardShouldRenderTechnologies()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["Technologies"] = "C#, Docker, Redis",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldContain("C#");
        html.ShouldContain("Docker");
        html.ShouldContain("Redis");
    }

    [Fact]
    public async Task ProjectCardShouldSkipTechnologiesWhenEmpty()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["Technologies"] = "",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldNotContain("font-mono"); // Tech badges use font-mono
    }

    [Fact]
    public async Task ProjectCardShouldRenderDemoLink()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["DemoUrl"] = "https://demo.example.com",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldContain("Live Demo");
        html.ShouldContain("href=\"https://demo.example.com\"");
    }

    [Fact]
    public async Task ProjectCardShouldRenderSourceLink()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["SourceUrl"] = "https://github.com/example/test",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldContain("Source Code");
        html.ShouldContain("href=\"https://github.com/example/test\"");
    }

    [Fact]
    public async Task ProjectCardShouldSkipLinksWhenBothEmpty()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["DemoUrl"] = "",
            ["SourceUrl"] = "",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldNotContain("Live Demo");
        html.ShouldNotContain("Source Code");
    }

    [Fact]
    public async Task ProjectCardShouldRenderImageWhenProvided()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["ImageUrl"] = "/images/project.png",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldContain("<img");
        html.ShouldContain("src=\"/images/project.png\"");
        html.ShouldContain("alt=\"Test\"");
    }

    [Fact]
    public async Task ProjectCardShouldRenderPlaceholderWhenNoImage()
    {
        var props = new Dictionary<string, object?>
        {
            ["Title"] = "Test",
            ["Description"] = "Desc",
            ["ImageUrl"] = "",
        };
        var html = await RenderComponentAsync<ProjectCard>(props);

        html.ShouldNotContain("<img");
        html.ShouldContain("linear-gradient"); // Placeholder gradient
    }

    // ── SkillBadge component tests ──

    [Fact]
    public async Task SkillBadgeShouldRenderNameAndLevel()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "C#",
            ["Level"] = "Expert",
        };
        var html = await RenderComponentAsync<SkillBadge>(props);

        html.ShouldContain("C#");
        html.ShouldContain("Expert");
    }

    [Fact]
    public async Task SkillBadgeShouldUseSuccessColorForExpert()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "C#",
            ["Level"] = "Expert",
        };
        var html = await RenderComponentAsync<SkillBadge>(props);
        html.ShouldContain("var(--color-success)");
    }

    [Fact]
    public async Task SkillBadgeShouldUsePrimaryColorForAdvanced()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "Docker",
            ["Level"] = "Advanced",
        };
        var html = await RenderComponentAsync<SkillBadge>(props);
        html.ShouldContain("var(--color-primary)");
    }

    [Fact]
    public async Task SkillBadgeShouldUseAccentColorForIntermediateOrOther()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "TypeScript",
            ["Level"] = "Intermediate",
        };
        var html = await RenderComponentAsync<SkillBadge>(props);
        html.ShouldContain("var(--color-accent)");
    }

    [Fact]
    public async Task SkillBadgeShouldSkipLevelWhenEmpty()
    {
        var props = new Dictionary<string, object?>
        {
            ["Name"] = "Git",
            ["Level"] = "",
        };
        var html = await RenderComponentAsync<SkillBadge>(props);

        html.ShouldContain("Git");
        html.ShouldNotContain("font-mono"); // Level badge uses font-mono
    }

    // ── ContactForm island tests ──

    [Fact]
    public async Task ContactFormShouldRenderFormElements()
    {
        var html = await RenderComponentAsync<ContactForm>();

        html.ShouldContain("<form");
        html.ShouldContain("id=\"contact-form\"");
        html.ShouldContain("input");
        html.ShouldContain("textarea");
        html.ShouldContain("button");
    }

    [Fact]
    public async Task ContactFormShouldRenderAllFieldLabels()
    {
        var html = await RenderComponentAsync<ContactForm>();

        html.ShouldContain("Name");
        html.ShouldContain("Email");
        html.ShouldContain("Subject");
        html.ShouldContain("Message");
    }

    [Fact]
    public async Task ContactFormShouldRenderCustomActionUrl()
    {
        var props = new Dictionary<string, object?>
        {
            ["ActionUrl"] = "/api/v2/contact",
        };
        var html = await RenderComponentAsync<ContactForm>(props);

        html.ShouldContain("action=\"/api/v2/contact\"");
    }

    [Fact]
    public async Task ContactFormShouldRenderDefaultActionUrl()
    {
        var html = await RenderComponentAsync<ContactForm>();
        html.ShouldContain("action=\"/api/contact\"");
    }

    [Fact]
    public void ContactFormShouldHaveClientLoadDirective()
    {
        var directive = DirectiveExtractor.GetDirective(typeof(ContactForm));
        directive.ShouldNotBeNull();
        directive.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }

    [Fact]
    public void ContactFormShouldProvideClientModuleUrl()
    {
        var form = new ContactForm();
        form.ClientModuleUrl.ShouldBe("/scripts/contact-form.js");
    }

    [Fact]
    public void ContactFormShouldCreateMetadata()
    {
        var form = new ContactForm();
        var metadata = form.CreateMetadata();
        metadata.ShouldNotBeNull();
        metadata.ComponentUrl.ShouldBe("/scripts/contact-form.js");
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }

    // ── ImageGallery island tests ──

    [Fact]
    public async Task ImageGalleryShouldRenderPlaceholdersWithNoUrls()
    {
        var props = new Dictionary<string, object?>
        {
            ["ImageUrls"] = "",
            ["Captions"] = "",
        };
        var html = await RenderComponentAsync<ImageGallery>(props);

        html.ShouldContain("image-gallery");
        html.ShouldContain("gallery-item");
    }

    [Fact]
    public async Task ImageGalleryShouldRenderImagesWithUrls()
    {
        var props = new Dictionary<string, object?>
        {
            ["ImageUrls"] = "/img/a.jpg, /img/b.jpg",
            ["Captions"] = "Photo A, Photo B",
        };
        var html = await RenderComponentAsync<ImageGallery>(props);

        html.ShouldContain("<img");
        html.ShouldContain("src=\"/img/a.jpg\"");
        html.ShouldContain("src=\"/img/b.jpg\"");
        html.ShouldContain("Photo A");
        html.ShouldContain("Photo B");
    }

    [Fact]
    public async Task ImageGalleryShouldRenderLightboxContainer()
    {
        var html = await RenderComponentAsync<ImageGallery>();
        html.ShouldContain("gallery-lightbox");
    }

    [Fact]
    public async Task ImageGalleryShouldRenderDataIndexAttributes()
    {
        var props = new Dictionary<string, object?>
        {
            ["ImageUrls"] = "/img/a.jpg, /img/b.jpg, /img/c.jpg",
            ["Captions"] = "",
        };
        var html = await RenderComponentAsync<ImageGallery>(props);

        html.ShouldContain("data-index=\"0\"");
        html.ShouldContain("data-index=\"1\"");
        html.ShouldContain("data-index=\"2\"");
    }

    [Fact]
    public async Task ImageGalleryShouldHandleMismatchedCaptions()
    {
        var props = new Dictionary<string, object?>
        {
            ["ImageUrls"] = "/img/a.jpg, /img/b.jpg",
            ["Captions"] = "Only First",
        };
        var html = await RenderComponentAsync<ImageGallery>(props);

        html.ShouldContain("Only First");
        // Second image has no caption, so no caption overlay for it
        html.ShouldContain("src=\"/img/b.jpg\"");
    }

    [Fact]
    public void ImageGalleryShouldHaveClientVisibleDirective()
    {
        var directive = DirectiveExtractor.GetDirective(typeof(ImageGallery));
        directive.ShouldNotBeNull();
        directive.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void ImageGalleryShouldHaveRootMarginValue()
    {
        var directive = DirectiveExtractor.GetDirective(typeof(ImageGallery));
        directive.ShouldNotBeNull();
        directive.Value.ShouldBe("200px");
    }

    [Fact]
    public void ImageGalleryShouldProvideClientModuleUrl()
    {
        var gallery = new ImageGallery();
        gallery.ClientModuleUrl.ShouldBe("/scripts/image-gallery.js");
    }

    [Fact]
    public void ImageGalleryShouldCreateMetadataWithVisibleDirective()
    {
        var gallery = new ImageGallery();
        var metadata = gallery.CreateMetadata();
        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        metadata.DirectiveValue.ShouldBe("200px");
    }

    // ── MobileNav island tests ──

    [Fact]
    public async Task MobileNavShouldRenderToggleButton()
    {
        var html = await RenderComponentAsync<MobileNav>();

        html.ShouldContain("mobile-nav-toggle");
        html.ShouldContain("Toggle navigation");
    }

    [Fact]
    public async Task MobileNavShouldRenderMenuOverlay()
    {
        var html = await RenderComponentAsync<MobileNav>();

        html.ShouldContain("mobile-nav-menu");
        html.ShouldContain("mobile-nav-close");
    }

    [Fact]
    public async Task MobileNavShouldRenderAllNavigationLinks()
    {
        var html = await RenderComponentAsync<MobileNav>();

        html.ShouldContain("href=\"/\"");
        html.ShouldContain("href=\"/projects\"");
        html.ShouldContain("href=\"/about\"");
        html.ShouldContain("href=\"/contact\"");
    }

    [Fact]
    public void MobileNavShouldHaveClientMediaDirective()
    {
        var directive = DirectiveExtractor.GetDirective(typeof(MobileNav));
        directive.ShouldNotBeNull();
        directive.DirectiveType.ShouldBe(ClientDirectiveType.Media);
    }

    [Fact]
    public void MobileNavShouldHaveCorrectMediaQuery()
    {
        var directive = DirectiveExtractor.GetDirective(typeof(MobileNav));
        directive.ShouldNotBeNull();
        directive.Value.ShouldBe("(max-width: 768px)");
    }

    [Fact]
    public void MobileNavShouldProvideClientModuleUrl()
    {
        var nav = new MobileNav();
        nav.ClientModuleUrl.ShouldBe("/scripts/mobile-nav.js");
    }

    [Fact]
    public void MobileNavShouldCreateMetadataWithMediaDirective()
    {
        var nav = new MobileNav();
        var metadata = nav.CreateMetadata();
        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Media);
        metadata.DirectiveValue.ShouldBe("(max-width: 768px)");
    }

    // ── Cross-cutting island directive tests ──

    [Fact]
    public void PortfolioShouldDemonstrateThreeDifferentDirectives()
    {
        var contactDirective = DirectiveExtractor.GetDirective(typeof(ContactForm));
        var galleryDirective = DirectiveExtractor.GetDirective(typeof(ImageGallery));
        var mobileNavDirective = DirectiveExtractor.GetDirective(typeof(MobileNav));

        contactDirective.ShouldNotBeNull();
        galleryDirective.ShouldNotBeNull();
        mobileNavDirective.ShouldNotBeNull();

        // Each uses a different directive type
        contactDirective.DirectiveType.ShouldBe(ClientDirectiveType.Load);
        galleryDirective.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        mobileNavDirective.DirectiveType.ShouldBe(ClientDirectiveType.Media);

        var directiveTypes = new HashSet<ClientDirectiveType>
        {
            contactDirective.DirectiveType,
            galleryDirective.DirectiveType,
            mobileNavDirective.DirectiveType,
        };
        directiveTypes.Count.ShouldBe(3);
    }

    [Fact]
    public void AllIslandsShouldImplementIClientComponent()
    {
        typeof(IClientComponent).IsAssignableFrom(typeof(ContactForm)).ShouldBeTrue();
        typeof(IClientComponent).IsAssignableFrom(typeof(ImageGallery)).ShouldBeTrue();
        typeof(IClientComponent).IsAssignableFrom(typeof(MobileNav)).ShouldBeTrue();
    }

    [Fact]
    public void AllIslandsShouldBeAtollPages()
    {
        // Islands are components but not pages
        typeof(IAtollPage).IsAssignableFrom(typeof(ContactForm)).ShouldBeFalse();
        typeof(IAtollPage).IsAssignableFrom(typeof(ImageGallery)).ShouldBeFalse();
        typeof(IAtollPage).IsAssignableFrom(typeof(MobileNav)).ShouldBeFalse();
    }

    [Fact]
    public void AllPagesShouldImplementIAtollPage()
    {
        typeof(IAtollPage).IsAssignableFrom(typeof(IndexPage)).ShouldBeTrue();
        typeof(IAtollPage).IsAssignableFrom(typeof(ProjectsPage)).ShouldBeTrue();
        typeof(IAtollPage).IsAssignableFrom(typeof(AboutPage)).ShouldBeTrue();
        typeof(IAtollPage).IsAssignableFrom(typeof(ContactPage)).ShouldBeTrue();
    }

    [Fact]
    public void AllPagesShouldHaveLayoutAttribute()
    {
        LayoutResolver.HasLayout(typeof(IndexPage)).ShouldBeTrue();
        LayoutResolver.HasLayout(typeof(ProjectsPage)).ShouldBeTrue();
        LayoutResolver.HasLayout(typeof(AboutPage)).ShouldBeTrue();
        LayoutResolver.HasLayout(typeof(ContactPage)).ShouldBeTrue();
    }

    [Fact]
    public void AllPagesShouldUsePortfolioLayout()
    {
        LayoutResolver.GetLayoutType(typeof(IndexPage)).ShouldBe(typeof(PortfolioLayout));
        LayoutResolver.GetLayoutType(typeof(ProjectsPage)).ShouldBe(typeof(PortfolioLayout));
        LayoutResolver.GetLayoutType(typeof(AboutPage)).ShouldBe(typeof(PortfolioLayout));
        LayoutResolver.GetLayoutType(typeof(ContactPage)).ShouldBe(typeof(PortfolioLayout));
    }

    // ── ASP.NET Core middleware integration tests ──

    private static HttpClient CreatePortfolioTestClient()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.RouteEntries.Add(("index.cs", typeof(IndexPage)));
                        options.RouteEntries.Add(("projects/index.cs", typeof(ProjectsPage)));
                        options.RouteEntries.Add(("about.cs", typeof(AboutPage)));
                        options.RouteEntries.Add(("contact.cs", typeof(ContactPage)));
                    });
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    [Fact]
    public async Task MiddlewareShouldServeHomePage()
    {
        using var client = CreatePortfolioTestClient();
        var response = await client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("Alex Chen");
        html.ShouldContain("Featured Projects");
    }

    [Fact]
    public async Task MiddlewareShouldServeProjectsPage()
    {
        using var client = CreatePortfolioTestClient();
        var response = await client.GetAsync("/projects");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("Projects");
        html.ShouldContain("Atoll Framework");
    }

    [Fact]
    public async Task MiddlewareShouldServeAboutPage()
    {
        using var client = CreatePortfolioTestClient();
        var response = await client.GetAsync("/about");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("About Me");
        html.ShouldContain("Skills");
    }

    [Fact]
    public async Task MiddlewareShouldServeContactPage()
    {
        using var client = CreatePortfolioTestClient();
        var response = await client.GetAsync("/contact");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();
        html.ShouldContain("Get in Touch");
        html.ShouldContain("contact-form");
    }

    [Fact]
    public async Task MiddlewareShouldReturn404ForUnknownRoute()
    {
        using var client = CreatePortfolioTestClient();
        var response = await client.GetAsync("/nonexistent");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MiddlewareShouldReturnHtmlContentType()
    {
        using var client = CreatePortfolioTestClient();
        var response = await client.GetAsync("/");
        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
    }

    [Fact]
    public async Task MiddlewareShouldServeAllPagesWithDoctype()
    {
        using var client = CreatePortfolioTestClient();

        var paths = new[] { "/", "/projects", "/about", "/contact" };
        foreach (var path in paths)
        {
            var response = await client.GetAsync(path);
            var html = await response.Content.ReadAsStringAsync();
            html.ShouldContain("<!DOCTYPE html>", customMessage: $"Page at {path} should start with DOCTYPE");
        }
    }

    // ── Helper ──

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
