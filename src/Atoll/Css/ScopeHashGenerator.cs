using System.IO.Hashing;
using System.Text;

namespace Atoll.Css;

/// <summary>
/// Generates deterministic scope hashes for CSS class scoping.
/// Uses XxHash64 on the component type full name to produce a short,
/// collision-resistant hash string used in <c>:where(.atoll-HASH)</c> selectors.
/// </summary>
public static class ScopeHashGenerator
{
    private const string Prefix = "atoll-";
    private const int HashLength = 8;

    /// <summary>
    /// Generates a scope hash from the specified component type.
    /// </summary>
    /// <param name="componentType">The component type to generate a hash for.</param>
    /// <returns>A deterministic scope hash string (e.g., <c>atoll-a1b2c3d4</c>).</returns>
    public static string Generate(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var fullName = componentType.FullName
            ?? throw new ArgumentException("Component type must have a full name.", nameof(componentType));

        return Generate(fullName);
    }

    /// <summary>
    /// Generates a scope hash from the specified identifier string.
    /// </summary>
    /// <param name="identifier">The identifier to generate a hash for.</param>
    /// <returns>A deterministic scope hash string (e.g., <c>atoll-a1b2c3d4</c>).</returns>
    public static string Generate(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        if (identifier.Length == 0)
        {
            throw new ArgumentException("Identifier must not be empty.", nameof(identifier));
        }

        var bytes = Encoding.UTF8.GetBytes(identifier);
        var hash = XxHash64.HashToUInt64(bytes);
        var hexHash = hash.ToString("x16");

        return string.Concat(Prefix, hexHash.AsSpan(0, HashLength));
    }

    /// <summary>
    /// Generates the CSS class name used for scoping (e.g., <c>.atoll-a1b2c3d4</c>).
    /// </summary>
    /// <param name="componentType">The component type.</param>
    /// <returns>The CSS class selector including the leading dot.</returns>
    public static string GenerateClassSelector(Type componentType)
    {
        return "." + Generate(componentType);
    }

    /// <summary>
    /// Generates the CSS class name used for scoping (e.g., <c>.atoll-a1b2c3d4</c>).
    /// </summary>
    /// <param name="identifier">The identifier string.</param>
    /// <returns>The CSS class selector including the leading dot.</returns>
    public static string GenerateClassSelector(string identifier)
    {
        return "." + Generate(identifier);
    }
}
