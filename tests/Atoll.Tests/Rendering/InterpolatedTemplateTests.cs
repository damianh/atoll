using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Rendering;

public sealed class InterpolatedTemplateTests
{
    [Fact]
    public async Task StaticHtmlOnlyShouldRenderDirectly()
    {
        var template = new InterpolatedTemplate(
            ["<h1>Hello World</h1>"],
            []);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<h1>Hello World</h1>");
    }

    [Fact]
    public async Task EmptyStaticHtmlShouldRenderNothing()
    {
        var template = new InterpolatedTemplate(
            [""],
            []);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task SingleExpressionBetweenHtmlParts()
    {
        var template = new InterpolatedTemplate(
            ["<p>", "</p>"],
            [RenderFragment.FromHtml("Hello")]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>Hello</p>");
    }

    [Fact]
    public async Task MultipleExpressionsInterleavedWithHtml()
    {
        var template = new InterpolatedTemplate(
            ["<div>", " and ", "</div>"],
            [RenderFragment.FromHtml("<b>bold</b>"), RenderFragment.FromHtml("<i>italic</i>")]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div><b>bold</b> and <i>italic</i></div>");
    }

    [Fact]
    public async Task TextExpressionsShouldBeEscaped()
    {
        var template = new InterpolatedTemplate(
            ["<p>", "</p>"],
            [RenderFragment.FromText("<script>alert('xss')</script>")]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;</p>");
    }

    [Fact]
    public async Task EmptyExpressionsShouldBeSkipped()
    {
        var template = new InterpolatedTemplate(
            ["<p>", "", "</p>"],
            [RenderFragment.Empty, RenderFragment.Empty]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p></p>");
    }

    [Fact]
    public async Task MixedEmptyAndNonEmptyExpressions()
    {
        var template = new InterpolatedTemplate(
            ["<div>", "", "", "</div>"],
            [RenderFragment.FromHtml("A"), RenderFragment.Empty, RenderFragment.FromHtml("C")]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div>AC</div>");
    }

    [Fact]
    public async Task EmptyHtmlPartsShouldNotProduceOutput()
    {
        var template = new InterpolatedTemplate(
            ["", "", ""],
            [RenderFragment.FromHtml("A"), RenderFragment.FromHtml("B")]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("AB");
    }

    [Fact]
    public async Task SingleAsyncExpressionShouldRenderCorrectly()
    {
        var template = new InterpolatedTemplate(
            ["<p>", "</p>"],
            [RenderFragment.FromAsync(async dest =>
            {
                await Task.Delay(1);
                dest.Write(RenderChunk.Html("async content"));
            })]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>async content</p>");
    }

    [Fact]
    public async Task AsyncExpressionFollowedBySyncShouldPreserveOrder()
    {
        var template = new InterpolatedTemplate(
            ["<div>", " | ", "</div>"],
            [
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("FIRST"));
                }),
                RenderFragment.FromHtml("SECOND")
            ]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div>FIRST | SECOND</div>");
    }

    [Fact]
    public async Task SyncFollowedByAsyncShouldPreserveOrder()
    {
        var template = new InterpolatedTemplate(
            ["<div>", " | ", "</div>"],
            [
                RenderFragment.FromHtml("FIRST"),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("SECOND"));
                })
            ]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div>FIRST | SECOND</div>");
    }

    [Fact]
    public async Task MultipleAsyncExpressionsShouldPreserveOrder()
    {
        var template = new InterpolatedTemplate(
            ["<ul>", "", "", "</ul>"],
            [
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(30);
                    dest.Write(RenderChunk.Html("<li>1</li>"));
                }),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(10);
                    dest.Write(RenderChunk.Html("<li>2</li>"));
                }),
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(20);
                    dest.Write(RenderChunk.Html("<li>3</li>"));
                })
            ]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<ul><li>1</li><li>2</li><li>3</li></ul>");
    }

    [Fact]
    public void ShouldThrowForNullHtmlParts()
    {
        Should.Throw<ArgumentNullException>(() =>
            new InterpolatedTemplate(null!, []));
    }

    [Fact]
    public void ShouldThrowForNullExpressions()
    {
        Should.Throw<ArgumentNullException>(() =>
            new InterpolatedTemplate([""], null!));
    }

    [Fact]
    public void ShouldThrowForMismatchedPartsCounts()
    {
        Should.Throw<ArgumentException>(() =>
            new InterpolatedTemplate(["a", "b"], [RenderFragment.Empty, RenderFragment.Empty]));
    }

    [Fact]
    public void ShouldThrowWhenHtmlPartsHasSameCountAsExpressions()
    {
        Should.Throw<ArgumentException>(() =>
            new InterpolatedTemplate(["a"], [RenderFragment.Empty]));
    }

    [Fact]
    public async Task NestedTemplatesShouldRenderCorrectly()
    {
        var innerTemplate = new InterpolatedTemplate(
            ["<span>", "</span>"],
            [RenderFragment.FromHtml("inner")]);

        var outerTemplate = new InterpolatedTemplate(
            ["<div>", "</div>"],
            [innerTemplate.ToRenderFragment()]);

        var fragment = outerTemplate.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div><span>inner</span></div>");
    }

    [Fact]
    public async Task NestedAsyncTemplatesShouldPreserveOrder()
    {
        var innerTemplate = new InterpolatedTemplate(
            ["<span>", "</span>"],
            [RenderFragment.FromAsync(async dest =>
            {
                await Task.Delay(5);
                dest.Write(RenderChunk.Html("async-inner"));
            })]);

        var outerTemplate = new InterpolatedTemplate(
            ["<div>", " | ", "</div>"],
            [
                innerTemplate.ToRenderFragment(),
                RenderFragment.FromHtml("after")
            ]);

        var fragment = outerTemplate.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div><span>async-inner</span> | after</div>");
    }

    [Fact]
    public async Task LargeNumberOfExpressionsShouldRenderCorrectly()
    {
        var count = 100;
        var htmlParts = new string[count + 1];
        var expressions = new RenderFragment[count];

        for (var i = 0; i <= count; i++)
        {
            htmlParts[i] = i == 0 ? "<div>" : i == count ? "</div>" : "";
        }

        for (var i = 0; i < count; i++)
        {
            var value = i.ToString();
            expressions[i] = RenderFragment.FromHtml(value);
        }

        var template = new InterpolatedTemplate(htmlParts, expressions);
        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        var expected = "<div>" + string.Join("", Enumerable.Range(0, count)) + "</div>";
        output.ShouldBe(expected);
    }

    [Fact]
    public async Task DefaultRenderFragmentExpressionShouldBeSkippedLikeEmpty()
    {
        var template = new InterpolatedTemplate(
            ["<p>", "</p>"],
            [default(RenderFragment)]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p></p>");
    }

    [Fact]
    public async Task MixedSyncAsyncAndEmptyExpressions()
    {
        var template = new InterpolatedTemplate(
            ["<div>", "-", "-", "-", "</div>"],
            [
                RenderFragment.FromHtml("sync"),
                RenderFragment.Empty,
                RenderFragment.FromAsync(async dest =>
                {
                    await Task.Delay(5);
                    dest.Write(RenderChunk.Html("async"));
                }),
                RenderFragment.FromHtml("final")
            ]);

        var fragment = template.ToRenderFragment();
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div>sync--async-final</div>");
    }
}
