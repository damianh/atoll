using Atoll.Reef.Navigation;

namespace Atoll.Reef.Tests.Navigation;

public sealed class PaginationInfoTests
{
    [Fact]
    public void ShouldStoreCurrentPage()
    {
        var info = new PaginationInfo(2, 5, "/blog");
        info.CurrentPage.ShouldBe(2);
    }

    [Fact]
    public void ShouldStoreTotalPages()
    {
        var info = new PaginationInfo(1, 7, "/blog");
        info.TotalPages.ShouldBe(7);
    }

    [Fact]
    public void ShouldStoreBaseUrl()
    {
        var info = new PaginationInfo(1, 3, "/articles");
        info.BaseUrl.ShouldBe("/articles");
    }

    [Fact]
    public void ShouldReturnBaseUrlForPageOne()
    {
        var info = new PaginationInfo(1, 5, "/articles");
        info.GetPageUrl(1).ShouldBe("/articles");
    }

    [Fact]
    public void ShouldReturnPageUrlForPageTwo()
    {
        var info = new PaginationInfo(2, 5, "/articles");
        info.GetPageUrl(2).ShouldBe("/articles/page/2");
    }

    [Fact]
    public void ShouldReturnPageUrlForLaterPages()
    {
        var info = new PaginationInfo(3, 5, "/articles");
        info.GetPageUrl(3).ShouldBe("/articles/page/3");
    }

    [Fact]
    public void ShouldTrimTrailingSlashFromBaseUrl()
    {
        var info = new PaginationInfo(1, 3, "/articles/");
        info.GetPageUrl(2).ShouldBe("/articles/page/2");
    }

    [Fact]
    public void ShouldIndicateHasPreviousWhenNotFirstPage()
    {
        new PaginationInfo(2, 5, "/blog").HasPrevious.ShouldBeTrue();
    }

    [Fact]
    public void ShouldIndicateNoPreviousOnFirstPage()
    {
        new PaginationInfo(1, 5, "/blog").HasPrevious.ShouldBeFalse();
    }

    [Fact]
    public void ShouldIndicateHasNextWhenNotLastPage()
    {
        new PaginationInfo(4, 5, "/blog").HasNext.ShouldBeTrue();
    }

    [Fact]
    public void ShouldIndicateNoNextOnLastPage()
    {
        new PaginationInfo(5, 5, "/blog").HasNext.ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowWhenCurrentPageIsZero()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new PaginationInfo(0, 5, "/blog"));
    }

    [Fact]
    public void ShouldThrowWhenTotalPagesIsZero()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new PaginationInfo(1, 0, "/blog"));
    }

    [Fact]
    public void ShouldThrowWhenCurrentPageExceedsTotalPages()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new PaginationInfo(6, 5, "/blog"));
    }
}
