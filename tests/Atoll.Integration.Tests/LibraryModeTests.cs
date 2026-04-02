using System.Net;
using Atoll.Core.Components;
using Atoll.Routing;
using Atoll.Server.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests;

/// <summary>
/// Tests that Atoll works in "library mode" — embedded in an existing
/// ASP.NET Core application alongside custom middleware and endpoints.
/// </summary>
public sealed class LibraryModeTests
{
    // ── Test components ──

    private sealed class HomePage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><body><h1>Atoll Home</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class AboutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><body><h1>Atoll About</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    // ── Helpers ──

    private static HttpClient CreateTestClient(
        Action<AtollOptions> configureAtoll,
        Action<IApplicationBuilder>? configureApp = null)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(configureAtoll);
                    services.AddLogging();
                });
                webHost.Configure(app =>
                {
                    configureApp?.Invoke(app);
                    app.UseAtoll();
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    // ── Coexistence with other middleware ──

    [Fact]
    public async Task ShouldCoexistWithCustomMiddleware()
    {
        using var client = CreateTestClient(
            options =>
            {
                options.RouteEntries.Add(("index.cs", typeof(HomePage)));
            },
            app =>
            {
                // Custom middleware for /api/health
                app.Map("/api/health", branch =>
                {
                    branch.Run(async ctx =>
                    {
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync("{\"status\":\"ok\"}");
                    });
                });
            });

        // Atoll page still works
        var pageResponse = await client.GetAsync("/");
        pageResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageContent = await pageResponse.Content.ReadAsStringAsync();
        pageContent.ShouldContain("Atoll Home");

        // Custom middleware still works
        var apiResponse = await client.GetAsync("/api/health");
        apiResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var apiContent = await apiResponse.Content.ReadAsStringAsync();
        apiContent.ShouldContain("ok");
    }

    [Fact]
    public async Task ShouldPassThroughUnmatchedRoutes()
    {
        using var client = CreateTestClient(
            options =>
            {
                options.RouteEntries.Add(("index.cs", typeof(HomePage)));
            });

        var response = await client.GetAsync("/nonexistent");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ── Multiple Atoll routes ──

    [Fact]
    public async Task ShouldHandleMultipleAtollRoutes()
    {
        using var client = CreateTestClient(options =>
        {
            options.RouteEntries.Add(("index.cs", typeof(HomePage)));
            options.RouteEntries.Add(("about.cs", typeof(AboutPage)));
        });

        var homeResponse = await client.GetAsync("/");
        homeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await homeResponse.Content.ReadAsStringAsync()).ShouldContain("Atoll Home");

        var aboutResponse = await client.GetAsync("/about");
        aboutResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await aboutResponse.Content.ReadAsStringAsync()).ShouldContain("Atoll About");
    }

    // ── Base path configuration ──

    [Fact]
    public async Task ShouldRespectBasePath()
    {
        using var client = CreateTestClient(options =>
        {
            options.BasePath = "/docs";
            options.RouteEntries.Add(("index.cs", typeof(HomePage)));
            options.RouteEntries.Add(("about.cs", typeof(AboutPage)));
        });

        // Should match under base path
        var response = await client.GetAsync("/docs/");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldContain("Atoll Home");

        var aboutResponse = await client.GetAsync("/docs/about");
        aboutResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await aboutResponse.Content.ReadAsStringAsync()).ShouldContain("Atoll About");
    }

    [Fact]
    public async Task ShouldNotMatchOutsideBasePath()
    {
        using var client = CreateTestClient(options =>
        {
            options.BasePath = "/docs";
            options.RouteEntries.Add(("index.cs", typeof(HomePage)));
        });

        // Should not match at root when base path is /docs
        var response = await client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ── AddAtoll configuration ──

    [Fact]
    public void ShouldThrowOnNullServices()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollServiceCollectionExtensions.AddAtoll(null!, _ => { }));
    }

    [Fact]
    public void ShouldThrowOnNullConfigure()
    {
        var services = new ServiceCollection();
        Should.Throw<ArgumentNullException>(() =>
            services.AddAtoll(null!));
    }

    // ── UseAtoll configuration ──

    [Fact]
    public void ShouldThrowOnNullApp()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollApplicationBuilderExtensions.UseAtoll(null!));
    }

    // ── Empty route table ──

    [Fact]
    public async Task ShouldHandleEmptyRouteTable()
    {
        using var client = CreateTestClient(_ => { });

        var response = await client.GetAsync("/");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ── Route entries take precedence ──

    [Fact]
    public async Task ShouldUseExplicitRouteEntries()
    {
        using var client = CreateTestClient(options =>
        {
            options.RouteEntries.Add(("custom/path.cs", typeof(HomePage)));
        });

        var response = await client.GetAsync("/custom/path");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldContain("Atoll Home");
    }
}
