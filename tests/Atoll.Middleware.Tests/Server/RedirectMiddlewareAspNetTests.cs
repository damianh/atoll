using System.Net;
using Atoll.Middleware.Server.Hosting;
using Atoll.Redirects;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Atoll.Middleware.Tests.Server;

public sealed class RedirectMiddlewareAspNetTests
{
    private static HttpClient CreateTestClient(RedirectMap redirectMap, int? statusCode = null)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.Configure(app =>
                {
                    if (statusCode.HasValue)
                    {
                        app.UseRedirects(redirectMap, statusCode.Value);
                    }
                    else
                    {
                        app.UseRedirects(redirectMap);
                    }

                    // Fallback: always 200 for unmatched paths
                    app.Run(async ctx =>
                    {
                        ctx.Response.StatusCode = 200;
                        await ctx.Response.WriteAsync("ok");
                    });
                });
            });

        var host = builder.Start();
        return host.GetTestClient();
    }

    private static RedirectMap CreateMap(params (string from, string to)[] entries)
    {
        return RedirectMap.Create(entries.Select(e => new KeyValuePair<string, string>(e.from, e.to)));
    }

    [Fact]
    public async Task ShouldReturn301ForMatchedPath()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map);
        client.DefaultRequestHeaders.Add("Allow-Redirect", "false");

        var response = await client.GetAsync("/old-page");

        ((int)response.StatusCode).ShouldBe(301);
    }

    [Fact]
    public async Task ShouldPassThroughForUnmatchedPath()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map);

        var response = await client.GetAsync("/other-page");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldSetLocationHeaderToTargetPath()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map);

        var response = await client.GetAsync("/old-page");

        response.Headers.Location!.OriginalString.ShouldBe("/new-page");
    }

    [Fact]
    public async Task ShouldPreserveQueryStringOnRedirect()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map);

        var response = await client.GetAsync("/old-page?foo=bar");

        response.Headers.Location!.OriginalString.ShouldBe("/new-page?foo=bar");
    }

    [Fact]
    public async Task ShouldNormalizeTrailingSlashBeforeLookup()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map);

        var response = await client.GetAsync("/old-page/");

        ((int)response.StatusCode).ShouldBe(301);
    }

    [Fact]
    public async Task ShouldNormalizeCaseBeforeLookup()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map);

        var response = await client.GetAsync("/OLD-PAGE");

        ((int)response.StatusCode).ShouldBe(301);
    }

    [Fact]
    public async Task ShouldUseCustomStatusCode()
    {
        var map = CreateMap(("/old-page", "/new-page"));
        using var client = CreateTestClient(map, statusCode: 302);

        var response = await client.GetAsync("/old-page");

        ((int)response.StatusCode).ShouldBe(302);
    }

    [Fact]
    public void ShouldThrowForNullRedirectMap()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                    webHost.Configure(app =>
                    {
                        app.UseRedirects(null!);
                    });
                });
            builder.Start();
        });
    }
}
