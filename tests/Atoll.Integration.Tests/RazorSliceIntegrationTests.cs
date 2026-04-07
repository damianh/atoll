using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Routing;
using Atoll.Routing.FileSystem;
using Atoll.Samples.Blog.Pages;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests;

/// <summary>
/// End-to-end integration tests for mixed C#/Razor component rendering.
/// Verifies that Razor slices (RazorSlices-based) and C# components interoperate
/// correctly through the full Atoll rendering pipeline: components, layouts, SSG.
/// </summary>
public sealed class RazorSliceIntegrationTests : IDisposable
{
    private readonly string _outputDir;

    public RazorSliceIntegrationTests()
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-razor-integration-" + id);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    // ── Route discovery ──

    [Fact]
    public void DiscoverRoutesFromAttributesShouldFindRazorPageInBlogSample()
    {
        // AboutRazorPage is in the Blog sample with [PageRoute("/about-razor")] on its
        // source-generated proxy class (via companion .cs file).
        var assemblies = new[] { typeof(AboutRazorPage).Assembly };
        var routes = RouteDiscovery.DiscoverRoutesFromAttributes(assemblies);

        routes.ShouldContain(r =>
            r.Pattern == "/about-razor" &&
            r.ComponentType == typeof(AboutRazorPage));
    }

    [Fact]
    public void DiscoverRoutesFromAttributesShouldFindBothCSharpAndRazorPages()
    {
        // Both C# pages (with [PageRoute]) and Razor pages should be discovered.
        var assemblies = new[] { typeof(AboutRazorPage).Assembly };
        var routes = RouteDiscovery.DiscoverRoutesFromAttributes(assemblies);

        // C# page: AboutPage has [PageRoute("/about")]
        routes.ShouldContain(r => r.Pattern == "/about");
        // Razor page: AboutRazorPage has [PageRoute("/about-razor")]
        routes.ShouldContain(r => r.Pattern == "/about-razor");
    }

    // ── Razor page SSG rendering ──

    [Fact]
    public async Task RazorPageShouldRenderViaStaticSiteGenerator()
    {
        // AboutRazorPage is a Razor-authored page with [PageRoute("/about-razor")]
        // and [Layout(typeof(BlogLayout))].
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        if (!result.IsSuccess)
        {
            throw result.PageResults[0].Error!;
        }

        result.TotalCount.ShouldBe(1);
        var html = result.PageResults[0].Html;
        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("About This Blog (Razor)");
        html.ShouldContain("Razor Slices");
    }

    [Fact]
    public async Task RazorPageShouldRenderWithCSharpLayout()
    {
        // The [Layout(typeof(BlogLayout))] on AboutRazorPage wraps Razor page in C# BlogLayout.
        // This tests the Razor-page-wrapped-in-C#-layout scenario.
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        var html = result.PageResults[0].Html;

        // BlogLayout (C# component) structure
        html.ShouldContain("<html");
        html.ShouldContain("<header");
        html.ShouldContain("<nav");
        html.ShouldContain("<footer");
        html.ShouldContain("Atoll Blog");

        // Razor page content is inside the layout
        html.ShouldContain("About This Blog (Razor)");
    }

    // ── Mixed C# + Razor SSG ──

    [Fact]
    public async Task MixedSiteShouldSsgBothCSharpAndRazorPages()
    {
        // Render a site with both a C# page (AboutPage) and a Razor page (AboutRazorPage).
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(2);

        File.Exists(Path.Combine(_outputDir, "about", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "about-razor", "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task MixedSiteCSharpPageShouldRenderCorrectly()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        var aboutHtml = result.PageResults.First(r => r.Route.UrlPath == "/about").Html;
        // C# page content
        aboutHtml.ShouldContain("About This Blog");
        aboutHtml.ShouldContain("Server-first rendering");
        // C# page wrapped in BlogLayout
        aboutHtml.ShouldContain("<header");
        aboutHtml.ShouldContain("<footer");
    }

    [Fact]
    public async Task MixedSiteRazorPageShouldRenderCorrectly()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        var razorHtml = result.PageResults.First(r => r.Route.UrlPath == "/about-razor").Html;
        // Razor page content
        razorHtml.ShouldContain("About This Blog (Razor)");
        razorHtml.ShouldContain("Razor Slices");
        // Razor page wrapped in BlogLayout
        razorHtml.ShouldContain("<header");
        razorHtml.ShouldContain("<footer");
    }

    [Fact]
    public async Task MixedSiteAllPagesShouldHaveDoctype()
    {
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        foreach (var pageResult in result.PageResults)
        {
            pageResult.Html.ShouldStartWith("<!DOCTYPE html>",
                customMessage: $"Page {pageResult.Route.UrlPath} missing DOCTYPE");
        }
    }

    // ── Layout chain across C#/Razor boundary ──

    [Fact]
    public void RazorPageShouldHaveLayoutAttribute()
    {
        // Verify the [Layout] attribute is on the Razor page proxy type.
        LayoutResolver.HasLayout(typeof(AboutRazorPage)).ShouldBeTrue();
    }

    [Fact]
    public async Task RazorPageWrappedInCSharpLayoutShouldRenderCorrectly()
    {
        // AboutRazorPage uses [Layout(typeof(BlogLayout))] where BlogLayout is a C# component.
        // Test the C#-layout-wraps-Razor-page scenario via the full SSG pipeline (which
        // internally invokes LayoutResolver.WrapWithLayouts and SliceComponentAdapter).
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);
        result.IsSuccess.ShouldBeTrue();

        var html = result.PageResults[0].Html;
        // C# BlogLayout structure wrapping the Razor page
        html.ShouldContain("<html");
        html.ShouldContain("Atoll Blog");
        html.ShouldContain("<header");
        html.ShouldContain("<footer");
        // Razor page content
        html.ShouldContain("About This Blog (Razor)");
    }

    // ── C# page and Razor page output equivalence ──

    [Fact]
    public async Task BothCSharpAndRazorPagesShouldHaveLayoutStructure()
    {
        // Verify that both page types get properly wrapped by their C# layout.
        var ssgOptions = new SsgOptions(_outputDir);
        var generator = new StaticSiteGenerator(ssgOptions);
        var routes = new[]
        {
            new RouteEntry("/about", typeof(AboutPage), "about.cs"),
            new RouteEntry("/about-razor", typeof(AboutRazorPage), "about-razor.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();

        foreach (var pageResult in result.PageResults)
        {
            var html = pageResult.Html;
            var path = pageResult.Route.UrlPath;
            html.ShouldContain("<html", customMessage: $"{path} missing <html>");
            html.ShouldContain("<header", customMessage: $"{path} missing <header>");
            html.ShouldContain("<nav", customMessage: $"{path} missing <nav>");
            html.ShouldContain("<main", customMessage: $"{path} missing <main>");
            html.ShouldContain("<footer", customMessage: $"{path} missing <footer>");
            html.ShouldContain("Atoll Blog", customMessage: $"{path} missing blog title");
        }
    }
}
