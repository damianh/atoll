using Atoll.Components;
using Atoll.Instructions;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Giscus.Tests;

public sealed class GiscusCommentsTests
{
    private static async Task<string> RenderAsync(Dictionary<string, object?> props)
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<GiscusComments>(dest, props);
        return dest.GetOutput();
    }

    private static Dictionary<string, object?> RequiredProps() => new()
    {
        [nameof(GiscusComments.Repo)] = "owner/my-repo",
        [nameof(GiscusComments.RepoId)] = "R_kgDOABCDEF",
        [nameof(GiscusComments.CategoryId)] = "DIC_kwDOABCDEF",
    };

    // ── Render: optional category ────────────────────────────────────────────

    [Fact]
    public async Task ShouldOmitDataCategoryWhenNotProvided()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldNotContain("data-category=");
    }

    [Fact]
    public async Task ShouldRenderDataCategoryWhenProvided()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Category)] = "Announcements";
        var html = await RenderAsync(props);

        html.ShouldContain("data-category=\"Announcements\"");
    }

    // ── Render: non-default values ───────────────────────────────────────────

    [Fact]
    public async Task ShouldRenderCustomMappingUrl()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Mapping)] = GiscusMapping.Url;
        var html = await RenderAsync(props);

        html.ShouldContain("data-mapping=\"url\"");
    }

    [Fact]
    public async Task ShouldRenderOgTitleMappingWithSpecialCharacter()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Mapping)] = GiscusMapping.OgTitle;
        var html = await RenderAsync(props);

        html.ShouldContain("data-mapping=\"og:title\"");
    }

    [Fact]
    public async Task ShouldRenderTermWhenProvided()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Mapping)] = GiscusMapping.Specific;
        props[nameof(GiscusComments.Term)] = "my-custom-term";
        var html = await RenderAsync(props);

        html.ShouldContain("data-term=\"my-custom-term\"");
    }

    [Fact]
    public async Task ShouldOmitDataTermWhenNotProvided()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldNotContain("data-term=");
    }

    [Fact]
    public async Task ShouldRenderStrictAsOneWhenEnabled()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Strict)] = true;
        var html = await RenderAsync(props);

        html.ShouldContain("data-strict=\"1\"");
    }

    [Fact]
    public async Task ShouldRenderReactionsEnabledAsZeroWhenDisabled()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.ReactionsEnabled)] = false;
        var html = await RenderAsync(props);

        html.ShouldContain("data-reactions-enabled=\"0\"");
    }

    [Fact]
    public async Task ShouldRenderEmitMetadataAsOneWhenEnabled()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.EmitMetadata)] = true;
        var html = await RenderAsync(props);

        html.ShouldContain("data-emit-metadata=\"1\"");
    }

    [Fact]
    public async Task ShouldRenderInputPositionTopWhenSet()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.InputPosition)] = GiscusInputPosition.Top;
        var html = await RenderAsync(props);

        html.ShouldContain("data-input-position=\"top\"");
    }

    [Fact]
    public async Task ShouldRenderCustomTheme()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Theme)] = "dark";
        var html = await RenderAsync(props);

        html.ShouldContain("data-theme=\"dark\"");
    }

    [Fact]
    public async Task ShouldRenderCustomLanguage()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Lang)] = "fr";
        var html = await RenderAsync(props);

        html.ShouldContain("data-lang=\"fr\"");
    }

    [Fact]
    public async Task ShouldRenderEagerLoading()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Loading)] = GiscusLoading.Eager;
        var html = await RenderAsync(props);

        html.ShouldContain("data-loading=\"eager\"");
    }

    // ── Render: HTML encoding ────────────────────────────────────────────────

    [Fact]
    public async Task ShouldHtmlEncodeRepoAttributeValue()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Repo)] = "owner/<repo>";
        var html = await RenderAsync(props);

        html.ShouldContain("data-repo=\"owner/&lt;repo&gt;\"");
        html.ShouldNotContain("data-repo=\"owner/<repo>\"");
    }

    [Fact]
    public async Task ShouldHtmlEncodeThemeAttributeValue()
    {
        var props = RequiredProps();
        props[nameof(GiscusComments.Theme)] = "https://example.com/theme.css?a=1&b=2";
        var html = await RenderAsync(props);

        html.ShouldContain("data-theme=\"https://example.com/theme.css?a=1&amp;b=2\"");
    }

    // ── Directive ────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveClientVisibleDirective()
    {
        var island = new GiscusComments();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void ShouldHaveRootMarginOf300px()
    {
        var island = new GiscusComments();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveValue.ShouldBe("300px");
    }

    // ── Module URL ───────────────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveCorrectClientModuleUrl()
    {
        var island = new GiscusComments();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-giscus.js");
    }

    // ── Island wrapper ───────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new GiscusComments
        {
            Repo = "owner/repo",
            RepoId = "R_kgDOABCDEF",
            CategoryId = "DIC_kwDOABCDEF",
        };
        await island.RenderIslandAsync(dest, new Dictionary<string, object?>
        {
            [nameof(GiscusComments.Repo)] = "owner/repo",
            [nameof(GiscusComments.RepoId)] = "R_kgDOABCDEF",
            [nameof(GiscusComments.CategoryId)] = "DIC_kwDOABCDEF",
        });
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"visible\"");
        html.ShouldContain("component-url=\"/scripts/atoll-giscus.js\"");
        html.ShouldContain("<div class=\"giscus\"");
    }
}
