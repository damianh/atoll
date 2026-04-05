using Atoll.Reef.Configuration;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Configuration;

public sealed class ReefConfigTests
{
    [Fact]
    public void ShouldHaveSensibleDefaults()
    {
        var config = new ReefConfig();

        config.ArticlesPerPage.ShouldBe(10);
        config.DefaultView.ShouldBe(DefaultView.List);
        config.TagPageEnabled.ShouldBeTrue();
        config.AuthorPageEnabled.ShouldBeTrue();
        config.RssEnabled.ShouldBeTrue();
        config.BasePath.ShouldBe("");
        config.CollectionName.ShouldBe("articles");
        config.Social.ShouldBeEmpty();
        config.CustomCss.ShouldBeEmpty();
        config.Authors.ShouldBeEmpty();
    }
}
