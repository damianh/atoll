using Atoll.Swell.Layouts;

namespace Atoll.Swell.Tests.Layouts;

public sealed class SlideLayoutResolverTests
{
    [Theory]
    [InlineData("default", typeof(DefaultSlideLayout))]
    [InlineData("cover", typeof(CoverSlideLayout))]
    [InlineData("center", typeof(CenterSlideLayout))]
    [InlineData("two-cols", typeof(TwoColsSlideLayout))]
    [InlineData("image-right", typeof(ImageRightSlideLayout))]
    [InlineData("image-left", typeof(ImageLeftSlideLayout))]
    [InlineData("section", typeof(SectionSlideLayout))]
    [InlineData("end", typeof(EndSlideLayout))]
    public void should_resolve_known_layout_name_to_correct_type(string layoutName, Type expectedType)
    {
        var resolved = SlideLayoutResolver.Resolve(layoutName);

        resolved.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("DEFAULT")]
    [InlineData("Cover")]
    [InlineData("TWO-COLS")]
    public void should_resolve_layout_name_case_insensitively(string layoutName)
    {
        var resolved = SlideLayoutResolver.Resolve(layoutName);

        resolved.ShouldNotBeNull();
    }

    [Fact]
    public void should_fall_back_to_default_layout_for_unknown_name()
    {
        var resolved = SlideLayoutResolver.Resolve("unknown-layout");

        resolved.ShouldBe(typeof(DefaultSlideLayout));
    }

    [Fact]
    public void should_fall_back_to_default_layout_for_null_name()
    {
        var resolved = SlideLayoutResolver.Resolve(null);

        resolved.ShouldBe(typeof(DefaultSlideLayout));
    }
}
