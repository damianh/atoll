using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Atoll.Content.Frontmatter;

/// <summary>
/// Binds raw YAML frontmatter strings to strongly-typed C# schema objects
/// using YamlDotNet deserialization with camelCase naming convention.
/// </summary>
/// <remarks>
/// <para>
/// The binder uses camelCase naming convention by default, matching the common
/// frontmatter convention where YAML keys are <c>camelCase</c> and C# properties
/// are <c>PascalCase</c>. For example, YAML key <c>pubDate</c> maps to C# property <c>PubDate</c>.
/// </para>
/// </remarks>
public static class FrontmatterBinder
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Deserializes a YAML frontmatter string into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The target schema type. Must have a parameterless constructor.</typeparam>
    /// <param name="yaml">The raw YAML string to deserialize.</param>
    /// <returns>A new instance of <typeparamref name="T"/> populated from the YAML data.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="yaml"/> is <c>null</c>.</exception>
    /// <exception cref="FrontmatterBindingException">The YAML could not be deserialized to the target type.</exception>
    public static T Bind<T>(string yaml) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(yaml);

        if (string.IsNullOrWhiteSpace(yaml))
        {
            return new T();
        }

        try
        {
            var result = Deserializer.Deserialize<T>(yaml);
            return result ?? new T();
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new FrontmatterBindingException(
                $"Failed to bind YAML frontmatter to type '{typeof(T).Name}': {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Deserializes a YAML frontmatter string into an object of the specified type.
    /// </summary>
    /// <param name="yaml">The raw YAML string to deserialize.</param>
    /// <param name="targetType">The target schema type. Must have a parameterless constructor.</param>
    /// <returns>A new instance of <paramref name="targetType"/> populated from the YAML data.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="yaml"/> or <paramref name="targetType"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="FrontmatterBindingException">The YAML could not be deserialized to the target type.</exception>
    public static object Bind(string yaml, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        ArgumentNullException.ThrowIfNull(targetType);

        if (string.IsNullOrWhiteSpace(yaml))
        {
            return Activator.CreateInstance(targetType)
                ?? throw new FrontmatterBindingException(
                    $"Cannot create an instance of type '{targetType.Name}'. Ensure it has a parameterless constructor.");
        }

        try
        {
            var result = Deserializer.Deserialize(yaml, targetType);
            return result
                ?? Activator.CreateInstance(targetType)
                ?? throw new FrontmatterBindingException(
                    $"Cannot create an instance of type '{targetType.Name}'. Ensure it has a parameterless constructor.");
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new FrontmatterBindingException(
                $"Failed to bind YAML frontmatter to type '{targetType.Name}': {ex.Message}",
                ex);
        }
    }
}

/// <summary>
/// Exception thrown when YAML frontmatter cannot be deserialized to a target type.
/// </summary>
public sealed class FrontmatterBindingException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="FrontmatterBindingException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    public FrontmatterBindingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="FrontmatterBindingException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public FrontmatterBindingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
