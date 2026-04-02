using Atoll.Build.Pipeline;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Pipeline;

public sealed class JsProcessorTests
{
    [Fact]
    public void ProcessShouldMinifyJsByDefault()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Fingerprint = false,
        });
        var result = processor.Process("function hello() { console.log('hello'); }");

        result.HasContent.ShouldBeTrue();
        // NUglify removes whitespace in minification
        result.Js.Length.ShouldBeLessThan("function hello() { console.log('hello'); }".Length);
    }

    [Fact]
    public void ProcessShouldNotMinifyWhenDisabled()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Minify = false,
            Fingerprint = false,
        });
        var original = "function hello() { console.log('hello'); }";
        var result = processor.Process(original);

        result.HasContent.ShouldBeTrue();
        result.Js.ShouldBe(original);
    }

    [Fact]
    public void ProcessShouldFingerprintByDefault()
    {
        var processor = new JsProcessor();
        var result = processor.Process("function hello() { console.log('hello'); }");

        result.HasContent.ShouldBeTrue();
        result.Hash.ShouldNotBeNull();
        result.FileName.ShouldContain(result.Hash!);
        result.FileName.ShouldEndWith(".js");
    }

    [Fact]
    public void ProcessShouldNotFingerprintWhenDisabled()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Fingerprint = false,
        });
        var result = processor.Process("var x = 1;");

        result.Hash.ShouldBeNull();
        result.FileName.ShouldBe("scripts.js");
    }

    [Fact]
    public void ProcessShouldRespectOutputSubdirectory()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Fingerprint = false,
            OutputSubdirectory = "_astro",
        });
        var result = processor.Process("var x = 1;");

        result.OutputPath.ShouldBe(Path.Combine("_astro", "scripts.js"));
    }

    [Fact]
    public void ProcessShouldHandleEmptySubdirectory()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Fingerprint = false,
            OutputSubdirectory = "",
        });
        var result = processor.Process("var x = 1;");

        result.OutputPath.ShouldBe("scripts.js");
    }

    [Fact]
    public void ProcessShouldReturnEmptyForEmptyInput()
    {
        var processor = new JsProcessor();
        var result = processor.Process("");

        result.HasContent.ShouldBeFalse();
        result.Js.ShouldBe("");
    }

    [Fact]
    public void ProcessShouldConcatenateMultipleSources()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Minify = false,
            Fingerprint = false,
        });
        var result = processor.Process(new[] { "var a = 1;", "var b = 2;" });

        result.HasContent.ShouldBeTrue();
        result.Js.ShouldContain("var a = 1;");
        result.Js.ShouldContain("var b = 2;");
    }

    [Fact]
    public void ProcessShouldReturnEmptyForEmptySourceList()
    {
        var processor = new JsProcessor();
        var result = processor.Process(Array.Empty<string>());

        result.HasContent.ShouldBeFalse();
    }

    [Fact]
    public void ProcessWithCustomFileNameShouldRespectIt()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Fingerprint = false,
        });
        var result = processor.Process("var x = 1;", "islands.js");

        result.FileName.ShouldBe("islands.js");
    }

    [Fact]
    public void ProcessWithCustomFileNameShouldFingerprint()
    {
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Fingerprint = true,
        });
        var result = processor.Process("var x = 1;", "islands.js");

        result.Hash.ShouldNotBeNull();
        result.FileName.ShouldStartWith("islands.");
        result.FileName.ShouldEndWith(".js");
    }

    [Fact]
    public void ProcessShouldThrowOnNullString()
    {
        var processor = new JsProcessor();
        Should.Throw<ArgumentNullException>(() => processor.Process((string)null!));
    }

    [Fact]
    public void ProcessShouldThrowOnNullSources()
    {
        var processor = new JsProcessor();
        Should.Throw<ArgumentNullException>(() => processor.Process((IEnumerable<string>)null!));
    }

    [Fact]
    public void JsProcessorOptionsShouldHaveSensibleDefaults()
    {
        var options = new JsProcessorOptions();

        options.Minify.ShouldBeTrue();
        options.Fingerprint.ShouldBeTrue();
        options.OutputFileName.ShouldBe("scripts.js");
        options.OutputSubdirectory.ShouldBe("_astro");
    }

    [Fact]
    public void JsProcessResultEmptyShouldHaveNoContent()
    {
        JsProcessResult.Empty.HasContent.ShouldBeFalse();
        JsProcessResult.Empty.Js.ShouldBe("");
        JsProcessResult.Empty.Hash.ShouldBeNull();
    }

    [Fact]
    public void ProcessShouldHandleInvalidJsGracefully()
    {
        // NUglify should fail on invalid JS; we should still get the original back
        var processor = new JsProcessor(new JsProcessorOptions
        {
            Minify = true,
            Fingerprint = false,
        });
        var invalidJs = "function { {{{";
        var result = processor.Process(invalidJs);

        result.HasContent.ShouldBeTrue();
        // Should return the original JS when minification fails
        result.Js.ShouldBe(invalidJs);
    }

    [Fact]
    public void FingerprintShouldBeDeterministic()
    {
        var processor = new JsProcessor();
        var result1 = processor.Process("var x = 42;");
        var result2 = processor.Process("var x = 42;");

        result1.Hash.ShouldBe(result2.Hash);
        result1.FileName.ShouldBe(result2.FileName);
    }
}
