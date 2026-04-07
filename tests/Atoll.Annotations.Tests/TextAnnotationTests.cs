using Atoll.Components;
using Atoll.Instructions;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Annotations.Tests;

public sealed class TextAnnotationTests
{
    private static async Task<string> RenderAsync(Dictionary<string, object?> props)
    {
        var dest = new StringRenderDestination();
        await ComponentRenderer.RenderComponentAsync<TextAnnotation>(dest, props);
        return dest.GetOutput();
    }

    private static Dictionary<string, object?> RequiredProps() => new()
    {
        [nameof(TextAnnotation.Repo)] = "owner/my-repo",
        [nameof(TextAnnotation.Target)] = AnnotationTarget.Issue,
    };

    // ── Render: container element ────────────────────────────────────────────

    [Fact]
    public async Task ShouldRenderAnnotationsContainerDiv()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldContain("<div class=\"atoll-annotations\"");
    }

    // ── Render: required parameters ─────────────────────────────────────────

    [Fact]
    public async Task ShouldRenderDataRepoAttribute()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldContain("data-repo=\"owner/my-repo\"");
    }

    [Fact]
    public async Task ShouldRenderDataTargetForIssue()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldContain("data-target=\"issue\"");
    }

    [Fact]
    public async Task ShouldRenderDataTargetForDiscussion()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.Target)] = AnnotationTarget.Discussion;
        var html = await RenderAsync(props);

        html.ShouldContain("data-target=\"discussion\"");
    }

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
        props[nameof(TextAnnotation.Category)] = "General";
        var html = await RenderAsync(props);

        html.ShouldContain("data-category=\"General\"");
    }

    // ── Render: optional labels ──────────────────────────────────────────────

    [Fact]
    public async Task ShouldOmitDataLabelsWhenNotProvided()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldNotContain("data-labels=");
    }

    [Fact]
    public async Task ShouldRenderDataLabelsWhenProvided()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.Labels)] = "feedback,docs";
        var html = await RenderAsync(props);

        html.ShouldContain("data-labels=\"feedback,docs\"");
    }

    // ── Render: defaults ─────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldRenderDefaultTitlePrefix()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldContain("data-title-prefix=\"Feedback:\"");
    }

    [Fact]
    public async Task ShouldRenderCustomTitlePrefix()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.TitlePrefix)] = "Bug:";
        var html = await RenderAsync(props);

        html.ShouldContain("data-title-prefix=\"Bug:\"");
    }

    [Fact]
    public async Task ShouldRenderDefaultContentSelector()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldContain("data-content-selector=\"article\"");
    }

    [Fact]
    public async Task ShouldRenderCustomContentSelector()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.ContentSelector)] = ".main-content";
        var html = await RenderAsync(props);

        html.ShouldContain("data-content-selector=\".main-content\"");
    }

    [Fact]
    public async Task ShouldRenderDefaultButtonText()
    {
        var html = await RenderAsync(RequiredProps());

        html.ShouldContain("data-button-text=\"&#128172;\"");
    }

    [Fact]
    public async Task ShouldRenderCustomButtonText()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.ButtonText)] = "Comment";
        var html = await RenderAsync(props);

        html.ShouldContain("data-button-text=\"Comment\"");
    }

    // ── Render: HTML encoding ────────────────────────────────────────────────

    [Fact]
    public async Task ShouldHtmlEncodeRepoAttributeValue()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.Repo)] = "owner/<repo>";
        var html = await RenderAsync(props);

        html.ShouldContain("data-repo=\"owner/&lt;repo&gt;\"");
    }

    [Fact]
    public async Task ShouldNotContainUnEncodedRepoValue()
    {
        var props = RequiredProps();
        props[nameof(TextAnnotation.Repo)] = "owner/<repo>";
        var html = await RenderAsync(props);

        html.ShouldNotContain("data-repo=\"owner/<repo>\"");
    }

    // ── Directive ────────────────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveClientIdleDirective()
    {
        var island = new TextAnnotation();

        var metadata = island.CreateMetadata();

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    // ── Module URL ───────────────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveCorrectClientModuleUrl()
    {
        var island = new TextAnnotation();

        island.ClientModuleUrl.ShouldBe("/scripts/atoll-annotations.js");
    }

    // ── Island wrapper ───────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldRenderAsIslandWrapper()
    {
        var dest = new StringRenderDestination();
        var island = new TextAnnotation
        {
            Repo = "owner/my-repo",
            Target = AnnotationTarget.Issue,
        };
        await island.RenderIslandAsync(dest, new Dictionary<string, object?>
        {
            [nameof(TextAnnotation.Repo)] = "owner/my-repo",
            [nameof(TextAnnotation.Target)] = AnnotationTarget.Issue,
        });
        var html = dest.GetOutput();

        html.ShouldContain("<atoll-island");
        html.ShouldContain("client=\"idle\"");
        html.ShouldContain("component-url=\"/scripts/atoll-annotations.js\"");
        html.ShouldContain("<div class=\"atoll-annotations\"");
    }
}
