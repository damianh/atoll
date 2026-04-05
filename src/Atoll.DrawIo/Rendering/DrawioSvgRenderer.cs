using System.Xml.Linq;
using Atoll.DrawIo.Model;
using Atoll.DrawIo.Parsing;

namespace Atoll.DrawIo.Rendering;

/// <summary>
/// High-level entry point for rendering draw.io diagrams to SVG strings.
/// </summary>
public static class DrawioSvgRenderer
{
    /// <summary>
    /// Renders the selected page of a <see cref="DrawioFile"/> to an SVG string.
    /// </summary>
    /// <param name="file">The parsed draw.io file.</param>
    /// <param name="options">Optional rendering options. Uses defaults when <c>null</c>.</param>
    /// <returns>A complete SVG document string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the requested page is not found.</exception>
    public static string RenderToSvg(DrawioFile file, DrawioRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(file);

        var page = SelectPage(file, options);
        return RenderPageToSvg(page, options);
    }

    /// <summary>
    /// Renders a single <see cref="DrawioPage"/> to an SVG string.
    /// </summary>
    /// <param name="page">The diagram page to render.</param>
    /// <param name="options">Optional rendering options. Uses defaults when <c>null</c>.</param>
    /// <returns>A complete SVG document string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="page"/> is <c>null</c>.</exception>
    public static string RenderPageToSvg(DrawioPage page, DrawioRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(page);

        options ??= new DrawioRenderOptions();
        var model = page.Model;

        // Determine layer visibility
        var visibleLayerIds = ResolveVisibleLayers(model, options);

        // Collect visible cells
        var visibleCells = model.Cells
            .Where(c => IsVisible(c, model, visibleLayerIds))
            .ToList();

        // Compute viewBox
        var bounds = BoundsCalculator.Compute(visibleCells, options.Padding);

        // Build <defs> with arrow markers
        var edges = visibleCells.Where(c => c.IsEdge).ToList();
        var markerElements = EdgeRenderer.CollectMarkers(edges).ToList();
        var defsElement = SvgElementBuilder.Svg("defs", markerElements.Cast<object>().ToArray());

        // Build root <svg>
        var viewBox = FormattableString.Invariant(
            $"{bounds.X} {bounds.Y} {bounds.Width} {bounds.Height}");

        var svgAttributes = new List<object>
        {
            new XAttribute("xmlns", SvgElementBuilder.SvgNs.NamespaceName),
            new XAttribute("viewBox", viewBox),
        };

        if (!string.IsNullOrEmpty(options.Width))
        {
            svgAttributes.Add(new XAttribute("width", options.Width));
        }

        if (!string.IsNullOrEmpty(options.Height))
        {
            svgAttributes.Add(new XAttribute("height", options.Height));
        }

        svgAttributes.Add(defsElement);

        // Background rectangle
        if (!string.IsNullOrEmpty(options.Background))
        {
            svgAttributes.Add(SvgElementBuilder.Svg("rect",
                new XAttribute("x", SvgElementBuilder.F(bounds.X)),
                new XAttribute("y", SvgElementBuilder.F(bounds.Y)),
                new XAttribute("width", SvgElementBuilder.F(bounds.Width)),
                new XAttribute("height", SvgElementBuilder.F(bounds.Height)),
                new XAttribute("style", $"fill:{options.Background}")));
        }

        // Render each layer as a <g> element
        var layers = model.Layers;

        if (layers.Count == 0)
        {
            // No explicit layers — render all visible cells directly
            RenderCellsIntoParent(svgAttributes, visibleCells, model);
        }
        else
        {
            foreach (var layer in layers)
            {
                var isLayerVisible = visibleLayerIds == null ||
                    visibleLayerIds.Contains(layer.Id) ||
                    visibleLayerIds.Contains(layer.Value);

                var layerDisplay = isLayerVisible ? "inline" : "none";
                var layerName = string.IsNullOrEmpty(layer.Value) ? layer.Id : layer.Value;

                var layerChildren = visibleCells
                    .Where(c => c.ParentId == layer.Id)
                    .ToList();

                var groupContent = new List<object>
                {
                    new XAttribute("id", $"layer-{layer.Id}"),
                    new XAttribute("data-layer-name", layerName),
                    new XAttribute("display", layerDisplay),
                };

                RenderCellsIntoParent(groupContent, layerChildren, model);

                svgAttributes.Add(SvgElementBuilder.Svg("g", groupContent.Cast<object>().ToArray()));
            }
        }

        var svgElement = SvgElementBuilder.Svg("svg", svgAttributes.ToArray());

        // Serialize without XML declaration
        return svgElement.ToString(SaveOptions.None);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DrawioPage SelectPage(DrawioFile file, DrawioRenderOptions? options)
    {
        if (file.Pages.Count == 0)
        {
            throw new ArgumentException("The draw.io file contains no pages.", nameof(file));
        }

        if (options == null || (options.PageIndex == null && string.IsNullOrEmpty(options.PageName)))
        {
            return file.Pages[0];
        }

        if (!string.IsNullOrEmpty(options.PageName))
        {
            var byName = file.Pages.FirstOrDefault(p =>
                string.Equals(p.Name, options.PageName, StringComparison.OrdinalIgnoreCase));
            if (byName != null)
            {
                return byName;
            }

            throw new ArgumentException(
                $"Page '{options.PageName}' not found in the draw.io file.", nameof(options));
        }

        var index = options.PageIndex!.Value;
        if (index < 0 || index >= file.Pages.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(options),
                $"Page index {index} is out of range (file has {file.Pages.Count} pages).");
        }

        return file.Pages[index];
    }

