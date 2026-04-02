using Atoll.Core.Components;
using Atoll.Core.Head;
using Atoll.Core.Instructions;
using Atoll.Core.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Rendering;

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
}
