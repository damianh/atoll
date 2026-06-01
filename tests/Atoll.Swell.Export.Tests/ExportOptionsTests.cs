using Atoll.Swell.Export;

namespace Atoll.Swell.Export.Tests;

public sealed class ExportOptionsTests
{
    [Fact]
    public void should_have_sensible_defaults()
    {
        var options = new ExportOptions();

        options.BaseUrl.ShouldBe("http://localhost:4321");
        options.SlidePath.ShouldBe("/");
        options.SlideCount.ShouldBe(0);
        options.OutputPath.ShouldBe("dist/slides");
        options.AspectRatio.ShouldBe("16/9");
    }

    [Fact]
    public void should_accept_custom_base_url()
    {
        var options = new ExportOptions { BaseUrl = "http://example.com" };

        options.BaseUrl.ShouldBe("http://example.com");
    }

    [Fact]
    public void should_accept_4x3_aspect_ratio()
    {
        var options = new ExportOptions { AspectRatio = "4/3" };

        options.AspectRatio.ShouldBe("4/3");
    }

    [Fact]
    public void should_accept_custom_output_path()
    {
        var options = new ExportOptions { OutputPath = "output/my-talk" };

        options.OutputPath.ShouldBe("output/my-talk");
    }

    [Fact]
    public void should_accept_slide_count()
    {
        var options = new ExportOptions { SlideCount = 12 };

        options.SlideCount.ShouldBe(12);
    }
}
