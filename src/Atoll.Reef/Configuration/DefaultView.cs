namespace Atoll.Reef.Configuration;

/// <summary>
/// Defines the default article listing view for the Reef theme.
/// </summary>
public enum DefaultView
{
    /// <summary>Vertical list view — compact title + date + description rows.</summary>
    List,

    /// <summary>Responsive card grid — image, title, excerpt, and meta per card.</summary>
    Grid,

    /// <summary>Tabular view — structured table with configurable columns.</summary>
    Table,
}
