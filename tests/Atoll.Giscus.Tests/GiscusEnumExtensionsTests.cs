using Shouldly;
using Xunit;

namespace Atoll.Giscus.Tests;

public sealed class GiscusEnumExtensionsTests
{
    // ── GiscusMapping ────────────────────────────────────────────────────────

    [Fact]
    public void GiscusMappingPathnameShouldReturnPathname()
    {
        GiscusMapping.Pathname.ToDataValue().ShouldBe("pathname");
    }

    [Fact]
    public void GiscusMappingUrlShouldReturnUrl()
    {
        GiscusMapping.Url.ToDataValue().ShouldBe("url");
    }

    [Fact]
    public void GiscusMappingTitleShouldReturnTitle()
    {
        GiscusMapping.Title.ToDataValue().ShouldBe("title");
    }

    [Fact]
    public void GiscusMappingOgTitleShouldReturnOgTitleWithColon()
    {
        GiscusMapping.OgTitle.ToDataValue().ShouldBe("og:title");
    }

    [Fact]
    public void GiscusMappingSpecificShouldReturnSpecific()
    {
        GiscusMapping.Specific.ToDataValue().ShouldBe("specific");
    }

    [Fact]
    public void GiscusMappingNumberShouldReturnNumber()
    {
        GiscusMapping.Number.ToDataValue().ShouldBe("number");
    }

    // ── GiscusInputPosition ──────────────────────────────────────────────────

    [Fact]
    public void GiscusInputPositionTopShouldReturnTop()
    {
        GiscusInputPosition.Top.ToDataValue().ShouldBe("top");
    }

    [Fact]
    public void GiscusInputPositionBottomShouldReturnBottom()
    {
        GiscusInputPosition.Bottom.ToDataValue().ShouldBe("bottom");
    }

    // ── GiscusLoading ────────────────────────────────────────────────────────

    [Fact]
    public void GiscusLoadingLazyShouldReturnLazy()
    {
        GiscusLoading.Lazy.ToDataValue().ShouldBe("lazy");
    }

    [Fact]
    public void GiscusLoadingEagerShouldReturnEager()
    {
        GiscusLoading.Eager.ToDataValue().ShouldBe("eager");
    }
}
