using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;

namespace Atoll.Build.Tests.Content.Markdown;

/// <summary>
/// Tests for <see cref="ComponentDirectiveExtension"/> and
/// <see cref="ComponentDirectiveRenderer"/> — placeholder emission, prop parsing,
/// child content extraction, fallback for unknown components.
/// </summary>
public sealed class ComponentDirectiveTests
{
    // ── Placeholder emission ──

    [Fact]
    public void ShouldEmitPlaceholderForRegisteredComponent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<LabelComponent>("label"),
        };

        var result = MarkdownRenderer.Render(":::label\n:::", options);

        result.Html.ShouldContain("<!--atoll:0-->");
    }

    [Fact]
    public void ShouldNotEmitPlaceholderForUnregisteredContainer()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap(),
        };

        var result = MarkdownRenderer.Render(":::unknown\n:::", options);

        result.Html.ShouldNotContain("<!--atoll:");
        // Falls back to default CustomContainerRenderer — emits a <div>
        result.Html.ShouldContain("<div");
    }

    [Fact]
    public void ShouldEmitSequentialPlaceholderIndices()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<LabelComponent>("label")
                .Add<CounterComponent>("counter"),
        };

        var result = MarkdownRenderer.Render(
            ":::label\n:::\n\n:::counter\n:::",
            options);

        result.Html.ShouldContain("<!--atoll:0-->");
        result.Html.ShouldContain("<!--atoll:1-->");
    }

    // ── ComponentReference collection ──

    [Fact]
    public void ShouldCollectComponentReferenceWithCorrectType()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<CounterComponent>("counter"),
        };

        var result = MarkdownRenderer.Render(":::counter\n:::", options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments
            .OfType<ComponentContentFragment>()
            .Single();
        compFragment.Reference.ComponentType.ShouldBe(typeof(CounterComponent));
    }

    [Fact]
    public void ShouldCollectPropsFromGenericAttributes()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<CounterComponent>("counter"),
        };

        var result = MarkdownRenderer.Render(
            ":::counter{initialCount=5 label=\"Click me\"}\n:::",
            options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments
            .OfType<ComponentContentFragment>()
            .Single();

        compFragment.Reference.Props.ShouldContainKey("initialCount");
        compFragment.Reference.Props["initialCount"].ShouldBe("5");
        compFragment.Reference.Props.ShouldContainKey("label");
        compFragment.Reference.Props["label"].ShouldBe("Click me");
    }

    [Fact]
    public void ShouldHaveNullChildHtmlForEmptyDirective()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<LabelComponent>("label"),
        };

        var result = MarkdownRenderer.Render(":::label\n:::", options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments
            .OfType<ComponentContentFragment>()
            .Single();

        compFragment.Reference.ChildHtml.ShouldBeNull();
    }

    [Fact]
    public void ShouldCapturePlainTextChildrenAsChildHtml()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<CalloutComponent>("callout"),
        };

        var result = MarkdownRenderer.Render(
            ":::callout\nHello world\n:::",
            options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments
            .OfType<ComponentContentFragment>()
            .Single();

        compFragment.Reference.ChildHtml.ShouldNotBeNull();
        compFragment.Reference.ChildHtml.ShouldContain("Hello world");
    }

    [Fact]
    public void ShouldCaptureMarkdownChildrenRenderedToHtml()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<CalloutComponent>("callout"),
        };

        var result = MarkdownRenderer.Render(
            ":::callout\nThis **markdown** content\n:::",
            options);

        result.Fragments.ShouldNotBeNull();
        var compFragment = result.Fragments
            .OfType<ComponentContentFragment>()
            .Single();

        // Child HTML should contain the rendered bold tag
        compFragment.Reference.ChildHtml.ShouldNotBeNull();
        compFragment.Reference.ChildHtml.ShouldContain("<strong>markdown</strong>");
    }

    // ── Multiple components ──

    [Fact]
    public void ShouldCollectMultipleComponentReferences()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<LabelComponent>("label")
                .Add<CounterComponent>("counter"),
        };

        var result = MarkdownRenderer.Render(
            ":::label\n:::\n\n:::counter{initialCount=3}\n:::",
            options);

        result.Fragments.ShouldNotBeNull();
        var componentFragments = result.Fragments
            .OfType<ComponentContentFragment>()
            .ToList();

        componentFragments.Count.ShouldBe(2);
        componentFragments[0].Reference.ComponentType.ShouldBe(typeof(LabelComponent));
        componentFragments[1].Reference.ComponentType.ShouldBe(typeof(CounterComponent));
        componentFragments[1].Reference.Props["initialCount"].ShouldBe("3");
    }

    // ── Fixtures ──

    private sealed class LabelComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<span>label</span>");
            return Task.CompletedTask;
        }
    }

    private sealed class CounterComponent : IAtollComponent
    {
        [Parameter] public int InitialCount { get; set; }

        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml($"<span>{InitialCount}</span>");
            return Task.CompletedTask;
        }
    }

    private sealed class CalloutComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"callout\">");
            context.WriteHtml("</div>");
            return Task.CompletedTask;
        }
    }
}
