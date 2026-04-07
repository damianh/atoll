using Atoll.Build.Ssg;
using Atoll.Components;
using Atoll.Rendering;
using Atoll.Routing;
using Atoll.Routing.FileSystem;
using Atoll.Tests.Routing.Fixtures;
using RazorSlices;

namespace Atoll.Tests.Routing;

/// <summary>
/// Tests for Razor-authored pages using <see cref="AtollPageSlice"/> and <see cref="AtollPageSlice{TModel}"/>.
/// Covers route discovery via attributes and rendering via SSG.
/// </summary>
public sealed class RazorPageSliceTests
{
    // ── Proxy type checks ──

    [Fact]
    public void AtollPageSliceProxyShouldImplementIRazorSliceProxy()
    {
        typeof(IRazorSliceProxy).IsAssignableFrom(typeof(AboutRazorPage)).ShouldBeTrue();
    }

    [Fact]
    public void AtollPageSliceProxyShouldBeNonAbstract()
    {
        typeof(AboutRazorPage).IsAbstract.ShouldBeFalse();
    }

    [Fact]
    public void AtollPageSliceProxyShouldHavePageRouteAttribute()
    {
        var attr = typeof(AboutRazorPage).GetCustomAttributes(typeof(PageRouteAttribute), inherit: false)
            .OfType<PageRouteAttribute>()
            .SingleOrDefault();

        attr.ShouldNotBeNull();
        attr.Pattern.ShouldBe("/razor-about");
    }

    // ── Route discovery via DiscoverRoutesFromAttributes ──

    [Fact]
    public void DiscoverRoutesFromAttributesShouldFindRazorPage()
    {
        var assemblies = new[] { typeof(AboutRazorPage).Assembly };
        var routes = RouteDiscovery.DiscoverRoutesFromAttributes(assemblies);

        routes.ShouldContain(r =>
            r.Pattern == "/razor-about" &&
            r.ComponentType == typeof(AboutRazorPage));
    }

    // ── Rendering via ComponentRenderer.RenderSliceAsync ──

    [Fact]
    public async Task ComponentRendererShouldRenderRazorPageViaSliceFragment()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderSliceAsync<AboutRazorPage>(destination);

        var html = destination.GetOutput();
        html.ShouldContain("<h1>About Razor</h1>");
    }

    // ── SSG renders Razor page via StaticSiteGenerator ──

    [Fact]
    public async Task SsgShouldRenderRazorPage()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "atoll-razor-page-" + Guid.NewGuid().ToString("N")[..8]);
        try
        {
            var options = new SsgOptions(outputDir);
            var generator = new StaticSiteGenerator(options);
            var routes = new[]
            {
                new RouteEntry("/razor-about", typeof(AboutRazorPage), "/razor-about"),
            };

            var result = await generator.GenerateAsync(routes);

            if (!result.IsSuccess)
            {
                throw result.PageResults[0].Error!;
            }
            result.TotalCount.ShouldBe(1);
            result.PageResults[0].Html.ShouldContain("<h1>About Razor</h1>");
            result.PageResults[0].Html.ShouldStartWith("<!DOCTYPE html>");
            result.PageResults[0].Html.ShouldContain("<h1>About Razor</h1>");
            result.PageResults[0].Html.ShouldStartWith("<!DOCTYPE html>");
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }
}
