using Atoll.Components;

namespace Atoll.Swell.Layouts;

/// <summary>
/// Maps layout name strings (from per-slide frontmatter) to the corresponding
/// <see cref="AtollComponent"/> layout type.
/// </summary>
public static class SlideLayoutResolver
{
    private static readonly IReadOnlyDictionary<string, Type> LayoutMap =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["default"] = typeof(DefaultSlideLayout),
            ["cover"] = typeof(CoverSlideLayout),
            ["center"] = typeof(CenterSlideLayout),
            ["two-cols"] = typeof(TwoColsSlideLayout),
            ["image-right"] = typeof(ImageRightSlideLayout),
            ["image-left"] = typeof(ImageLeftSlideLayout),
            ["section"] = typeof(SectionSlideLayout),
            ["end"] = typeof(EndSlideLayout),
        };

    /// <summary>
    /// Resolves the layout component type for the given layout name.
    /// Falls back to <see cref="DefaultSlideLayout"/> for unknown names.
    /// </summary>
    /// <param name="layoutName">The layout name from per-slide frontmatter.</param>
    /// <returns>The resolved layout component <see cref="Type"/>.</returns>
    public static Type Resolve(string? layoutName)
    {
        if (layoutName is not null && LayoutMap.TryGetValue(layoutName, out var type))
        {
            return type;
        }

        return typeof(DefaultSlideLayout);
    }
}
