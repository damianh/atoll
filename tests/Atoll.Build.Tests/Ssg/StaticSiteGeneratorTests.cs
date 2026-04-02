using Atoll.Build.Ssg;
using Atoll.Core.Components;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Ssg;

public sealed class StaticSiteGeneratorTests : IDisposable
{
    private readonly string _outputDir;

    public StaticSiteGeneratorTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), "atoll-ssg-test-" + Guid.NewGuid().ToString("N")[..8]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    [Fact]
    public async Task ShouldGenerateStaticSiteWithSinglePage()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(1);
        result.SuccessCount.ShouldBe(1);
        result.FailureCount.ShouldBe(0);

        var pageResult = result.PageResults[0];
        pageResult.IsSuccess.ShouldBeTrue();
        pageResult.Html.ShouldContain("<h1>Home</h1>");
        pageResult.Html.ShouldStartWith("<!DOCTYPE html>");
        File.Exists(pageResult.OutputPath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldGenerateMultipleStaticPages()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
            new RouteEntry("/contact", typeof(TestContactPage), "contact.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(3);

        var htmlContents = result.PageResults.Select(r => r.Html).ToList();
        htmlContents.ShouldContain(h => h.Contains("<h1>Home</h1>", StringComparison.Ordinal));
        htmlContents.ShouldContain(h => h.Contains("<h1>About</h1>", StringComparison.Ordinal));
        htmlContents.ShouldContain(h => h.Contains("<h1>Contact</h1>", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ShouldGenerateDynamicPages()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestBlogPage), "blog/[slug].cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(2);

        var htmlContents = result.PageResults.Select(r => r.Html).ToList();
        htmlContents.ShouldContain(h => h.Contains("<h1>Blog: hello-world</h1>", StringComparison.Ordinal));
        htmlContents.ShouldContain(h => h.Contains("<h1>Blog: second-post</h1>", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ShouldPassPropsFromStaticPathsToComponent()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestBlogPageWithProps), "blog/[slug].cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(1);
        result.PageResults[0].Html.ShouldContain("<h1>My Post Title</h1>");
    }

    [Fact]
    public async Task ShouldSkipEndpoints()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
            new RouteEntry("/api/posts", typeof(TestApiEndpoint), "api/posts.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.TotalCount.ShouldBe(1);
        result.PageResults[0].Route.UrlPath.ShouldBe("/");
    }

    [Fact]
    public async Task ShouldWriteOutputFilesWithCleanUrls()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();

        // Verify output file structure
        var indexPath = Path.Combine(_outputDir, "index.html");
        File.Exists(indexPath).ShouldBeTrue();

        var aboutPath = Path.Combine(_outputDir, "about", "index.html");
        File.Exists(aboutPath).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCleanOutputDirectoryBeforeGeneration()
    {
        Directory.CreateDirectory(_outputDir);
        var oldFile = Path.Combine(_outputDir, "old-file.html");
        await File.WriteAllTextAsync(oldFile, "old content");

        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
        };

        await generator.GenerateAsync(routes);

        File.Exists(oldFile).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldNotCleanOutputDirectoryWhenDisabled()
    {
        Directory.CreateDirectory(_outputDir);
        var oldFile = Path.Combine(_outputDir, "old-file.html");
        await File.WriteAllTextAsync(oldFile, "old content");

        var options = new SsgOptions(_outputDir) { CleanOutputDirectory = false };
        var generator = new StaticSiteGenerator(options);
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
        };

        await generator.GenerateAsync(routes);

        File.Exists(oldFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldHandleEmptyRouteTable()
    {
        var generator = CreateGenerator();
        var result = await generator.GenerateAsync([]);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task ShouldReportTimingInformation()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.TotalElapsed.ShouldBeGreaterThan(TimeSpan.Zero);
        result.PageResults[0].Elapsed.ShouldBeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ShouldCaptureSinglePageErrors()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
            new RouteEntry("/error", typeof(TestErrorPage), "error.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeFalse();
        result.SuccessCount.ShouldBe(1);
        result.FailureCount.ShouldBe(1);

        var failure = result.Failures[0];
        failure.IsSuccess.ShouldBeFalse();
        failure.Error.ShouldNotBeNull();
        failure.Error!.Message.ShouldContain("Page render error");
    }

    [Fact]
    public async Task ShouldRenderPageWithLayout()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestPageWithLayout), "index.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        var html = result.PageResults[0].Html;
        html.ShouldContain("<html>");
        html.ShouldContain("<body>");
        html.ShouldContain("<h1>Hello from layout page</h1>");
        html.ShouldContain("</body>");
        html.ShouldContain("</html>");
    }

    [Fact]
    public async Task ShouldGeneratePagesSequentiallyWithConcurrencyOne()
    {
        var options = new SsgOptions(_outputDir) { MaxConcurrency = 1 };
        var generator = new StaticSiteGenerator(options);
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task ShouldGeneratePagesInParallelByDefault()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/page1", typeof(TestHomePage), "page1.cs"),
            new RouteEntry("/page2", typeof(TestAboutPage), "page2.cs"),
            new RouteEntry("/page3", typeof(TestContactPage), "page3.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task ShouldGenerateDynamicPagesWithOutputFiles()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/blog/[slug]", typeof(TestBlogPage), "blog/[slug].cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();

        // Check output files exist for dynamic routes
        var helloPath = Path.Combine(_outputDir, "blog", "hello-world", "index.html");
        File.Exists(helloPath).ShouldBeTrue();
        var helloContent = await File.ReadAllTextAsync(helloPath);
        helloContent.ShouldContain("<h1>Blog: hello-world</h1>");

        var secondPath = Path.Combine(_outputDir, "blog", "second-post", "index.html");
        File.Exists(secondPath).ShouldBeTrue();
    }

    [Fact]
    public void ShouldThrowOnNullRoutes()
    {
        var generator = CreateGenerator();
        Should.ThrowAsync<ArgumentNullException>(() => generator.GenerateAsync(null!));
    }

    [Fact]
    public void ShouldThrowOnNullOptions()
    {
        Should.Throw<ArgumentNullException>(() => new StaticSiteGenerator(null!));
    }

    [Fact]
    public async Task ShouldGenerateMixedStaticAndDynamicRoutes()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
            new RouteEntry("/about", typeof(TestAboutPage), "about.cs"),
            new RouteEntry("/blog/[slug]", typeof(TestBlogPage), "blog/[slug].cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.IsSuccess.ShouldBeTrue();
        result.TotalCount.ShouldBe(4); // 2 static + 2 dynamic

        // All output files should exist
        File.Exists(Path.Combine(_outputDir, "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "about", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "blog", "hello-world", "index.html")).ShouldBeTrue();
        File.Exists(Path.Combine(_outputDir, "blog", "second-post", "index.html")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldAutoInsertDoctype()
    {
        var generator = CreateGenerator();
        var routes = new[]
        {
            new RouteEntry("/", typeof(TestHomePage), "index.cs"),
        };

        var result = await generator.GenerateAsync(routes);

        result.PageResults[0].Html.ShouldStartWith("<!DOCTYPE html>");
    }

    private StaticSiteGenerator CreateGenerator()
    {
        return new StaticSiteGenerator(new SsgOptions(_outputDir));
    }

    // ── Test helpers ─────────────────────────────────────────────

    private sealed class TestHomePage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Home</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestAboutPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>About</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestContactPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Contact</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestErrorPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            throw new InvalidOperationException("Page render error");
        }
    }

    private sealed class TestBaseLayout : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<html><head></head><body>");
            await RenderSlotAsync();
            context.WriteHtml("</body></html>");
        }
    }

    [Layout(typeof(TestBaseLayout))]
    private sealed class TestPageWithLayout : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Hello from layout page</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestBlogPage : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Slug { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(new Dictionary<string, string> { ["slug"] = "hello-world" }),
                new StaticPath(new Dictionary<string, string> { ["slug"] = "second-post" }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>Blog: {Slug}</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestBlogPageWithProps : AtollComponent, IAtollPage, IStaticPathsProvider
    {
        [Parameter]
        public string Title { get; set; } = "";

        public Task<IReadOnlyList<StaticPath>> GetStaticPathsAsync()
        {
            IReadOnlyList<StaticPath> paths = new[]
            {
                new StaticPath(
                    new Dictionary<string, string> { ["slug"] = "my-post" },
                    new Dictionary<string, object?> { ["Title"] = "My Post Title" }),
            };
            return Task.FromResult(paths);
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml($"<h1>{Title}</h1>");
            return Task.CompletedTask;
        }
    }

    private sealed class TestApiEndpoint : IAtollEndpoint
    {
        public Task<AtollResponse> GetAsync(EndpointContext context)
        {
            return Task.FromResult(AtollResponse.Json(new { data = "test" }));
        }
    }
}
