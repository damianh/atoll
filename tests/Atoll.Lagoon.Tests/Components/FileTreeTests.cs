using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class FileTreeTests
{
    private static async Task<string> RenderFileTreeAsync(IReadOnlyList<FileTreeItem> items)
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Items"] = items,
        };
        await ComponentRenderer.RenderComponentAsync<FileTree>(destination, props);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderDirectoryWithDetailsSummary()
    {
        var html = await RenderFileTreeAsync([new FileTreeItem("src", isDirectory: true)]);

        html.ShouldContain("file-tree-dir");
        html.ShouldContain("<details open>");
        html.ShouldContain("<summary>");
        html.ShouldContain("src/");
    }

    [Fact]
    public async Task ShouldRenderNestedChildren()
    {
        var items = new List<FileTreeItem>
        {
            new FileTreeItem("src", isDirectory: true, children: new List<FileTreeItem>
            {
                new FileTreeItem("index.ts"),
            })
        };

        var html = await RenderFileTreeAsync(items);

        html.ShouldContain("src/");
        html.ShouldContain("index.ts");
        html.ShouldContain("<ul role=\"group\">");
    }

    [Fact]
    public async Task ShouldApplyHighlightClassToFile()
    {
        var html = await RenderFileTreeAsync([new FileTreeItem("important.ts", isHighlighted: true)]);

        html.ShouldContain("file-tree-highlight");
    }

    [Fact]
    public async Task ShouldApplyHighlightClassToDirectory()
    {
        var html = await RenderFileTreeAsync([new FileTreeItem("src", isDirectory: true, isHighlighted: true)]);

        html.ShouldContain("file-tree-highlight");
    }

    [Fact]
    public async Task ShouldHtmlEncodeFileName()
    {
        var html = await RenderFileTreeAsync([new FileTreeItem("<script>alert(1)</script>.ts")]);

        html.ShouldContain("&lt;script&gt;");
        html.ShouldNotContain("<script>alert(1)</script>");
    }

}
