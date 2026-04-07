using System.Net;
using Atoll.Build.Pipeline;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
using Shouldly;
using Xunit;

namespace Atoll.Middleware.Tests.Server;

/// <summary>
/// Integration tests for Cache-Control headers served by the preview server static file handler.
/// </summary>
public sealed class PreviewCacheControlTests : IDisposable
{
    private readonly string _tempDir;

    public PreviewCacheControlTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-preview-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        // Create _atoll sub-directory with fingerprinted and non-fingerprinted assets
        var astroDir = Path.Combine(_tempDir, "_atoll");
        Directory.CreateDirectory(astroDir);
        File.WriteAllText(Path.Combine(astroDir, "styles.a1b2c3d4.css"), "body { color: red; }");
        File.WriteAllText(Path.Combine(astroDir, "scripts.00aabbcc.js"), "console.log('hi');");
        File.WriteAllText(Path.Combine(astroDir, "image.png"), "fake-png-data");

        // Root files
        File.WriteAllText(Path.Combine(_tempDir, "index.html"), "<html><body>Home</body></html>");
        File.WriteAllText(Path.Combine(_tempDir, "about.html"), "<html><body>About</body></html>");
        File.WriteAllText(Path.Combine(_tempDir, "robots.txt"), "User-agent: *\nAllow: /");
        File.WriteAllText(Path.Combine(_tempDir, "sitemap.xml"), "<urlset/>");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* best-effort */ }
    }

    // ── Helper ──────────────────────────────────────────────────────

    private HttpClient CreatePreviewClient()
    {
        var tempDir = _tempDir;

        var host = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.ConfigureServices(services => services.AddLogging());
                webHost.Configure(app =>
                {
                    var fileProvider = new PhysicalFileProvider(tempDir);

                    var staticFileOptions = new StaticFileOptions
                    {
                        FileProvider = fileProvider,
                        OnPrepareResponse = ctx =>
                        {
                            var path = ctx.Context.Request.Path.Value ?? "";
                            var cacheControl = ResolveCacheControl(path);
                            ctx.Context.Response.Headers["Cache-Control"] = cacheControl;

                            var contentType = ctx.Context.Response.ContentType ?? "";
                            if (IsCompressibleContentType(contentType))
                            {
                                ctx.Context.Response.Headers["Vary"] = "Accept-Encoding";
                            }
                        },
                    };

                    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
                    app.UseStaticFiles(staticFileOptions);
                });
            })
            .Start();

        return host.GetTestClient();
    }

    // Mirrors PreviewCommandHandler.ResolveCacheControl
    private static string ResolveCacheControl(string path)
    {
        if (FingerprintDetector.IsFingerprintedAsset(path))
        {
            return "public, max-age=31536000, immutable";
        }

        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("/", StringComparison.OrdinalIgnoreCase)
            || path == "")
        {
            return "public, max-age=0, must-revalidate";
        }

        return "public, max-age=3600";
    }

    private static bool IsCompressibleContentType(string contentType)
    {
        return contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase)
            || contentType.StartsWith("image/svg+xml", StringComparison.OrdinalIgnoreCase);
    }

    // ================================================================
    // Fingerprinted Asset Tests
    // ================================================================

    [Fact]
    public async Task ShouldServeImmutableCacheControlForFingerprintedCss()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/_atoll/styles.a1b2c3d4.css");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull();
        response.Headers.CacheControl!.ToString().ShouldBe("public, max-age=31536000, immutable");
    }

    [Fact]
    public async Task ShouldServeImmutableCacheControlForFingerprintedJs()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/_atoll/scripts.00aabbcc.js");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl!.ToString().ShouldBe("public, max-age=31536000, immutable");
    }

    [Fact]
    public async Task ShouldServeDefaultCacheControlForNonFingerprintedAssetUnderAstro()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/_atoll/image.png");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl!.ToString().ShouldBe("public, max-age=3600");
    }

    // ================================================================
    // HTML File Tests
    // ================================================================

    [Fact]
    public async Task ShouldServeMustRevalidateForHtmlFile()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/about.html");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        // ASP.NET Core normalises Cache-Control tokens; verify all required directives are present
        var cacheControl = response.Headers.CacheControl!;
        cacheControl.Public.ShouldBeTrue();
        cacheControl.MaxAge.ShouldBe(TimeSpan.Zero);
        cacheControl.MustRevalidate.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldServeMustRevalidateForIndexHtml()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/index.html");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var cacheControl = response.Headers.CacheControl!;
        cacheControl.Public.ShouldBeTrue();
        cacheControl.MaxAge.ShouldBe(TimeSpan.Zero);
        cacheControl.MustRevalidate.ShouldBeTrue();
    }

    // ================================================================
    // Non-HTML Static File Tests
    // ================================================================

    [Fact]
    public async Task ShouldServeDefaultCacheControlForRobotsTxt()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/robots.txt");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl!.ToString().ShouldBe("public, max-age=3600");
    }

    [Fact]
    public async Task ShouldServeDefaultCacheControlForSitemapXml()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/sitemap.xml");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl!.ToString().ShouldBe("public, max-age=3600");
    }

    // ================================================================
    // Vary Header Tests
    // ================================================================

    [Fact]
    public async Task ShouldSetVaryHeaderForCompressibleCssFile()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/_atoll/styles.a1b2c3d4.css");

        response.Headers.Vary.ShouldContain("Accept-Encoding");
    }

    [Fact]
    public async Task ShouldSetVaryHeaderForJsFile()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/_atoll/scripts.00aabbcc.js");

        response.Headers.Vary.ShouldContain("Accept-Encoding");
    }

    [Fact]
    public async Task ShouldSetVaryHeaderForHtmlFile()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/about.html");

        response.Headers.Vary.ShouldContain("Accept-Encoding");
    }

    [Fact]
    public async Task ShouldNotSetVaryHeaderForImageFile()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/_atoll/image.png");

        response.Headers.Contains("Vary").ShouldBeFalse();
    }

    // ================================================================
    // 404 Tests
    // ================================================================

    [Fact]
    public async Task ShouldReturn404ForMissingFile()
    {
        using var client = CreatePreviewClient();

        var response = await client.GetAsync("/nonexistent.html");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
