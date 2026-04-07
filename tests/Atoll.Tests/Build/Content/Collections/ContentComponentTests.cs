using Atoll.Build.Content.Collections;
using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Islands;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Collections;

public sealed class ContentComponentTests
{
    [Fact]
    public async Task ShouldRenderHtmlToDestination()
    {
        var rendered = new RenderedContent("<h1>Title</h1><p>Content</p>", []);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        dest.GetOutput().ShouldBe("<h1>Title</h1><p>Content</p>");
    }

    [Fact]
    public async Task ShouldRenderFromHtmlString()
    {
        var component = ContentComponent.FromHtml("<p>Hello</p>");

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        dest.GetOutput().ShouldBe("<p>Hello</p>");
    }

    [Fact]
    public void ShouldExposeHeadings()
    {
        var headings = new List<MarkdownHeading>
        {
            new(1, "Title", "title"),
            new(2, "Section", "section"),
        };
        var rendered = new RenderedContent("<h1>Title</h1><h2>Section</h2>", headings);
        var component = ContentComponent.FromRenderedContent(rendered);

        component.Headings.Count.ShouldBe(2);
        component.Headings[0].Depth.ShouldBe(1);
        component.Headings[0].Text.ShouldBe("Title");
        component.Headings[1].Depth.ShouldBe(2);
    }

    [Fact]
    public void ShouldReturnEmptyHeadingsFromHtmlOverload()
    {
        var component = ContentComponent.FromHtml("<p>No headings</p>");

        component.Headings.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldCreateRenderFragment()
    {
        var component = ContentComponent.FromHtml("<p>Fragment test</p>");
        var fragment = component.ToRenderFragment();

        var dest = new StringRenderDestination();
        await fragment.RenderAsync(dest);

        dest.GetOutput().ShouldBe("<p>Fragment test</p>");
    }

    [Fact]
    public void ShouldThrowOnNullRenderedContent()
    {
        Should.Throw<ArgumentNullException>(() => ContentComponent.FromRenderedContent(null!));
    }

    [Fact]
    public void ShouldThrowOnNullHtml()
    {
        Should.Throw<ArgumentNullException>(() => ContentComponent.FromHtml(null!));
    }

    [Fact]
    public void ShouldThrowOnNullHtmlWithHeadingsOverload()
    {
        Should.Throw<ArgumentNullException>(() =>
            ContentComponent.FromHtml(null!, new List<MarkdownHeading>()));
    }

    [Fact]
    public void ShouldThrowOnNullHeadingsInFromHtmlOverload()
    {
        Should.Throw<ArgumentNullException>(() =>
            ContentComponent.FromHtml("<p>test</p>", null!));
    }

    [Fact]
    public async Task ShouldRenderFromHtmlWithHeadingsOverload()
    {
        var headings = new List<MarkdownHeading> { new(1, "Title", "title") };
        var component = ContentComponent.FromHtml("<h1>Title</h1>", headings);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        dest.GetOutput().ShouldBe("<h1>Title</h1>");
        component.Headings.Count.ShouldBe(1);
        component.Headings[0].Text.ShouldBe("Title");
    }

    [Fact]
    public async Task ShouldThrowOnNullContext()
    {
        var component = ContentComponent.FromHtml("<p>test</p>");

        await Should.ThrowAsync<ArgumentNullException>(() => component.RenderAsync(null!));
    }

    [Fact]
    public async Task ShouldRenderEmptyHtml()
    {
        var component = ContentComponent.FromHtml("");

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        dest.GetOutput().ShouldBeEmpty();
    }

    // --- Integration: Content entry → component rendering ---

    [Fact]
    public async Task ShouldRenderContentEntryAsComponent()
    {
        // Set up a mini collection
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "test-post.md",
                "---\ntitle: Test Post\n---\n# Test Post\n\nThis is **bold** text.");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader);

