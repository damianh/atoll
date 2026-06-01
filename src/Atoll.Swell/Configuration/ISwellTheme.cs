namespace Atoll.Swell.Configuration;

/// <summary>
/// Defines the contract for a Swell theme override. Implement this interface in a
/// separate NuGet package (e.g. <c>Atoll.Swell.Theme.Dark</c>) to override CSS or
/// provide custom slide layout component types.
/// </summary>
/// <remarks>
/// <para>
/// A theme package should provide a public class implementing <see cref="ISwellTheme"/>
/// and register it with the Atoll service container via extension methods or
/// by registering as a named service.
/// </para>
/// <para>
/// The default theme is <see cref="DefaultSwellTheme"/> and is used when no custom theme
/// is configured in the deck headmatter.
/// </para>
/// </remarks>
public interface ISwellTheme
{
    /// <summary>
    /// Gets the theme name. Must match the <c>theme</c> property in deck headmatter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets additional CSS to inject after the default Swell CSS.
    /// Return <see langword="null"/> or empty string for no additional styles.
    /// </summary>
    string? AdditionalCss { get; }

    /// <summary>
    /// Optionally overrides the component type for a named slide layout.
    /// Return <see langword="null"/> to use the default layout for that name.
    /// </summary>
    /// <param name="layoutName">The layout name (e.g. "cover", "center").</param>
    /// <returns>A custom component type, or <see langword="null"/> to use the default.</returns>
    Type? ResolveLayoutOverride(string layoutName);
}
