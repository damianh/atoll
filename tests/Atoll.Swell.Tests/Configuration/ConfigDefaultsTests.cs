using Atoll.Swell.Configuration;
using Atoll.Swell.Markdown;

namespace Atoll.Swell.Tests.Configuration;

public sealed class DeckConfigTests
{
    [Fact]
    public void should_have_sensible_defaults()
    {
        var config = new DeckConfig();

        config.Title.ShouldBe("");
        config.AspectRatio.ShouldBe(AspectRatio.Ratio16x9);
        config.Transition.ShouldBe(TransitionType.None);
        config.SlideNumbers.ShouldBeTrue();
        config.Theme.ShouldBe("default");
        config.Export.ShouldBeEmpty();
    }
}

public sealed class SlideConfigTests
{
    [Fact]
    public void should_have_sensible_defaults()
    {
        var config = new SlideConfig();

        config.Layout.ShouldBe("default");
        config.Background.ShouldBeNull();
        config.Class.ShouldBeNull();
        config.Transition.ShouldBeNull();
        config.SlideNumber.ShouldBeNull();
    }
}