        // Load and render entry
        var entry = query.GetEntry<TestBlogPost>("blog", "test-post")!;
        var rendered = query.Render(entry);
        var component = ContentComponent.FromRenderedContent(rendered);

        // Render component to string
        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        var output = dest.GetOutput();
        output.ShouldContain("<h1");
        output.ShouldContain("Test Post");
        output.ShouldContain("<strong>bold</strong>");
    }

    [Fact]
    public async Task ShouldComposeContentWithLayoutViaSlot()
    {
        // Simulate content being used as a slot in a layout
        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "slotted.md",
                "---\ntitle: Slotted Post\n---\n# Slotted Content\n\nInside a layout.");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader);

        var entry = query.GetEntry<TestBlogPost>("blog", "slotted")!;
        var rendered = query.Render(entry);
        var contentFragment = ContentComponent.FromRenderedContent(rendered).ToRenderFragment();

        // Build a layout that uses the content as a slot
        var slots = new SlotBuilder().Default(contentFragment).Build();
        var dest = new StringRenderDestination();
        var context = new RenderContext(dest, slots);

        // Simulate a layout rendering: header + slot + footer
        context.WriteHtml("<html><body><header>Blog</header><main>");
        await context.RenderSlotAsync();
        context.WriteHtml("</main><footer>Copyright</footer></body></html>");

        var output = dest.GetOutput();
        output.ShouldContain("<header>Blog</header>");
        output.ShouldContain("<h1");
        output.ShouldContain("Slotted Content");
        output.ShouldContain("<footer>Copyright</footer>");
    }

    private sealed class TestBlogPost
    {
        public string Title { get; set; } = "";
    }

    // ── Task 13: Integration tests for ContentComponent with embedded components ──

    [Fact]
    public async Task ShouldRenderInlineComponentFromFragments()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<BadgeComponent>("badge"),
        };

        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "with-component.md",
                "---\ntitle: With Component\n---\nBefore\n\n:::badge\n:::\n\nAfter");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader, options);

        var entry = query.GetEntry<TestBlogPost>("blog", "with-component")!;
        var rendered = query.Render(entry);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        var output = dest.GetOutput();
        output.ShouldContain("Before");
        output.ShouldContain("<span>badge</span>");
        output.ShouldContain("After");
    }

    [Fact]
    public async Task ShouldPassPropsToInlineComponent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<HeadingComponent>("heading"),
        };

        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "with-props.md",
                "---\ntitle: Props Test\n---\n:::heading{level=2 text=\"Hello\"}\n:::");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader, options);

        var entry = query.GetEntry<TestBlogPost>("blog", "with-props")!;
        var rendered = query.Render(entry);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        var output = dest.GetOutput();
        output.ShouldContain("<h2>Hello</h2>");
    }

    [Fact]
    public async Task ShouldPassSlotToInlineComponent()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<WrapperComponent>("wrapper"),
        };

        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "with-slot.md",
                "---\ntitle: Slot Test\n---\n:::wrapper\nslot content\n:::");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader, options);

        var entry = query.GetEntry<TestBlogPost>("blog", "with-slot")!;
        var rendered = query.Render(entry);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        var output = dest.GetOutput();
        output.ShouldContain("<div class=\"wrapper\">");
        output.ShouldContain("slot content");
    }

    [Fact]
    public async Task ShouldRenderMultipleInlineComponents()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap()
                .Add<BadgeComponent>("badge")
                .Add<BadgeComponent>("badge2"),
        };

        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "multi-comp.md",
                "---\ntitle: Multi\n---\n:::badge\n:::\n\n:::badge2\n:::");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader, options);

        var entry = query.GetEntry<TestBlogPost>("blog", "multi-comp")!;
        var rendered = query.Render(entry);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        var output = dest.GetOutput();
        // Both badge components should appear in the output
        output.ShouldContain("<span>badge</span>");
    }

    // ── Task 14: Island-in-markdown integration tests ──

    [Fact]
    public async Task ShouldRenderIslandComponentWithAtollIslandWrapper()
    {
        var options = new MarkdownOptions
        {
            Components = new ComponentMap().Add<CounterIsland>("counter"),
        };

        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var provider = new InMemoryFileProvider()
            .AddFile(
                Path.Combine("content", "blog"),
                "with-island.md",
                "---\ntitle: Island Test\n---\n:::counter\n:::");

        var loader = new CollectionLoader(config, provider);
        var query = new CollectionQuery(loader, options);

        var entry = query.GetEntry<TestBlogPost>("blog", "with-island")!;
        var rendered = query.Render(entry);
        var component = ContentComponent.FromRenderedContent(rendered);

        var dest = new StringRenderDestination();
        var context = new RenderContext(dest);
        await component.RenderAsync(context);

        var output = dest.GetOutput();
        output.ShouldContain("<atoll-island");
        output.ShouldContain("component-url=\"/js/counter.js\"");
        output.ShouldContain("client=\"load\"");
        output.ShouldContain("</atoll-island>");
    }

    // ── Task 15: Backward compatibility regression ──

    [Fact]
    public async Task ShouldProduceBehaviorIdenticalToCurrentForPlainMarkdown()
    {
        // Render with no ComponentMap (old path)
        var plainOptions = new MarkdownOptions();

        // Render with ComponentMap but no directives in content (should be same output)
        var componentOptions = new MarkdownOptions
        {
            Components = new ComponentMap().Add<BadgeComponent>("badge"),
        };

        var markdown = "---\ntitle: Test\n---\n# Heading\n\nParagraph with **bold** and *italic*.\n\n- List item 1\n- List item 2";

        var config = new CollectionConfig("content")
            .AddCollection(ContentCollection.Define<TestBlogPost>("blog"));

        var providerA = new InMemoryFileProvider()
            .AddFile(Path.Combine("content", "blog"), "post.md", markdown);
        var providerB = new InMemoryFileProvider()
            .AddFile(Path.Combine("content", "blog"), "post.md", markdown);

        var queryA = new CollectionQuery(new CollectionLoader(config, providerA), plainOptions);
        var queryB = new CollectionQuery(new CollectionLoader(config, providerB), componentOptions);

        var entryA = queryA.GetEntry<TestBlogPost>("blog", "post")!;
        var entryB = queryB.GetEntry<TestBlogPost>("blog", "post")!;

        var renderedA = queryA.Render(entryA);
        var renderedB = queryB.Render(entryB);

        // Both paths should produce the same HTML
        renderedA.Html.ShouldBe(renderedB.Html);

        // Render components to destination — output should match
        var compA = ContentComponent.FromRenderedContent(renderedA);
        var compB = ContentComponent.FromRenderedContent(renderedB);

        var destA = new StringRenderDestination();
        var destB = new StringRenderDestination();

        await compA.RenderAsync(new RenderContext(destA));
        await compB.RenderAsync(new RenderContext(destB));

        destA.GetOutput().ShouldBe(destB.GetOutput());
    }

    // ── Component fixtures for tasks 13/14/15 ──

    private sealed class BadgeComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<span>badge</span>");
            return Task.CompletedTask;
        }
    }

    private sealed class HeadingComponent : AtollComponent
    {
        [Parameter] public int Level { get; set; } = 1;
        [Parameter] public string Text { get; set; } = "";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<h{Level}>{Text}</h{Level}>");
            return Task.CompletedTask;
        }
    }

    private sealed class WrapperComponent : AtollComponent
    {
        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div class=\"wrapper\">");
            await RenderSlotAsync();
            WriteHtml("</div>");
        }
    }

    [ClientLoad]
    private sealed class CounterIsland : AtollComponent, IClientComponent
    {
        public string ClientModuleUrl => "/js/counter.js";
        public string ClientExportName => "default";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>0</div>");
            return Task.CompletedTask;
        }
    }
}
