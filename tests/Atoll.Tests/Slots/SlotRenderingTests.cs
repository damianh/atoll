using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Slots;

/// <summary>
/// Integration tests for the slot system. These tests exercise slots
/// in combination with class-based components, delegate components,
/// the <see cref="SlotBuilder"/>, and nested component composition.
/// </summary>
public sealed class SlotRenderingTests
{
    // ── Async slot content ──

    [Fact]
    public async Task AsyncSlotContentShouldRenderCorrectly()
    {
        var destination = new StringRenderDestination();
        var asyncFragment = RenderFragment.FromAsync(async dest =>
        {
            dest.Write(RenderChunk.Html("<p>"));
            await Task.Delay(1);
            dest.Write(RenderChunk.Text("Loaded"));
            dest.Write(RenderChunk.Html("</p>"));
        });
        var slots = SlotCollection.FromDefault(asyncFragment);

        await ComponentRenderer.RenderComponentAsync<WrapperComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe("<div class=\"wrapper\"><p>Loaded</p></div>");
    }

    [Fact]
    public async Task AsyncNamedSlotsShouldRenderInCorrectPosition()
    {
        var destination = new StringRenderDestination();
        var headerFragment = RenderFragment.FromAsync(async dest =>
        {
            await Task.Delay(1);
            dest.Write(RenderChunk.Html("<h1>Async Header</h1>"));
        });
        var footerFragment = RenderFragment.FromAsync(async dest =>
        {
            await Task.Delay(1);
            dest.Write(RenderChunk.Html("<small>Async Footer</small>"));
        });
        var slotsDict = new Dictionary<string, RenderFragment>
        {
            ["header"] = headerFragment,
            ["footer"] = footerFragment,
        };
        var slots = new SlotCollection(slotsDict);

        await ComponentRenderer.RenderComponentAsync<LayoutComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<header><h1>Async Header</h1></header>" +
            "<main></main>" +
            "<footer><small>Async Footer</small></footer>");
    }

    // ── SlotBuilder with async content ──

    [Fact]
    public async Task SlotBuilderAsyncSlotsShouldRenderWithComponent()
    {
        var destination = new StringRenderDestination();
        var slots = new SlotBuilder()
            .DefaultAsync(async dest =>
            {
                await Task.Delay(1);
                dest.Write(RenderChunk.Html("<p>Async body</p>"));
            })
            .NamedAsync("header", async dest =>
            {
                await Task.Delay(1);
                dest.Write(RenderChunk.Html("<h1>Async title</h1>"));
            })
            .Build();

        await ComponentRenderer.RenderComponentAsync<LayoutComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<header><h1>Async title</h1></header>" +
            "<main><p>Async body</p></main>" +
            "<footer></footer>");
    }

    // ── Nested components with slots ──

    [Fact]
    public async Task NestedComponentsShouldPassSlotsIndependently()
    {
        var destination = new StringRenderDestination();

        // Build an inner card component as a slot for the layout
        var innerCardFragment = ComponentRenderer.ToFragment<CardComponent>(
            new Dictionary<string, object?> { ["Title"] = "Inner Card" },
            SlotCollection.FromDefault(RenderFragment.FromHtml("<p>Card body</p>")));

        var slots = new SlotBuilder()
            .Default(innerCardFragment)
            .NamedHtml("header", "<h1>Page Title</h1>")
            .Build();

        await ComponentRenderer.RenderComponentAsync<LayoutComponent>(
            destination, EmptyProps(), slots);

        var output = destination.GetOutput();
        output.ShouldContain("<h1>Page Title</h1>");
        output.ShouldContain("<div class=\"card\">");
        output.ShouldContain("<h2>Inner Card</h2>");
        output.ShouldContain("<p>Card body</p>");
    }

    [Fact]
    public async Task DeeplyNestedComponentsShouldRenderCorrectly()
    {
        var destination = new StringRenderDestination();

        // Level 3: simple text
        var innerContent = RenderFragment.FromText("Deep content");

        // Level 2: card with the text as default slot
        var cardFragment = ComponentRenderer.ToFragment<CardComponent>(
            new Dictionary<string, object?> { ["Title"] = "Level 2" },
            SlotCollection.FromDefault(innerContent));

        // Level 1: wrapper with the card as default slot
        var wrapperSlots = SlotCollection.FromDefault(cardFragment);

        await ComponentRenderer.RenderComponentAsync<WrapperComponent>(
            destination, EmptyProps(), wrapperSlots);

        var output = destination.GetOutput();
        output.ShouldBe(
            "<div class=\"wrapper\">" +
            "<div class=\"card\"><h2>Level 2</h2>Deep content</div>" +
            "</div>");
    }

    // ── Delegate components with slot interactions ──

    [Fact]
    public async Task DelegateComponentShouldRenderMultipleNamedSlots()
    {
        var destination = new StringRenderDestination();
        var slots = new SlotBuilder()
            .NamedHtml("nav", "<ul><li>Home</li><li>About</li></ul>")
            .NamedHtml("content", "<article>Main article</article>")
            .NamedHtml("sidebar", "<aside>Links</aside>")
            .Build();

        ComponentDelegate threeColumn = async ctx =>
        {
            ctx.WriteHtml("<div class=\"layout\">");
            ctx.WriteHtml("<nav>");
            await ctx.RenderSlotAsync("nav");
            ctx.WriteHtml("</nav>");
            ctx.WriteHtml("<main>");
            await ctx.RenderSlotAsync("content");
            ctx.WriteHtml("</main>");
            ctx.WriteHtml("<aside>");
            await ctx.RenderSlotAsync("sidebar");
            ctx.WriteHtml("</aside>");
            ctx.WriteHtml("</div>");
        };

        await ComponentRenderer.RenderDelegateAsync(threeColumn, destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"layout\">" +
            "<nav><ul><li>Home</li><li>About</li></ul></nav>" +
            "<main><article>Main article</article></main>" +
            "<aside><aside>Links</aside></aside>" +
            "</div>");
    }

    [Fact]
    public async Task DelegateComponentShouldUseSlotFallbackWhenSlotMissing()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.Empty;

        ComponentDelegate component = async ctx =>
        {
            ctx.WriteHtml("<section>");
            await ctx.RenderSlotAsync("content",
                RenderFragment.FromHtml("<p>Default content</p>"));
            ctx.WriteHtml("</section>");
        };

        await ComponentRenderer.RenderDelegateAsync(component, destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe("<section><p>Default content</p></section>");
    }

    // ── HasSlot conditional rendering ──

    [Fact]
    public async Task HasSlotShouldEnableConditionalSections()
    {
        var destination = new StringRenderDestination();
        var slots = new SlotBuilder()
            .NamedHtml("header", "<h1>Title</h1>")
            .Build();

        ComponentDelegate component = async ctx =>
        {
            if (ctx.HasSlot("header"))
            {
                ctx.WriteHtml("<header>");
                await ctx.RenderSlotAsync("header");
                ctx.WriteHtml("</header>");
            }

            ctx.WriteHtml("<main>Body</main>");

            if (ctx.HasSlot("footer"))
            {
                ctx.WriteHtml("<footer>");
                await ctx.RenderSlotAsync("footer");
                ctx.WriteHtml("</footer>");
            }
        };

        await ComponentRenderer.RenderDelegateAsync(component, destination, EmptyProps(), slots);

        var output = destination.GetOutput();
        output.ShouldContain("<header><h1>Title</h1></header>");
        output.ShouldContain("<main>Body</main>");
        output.ShouldNotContain("<footer>");
    }

    // ── Slot with text escaping ──

    [Fact]
    public async Task SlotWithTextContentShouldEscapeHtml()
    {
        var destination = new StringRenderDestination();
        var slots = new SlotBuilder()
            .DefaultText("<script>alert('xss')</script>")
            .Build();

        await ComponentRenderer.RenderComponentAsync<WrapperComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"wrapper\">" +
            "&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;" +
            "</div>");
    }

    // ── Empty slots ──

    [Fact]
    public async Task EmptySlotsShouldProduceNoContent()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.Empty;

        await ComponentRenderer.RenderComponentAsync<LayoutComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<header></header><main></main><footer></footer>");
    }

    [Fact]
    public async Task EmptyDefaultSlotShouldProduceNoContent()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.FromDefault(RenderFragment.Empty);

        await ComponentRenderer.RenderComponentAsync<WrapperComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe("<div class=\"wrapper\"></div>");
    }

    // ── Multiple slot renders ──

    [Fact]
    public async Task SameSlotRenderedTwiceShouldProduceContentTwice()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<em>repeat</em>"));

        ComponentDelegate component = async ctx =>
        {
            ctx.WriteHtml("<div>");
            await ctx.RenderSlotAsync();
            ctx.WriteHtml("|");
            await ctx.RenderSlotAsync();
            ctx.WriteHtml("</div>");
        };

        await ComponentRenderer.RenderDelegateAsync(component, destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe("<div><em>repeat</em>|<em>repeat</em></div>");
    }

    // ── Slot with component fragment ──

    [Fact]
    public async Task SlotContainingComponentFragmentShouldRender()
    {
        var destination = new StringRenderDestination();
        var componentFragment = ComponentRenderer.ToFragment<SimpleGreeting>();
        var slots = SlotCollection.FromDefault(componentFragment);

        await ComponentRenderer.RenderComponentAsync<WrapperComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"wrapper\"><p>Hello, World!</p></div>");
    }

    // ── Slot with interpolated template fragment ──

    [Fact]
    public async Task SlotContainingInterpolatedTemplateShouldRender()
    {
        var destination = new StringRenderDestination();
        var template = new InterpolatedTemplate(
            ["<ul>", "</ul>"],
            [RenderFragment.FromHtml("<li>Item 1</li><li>Item 2</li>")]);
        var slots = SlotCollection.FromDefault(template.ToRenderFragment());

        await ComponentRenderer.RenderComponentAsync<WrapperComponent>(
            destination, EmptyProps(), slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"wrapper\"><ul><li>Item 1</li><li>Item 2</li></ul></div>");
    }

    // ── Helper ──

    private static Dictionary<string, object?> EmptyProps() => [];

    // ── Test components ──

    private sealed class WrapperComponent : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"wrapper\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    private sealed class LayoutComponent : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<header>");
            await RenderSlotAsync("header");
            WriteHtml("</header><main>");
            await RenderSlotAsync();
            WriteHtml("</main><footer>");
            await RenderSlotAsync("footer");
            WriteHtml("</footer>");
        }
    }

    private sealed class CardComponent : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = "";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"card\"><h2>");
            WriteText(Title);
            WriteHtml("</h2>");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    private sealed class SimpleGreeting : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Hello, World!</p>");
            return Task.CompletedTask;
        }
    }
}
