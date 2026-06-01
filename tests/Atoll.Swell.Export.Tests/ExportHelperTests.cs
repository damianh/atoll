using Atoll.Swell.Export;

namespace Atoll.Swell.Export.Tests;

public sealed class ExportHelperTests
{
    // ── ResolveHeight ──

    [Fact]
    public void should_resolve_16_9_to_720()
    {
        ExportHelper.ResolveHeight("16/9").ShouldBe(720);
    }

    [Fact]
    public void should_resolve_4_3_to_960()
    {
        ExportHelper.ResolveHeight("4/3").ShouldBe(960);
    }

    [Fact]
    public void should_resolve_3_2_to_853()
    {
        ExportHelper.ResolveHeight("3/2").ShouldBe(853);
    }

    [Fact]
    public void should_fallback_to_720_for_invalid_ratio()
    {
        ExportHelper.ResolveHeight("invalid").ShouldBe(720);
    }

    [Fact]
    public void should_fallback_to_720_for_empty_string()
    {
        ExportHelper.ResolveHeight("").ShouldBe(720);
    }

    // ── ValidateSlideCount ──

    [Fact]
    public void should_throw_for_zero_slide_count()
    {
        var options = new ExportOptions { SlideCount = 0 };

        var ex = Should.Throw<ArgumentException>(() => ExportHelper.ValidateSlideCount(options));
        ex.Message.ShouldContain("SlideCount");
    }

    [Fact]
    public void should_throw_for_negative_slide_count()
    {
        var options = new ExportOptions { SlideCount = -1 };

        Should.Throw<ArgumentException>(() => ExportHelper.ValidateSlideCount(options));
    }

    [Fact]
    public void should_not_throw_for_positive_slide_count()
    {
        var options = new ExportOptions { SlideCount = 5 };

        Should.NotThrow(() => ExportHelper.ValidateSlideCount(options));
    }
}
