using Atoll.Build.Ssg;
using Atoll.Core.Components;
using Atoll.Routing;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Ssg;

public sealed class SsgTypesTests
{
    [Fact]
    public void SsgOptionsShouldHaveDefaults()
    {
        var options = new SsgOptions("/output");

        options.OutputDirectory.ShouldBe("/output");
        options.BaseUrl.ShouldBe("");
        options.BasePath.ShouldBe("");
        options.MaxConcurrency.ShouldBe(-1);
        options.CleanOutputDirectory.ShouldBeTrue();
    }

    [Fact]
    public void SsgOptionsShouldThrowOnNullOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() => new SsgOptions(null!));
    }

    [Fact]
    public void SsgOptionsShouldAllowSettingProperties()
    {
        var options = new SsgOptions("/output")
        {
            BaseUrl = "https://example.com",
            BasePath = "/docs",
            MaxConcurrency = 4,
            CleanOutputDirectory = false,
        };

        options.BaseUrl.ShouldBe("https://example.com");
        options.BasePath.ShouldBe("/docs");
        options.MaxConcurrency.ShouldBe(4);
        options.CleanOutputDirectory.ShouldBeFalse();
    }

    [Fact]
    public void SsgRouteShouldStoreAllProperties()
    {
        var parameters = new Dictionary<string, string> { ["slug"] = "test" };
        var props = new Dictionary<string, object?> { ["Title"] = "Test" };

        var route = new SsgRoute("/blog/test", typeof(TestPage), parameters, props);

        route.UrlPath.ShouldBe("/blog/test");
        route.ComponentType.ShouldBe(typeof(TestPage));
        route.Parameters["slug"].ShouldBe("test");
        route.Props["Title"].ShouldBe("Test");
    }

    [Fact]
    public void SsgRouteShouldDefaultToEmptyPropsAndParams()
    {
        var route = new SsgRoute("/about", typeof(TestPage));

        route.UrlPath.ShouldBe("/about");
        route.Parameters.Count.ShouldBe(0);
        route.Props.Count.ShouldBe(0);
    }

    [Fact]
    public void SsgRouteShouldThrowOnNullUrlPath()
    {
        Should.Throw<ArgumentNullException>(() => new SsgRoute(null!, typeof(TestPage)));
    }

    [Fact]
    public void SsgRouteShouldThrowOnNullComponentType()
    {
        Should.Throw<ArgumentNullException>(() => new SsgRoute("/about", null!));
    }

    [Fact]
    public void SsgPageResultSuccessShouldHaveCorrectProperties()
    {
        var route = new SsgRoute("/about", typeof(TestPage));
        var elapsed = TimeSpan.FromMilliseconds(42);
        var result = new SsgPageResult(route, "/output/about/index.html", "<h1>About</h1>", elapsed);

        result.IsSuccess.ShouldBeTrue();
        result.Route.ShouldBe(route);
        result.OutputPath.ShouldBe("/output/about/index.html");
        result.Html.ShouldBe("<h1>About</h1>");
        result.Elapsed.ShouldBe(elapsed);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void SsgPageResultFailureShouldHaveCorrectProperties()
    {
        var route = new SsgRoute("/error", typeof(TestPage));
        var error = new InvalidOperationException("Render failed");
        var elapsed = TimeSpan.FromMilliseconds(5);
        var result = new SsgPageResult(route, error, elapsed);

        result.IsSuccess.ShouldBeFalse();
        result.Route.ShouldBe(route);
        result.OutputPath.ShouldBe("");
        result.Html.ShouldBe("");
        result.Elapsed.ShouldBe(elapsed);
        result.Error.ShouldBe(error);
    }

    [Fact]
    public void SsgResultShouldComputeAggregates()
    {
        var route1 = new SsgRoute("/", typeof(TestPage));
        var route2 = new SsgRoute("/error", typeof(TestPage));

        var results = new List<SsgPageResult>
        {
            new(route1, "/output/index.html", "<h1>Home</h1>", TimeSpan.FromMilliseconds(10)),
            new(route2, new Exception("fail"), TimeSpan.FromMilliseconds(5)),
        };

        var ssgResult = new SsgResult(results, TimeSpan.FromMilliseconds(20));

        ssgResult.TotalCount.ShouldBe(2);
        ssgResult.SuccessCount.ShouldBe(1);
        ssgResult.FailureCount.ShouldBe(1);
        ssgResult.IsSuccess.ShouldBeFalse();
        ssgResult.Failures.Count.ShouldBe(1);
        ssgResult.TotalElapsed.TotalMilliseconds.ShouldBe(20);
    }

    [Fact]
    public void SsgResultAllSuccessShouldReportSuccess()
    {
        var route = new SsgRoute("/", typeof(TestPage));
        var results = new List<SsgPageResult>
        {
            new(route, "/output/index.html", "<h1>Home</h1>", TimeSpan.FromMilliseconds(10)),
        };

        var ssgResult = new SsgResult(results, TimeSpan.FromMilliseconds(15));

        ssgResult.IsSuccess.ShouldBeTrue();
        ssgResult.Failures.Count.ShouldBe(0);
    }

    [Fact]
    public void SsgResultEmptyShouldReportSuccess()
    {
        var ssgResult = new SsgResult([], TimeSpan.FromMilliseconds(1));

        ssgResult.IsSuccess.ShouldBeTrue();
        ssgResult.TotalCount.ShouldBe(0);
    }

    private sealed class TestPage : AtollComponent, IAtollPage
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<h1>Test</h1>");
            return Task.CompletedTask;
        }
    }
}
