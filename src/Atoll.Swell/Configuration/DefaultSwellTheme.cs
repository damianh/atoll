namespace Atoll.Swell.Configuration;

/// <summary>
/// The built-in default Swell theme. Uses the CSS defined in <c>SwellTheme</c> and all
/// default slide layout components. No CSS overrides or layout overrides are applied.
/// </summary>
public sealed class DefaultSwellTheme : ISwellTheme
{
    /// <summary>The singleton instance of the default theme.</summary>
    public static readonly DefaultSwellTheme Instance = new();

    private DefaultSwellTheme() { }

    /// <inheritdoc />
    public string Name => "default";

    /// <inheritdoc />
    public string? AdditionalCss => null;

    /// <inheritdoc />
    public Type? ResolveLayoutOverride(string layoutName) => null;
}
