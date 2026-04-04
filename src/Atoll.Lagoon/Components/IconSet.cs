namespace Atoll.Lagoon.Components;

/// <summary>
/// Provides the built-in SVG path data for Atoll.Lagoon icons.
/// All icons use a 24×24 viewBox with <c>fill="currentColor"</c>.
/// </summary>
public static class IconSet
{
    private static readonly IReadOnlyDictionary<IconName, string> Paths =
        new Dictionary<IconName, string>
        {
            [IconName.Information] =
                "<circle cx=\"12\" cy=\"12\" r=\"10\"/>" +
                "<line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"/>" +
                "<line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"/>",

            [IconName.Tip] =
                "<path d=\"M12 2a7 7 0 0 1 7 7c0 3.5-2.5 5.5-3 7H8c-.5-1.5-3-3.5-3-7a7 7 0 0 1 7-7z\"/>" +
                "<path d=\"M9 21h6\"/>" +
                "<path d=\"M9.7 17a4.5 4.5 0 0 1-.7-2.5\"/>",

            [IconName.Warning] =
                "<path d=\"M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z\"/>" +
                "<line x1=\"12\" y1=\"9\" x2=\"12\" y2=\"13\"/>" +
                "<line x1=\"12\" y1=\"17\" x2=\"12.01\" y2=\"17\"/>",

            [IconName.Danger] =
                "<polygon points=\"7.86 2 16.14 2 22 7.86 22 16.14 16.14 22 7.86 22 2 16.14 2 7.86 7.86 2\"/>" +
                "<line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"/>" +
                "<line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"/>",

            [IconName.Star] =
                "<polygon points=\"12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2\"/>",

            [IconName.Rocket] =
                "<path d=\"M4.5 16.5c-1.5 1.26-2 5-2 5s3.74-.5 5-2c.71-.84.7-2.13-.09-2.91a2.18 2.18 0 0 0-2.91-.09z\"/>" +
                "<path d=\"m12 15-3-3a22 22 0 0 1 2-3.95A12.88 12.88 0 0 1 22 2c0 2.72-.78 7.5-6 11a22.35 22.35 0 0 1-4 2z\"/>" +
                "<path d=\"M9 12H4s.55-3.03 2-4c1.62-1.08 5 0 5 0\"/>" +
                "<path d=\"M12 15v5s3.03-.55 4-2c1.08-1.62 0-5 0-5\"/>",

            [IconName.ExternalLink] =
                "<path d=\"M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6\"/>" +
                "<polyline points=\"15 3 21 3 21 9\"/>" +
                "<line x1=\"10\" y1=\"14\" x2=\"21\" y2=\"3\"/>",

            [IconName.Document] =
                "<path d=\"M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z\"/>" +
                "<polyline points=\"14 2 14 8 20 8\"/>",

            [IconName.Folder] =
                "<path d=\"M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z\"/>",

            [IconName.FolderOpen] =
                "<path d=\"M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z\"/>" +
                "<polyline points=\"2 10 2 19 22 19 22 10 2 10\"/>",

            [IconName.File] =
                "<path d=\"M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z\"/>" +
                "<polyline points=\"13 2 13 9 20 9\"/>",

            [IconName.Pencil] =
                "<path d=\"M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7\"/>" +
                "<path d=\"M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z\"/>",

            [IconName.Check] =
                "<polyline points=\"20 6 9 17 4 12\"/>",

            [IconName.Heart] =
                "<path d=\"M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z\"/>",

            [IconName.ArrowRight] =
                "<line x1=\"5\" y1=\"12\" x2=\"19\" y2=\"12\"/>" +
                "<polyline points=\"12 5 19 12 12 19\"/>",

            [IconName.ArrowLeft] =
                "<line x1=\"19\" y1=\"12\" x2=\"5\" y2=\"12\"/>" +
                "<polyline points=\"12 19 5 12 12 5\"/>",

            [IconName.Plus] =
                "<line x1=\"12\" y1=\"5\" x2=\"12\" y2=\"19\"/>" +
                "<line x1=\"5\" y1=\"12\" x2=\"19\" y2=\"12\"/>",

            [IconName.Minus] =
                "<line x1=\"5\" y1=\"12\" x2=\"19\" y2=\"12\"/>",

            [IconName.ChevronRight] =
                "<polyline points=\"9 18 15 12 9 6\"/>",

            [IconName.ChevronDown] =
                "<polyline points=\"6 9 12 15 18 9\"/>",

            [IconName.Sun] =
                "<circle cx=\"12\" cy=\"12\" r=\"5\"/>" +
                "<line x1=\"12\" y1=\"1\" x2=\"12\" y2=\"3\"/>" +
                "<line x1=\"12\" y1=\"21\" x2=\"12\" y2=\"23\"/>" +
                "<line x1=\"4.22\" y1=\"4.22\" x2=\"5.64\" y2=\"5.64\"/>" +
                "<line x1=\"18.36\" y1=\"18.36\" x2=\"19.78\" y2=\"19.78\"/>" +
                "<line x1=\"1\" y1=\"12\" x2=\"3\" y2=\"12\"/>" +
                "<line x1=\"21\" y1=\"12\" x2=\"23\" y2=\"12\"/>" +
                "<line x1=\"4.22\" y1=\"19.78\" x2=\"5.64\" y2=\"18.36\"/>" +
                "<line x1=\"18.36\" y1=\"5.64\" x2=\"19.78\" y2=\"4.22\"/>",

            [IconName.Moon] =
                "<path d=\"M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z\"/>",

            [IconName.Search] =
                "<circle cx=\"11\" cy=\"11\" r=\"8\"/>" +
                "<line x1=\"21\" y1=\"21\" x2=\"16.65\" y2=\"16.65\"/>",

            [IconName.Menu] =
                "<line x1=\"3\" y1=\"12\" x2=\"21\" y2=\"12\"/>" +
                "<line x1=\"3\" y1=\"6\" x2=\"21\" y2=\"6\"/>" +
                "<line x1=\"3\" y1=\"18\" x2=\"21\" y2=\"18\"/>",

            [IconName.Close] =
                "<line x1=\"18\" y1=\"6\" x2=\"6\" y2=\"18\"/>" +
                "<line x1=\"6\" y1=\"6\" x2=\"18\" y2=\"18\"/>",
        };

    /// <summary>
    /// Attempts to get the SVG inner content (path data) for the specified icon.
    /// </summary>
    /// <param name="name">The icon name.</param>
    /// <param name="svgContent">
    /// When this method returns <c>true</c>, contains the SVG inner elements string; otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if the icon was found; otherwise, <c>false</c>.</returns>
    public static bool TryGetSvgContent(IconName name, out string? svgContent)
    {
        return Paths.TryGetValue(name, out svgContent);
    }
}
