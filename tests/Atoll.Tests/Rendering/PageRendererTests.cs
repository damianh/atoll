using Atoll.Components;
using Atoll.Head;
using Atoll.Instructions;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Rendering;

public sealed class PageRendererTests
{
    // ── Simple page components for testing ──

    private sealed class SimplePage : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head><title>Test</title></head><body><h1>Hello</h1></body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class PageWithoutDoctype : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head></head><body>Content</body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class PageWithDoctype : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<!DOCTYPE html><html><head></head><body>Content</body></html>");
            return Task.CompletedTask;
        }
    }

    private sealed class MinimalPage : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<p>Just some content</p>");
            return Task.CompletedTask;
        }
    }

    private sealed class PageWithTitle : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = "Default";

        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml($"<html><head><title>{Title}</title></head><body><p>Page</p></body></html>");
            return Task.CompletedTask;
        }
    }

    // ── DOCTYPE auto-insertion ──

    [Fact]
    public async Task ShouldPrependDoctypeWhenMissing()
    {
        var renderer = new PageRenderer();

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        result.Html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task ShouldNotDuplicateDoctypeWhenPresent()
    {
        var renderer = new PageRenderer();

        var result = await renderer.RenderPageAsync<PageWithDoctype>();

        result.Html.ShouldStartWith("<!DOCTYPE html><html>");
        result.Html.IndexOf("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase)
            .ShouldBe(result.Html.LastIndexOf("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ShouldPrependDoctypeToMinimalContent()
    {
        var renderer = new PageRenderer();

        var result = await renderer.RenderPageAsync<MinimalPage>();

        result.Html.ShouldStartWith("<!DOCTYPE html>");
        result.Html.ShouldContain("<p>Just some content</p>");
    }

    // ── Head content injection ──

    [Fact]
    public async Task ShouldInjectHeadManagerContentBeforeClosingHeadTag()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        result.Html.ShouldContain(
            "<link rel=\"stylesheet\" href=\"/css/main.css\">\n</head>");
    }

    [Fact]
    public async Task ShouldInjectHeadInstructionsBeforeClosingHeadTag()
    {
        var renderer = new PageRenderer();
        renderer.InstructionProcessor.Add(
            HeadInstruction.Stylesheet("/css/theme.css"));

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        result.Html.ShouldContain(
            "<link rel=\"stylesheet\" href=\"/css/theme.css\">\n</head>");
    }

    [Fact]
    public async Task ShouldInjectBothHeadManagerAndInstructionContent()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(
            new HeadElement("meta")
                .SetAttribute("charset", "utf-8"));
        renderer.InstructionProcessor.Add(
            HeadInstruction.Stylesheet("/css/main.css"));

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        result.Html.ShouldContain("<meta charset=\"utf-8\">");
        result.Html.ShouldContain("<link rel=\"stylesheet\" href=\"/css/main.css\">");
    }

    [Fact]
    public async Task ShouldAppendHeadContentWhenNoClosingHeadTag()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(new HeadElement("title") { Content = "Injected" });

        var result = await renderer.RenderPageAsync<MinimalPage>();

        result.Html.ShouldContain("<title>Injected</title>");
    }

    // ── Script injection ──

    [Fact]
    public async Task ShouldInjectScriptInstructionsBeforeClosingBodyTag()
    {
        var renderer = new PageRenderer();
        renderer.InstructionProcessor.Add(
            ScriptInstruction.External("/js/app.js"));

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        result.Html.ShouldContain(
            "<script src=\"/js/app.js\"></script>\n</body>");
    }

    [Fact]
    public async Task ShouldAppendScriptsWhenNoClosingBodyTag()
    {
        var renderer = new PageRenderer();
        renderer.InstructionProcessor.Add(
            ScriptInstruction.External("/js/app.js"));

        var result = await renderer.RenderPageAsync<MinimalPage>();

        result.Html.ShouldContain("<script src=\"/js/app.js\"></script>");
    }

    // ── Component rendering ──

    [Fact]
    public async Task ShouldRenderComponentContent()
    {
        var renderer = new PageRenderer();

        var result = await renderer.RenderPageAsync<SimplePage>();

        result.Html.ShouldContain("<h1>Hello</h1>");
    }

    [Fact]
    public async Task ShouldRenderComponentWithProps()
    {
        var renderer = new PageRenderer();
        var props = new Dictionary<string, object?> { ["Title"] = "Custom Title" };

        var result = await renderer.RenderPageAsync<PageWithTitle>(props);

        result.Html.ShouldContain("<title>Custom Title</title>");
    }

    [Fact]
    public async Task ShouldRenderComponentInstance()
    {
        var renderer = new PageRenderer();
        var component = new SimplePage();

        var result = await renderer.RenderPageAsync(component);

        result.Html.ShouldContain("<h1>Hello</h1>");
    }

    [Fact]
    public async Task ShouldRenderDelegateComponent()
    {
        var renderer = new PageRenderer();
        ComponentDelegate pageDelegate = ctx =>
        {
            ctx.WriteHtml("<html><head></head><body><p>Delegate Page</p></body></html>");
            return Task.CompletedTask;
        };

        var result = await renderer.RenderPageAsync(pageDelegate);

        result.Html.ShouldContain("<p>Delegate Page</p>");
        result.Html.ShouldStartWith("<!DOCTYPE html>");
    }

    // ── PageRenderResult ──

    [Fact]
    public async Task ResultShouldExposeHtmlProperty()
    {
        var renderer = new PageRenderer();

        var result = await renderer.RenderPageAsync<SimplePage>();

        result.Html.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResultShouldWriteToStream()
    {
        var renderer = new PageRenderer();
        var result = await renderer.RenderPageAsync<SimplePage>();

        using var stream = new MemoryStream();
        await result.WriteToStreamAsync(stream);

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        content.ShouldBe(result.Html);
    }

    // ── Full integration scenario ──

    [Fact]
    public async Task FullPageRenderShouldProduceCorrectOutput()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(
            new HeadElement("meta")
                .SetAttribute("charset", "utf-8"));
        renderer.HeadManager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));
        renderer.InstructionProcessor.Add(
            HeadInstruction.Meta("description", "A test page"));
        renderer.InstructionProcessor.Add(
            ScriptInstruction.External("/js/app.js"));

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        // DOCTYPE is present
        result.Html.ShouldStartWith("<!DOCTYPE html>");

        // Head content is injected before </head>
        var headCloseIndex = result.Html.IndexOf("</head>", StringComparison.Ordinal);
        var metaCharsetIndex = result.Html.IndexOf("<meta charset=\"utf-8\">");
        var linkIndex = result.Html.IndexOf("<link rel=\"stylesheet\"");
        var metaDescIndex = result.Html.IndexOf("<meta name=\"description\"");

        headCloseIndex.ShouldBeGreaterThan(0);
        metaCharsetIndex.ShouldBeGreaterThan(0);
        metaCharsetIndex.ShouldBeLessThan(headCloseIndex);
        linkIndex.ShouldBeGreaterThan(0);
        linkIndex.ShouldBeLessThan(headCloseIndex);
        metaDescIndex.ShouldBeGreaterThan(0);
        metaDescIndex.ShouldBeLessThan(headCloseIndex);

        // Script is injected before </body>
        var bodyCloseIndex = result.Html.IndexOf("</body>", StringComparison.Ordinal);
        var scriptIndex = result.Html.IndexOf("<script src=\"/js/app.js\"");

        bodyCloseIndex.ShouldBeGreaterThan(0);
        scriptIndex.ShouldBeGreaterThan(0);
        scriptIndex.ShouldBeLessThan(bodyCloseIndex);
    }

    // ── Null argument validation ──

    [Fact]
    public async Task RenderPageAsyncWithPropsShouldThrowForNullProps()
    {
        var renderer = new PageRenderer();

        await Should.ThrowAsync<ArgumentNullException>(
            () => renderer.RenderPageAsync<SimplePage>(null!));
    }

    [Fact]
    public async Task RenderPageAsyncWithComponentShouldThrowForNullComponent()
    {
        var renderer = new PageRenderer();

        await Should.ThrowAsync<ArgumentNullException>(
            () => renderer.RenderPageAsync((IAtollComponent)null!));
    }

    [Fact]
    public async Task RenderPageAsyncWithDelegateShouldThrowForNullDelegate()
    {
        var renderer = new PageRenderer();

        await Should.ThrowAsync<ArgumentNullException>(
            () => renderer.RenderPageAsync((ComponentDelegate)null!));
    }

    [Fact]
    public void PageRenderResultShouldThrowForNullHtml()
    {
        Should.Throw<ArgumentNullException>(() => new PageRenderResult(null!));
    }

    [Fact]
    public async Task WriteToStreamAsyncShouldThrowForNullStream()
    {
        var result = new PageRenderResult("<html></html>");

        await Should.ThrowAsync<ArgumentNullException>(
            () => result.WriteToStreamAsync(null!));
    }

    // ── Untested overloads ──

    [Fact]
    public async Task ShouldRenderComponentWithPropsAndSlots()
    {
        var renderer = new PageRenderer();
        var props = new Dictionary<string, object?> { ["Title"] = "Slotted" };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml("<p>Body</p>"));

        var result = await renderer.RenderPageAsync<PageWithSlot>(props, slots);

        result.Html.ShouldContain("<h2>Slotted</h2>");
        result.Html.ShouldContain("<p>Body</p>");
        result.Html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task ShouldRenderComponentInstanceWithProps()
    {
        var renderer = new PageRenderer();
        var component = new PageWithTitle();
        var props = new Dictionary<string, object?> { ["Title"] = "Instance Title" };

        var result = await renderer.RenderPageAsync(component, props);

        result.Html.ShouldContain("<title>Instance Title</title>");
        result.Html.ShouldStartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task WriteToStreamAsyncWithCustomEncodingShouldWork()
    {
        var result = new PageRenderResult("<!DOCTYPE html><html><body>Test</body></html>");

        using var stream = new MemoryStream();
        await result.WriteToStreamAsync(stream, System.Text.Encoding.ASCII);

        stream.Position = 0;
        using var reader = new StreamReader(stream, System.Text.Encoding.ASCII);
        var content = await reader.ReadToEndAsync();

        content.ShouldBe(result.Html);
    }

    [Fact]
    public async Task WriteToStreamAsyncShouldThrowForNullEncoding()
    {
        var result = new PageRenderResult("<html></html>");

        await Should.ThrowAsync<ArgumentNullException>(
            () => result.WriteToStreamAsync(new MemoryStream(), null!));
    }

    [Fact]
    public async Task RenderPageAsyncWithPropsAndSlotsShouldThrowForNullSlots()
    {
        var renderer = new PageRenderer();
        var props = new Dictionary<string, object?>();

        await Should.ThrowAsync<ArgumentNullException>(
            () => renderer.RenderPageAsync<SimplePage>(props, null!));
    }

    // ── End-to-end integration scenarios ──

    [Fact]
    public async Task EndToEndWithComponentsSlotsHeadAndScripts()
    {
        var renderer = new PageRenderer();

        // Simulate a real page setup: head elements, instructions, scripts
        renderer.HeadManager.Add(
            new HeadElement("meta")
                .SetAttribute("charset", "utf-8"));
        renderer.HeadManager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/reset.css"));
        renderer.HeadManager.Add(
            new HeadElement("title") { Content = "My App" });

        renderer.InstructionProcessor.Add(
            HeadInstruction.Stylesheet("/css/components.css"));
        renderer.InstructionProcessor.Add(
            HeadInstruction.Meta("viewport", "width=device-width, initial-scale=1"));
        renderer.InstructionProcessor.Add(
            ScriptInstruction.External("/js/bundle.js"));
        renderer.InstructionProcessor.Add(
            ScriptInstruction.Module("/js/hydrate.js"));

        var props = new Dictionary<string, object?> { ["Title"] = "Dashboard" };
        var slots = SlotCollection.FromDefault(
            RenderFragment.FromHtml("<p>Welcome to the dashboard</p>"));

        var result = await renderer.RenderPageAsync<FullPageComponent>(props, slots);

        var html = result.Html;

        // DOCTYPE
        html.ShouldStartWith("<!DOCTYPE html>");

        // Head content injected
        html.ShouldContain("<meta charset=\"utf-8\">");
        html.ShouldContain("<link rel=\"stylesheet\" href=\"/css/reset.css\">");
        html.ShouldContain("<title>My App</title>");
        html.ShouldContain("<link rel=\"stylesheet\" href=\"/css/components.css\">");
        html.ShouldContain("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");

        // Head content before </head>
        var headCloseIdx = html.IndexOf("</head>", StringComparison.Ordinal);
        headCloseIdx.ShouldBeGreaterThan(0);
        html.IndexOf("<meta charset=\"utf-8\">", StringComparison.Ordinal)
            .ShouldBeLessThan(headCloseIdx);

        // Body content
        html.ShouldContain("<h1>Dashboard</h1>");
        html.ShouldContain("<p>Welcome to the dashboard</p>");

        // Scripts before </body>
        var bodyCloseIdx = html.IndexOf("</body>", StringComparison.Ordinal);
        bodyCloseIdx.ShouldBeGreaterThan(0);
        html.IndexOf("<script src=\"/js/bundle.js\"></script>", StringComparison.Ordinal)
            .ShouldBeLessThan(bodyCloseIdx);
        html.IndexOf("<script type=\"module\" src=\"/js/hydrate.js\"></script>", StringComparison.Ordinal)
            .ShouldBeLessThan(bodyCloseIdx);
    }

    [Fact]
    public async Task EndToEndDelegatePageWithNestedComponents()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(
            new HeadElement("title") { Content = "Blog" });
        renderer.InstructionProcessor.Add(
            ScriptInstruction.External("/js/blog.js"));

        ComponentDelegate blogPage = async ctx =>
        {
            ctx.WriteHtml("<html><head></head><body>");
            ctx.WriteHtml("<h1>Blog</h1>");
            // Embed a component as a fragment
            var cardFragment = ComponentRenderer.ToFragment<CardWidget>(
                new Dictionary<string, object?> { ["Title"] = "Latest Post" });
            await ctx.RenderAsync(cardFragment);
            ctx.WriteHtml("</body></html>");
        };

        var result = await renderer.RenderPageAsync(blogPage);

        result.Html.ShouldStartWith("<!DOCTYPE html>");
        result.Html.ShouldContain("<title>Blog</title>");
        result.Html.ShouldContain("<h1>Blog</h1>");
        result.Html.ShouldContain("<div class=\"card\"><h2>Latest Post</h2></div>");
        result.Html.ShouldContain("<script src=\"/js/blog.js\"></script>");
    }

    [Fact]
    public async Task DuplicateHeadElementsShouldBeDeduplicatedInPageRender()
    {
        var renderer = new PageRenderer();

        // Add the same stylesheet twice via HeadManager
        renderer.HeadManager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));
        renderer.HeadManager.Add(
            new HeadElement("link")
                .SetAttribute("rel", "stylesheet")
                .SetAttribute("href", "/css/main.css"));

        // Add the same one again via instruction
        renderer.InstructionProcessor.Add(
            HeadInstruction.Stylesheet("/css/theme.css"));
        renderer.InstructionProcessor.Add(
            HeadInstruction.Stylesheet("/css/theme.css"));

        var result = await renderer.RenderPageAsync<PageWithoutDoctype>();

        // HeadManager deduplicates, so only one /css/main.css link
        var mainCssCount = CountOccurrences(result.Html, "/css/main.css");
        mainCssCount.ShouldBe(1);

        // InstructionProcessor deduplicates, so only one /css/theme.css link
        var themeCssCount = CountOccurrences(result.Html, "/css/theme.css");
        themeCssCount.ShouldBe(1);
    }

    [Fact]
    public async Task EmptyPageShouldStillGetDoctypeAndHeadContent()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(
            new HeadElement("meta")
                .SetAttribute("charset", "utf-8"));

        ComponentDelegate emptyPage = _ => Task.CompletedTask;

        var result = await renderer.RenderPageAsync(emptyPage);

        result.Html.ShouldStartWith("<!DOCTYPE html>");
        result.Html.ShouldContain("<meta charset=\"utf-8\">");
    }

    [Fact]
    public async Task InterpolatedTemplatePageShouldWorkWithPageRenderer()
    {
        var renderer = new PageRenderer();
        renderer.HeadManager.Add(
            new HeadElement("title") { Content = "Template Page" });

        ComponentDelegate templatePage = async ctx =>
        {
            var template = new InterpolatedTemplate(
                ["<html><head></head><body><h1>", "</h1><p>", "</p></body></html>"],
                [
                    RenderFragment.FromText("Welcome"),
                    RenderFragment.FromHtml("<em>Content here</em>"),
                ]);
            await ctx.RenderAsync(template.ToRenderFragment());
        };

        var result = await renderer.RenderPageAsync(templatePage);

        result.Html.ShouldStartWith("<!DOCTYPE html>");
        result.Html.ShouldContain("<title>Template Page</title>");
        result.Html.ShouldContain("<h1>Welcome</h1>");
        result.Html.ShouldContain("<em>Content here</em>");
    }

    // ── Helper methods ──

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    // ── Additional test component types ──

    private sealed class PageWithSlot : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = "";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head></head><body><h2>");
            WriteText(Title);
            WriteHtml("</h2>");
            await RenderSlotAsync();
            WriteHtml("</body></html>");
        }
    }

    private sealed class FullPageComponent : AtollComponent
    {
        [Parameter]
        public string Title { get; set; } = "";

        protected override async Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<html><head></head><body><h1>");
            WriteText(Title);
            WriteHtml("</h1>");
            await RenderSlotAsync();
            WriteHtml("</body></html>");
        }
    }

    private sealed class CardWidget : AtollComponent
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
}