    /// <summary>
    /// Returns the set of layer IDs/names that are visible, or <c>null</c> if all layers are visible.
    /// </summary>
    private static HashSet<string>? ResolveVisibleLayers(MxGraphModel model, DrawioRenderOptions options)
    {
        if (options.VisibleLayers != null)
        {
            return new HashSet<string>(options.VisibleLayers, StringComparer.OrdinalIgnoreCase);
        }

        if (options.HiddenLayers != null && options.HiddenLayers.Count > 0)
        {
            var hidden = new HashSet<string>(options.HiddenLayers, StringComparer.OrdinalIgnoreCase);
            var visible = model.Layers
                .Where(l => !hidden.Contains(l.Id) && !hidden.Contains(l.Value))
                .Select(l => l.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return visible;
        }

        return null; // All layers visible
    }

    /// <summary>Determines whether a cell should be rendered based on layer visibility.</summary>
    private static bool IsVisible(MxCell cell, MxGraphModel model, HashSet<string>? visibleLayerIds)
    {
        if (cell.IsLayer)
        {
            return false; // Layers are rendered as <g> wrappers, not individual cells
        }

        if (cell.Id is "0" or "1")
        {
            return false; // Skip root and default layer sentinel cells
        }

        if (!cell.IsVertex && !cell.IsEdge)
        {
            return false;
        }

        if (visibleLayerIds == null)
        {
            return true;
        }

        // Check if parent layer is visible
        var parentLayer = model.Layers.FirstOrDefault(l => l.Id == cell.ParentId);
        if (parentLayer != null)
        {
            return visibleLayerIds.Contains(parentLayer.Id) ||
                   visibleLayerIds.Contains(parentLayer.Value);
        }

        return true; // No explicit layer parent → always visible
    }

    private static void RenderCellsIntoParent(List<object> target, IList<MxCell> cells, MxGraphModel model)
    {
        foreach (var cell in cells.Where(c => c.IsVertex))
        {
            var shapeEl = ShapeRenderer.Render(cell);
            if (shapeEl != null)
            {
                target.Add(shapeEl);
            }

            // Label
            if (cell.Geometry != null)
            {
                var textEl = TextRenderer.Render(cell,
                    cell.Geometry.X, cell.Geometry.Y,
                    cell.Geometry.Width, cell.Geometry.Height);
                if (textEl != null)
                {
                    target.Add(textEl);
                }
            }
        }

        foreach (var cell in cells.Where(c => c.IsEdge))
        {
            foreach (var el in EdgeRenderer.Render(cell, id => model.GetCellById(id)))
            {
                target.Add(el);
            }
        }
    }
}
