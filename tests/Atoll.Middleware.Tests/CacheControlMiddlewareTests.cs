using System.Collections.ObjectModel;
using System.Text;
using Atoll.Build.Pipeline;
using Atoll.Routing;

namespace Atoll.Middleware.Tests;

public sealed class CacheControlMiddlewareTests
{
    private static MiddlewareContext CreateGetContext(string path, string? ifNoneMatch = null)
    {
        var headers = new Dictionary<string, string>();
        if (ifNoneMatch is not null)
        {
            headers["If-None-Match"] = ifNoneMatch;
        }
        var request = new EndpointRequest(
            "GET",
            new Uri($"http://localhost{path}"),
            new ReadOnlyDictionary<string, string>(headers));
        return new MiddlewareContext(path, new Dictionary<string, string>(), request);
    }

    private static AtollResponse CreateBodyResponse(string body, int statusCode = 200)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "text/plain; charset=utf-8"
        };
        return new AtollResponse(statusCode, new ReadOnlyDictionary<string, string>(headers), bytes);
    }

    private static string ComputeExpectedETag(string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        var hash = AssetFingerprinter.ComputeHash(bytes, 64);
        return $"W/\"{hash}\"";
    }

    // ----------------------------------------------------------------
    // 200 with body → ETag added
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldAddETagToResponseWithBody()
    {
        var middleware = CacheControlMiddleware.Create();
        var downstream = CreateBodyResponse("Hello, World!");
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.Headers.ContainsKey("ETag").ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAddWeakETagFormat()
    {
        var middleware = CacheControlMiddleware.Create();
        var downstream = CreateBodyResponse("content");
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        var etag = response.Headers["ETag"];
        etag.ShouldStartWith("W/\"");
        etag.ShouldEndWith("\"");
    }

    [Fact]
    public async Task ShouldAddFullSha256ETagOf64HexChars()
    {
        var middleware = CacheControlMiddleware.Create();
        var downstream = CreateBodyResponse("some content");
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        var etag = response.Headers["ETag"];
        // Strip W/"..."
        var hash = etag[3..^1]; // remove W/" and trailing "
        hash.Length.ShouldBe(64);
        hash.ShouldMatch("^[0-9a-f]+$");
    }

    [Fact]
    public async Task ShouldProduceDeterministicETag()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "deterministic content";
        var context = CreateGetContext("/");

        var r1 = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));
        var r2 = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        r1.Headers["ETag"].ShouldBe(r2.Headers["ETag"]);
    }

    [Fact]
    public async Task ShouldMatchExpectedETagValue()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "known content";
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        response.Headers["ETag"].ShouldBe(ComputeExpectedETag(body));
    }

    // ----------------------------------------------------------------
    // 200 without body → pass through
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldPassThroughResponseWithNoBody()
    {
        var middleware = CacheControlMiddleware.Create();
        var downstream = new AtollResponse(200);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.ShouldBeSameAs(downstream);
        response.Headers.ContainsKey("ETag").ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldPassThroughResponseWithEmptyBody()
    {
        var middleware = CacheControlMiddleware.Create();
        var headers = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        var downstream = new AtollResponse(200, headers, []);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.ShouldBeSameAs(downstream);
    }

    // ----------------------------------------------------------------
    // Non-200 responses → pass through
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldPassThroughNon200ResponseWithBody()
    {
        var middleware = CacheControlMiddleware.Create();
        var downstream = CreateBodyResponse("error", statusCode: 500);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.ShouldBeSameAs(downstream);
        response.Headers.ContainsKey("ETag").ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldPassThrough404ResponseWithBody()
    {
        var middleware = CacheControlMiddleware.Create();
        var downstream = CreateBodyResponse("not found", statusCode: 404);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.ShouldBeSameAs(downstream);
    }

    // ----------------------------------------------------------------
    // If-None-Match matching → 304
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldReturn304WhenIfNoneMatchMatchesETag()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "cached content";
        var expectedETag = ComputeExpectedETag(body);
        var context = CreateGetContext("/", ifNoneMatch: expectedETag);

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        response.StatusCode.ShouldBe(304);
        response.Body.ShouldBeNull();
    }

    [Fact]
    public async Task ShouldIncludeETagOn304Response()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "cached content";
        var expectedETag = ComputeExpectedETag(body);
        var context = CreateGetContext("/", ifNoneMatch: expectedETag);

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        response.Headers["ETag"].ShouldBe(expectedETag);
    }

    [Fact]
    public async Task ShouldReturn200WhenIfNoneMatchDoesNotMatch()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "some content";
        var context = CreateGetContext("/", ifNoneMatch: "W/\"wrong-etag\"");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        response.StatusCode.ShouldBe(200);
        response.Headers.ContainsKey("ETag").ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturn200WhenNoIfNoneMatchHeader()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "some content";
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        response.StatusCode.ShouldBe(200);
        response.Headers.ContainsKey("ETag").ShouldBeTrue();
    }

    // ----------------------------------------------------------------
    // Double-ETag guard
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldNotOverwriteExistingETagHeader()
    {
        var middleware = CacheControlMiddleware.Create();
        var bytes = Encoding.UTF8.GetBytes("body");
        var existingETag = "W/\"existing-etag\"";
        var headers = new Dictionary<string, string>
        {
            ["ETag"] = existingETag,
            ["Content-Type"] = "text/plain"
        };
        var downstream = new AtollResponse(200, new ReadOnlyDictionary<string, string>(headers), bytes);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.ShouldBeSameAs(downstream);
        response.Headers["ETag"].ShouldBe(existingETag);
    }

    // ----------------------------------------------------------------
    // Cache-Control header behaviour
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldAddDefaultCacheControlHeaderWhenEnabled()
    {
        var middleware = CacheControlMiddleware.Create();
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse("data")));

        response.Headers.ContainsKey("Cache-Control").ShouldBeTrue();
        response.Headers["Cache-Control"].ShouldBe("no-cache");
    }

    [Fact]
    public async Task ShouldUseCustomDefaultCacheControl()
    {
        var options = new CacheControlMiddlewareOptions { DefaultCacheControl = "public, max-age=3600" };
        var middleware = CacheControlMiddleware.Create(options);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse("data")));

        response.Headers["Cache-Control"].ShouldBe("public, max-age=3600");
    }

    [Fact]
    public async Task ShouldNotAddCacheControlWhenDisabled()
    {
        var options = new CacheControlMiddlewareOptions { IncludeCacheControlHeader = false };
        var middleware = CacheControlMiddleware.Create(options);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse("data")));

        response.Headers.ContainsKey("Cache-Control").ShouldBeFalse();
        response.Headers.ContainsKey("ETag").ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldNotOverwriteExistingCacheControlHeader()
    {
        var middleware = CacheControlMiddleware.Create();
        var bytes = Encoding.UTF8.GetBytes("body");
        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = "text/plain",
            ["Cache-Control"] = "public, max-age=31536000, immutable"
        };
        var downstream = new AtollResponse(200, new ReadOnlyDictionary<string, string>(headers), bytes);
        var context = CreateGetContext("/");

        var response = await middleware(context, () => Task.FromResult(downstream));

        response.Headers["Cache-Control"].ShouldBe("public, max-age=31536000, immutable");
    }

    // ----------------------------------------------------------------
    // Validate ArgumentNullException
    // ----------------------------------------------------------------

    [Fact]
    public void ShouldThrowWhenOptionsIsNull()
    {
        Should.Throw<ArgumentNullException>(() => CacheControlMiddleware.Create(null!));
    }

    // ----------------------------------------------------------------
    // If-None-Match: * wildcard (RFC 7232 §3.2)
    // ----------------------------------------------------------------

    [Fact]
    public async Task ShouldReturn304WhenIfNoneMatchIsWildcard()
    {
        var middleware = CacheControlMiddleware.Create();
        var body = "any content";
        var context = CreateGetContext("/", ifNoneMatch: "*");

        var response = await middleware(context, () => Task.FromResult(CreateBodyResponse(body)));

        response.StatusCode.ShouldBe(304);
        response.Body.ShouldBeNull();
        response.Headers.ContainsKey("ETag").ShouldBeTrue();
    }
}
