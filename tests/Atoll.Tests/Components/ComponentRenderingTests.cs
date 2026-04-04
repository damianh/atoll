using Atoll.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Atoll.Tests.Components.Fixtures;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Components;

public sealed class ComponentRenderingTests
{
    // ── Class-based component tests ──

    [Fact]
    public async Task ShouldRenderClassBasedComponentWithNoProps()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<SimpleComponent>(destination);

        destination.GetOutput().ShouldBe("<p>Hello, World!</p>");
    }

    [Fact]
    public async Task ShouldRenderClassBasedComponentWithProps()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Title"] = "My Card" };

        await ComponentRenderer.RenderComponentAsync<CardComponent>(destination, props);

        destination.GetOutput().ShouldBe("<div class=\"card\"><h2>My Card</h2></div>");
    }

    [Fact]
    public async Task ShouldBindParametersCaseInsensitively()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["title"] = "lowercase" };

        await ComponentRenderer.RenderComponentAsync<CardComponent>(destination, props);

        destination.GetOutput().ShouldBe("<div class=\"card\"><h2>lowercase</h2></div>");
    }

    [Fact]
    public async Task ShouldRenderComponentWithDefaultSlot()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Title"] = "Card" };
        var slotContent = RenderFragment.FromHtml("<p>Slot content</p>");
        var slots = SlotCollection.FromDefault(slotContent);

        await ComponentRenderer.RenderComponentAsync<CardWithSlotComponent>(
            destination, props, slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"card\"><h2>Card</h2><p>Slot content</p></div>");
    }

    [Fact]
    public async Task ShouldRenderComponentWithNamedSlots()
    {
        var destination = new StringRenderDestination();
        var slotsDict = new Dictionary<string, RenderFragment>
        {
            ["header"] = RenderFragment.FromHtml("<h1>Header</h1>"),
            ["footer"] = RenderFragment.FromHtml("<footer>Footer</footer>"),
        };
        var slots = new SlotCollection(slotsDict);

        await ComponentRenderer.RenderComponentAsync<LayoutComponent>(destination, Props(), slots);

        destination.GetOutput().ShouldBe(
            "<header><h1>Header</h1></header><main></main><footer><footer>Footer</footer></footer>");
    }

    [Fact]
    public async Task ShouldRenderComponentWithSlotFallback()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<ComponentWithFallback>(destination);

        destination.GetOutput().ShouldBe("<div><em>Default content</em></div>");
    }

    [Fact]
    public async Task ShouldRenderSlotFallbackWhenSlotNotProvided()
    {
        var destination = new StringRenderDestination();
        // Provide only a "header" slot, not "sidebar"
        var slotsDict = new Dictionary<string, RenderFragment>
        {
            ["header"] = RenderFragment.FromHtml("<h1>Header</h1>"),
        };
        var slots = new SlotCollection(slotsDict);

        await ComponentRenderer.RenderComponentAsync<ComponentWithNamedFallback>(
            destination, Props(), slots);

        destination.GetOutput().ShouldBe(
            "<header><h1>Header</h1></header><aside><em>No sidebar</em></aside>");
    }

    [Fact]
    public async Task ShouldOverrideFallbackWhenSlotProvided()
    {
        var destination = new StringRenderDestination();
        var slotsDict = new Dictionary<string, RenderFragment>
        {
            ["header"] = RenderFragment.FromHtml("<h1>Header</h1>"),
            ["sidebar"] = RenderFragment.FromHtml("<nav>Nav links</nav>"),
        };
        var slots = new SlotCollection(slotsDict);

        await ComponentRenderer.RenderComponentAsync<ComponentWithNamedFallback>(
            destination, Props(), slots);

        destination.GetOutput().ShouldBe(
            "<header><h1>Header</h1></header><aside><nav>Nav links</nav></aside>");
    }

    [Fact]
    public async Task ShouldThrowWhenRequiredParameterMissing()
    {
        var destination = new StringRenderDestination();

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => ComponentRenderer.RenderComponentAsync<RequiredParamComponent>(destination));

        ex.Message.ShouldContain("Required parameter 'Name'");
    }

    [Fact]
    public async Task ShouldNotThrowWhenOptionalParameterMissing()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderComponentAsync<OptionalParamComponent>(destination);

        destination.GetOutput().ShouldBe("<p></p>");
    }

    [Fact]
    public async Task ShouldBindNullableValueTypeParameter()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = null };

        await ComponentRenderer.RenderComponentAsync<NullableParamComponent>(destination, props);

        destination.GetOutput().ShouldBe("<p>none</p>");
    }

    [Fact]
    public async Task ShouldBindNullableValueTypeWithValue()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Count"] = 42 };

        await ComponentRenderer.RenderComponentAsync<NullableParamComponent>(destination, props);

        destination.GetOutput().ShouldBe("<p>42</p>");
    }

    [Fact]
    public async Task ShouldThrowWhenAssigningNullToNonNullableValueType()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Value"] = null };

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => ComponentRenderer.RenderComponentAsync<NonNullableValueTypeComponent>(
                destination, props));

        ex.Message.ShouldContain("Cannot assign null");
    }

    [Fact]
    public async Task ShouldConvertCompatiblePropTypes()
    {
        var destination = new StringRenderDestination();
        // Provide an int when string is expected via Convert.ChangeType
        var props = new Dictionary<string, object?> { ["Value"] = 123 };

        await ComponentRenderer.RenderComponentAsync<StringParamComponent>(destination, props);

        destination.GetOutput().ShouldBe("<p>123</p>");
    }

    [Fact]
    public async Task ShouldThrowWhenPropTypeIncompatible()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Value"] = "not-an-int" };

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => ComponentRenderer.RenderComponentAsync<NonNullableValueTypeComponent>(
                destination, props));

        ex.Message.ShouldContain("Cannot convert");
    }

    // ── Functional delegate component tests ──

    [Fact]
    public async Task ShouldRenderDelegateComponent()
    {
        var destination = new StringRenderDestination();
        ComponentDelegate greeting = (ctx) =>
        {
            ctx.WriteHtml("<h1>Hello from delegate!</h1>");
            return Task.CompletedTask;
        };

        await ComponentRenderer.RenderDelegateAsync(greeting, destination);

        destination.GetOutput().ShouldBe("<h1>Hello from delegate!</h1>");
    }

    [Fact]
    public async Task ShouldRenderDelegateComponentWithProps()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["name"] = "Alice" };
        ComponentDelegate greeting = (ctx) =>
        {
            var name = ctx.GetProp<string>("name");
            ctx.WriteHtml("<h1>Hello, ");
            ctx.WriteText(name);
            ctx.WriteHtml("!</h1>");
            return Task.CompletedTask;
        };

        await ComponentRenderer.RenderDelegateAsync(greeting, destination, props);

        destination.GetOutput().ShouldBe("<h1>Hello, Alice!</h1>");
    }

    [Fact]
    public async Task ShouldRenderDelegateComponentWithSlot()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<p>Inner content</p>"));
        ComponentDelegate wrapper = async (ctx) =>
        {
            ctx.WriteHtml("<div class=\"wrapper\">");
            await ctx.RenderSlotAsync();
            ctx.WriteHtml("</div>");
        };

        await ComponentRenderer.RenderDelegateAsync(wrapper, destination, Props(), slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"wrapper\"><p>Inner content</p></div>");
    }

    [Fact]
    public async Task ShouldRenderDelegateWithAsyncContent()
    {
        var destination = new StringRenderDestination();
        ComponentDelegate asyncComponent = async (ctx) =>
        {
            ctx.WriteHtml("<div>");
            await Task.Delay(1); // Simulate async work
            ctx.WriteHtml("<p>Async result</p>");
            ctx.WriteHtml("</div>");
        };

        await ComponentRenderer.RenderDelegateAsync(asyncComponent, destination);

        destination.GetOutput().ShouldBe("<div><p>Async result</p></div>");
    }

    // ── ToFragment tests ──

    [Fact]
    public async Task ShouldCreateFragmentFromComponent()
    {
        var props = new Dictionary<string, object?> { ["Title"] = "Fragment Card" };
        var fragment = ComponentRenderer.ToFragment<CardComponent>(props);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div class=\"card\"><h2>Fragment Card</h2></div>");
    }

    [Fact]
    public async Task ShouldCreateFragmentFromDelegate()
    {
        ComponentDelegate badge = (ctx) =>
        {
            var label = ctx.GetProp<string>("label");
            ctx.WriteHtml($"<span class=\"badge\">{label}</span>");
            return Task.CompletedTask;
        };
        var props = new Dictionary<string, object?> { ["label"] = "New" };
        var fragment = ComponentRenderer.ToFragment(badge, props);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<span class=\"badge\">New</span>");
    }

    [Fact]
    public async Task ShouldCreateFragmentFromComponentWithSlots()
    {
        var props = new Dictionary<string, object?> { ["Title"] = "Slot Card" };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<p>Body</p>"));
        var fragment = ComponentRenderer.ToFragment<CardWithSlotComponent>(props, slots);

        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<div class=\"card\"><h2>Slot Card</h2><p>Body</p></div>");
    }

    // ── RenderContext tests ──

    [Fact]
    public async Task ShouldEscapeTextInWriteText()
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?> { ["Title"] = "<script>alert('xss')</script>" };

        await ComponentRenderer.RenderComponentAsync<CardComponent>(destination, props);

        destination.GetOutput().ShouldBe(
            "<div class=\"card\"><h2>&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;</h2></div>");
    }

    [Fact]
    public async Task ShouldReturnDefaultValueWhenPropNotFound()
    {
        var destination = new StringRenderDestination();
        ComponentDelegate component = (ctx) =>
        {
            var value = ctx.GetProp("missing", "fallback");
            ctx.WriteText(value);
            return Task.CompletedTask;
        };

        await ComponentRenderer.RenderDelegateAsync(component, destination);

        destination.GetOutput().ShouldBe("fallback");
    }

    [Fact]
    public async Task ShouldThrowWhenPropNotFoundWithoutDefault()
    {
        var destination = new StringRenderDestination();
        ComponentDelegate component = (ctx) =>
        {
            ctx.GetProp<string>("missing");
            return Task.CompletedTask;
        };

        await Should.ThrowAsync<KeyNotFoundException>(
            () => ComponentRenderer.RenderDelegateAsync(component, destination));
    }

    [Fact]
    public async Task ShouldReportHasSlotCorrectly()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("content"));
        ComponentDelegate component = (ctx) =>
        {
            ctx.WriteHtml(ctx.HasSlot("default") ? "yes" : "no");
            ctx.WriteHtml(",");
            ctx.WriteHtml(ctx.HasSlot("missing") ? "yes" : "no");
            return Task.CompletedTask;
        };

        await ComponentRenderer.RenderDelegateAsync(component, destination, Props(), slots);

        destination.GetOutput().ShouldBe("yes,no");
    }

    [Fact]
    public async Task ShouldRenderFragmentViaContext()
    {
        var destination = new StringRenderDestination();
        var child = RenderFragment.FromHtml("<span>child</span>");
        ComponentDelegate component = async (ctx) =>
        {
            ctx.WriteHtml("<div>");
            await ctx.RenderAsync(child);
            ctx.WriteHtml("</div>");
        };

        await ComponentRenderer.RenderDelegateAsync(component, destination);

        destination.GetOutput().ShouldBe("<div><span>child</span></div>");
    }

    [Fact]
    public async Task ShouldRenderNestedComponents()
    {
        var destination = new StringRenderDestination();
        var innerSlot = ComponentRenderer.ToFragment<SimpleComponent>();
        var props = new Dictionary<string, object?> { ["Title"] = "Outer" };
        var slots = SlotCollection.FromDefault(innerSlot);

        await ComponentRenderer.RenderComponentAsync<CardWithSlotComponent>(
            destination, props, slots);

        destination.GetOutput().ShouldBe(
            "<div class=\"card\"><h2>Outer</h2><p>Hello, World!</p></div>");
    }

    [Fact]
    public async Task ShouldRenderComponentInstance()
    {
        var destination = new StringRenderDestination();
        var component = new CardComponent();
        var props = new Dictionary<string, object?> { ["Title"] = "Instance" };

        await ComponentRenderer.RenderComponentAsync(component, destination, props);

        destination.GetOutput().ShouldBe("<div class=\"card\"><h2>Instance</h2></div>");
    }

    [Fact]
    public void ShouldThrowWhenContextAccessedOutsideRender()
    {
        var component = new ContextAccessTestComponent();

        Should.Throw<InvalidOperationException>(() => component.TryAccessContext());
    }

    // ── RenderSliceAsync / ToSliceFragment tests ──

    [Fact]
    public async Task RenderSliceAsyncShouldRenderSimpleSlice()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderSliceAsync<SimpleSlice>(destination);

        destination.GetOutput().ShouldContain("<p>Hello from Razor!</p>");
    }

    [Fact]
    public async Task RenderSliceAsyncShouldRenderSliceWithDefaultSlot()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<em>slot content</em>"));

        await ComponentRenderer.RenderSliceAsync<SliceWithDefaultSlot>(destination, slots);

        destination.GetOutput().ShouldContain("<em>slot content</em>");
    }

    [Fact]
    public async Task RenderSliceAsyncShouldRenderSliceWithNoSlotsOverload()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderSliceAsync<SimpleSlice>(destination);

        destination.GetOutput().ShouldContain("<p>Hello from Razor!</p>");
    }

    [Fact]
    public async Task RenderSliceAsyncShouldRenderTypedSlice()
    {
        var destination = new StringRenderDestination();

        await ComponentRenderer.RenderSliceAsync<GreetingSlice, GreetingModel>(
            destination, new GreetingModel("Alice"));

        destination.GetOutput().ShouldContain("Hello, Alice!");
    }

    [Fact]
    public async Task RenderSliceAsyncShouldRenderTypedSliceWithSlots()
    {
        var destination = new StringRenderDestination();
        var slots = SlotCollection.Empty;

        await ComponentRenderer.RenderSliceAsync<GreetingSlice, GreetingModel>(
            destination, new GreetingModel("Bob"), slots);

        destination.GetOutput().ShouldContain("Hello, Bob!");
    }

    [Fact]
    public async Task ToSliceFragmentShouldRenderSliceWhenEvaluated()
    {
        var fragment = ComponentRenderer.ToSliceFragment<SimpleSlice>();

        var output = await fragment.RenderToStringAsync();

        output.ShouldContain("<p>Hello from Razor!</p>");
    }

    [Fact]
    public async Task ToSliceFragmentWithSlotsShouldInjectSlots()
    {
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<b>body</b>"));
        var fragment = ComponentRenderer.ToSliceFragment<SliceWithDefaultSlot>(slots);

        var output = await fragment.RenderToStringAsync();

        output.ShouldContain("<b>body</b>");
    }

    [Fact]
    public async Task ToSliceFragmentTypedShouldRenderWithModel()
    {
        var fragment = ComponentRenderer.ToSliceFragment<GreetingSlice, GreetingModel>(
            new GreetingModel("Charlie"));

        var output = await fragment.RenderToStringAsync();

        output.ShouldContain("Hello, Charlie!");
    }

    [Fact]
    public async Task ToSliceFragmentTypedWithSlotsShouldRenderWithModelAndSlots()
    {
        var slots = SlotCollection.Empty;
        var fragment = ComponentRenderer.ToSliceFragment<GreetingSlice, GreetingModel>(
            new GreetingModel("Diana"), slots);

        var output = await fragment.RenderToStringAsync();

        output.ShouldContain("Hello, Diana!");
    }

    [Fact]
    public async Task RenderSliceAsyncShouldThrowWhenDestinationIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => ComponentRenderer.RenderSliceAsync<SimpleSlice>(null!).AsTask());
    }

    [Fact]
    public async Task RenderSliceAsyncWithSlotsShouldThrowWhenDestinationIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => ComponentRenderer.RenderSliceAsync<SimpleSlice>(null!, SlotCollection.Empty).AsTask());
    }

    [Fact]
    public async Task RenderSliceAsyncWithSlotsShouldThrowWhenSlotsIsNull()
    {
        var destination = new StringRenderDestination();
        await Should.ThrowAsync<ArgumentNullException>(
            () => ComponentRenderer.RenderSliceAsync<SimpleSlice>(destination, null!).AsTask());
    }

    [Fact]
    public async Task RenderSliceAsyncTypedShouldThrowWhenDestinationIsNull()
    {
        await Should.ThrowAsync<ArgumentNullException>(
            () => ComponentRenderer.RenderSliceAsync<GreetingSlice, GreetingModel>(
                null!, new GreetingModel("X")).AsTask());
    }

    [Fact]
    public void ToSliceFragmentWithSlotsShouldThrowWhenSlotsIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ComponentRenderer.ToSliceFragment<SimpleSlice>(null!));
    }

    [Fact]
    public void ToSliceFragmentTypedWithSlotsShouldThrowWhenSlotsIsNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ComponentRenderer.ToSliceFragment<GreetingSlice, GreetingModel>(
                new GreetingModel("X"), null!));
    }

    // ── Helper methods ──

    private static Dictionary<string, object?> Props()
    {
        return [];
    }

    // ── Test component types ──

    private sealed class SimpleComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Hello, World!</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class CardComponent : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"card\"><h2>");
            WriteText(Title);
            WriteHtml("</h2></div>");
            return Task.CompletedTask;
        }
    }

    private sealed class CardWithSlotComponent : AtollComponent
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

    private sealed class ComponentWithFallback : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>");
            await RenderSlotAsync("default", RenderFragment.FromHtml("<em>Default content</em>"));
            WriteHtml("</div>");
        }
    }

    private sealed class ComponentWithNamedFallback : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<header>");
            await RenderSlotAsync("header");
            WriteHtml("</header><aside>");
            await RenderSlotAsync("sidebar", RenderFragment.FromHtml("<em>No sidebar</em>"));
            WriteHtml("</aside>");
        }
    }

    private sealed class RequiredParamComponent : AtollComponent
    {
        [Parameter(Required = true)]
        public string Name { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteText(Name);
            return Task.CompletedTask;
        }
    }

    private sealed class OptionalParamComponent : AtollComponent
    {
        [Parameter]
        public string? Label { get; set; }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>");
            WriteText(Label ?? "");
            WriteHtml("</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class NullableParamComponent : AtollComponent
    {
        [Parameter]
        public int? Count { get; set; }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>");
            WriteText(Count.HasValue ? Count.Value.ToString() : "none");
            WriteHtml("</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class NonNullableValueTypeComponent : AtollComponent
    {
        [Parameter]
        public int Value { get; set; }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<p>{Value}</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class StringParamComponent : AtollComponent
    {
        [Parameter]
        public string Value { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>");
            WriteText(Value);
            WriteHtml("</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class ContextAccessTestComponent : AtollComponent
    {
        public void TryAccessContext()
        {
            _ = Context; // Should throw
        }

        protected override Task RenderCoreAsync(RenderContext context)
        {
            return Task.CompletedTask;
        }
    }
}
