using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;

namespace Atoll.Tests.Slots;

public sealed class SlotBuilderTests
{
    // ── Build behavior ──

    [Fact]
    public void BuildWithNoSlotsShouldReturnEmptyCollection()
    {
        var builder = new SlotBuilder();

        var slots = builder.Build();

        slots.Count.ShouldBe(0);
        slots.ShouldBeSameAs(SlotCollection.Empty);
    }

    [Fact]
    public void BuildShouldResetBuilder()
    {
        var builder = new SlotBuilder();
        builder.DefaultHtml("<p>Content</p>");

        var first = builder.Build();
        var second = builder.Build();

        first.Count.ShouldBe(1);
        second.Count.ShouldBe(0);
        second.ShouldBeSameAs(SlotCollection.Empty);
    }

    // ── Default slot ──

    [Fact]
    public async Task DefaultShouldAddDefaultSlotFragment()
    {
        var fragment = RenderFragment.FromHtml("<p>Hello</p>");
        var slots = new SlotBuilder()
            .Default(fragment)
            .Build();

        slots.HasDefaultSlot.ShouldBeTrue();
        var output = await slots.GetSlotFragment("default").RenderToStringAsync();
        output.ShouldBe("<p>Hello</p>");
    }

    [Fact]
    public async Task DefaultHtmlShouldAddTrustedHtml()
    {
        var slots = new SlotBuilder()
            .DefaultHtml("<strong>Bold</strong>")
            .Build();

        slots.HasDefaultSlot.ShouldBeTrue();
        var output = await slots.GetSlotFragment("default").RenderToStringAsync();
        output.ShouldBe("<strong>Bold</strong>");
    }

    [Fact]
    public async Task DefaultTextShouldAddEscapedText()
    {
        var slots = new SlotBuilder()
            .DefaultText("<script>alert('xss')</script>")
            .Build();

        slots.HasDefaultSlot.ShouldBeTrue();
        var output = await slots.GetSlotFragment("default").RenderToStringAsync();
        output.ShouldBe("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");
    }

    [Fact]
    public async Task DefaultAsyncShouldAddAsyncFragment()
    {
        var slots = new SlotBuilder()
            .DefaultAsync(destination =>
            {
                destination.Write(RenderChunk.Html("<p>Async content</p>"));
                return default;
            })
            .Build();

        slots.HasDefaultSlot.ShouldBeTrue();
        var output = await slots.GetSlotFragment("default").RenderToStringAsync();
        output.ShouldBe("<p>Async content</p>");
    }

    // ── Named slots ──

    [Fact]
    public async Task NamedShouldAddNamedSlotFragment()
    {
        var fragment = RenderFragment.FromHtml("<h1>Header</h1>");
        var slots = new SlotBuilder()
            .Named("header", fragment)
            .Build();

        slots.HasSlot("header").ShouldBeTrue();
        var output = await slots.GetSlotFragment("header").RenderToStringAsync();
        output.ShouldBe("<h1>Header</h1>");
    }

    [Fact]
    public async Task NamedHtmlShouldAddTrustedHtml()
    {
        var slots = new SlotBuilder()
            .NamedHtml("footer", "<footer>Links</footer>")
            .Build();

        slots.HasSlot("footer").ShouldBeTrue();
        var output = await slots.GetSlotFragment("footer").RenderToStringAsync();
        output.ShouldBe("<footer>Links</footer>");
    }

    [Fact]
    public async Task NamedTextShouldAddEscapedText()
    {
        var slots = new SlotBuilder()
            .NamedText("title", "Hello & Welcome")
            .Build();

        slots.HasSlot("title").ShouldBeTrue();
        var output = await slots.GetSlotFragment("title").RenderToStringAsync();
        output.ShouldBe("Hello &amp; Welcome");
    }

    [Fact]
    public async Task NamedAsyncShouldAddAsyncFragment()
    {
        var slots = new SlotBuilder()
            .NamedAsync("sidebar", destination =>
            {
                destination.Write(RenderChunk.Html("<nav>Nav</nav>"));
                return default;
            })
            .Build();

        slots.HasSlot("sidebar").ShouldBeTrue();
        var output = await slots.GetSlotFragment("sidebar").RenderToStringAsync();
        output.ShouldBe("<nav>Nav</nav>");
    }

    // ── Multiple slots ──

    [Fact]
    public async Task ShouldBuildMultipleNamedSlots()
    {
        var slots = new SlotBuilder()
            .DefaultHtml("<main>Content</main>")
            .NamedHtml("header", "<h1>Title</h1>")
            .NamedHtml("footer", "<footer>Footer</footer>")
            .Build();

        slots.Count.ShouldBe(3);
        slots.HasDefaultSlot.ShouldBeTrue();
        slots.HasSlot("header").ShouldBeTrue();
        slots.HasSlot("footer").ShouldBeTrue();

        (await slots.GetSlotFragment("default").RenderToStringAsync()).ShouldBe("<main>Content</main>");
        (await slots.GetSlotFragment("header").RenderToStringAsync()).ShouldBe("<h1>Title</h1>");
        (await slots.GetSlotFragment("footer").RenderToStringAsync()).ShouldBe("<footer>Footer</footer>");
    }

    // ── Fluent chaining ──

