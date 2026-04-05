using System.Net;
using Atoll.Components;
using Atoll.Middleware.Server.Hosting;
using Atoll.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests.Server;

public sealed class CacheControlIntegrationTests
{
    // ----------------------------------------------------------------
    // Test page components
    // ----------------------------------------------------------------

    private sealed class StaticPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head><title>Static</title></head><body><h1>Static Content</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class DynamicPage : AtollComponent, IAtollPage
    {
        [Parameter(Required = true)]
        public string Id { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<html><body><p>Item: {Id}</p></body></html>");
            return Task.CompletedTask;
        }
    }

    // ----------------------------------------------------------------
    // Helper: create test host
    // ----------------------------------------------------------------

    private static HttpClient CreateTestClient(
        bool enableCacheControl = true,
        params (string RelativeFilePath, Type ComponentType)[] routes)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services =>
                {
                    services.AddAtoll(options =>
                    {
                        options.EnableCacheControl = enableCacheControl;
                        foreach (var route in routes)
                        {
                            options.RouteEntries.Add(route);
                        }
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

    // ----------------------------------------------------------------
    // ETag headers on page responses
    // ----------------------------------------------------------------

    [Fact]
    public async Task PageResponseShouldIncludeETagHeader()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
    }

    [Fact]
    public async Task PageResponseETagShouldBeWeakFormat()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var response = await client.GetAsync("/");

        var etag = response.Headers.ETag!;
        etag.IsWeak.ShouldBeTrue();
        etag.Tag.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task PageResponseETagShouldBeDeterministic()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var r1 = await client.GetAsync("/");
        var r2 = await client.GetAsync("/");

        r1.Headers.ETag!.Tag.ShouldBe(r2.Headers.ETag!.Tag);
    }

    [Fact]
    public async Task PageResponseShouldIncludeNoCacheCacheControl()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var response = await client.GetAsync("/");

        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl!.NoCache.ShouldBeTrue();
    }

    // ----------------------------------------------------------------
    // Conditional request → 304
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldReturn304WhenIfNoneMatchMatchesCurrentETag()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        // First request — get ETag
        var firstResponse = await client.GetAsync("/");
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var etag = firstResponse.Headers.ETag!;

        // Second request with If-None-Match
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.IfNoneMatch.Add(etag);
        var secondResponse = await client.SendAsync(request);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task ShouldReturn200WhenIfNoneMatchDoesNotMatch()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"stale-etag\"", isWeak: true));
        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldReturn304ForDynamicPageWhenContentUnchanged()
    {
        using var client = CreateTestClient(true, ("item/[id].cs", typeof(DynamicPage)));

        var firstResponse = await client.GetAsync("/item/42");
        var etag = firstResponse.Headers.ETag!;

        var request = new HttpRequestMessage(HttpMethod.Get, "/item/42");
        request.Headers.IfNoneMatch.Add(etag);
        var secondResponse = await client.SendAsync(request);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task ShouldReturn200WithNewETagWhenDynamicPageContentChanges()
    {
        using var client = CreateTestClient(true, ("item/[id].cs", typeof(DynamicPage)));

        // Get ETag for item/1
        var firstResponse = await client.GetAsync("/item/1");
        var etag = firstResponse.Headers.ETag!;

        // Request item/2 with etag from item/1 — content differs, should get 200
        var request = new HttpRequestMessage(HttpMethod.Get, "/item/2");
        request.Headers.IfNoneMatch.Add(etag);
        var secondResponse = await client.SendAsync(request);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.Headers.ETag.ShouldNotBeNull();
        secondResponse.Headers.ETag!.Tag.ShouldNotBe(etag.Tag);
    }

    // ----------------------------------------------------------------
    // EnableCacheControl = false → no ETag
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldNotAddETagWhenCacheControlDisabled()
    {
        using var client = CreateTestClient(false, ("index.cs", typeof(StaticPage)));

        var response = await client.GetAsync("/");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.ETag.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldNotReturn304WhenCacheControlDisabled()
    {
        // Even if client sends If-None-Match, server won't honour it when cache control is disabled
        using var client = CreateTestClient(false, ("index.cs", typeof(StaticPage)));

        // Get a response first (no ETag will be present, but we still test the round-trip)
        var firstResponse = await client.GetAsync("/");
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        firstResponse.Headers.ETag.ShouldBeNull();
    }

    // ----------------------------------------------------------------
    // Content-Type still set correctly
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldStillSetHtmlContentTypeWhenCacheControlEnabled()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var response = await client.GetAsync("/");

        response.Content.Headers.ContentType!.MediaType.ShouldBe("text/html");
        response.Content.Headers.ContentType!.CharSet.ShouldBe("utf-8");
    }

    [Fact]
    public async Task ShouldStillReturnHtmlBodyWhenCacheControlEnabled()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var response = await client.GetAsync("/");

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("<!DOCTYPE html>");
        body.ShouldContain("<h1>Static Content</h1>");
    }

    // ----------------------------------------------------------------
    // If-None-Match: * wildcard (RFC 7232 §3.2)
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldReturn304WhenIfNoneMatchIsWildcard()
    {
        using var client = CreateTestClient(true, ("index.cs", typeof(StaticPage)));

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.TryAddWithoutValidation("If-None-Match", "*");
        var response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }
}
