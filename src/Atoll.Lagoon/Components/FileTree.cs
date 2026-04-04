using Atoll.Components;

namespace Atoll.Lagoon.Components;

/// <summary>
/// Renders a file/directory tree structure from a structured data model.
/// Directories use native <c>&lt;details&gt;/&lt;summary&gt;</c> for expand/collapse.
/// </summary>
public sealed class FileTree : AtollComponent
{
    /// <summary>Gets or sets the root-level items to display.</summary>
    [Parameter(Required = true)]
    public IReadOnlyList<FileTreeItem> Items { get; set; } = [];

    /// <inheritdoc />
    protected override async Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<div class=\"file-tree\" role=\"tree\"><ul role=\"group\">");
        foreach (var item in Items)
        {
            await RenderItemAsync(item);
        }
        WriteHtml("</ul></div>");
    }

    private async Task RenderItemAsync(FileTreeItem item)
    {
        var highlightClass = item.IsHighlighted ? " file-tree-highlight" : string.Empty;

        if (item.IsDirectory)
        {
            WriteHtml($"<li role=\"treeitem\" class=\"file-tree-dir{highlightClass}\"><details open><summary>");
            var folderIconProps = new Dictionary<string, object?> { ["Name"] = IconName.FolderOpen };
            var folderIconFragment = ComponentRenderer.ToFragment<Icon>(folderIconProps);
            await RenderAsync(folderIconFragment);
            WriteText(item.Name + "/");
            WriteHtml("</summary>");
            if (item.Children is { Count: > 0 })
            {
                WriteHtml("<ul role=\"group\">");
                foreach (var child in item.Children)
                {
                    await RenderItemAsync(child);
                }
                WriteHtml("</ul>");
            }
            WriteHtml("</details></li>");
        }
        else
        {
            WriteHtml($"<li role=\"treeitem\" class=\"file-tree-file{highlightClass}\">");
            var fileIconProps = new Dictionary<string, object?> { ["Name"] = IconName.File };
            var fileIconFragment = ComponentRenderer.ToFragment<Icon>(fileIconProps);
            await RenderAsync(fileIconFragment);
            WriteText(item.Name);
            WriteHtml("</li>");
        }
    }

}