    [Fact]
    public void FluentChainShouldReturnSameBuilderInstance()
    {
        var builder = new SlotBuilder();

        var result1 = builder.DefaultHtml("<p>Content</p>");
        var result2 = result1.NamedHtml("header", "<h1>H</h1>");
        var result3 = result2.NamedText("title", "Title");

        result1.ShouldBeSameAs(builder);
        result2.ShouldBeSameAs(builder);
        result3.ShouldBeSameAs(builder);
    }

    // ── Duplicate slot detection ──

    [Fact]
    public void ShouldThrowWhenDefaultSlotDefinedTwice()
    {
        var builder = new SlotBuilder()
            .DefaultHtml("<p>First</p>");

        var ex = Should.Throw<InvalidOperationException>(
            () => builder.DefaultHtml("<p>Second</p>"));

        ex.Message.ShouldContain("default");
        ex.Message.ShouldContain("already been defined");
    }

    [Fact]
    public void ShouldThrowWhenNamedSlotDefinedTwice()
    {
        var builder = new SlotBuilder()
            .NamedHtml("header", "<h1>First</h1>");

        var ex = Should.Throw<InvalidOperationException>(
            () => builder.NamedHtml("header", "<h1>Second</h1>"));

        ex.Message.ShouldContain("header");
        ex.Message.ShouldContain("already been defined");
    }

    // ── HasSlot on builder ──

    [Fact]
    public void HasSlotShouldReturnFalseBeforeDefining()
    {
        var builder = new SlotBuilder();

        builder.HasSlot("header").ShouldBeFalse();
    }

    [Fact]
    public void HasSlotShouldReturnTrueAfterDefining()
    {
        var builder = new SlotBuilder();
        builder.NamedHtml("header", "<h1>H</h1>");

        builder.HasSlot("header").ShouldBeTrue();
    }

    // ── Count on builder ──

    [Fact]
    public void CountShouldReflectDefinedSlots()
    {
        var builder = new SlotBuilder();

        builder.Count.ShouldBe(0);

        builder.DefaultHtml("<p>Content</p>");
        builder.Count.ShouldBe(1);

        builder.NamedHtml("header", "<h1>H</h1>");
        builder.Count.ShouldBe(2);
    }

    // ── Null argument validation ──

    [Fact]
    public void NamedShouldThrowForNullName()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.Named(null!, RenderFragment.Empty));
    }

    [Fact]
    public void NamedHtmlShouldThrowForNullName()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.NamedHtml(null!, "<p>Content</p>"));
    }

    [Fact]
    public void NamedHtmlShouldThrowForNullHtml()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.NamedHtml("header", null!));
    }

    [Fact]
    public void NamedTextShouldThrowForNullName()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.NamedText(null!, "text"));
    }

    [Fact]
    public void NamedTextShouldThrowForNullText()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.NamedText("title", null!));
    }

    [Fact]
    public void DefaultHtmlShouldThrowForNullHtml()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.DefaultHtml(null!));
    }

    [Fact]
    public void DefaultTextShouldThrowForNullText()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.DefaultText(null!));
    }

    [Fact]
    public void NamedAsyncShouldThrowForNullName()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.NamedAsync(null!, _ => default));
    }

    [Fact]
    public void NamedAsyncShouldThrowForNullRenderer()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.NamedAsync("sidebar", null!));
    }

    [Fact]
    public void DefaultAsyncShouldThrowForNullRenderer()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.DefaultAsync(null!));
    }

    [Fact]
    public void HasSlotShouldThrowForNullName()
    {
        var builder = new SlotBuilder();

        Should.Throw<ArgumentNullException>(
            () => builder.HasSlot(null!));
    }

    // ── Integration: SlotBuilder with ComponentRenderer ──

    [Fact]
    public async Task ShouldWorkWithComponentRendererForComposition()
    {
        var destination = new StringRenderDestination();
        var slots = new SlotBuilder()
            .DefaultHtml("<p>Main content</p>")
            .NamedHtml("header", "<h1>Page Title</h1>")
            .Build();

        // Use a delegate component that renders named slots
        ComponentDelegate layout = async (ctx) =>
        {
            ctx.WriteHtml("<header>");
            await ctx.RenderSlotAsync("header");
            ctx.WriteHtml("</header><main>");
            await ctx.RenderSlotAsync();
            ctx.WriteHtml("</main>");
        };

        await ComponentRenderer.RenderDelegateAsync(
            layout,
            destination,
            new Dictionary<string, object?>(),
            slots);

        destination.GetOutput().ShouldBe(
            "<header><h1>Page Title</h1></header><main><p>Main content</p></main>");
    }

    [Fact]
    public async Task ShouldWorkWithSlotFallbackWhenSlotNotProvided()
    {
        var destination = new StringRenderDestination();
        var slots = new SlotBuilder()
            .DefaultHtml("<p>Main content</p>")
            .Build();

        ComponentDelegate layout = async (ctx) =>
        {
            ctx.WriteHtml("<nav>");
            await ctx.RenderSlotAsync("sidebar", RenderFragment.FromHtml("<em>No sidebar</em>"));
            ctx.WriteHtml("</nav><main>");
            await ctx.RenderSlotAsync();
            ctx.WriteHtml("</main>");
        };

        await ComponentRenderer.RenderDelegateAsync(
            layout,
            destination,
            new Dictionary<string, object?>(),
            slots);

        destination.GetOutput().ShouldBe(
            "<nav><em>No sidebar</em></nav><main><p>Main content</p></main>");
    }
}
