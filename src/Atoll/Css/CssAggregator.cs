using System.Text;

namespace Atoll.Css;

/// <summary>
/// Collects CSS from rendered component types during page rendering.
/// CSS is collected in the order components are rendered, deduplicated
/// by component type, and can be retrieved as a single aggregated string.
/// </summary>
/// <remarks>
/// <para>
/// The aggregator tracks which component types have already contributed CSS
/// to avoid duplicate style blocks. Each component type contributes CSS at most
/// once per page render, regardless of how many instances are rendered.
/// </para>
/// </remarks>
public sealed class CssAggregator
{
    private readonly List<CssEntry> _entries = [];
    private readonly HashSet<Type> _seenTypes = [];
    private readonly HashSet<string> _seenIdentifiers = [];

    /// <summary>
    /// Adds CSS from the specified component type. The CSS is extracted from
    /// <see cref="StylesAttribute"/> declarations and scoped automatically.
    /// If the component type has already been added, this is a no-op.
    /// </summary>
    /// <param name="componentType">The component type to extract CSS from.</param>
    /// <returns><c>true</c> if CSS was added; <c>false</c> if the type was already seen or has no styles.</returns>
    public bool Add(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        if (!_seenTypes.Add(componentType))
        {
            return false;
        }

        if (!StyleScoper.HasStyles(componentType))
        {
            return false;
        }

        var scopedCss = StyleScoper.ExtractAndScope(componentType);
        if (scopedCss.Length == 0)
        {
            return false;
        }

        var isGlobal = StyleScoper.IsGlobal(componentType);
        _entries.Add(new CssEntry(componentType, scopedCss, isGlobal));
        return true;
    }

    /// <summary>
    /// Adds raw CSS with a string identifier for deduplication.
    /// </summary>
    /// <param name="identifier">A unique identifier for deduplication.</param>
    /// <param name="css">The CSS text to add.</param>
    /// <param name="isGlobal">Whether the CSS should be treated as global (unscoped).</param>
    /// <returns><c>true</c> if the CSS was added; <c>false</c> if the identifier was already seen.</returns>
    public bool Add(string identifier, string css, bool isGlobal)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(css);

        if (!_seenIdentifiers.Add(identifier))
        {
            return false;
        }

        if (css.Length == 0)
        {
            return false;
        }

        _entries.Add(new CssEntry(null, css, isGlobal));
        return true;
    }

    /// <summary>
    /// Gets the number of CSS entries that have been collected.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets all collected CSS entries in render order.
    /// </summary>
    /// <returns>A read-only list of CSS entries.</returns>
    public IReadOnlyList<CssEntry> GetEntries()
    {
        return _entries;
    }

    /// <summary>
    /// Gets all collected CSS as a single concatenated string.
    /// </summary>
    /// <returns>The combined CSS text.</returns>
    public string GetCombinedCss()
    {
        if (_entries.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var entry in _entries)
        {
            if (builder.Length > 0)
            {
                builder.Append('\n');
            }
            builder.Append(entry.Css);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Clears all collected CSS entries.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
        _seenTypes.Clear();
        _seenIdentifiers.Clear();
    }
}

/// <summary>
/// Represents a single CSS entry collected during page rendering.
/// </summary>
/// <param name="ComponentType">The component type that contributed this CSS, or <c>null</c> for raw entries.</param>
/// <param name="Css">The CSS text (already scoped if applicable).</param>
/// <param name="IsGlobal">Whether the CSS is global (unscoped).</param>
public sealed record CssEntry(Type? ComponentType, string Css, bool IsGlobal);
