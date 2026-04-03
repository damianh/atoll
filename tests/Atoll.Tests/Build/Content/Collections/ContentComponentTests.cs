using Atoll.Content.Collections;
using Atoll.Content.Markdown;
using Atoll.Components;
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
}
