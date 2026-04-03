using Atoll.Build.Pipeline;
using Atoll.Components;
using Atoll.Css;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Pipeline;

public sealed class CssProcessorTests
{
    [Fact]
    public void ProcessShouldReturnEmptyForNoComponentStyles()
    {
        var processor = new CssProcessor();
        var result = processor.Process(new[] { typeof(NoStyleComponent) });

        result.HasContent.ShouldBeFalse();
        result.Css.ShouldBe("");
    }

    [Fact]
    public void ProcessShouldExtractAndMinifyCssFromComponents()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Minify = true,
            Fingerprint = false,
        });
        var result = processor.Process(new[] { typeof(StyledComponent) });

        result.HasContent.ShouldBeTrue();
        // CSS should be minified (no extra whitespace)
        result.Css.ShouldNotContain("  ");
    }

    [Fact]
    public void ProcessShouldFingerprintByDefault()
    {
        var processor = new CssProcessor();
        var result = processor.Process(new[] { typeof(StyledComponent) });

        result.HasContent.ShouldBeTrue();
        result.Hash.ShouldNotBeNull();
        result.FileName.ShouldContain(result.Hash!);
        result.FileName.ShouldEndWith(".css");
    }

    [Fact]
    public void ProcessShouldNotFingerprintWhenDisabled()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Fingerprint = false,
        });
        var result = processor.Process(new[] { typeof(StyledComponent) });

        result.HasContent.ShouldBeTrue();
        result.Hash.ShouldBeNull();
        result.FileName.ShouldBe("styles.css");
    }

    [Fact]
    public void ProcessShouldRespectOutputSubdirectory()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Fingerprint = false,
            OutputSubdirectory = "_astro",
        });
        var result = processor.Process(new[] { typeof(StyledComponent) });

        result.OutputPath.ShouldBe(Path.Combine("_astro", "styles.css"));
    }

    [Fact]
    public void ProcessShouldHandleEmptySubdirectory()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Fingerprint = false,
            OutputSubdirectory = "",
        });
        var result = processor.Process(new[] { typeof(StyledComponent) });

        result.OutputPath.ShouldBe("styles.css");
    }

    [Fact]
    public void ProcessShouldRewriteUrlsWhenBasePathSet()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Minify = false,
            Fingerprint = false,
            BasePath = "/docs",
        });
        var result = processor.Process(new[] { typeof(UrlComponent) });

        result.HasContent.ShouldBeTrue();
        result.Css.ShouldContain("/docs/images/bg.png");
    }

    [Fact]
    public void ProcessShouldNotRewriteUrlsWhenNoBasePath()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Minify = false,
            Fingerprint = false,
        });
        var result = processor.Process(new[] { typeof(UrlComponent) });

        result.HasContent.ShouldBeTrue();
        result.Css.ShouldContain("/images/bg.png");
        result.Css.ShouldNotContain("/docs/images/bg.png");
    }

    [Fact]
    public void ProcessWithRawCssShouldMinifyAndFingerprint()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Minify = true,
            Fingerprint = true,
        });
        var result = processor.Process("body { color: red; }");

        result.HasContent.ShouldBeTrue();
        result.Hash.ShouldNotBeNull();
        result.FileName.ShouldContain(result.Hash!);
    }

    [Fact]
    public void ProcessWithEmptyRawCssShouldReturnEmpty()
    {
        var processor = new CssProcessor();
        var result = processor.Process("");

        result.HasContent.ShouldBeFalse();
    }

    [Fact]
    public void ProcessWithAggregatorShouldWork()
    {
        var aggregator = new CssAggregator();
        aggregator.Add("test-id", ".card { padding: 1rem; }", false);

        var processor = new CssProcessor(new CssProcessorOptions
        {
            Fingerprint = false,
            Minify = false,
        });
        var result = processor.Process(aggregator);

        result.HasContent.ShouldBeTrue();
        result.Css.ShouldContain(".card");
    }

    [Fact]
    public void ProcessWithEmptyAggregatorShouldReturnEmpty()
    {
        var aggregator = new CssAggregator();

        var processor = new CssProcessor();
        var result = processor.Process(aggregator);

        result.HasContent.ShouldBeFalse();
    }

    [Fact]
    public void ProcessShouldUseCustomOutputFileName()
    {
        var processor = new CssProcessor(new CssProcessorOptions
        {
            Fingerprint = false,
            OutputFileName = "app.css",
        });
        var result = processor.Process("body { margin: 0; }");

        result.FileName.ShouldBe("app.css");
    }

    [Fact]
    public void CssProcessorOptionsShouldHaveSensibleDefaults()
    {
        var options = new CssProcessorOptions();

        options.Minify.ShouldBeTrue();
        options.BasePath.ShouldBe("");
        options.Fingerprint.ShouldBeTrue();
        options.OutputFileName.ShouldBe("styles.css");
        options.OutputSubdirectory.ShouldBe("_astro");
    }

    [Fact]
    public void CssProcessResultEmptyShouldHaveNoContent()
    {
        CssProcessResult.Empty.HasContent.ShouldBeFalse();
        CssProcessResult.Empty.Css.ShouldBe("");
        CssProcessResult.Empty.Hash.ShouldBeNull();
    }

    [Fact]
    public void ProcessShouldThrowOnNullComponentTypes()
    {
        var processor = new CssProcessor();
        Should.Throw<ArgumentNullException>(() => processor.Process((IEnumerable<Type>)null!));
    }

    [Fact]
    public void ProcessShouldThrowOnNullRawCss()
    {
        var processor = new CssProcessor();
        Should.Throw<ArgumentNullException>(() => processor.Process((string)null!));
    }

    [Fact]
    public void ProcessShouldThrowOnNullAggregator()
    {
        var processor = new CssProcessor();
        Should.Throw<ArgumentNullException>(() => processor.Process((CssAggregator)null!));
    }

    // Test components
    private sealed class NoStyleComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div>No styles</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".card { padding: 1rem; }")]
    private sealed class StyledComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class='card'>Content</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".bg { background: url('/images/bg.png'); }")]
    private sealed class UrlComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class='bg'>Content</div>");
            return Task.CompletedTask;
        }
    }
}
