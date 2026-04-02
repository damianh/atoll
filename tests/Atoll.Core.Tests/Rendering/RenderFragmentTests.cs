using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Rendering;

public sealed class RenderFragmentTests
{
    [Fact]
    public async Task FromHtmlShouldRenderTrustedHtml()
    {
        var fragment = RenderFragment.FromHtml("<h1>Hello World</h1>");

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<h1>Hello World</h1>");
    }

    [Fact]
    public async Task FromHtmlShouldNotEscapeContent()
    {
        var fragment = RenderFragment.FromHtml("<script>alert('test')</script>");

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<script>alert('test')</script>");
    }

    [Fact]
    public async Task FromTextShouldEscapeContent()
    {
        var fragment = RenderFragment.FromText("<script>alert('xss')</script>");

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");
    }

    [Fact]
    public async Task FromTextShouldEscapeAmpersandsAndQuotes()
    {
        var fragment = RenderFragment.FromText("A & B \"quoted\"");

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("A &amp; B &quot;quoted&quot;");
    }

    [Fact]
    public async Task EmptyFragmentShouldRenderNothing()
    {
        var output = await RenderFragment.Empty.RenderToStringAsync();

        output.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task FromHtmlWithEmptyStringShouldRenderNothing()
    {
        var fragment = RenderFragment.FromHtml(string.Empty);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task FromTextWithEmptyStringShouldRenderNothing()
    {
        var fragment = RenderFragment.FromText(string.Empty);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task FromHtmlStringShouldRenderTrustedHtml()
    {
        var htmlString = new HtmlString("<div>trusted</div>");
        var fragment = RenderFragment.FromHtmlString(htmlString);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div>trusted</div>");
    }

    [Fact]
    public async Task FromHtmlStringWithEmptyValueShouldRenderNothing()
    {
        var fragment = RenderFragment.FromHtmlString(HtmlString.Empty);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task FromAsyncShouldRenderAsyncContent()
    {
        var fragment = RenderFragment.FromAsync(async destination =>
        {
            await Task.Delay(1);
            destination.Write(RenderChunk.Html("<p>async content</p>"));
        });

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>async content</p>");
    }

    [Fact]
    public async Task ConcatShouldRenderAllFragmentsInOrder()
    {
        var fragment = RenderFragment.Concat(
            RenderFragment.FromHtml("<h1>Title</h1>"),
            RenderFragment.FromHtml("<p>Body</p>"),
            RenderFragment.FromHtml("<footer>Footer</footer>")
        );

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<h1>Title</h1><p>Body</p><footer>Footer</footer>");
    }

    [Fact]
    public async Task ConcatWithSingleFragmentShouldReturnThatFragment()
    {
        var original = RenderFragment.FromHtml("<p>only</p>");
        var fragment = RenderFragment.Concat(original);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>only</p>");
    }

    [Fact]
    public async Task ConcatWithNoFragmentsShouldReturnEmpty()
    {
        var fragment = RenderFragment.Concat();

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task ConcatShouldHandleMixedHtmlAndText()
    {
        var fragment = RenderFragment.Concat(
            RenderFragment.FromHtml("<p>"),
            RenderFragment.FromText("User <script> input"),
            RenderFragment.FromHtml("</p>")
        );

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>User &lt;script&gt; input</p>");
    }

    [Fact]
    public async Task ConcatShouldHandleMixedSyncAndAsyncFragments()
    {
        var fragment = RenderFragment.Concat(
            RenderFragment.FromHtml("<div>"),
            RenderFragment.FromAsync(async destination =>
            {
                await Task.Delay(1);
                destination.Write(RenderChunk.Html("<span>async</span>"));
            }),
            RenderFragment.FromHtml("</div>")
        );

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div><span>async</span></div>");
    }

    [Fact]
    public void FromHtmlShouldThrowForNullHtml()
    {
        Should.Throw<ArgumentNullException>(() => RenderFragment.FromHtml(null!));
    }

    [Fact]
    public void FromTextShouldThrowForNullText()
    {
        Should.Throw<ArgumentNullException>(() => RenderFragment.FromText(null!));
    }

    [Fact]
    public void FromAsyncShouldThrowForNullRenderer()
    {
        Should.Throw<ArgumentNullException>(() => RenderFragment.FromAsync(null!));
    }

    [Fact]
    public void ConcatShouldThrowForNullArray()
    {
        Should.Throw<ArgumentNullException>(() => RenderFragment.Concat(null!));
    }

    [Fact]
    public async Task RenderAsyncShouldThrowForNullDestination()
    {
        var fragment = RenderFragment.FromHtml("<p>test</p>");

        await Should.ThrowAsync<ArgumentNullException>(
            () => fragment.RenderAsync(null!).AsTask());
    }

    [Fact]
    public void DefaultFragmentShouldBeEmpty()
    {
        var fragment = default(RenderFragment);

        fragment.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public async Task DefaultFragmentShouldRenderNothing()
    {
        var fragment = default(RenderFragment);
        var destination = new StringRenderDestination();

        await fragment.RenderAsync(destination);

        destination.GetOutput().ShouldBe(string.Empty);
    }

    [Fact]
    public void EmptyFragmentShouldReportIsEmpty()
    {
        RenderFragment.Empty.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void NonEmptyFragmentShouldNotReportIsEmpty()
    {
        var fragment = RenderFragment.FromHtml("<p>content</p>");

        fragment.IsEmpty.ShouldBeFalse();
    }
}
