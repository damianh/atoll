using System.Text.Json;

namespace Atoll.Configuration;

/// <summary>
/// Loads <see cref="AtollConfig"/> from an <c>atoll.json</c> file on disk.
/// Searches the specified directory (or current directory) for the configuration file.
/// </summary>
/// <remarks>
/// <para>
/// If no <c>atoll.json</c> is found, <see cref="LoadAsync"/> returns a default
/// <see cref="AtollConfig"/> with all defaults applied. This makes the configuration
/// file entirely optional for simple projects.
/// </para>
/// </remarks>
public static class AtollConfigLoader
{
    /// <summary>
    /// The default configuration file name.
    /// </summary>
    public const string DefaultFileName = "atoll.json";

    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Loads the <see cref="AtollConfig"/> from the specified directory.
    /// Returns a default configuration if the file does not exist.
    /// </summary>
    /// <param name="directory">The directory to search for <c>atoll.json</c>.</param>
    /// <param name="cancellationToken">A token to cancel the load operation.</param>
    /// <returns>The loaded or default configuration.</returns>
    public static async Task<AtollConfig> LoadAsync(string directory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(directory);

        var filePath = Path.Combine(directory, DefaultFileName);
        return await LoadFromFileAsync(filePath, cancellationToken);
    }

    /// <summary>
    /// Loads the <see cref="AtollConfig"/> from the specified file path.
    /// Returns a default configuration if the file does not exist.
    /// </summary>
    /// <param name="filePath">The full path to the <c>atoll.json</c> file.</param>
    /// <param name="cancellationToken">A token to cancel the load operation.</param>
    /// <returns>The loaded or default configuration.</returns>
    public static async Task<AtollConfig> LoadFromFileAsync(string filePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            return new AtollConfig();
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Deserialize(json);
    }

    /// <summary>
    /// Deserializes an <see cref="AtollConfig"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized configuration.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the JSON cannot be deserialized.
    /// </exception>
    public static AtollConfig Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<AtollConfig>(json, DeserializeOptions) ?? new AtollConfig();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse atoll.json: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes an <see cref="AtollConfig"/> to a JSON string.
    /// </summary>
    /// <param name="config">The configuration to serialize.</param>
    /// <returns>The JSON string representation.</returns>
    public static string Serialize(AtollConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });
    }

    /// <summary>
    /// Resolves the output directory path, making it absolute relative to the project root.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="projectRoot">The project root directory.</param>
    /// <returns>The absolute path to the output directory.</returns>
    public static string ResolveOutputDirectory(AtollConfig config, string projectRoot)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(projectRoot);

        return Path.IsPathRooted(config.OutDir)
            ? config.OutDir
            : Path.GetFullPath(Path.Combine(projectRoot, config.OutDir));
    }

    /// <summary>
    /// Resolves the source pages directory path, making it absolute relative to the project root.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="projectRoot">The project root directory.</param>
    /// <returns>The absolute path to the source pages directory.</returns>
    public static string ResolveSrcDirectory(AtollConfig config, string projectRoot)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(projectRoot);

        return Path.IsPathRooted(config.SrcDir)
            ? config.SrcDir
            : Path.GetFullPath(Path.Combine(projectRoot, config.SrcDir));
    }

    /// <summary>
    /// Resolves the public directory path, making it absolute relative to the project root.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="projectRoot">The project root directory.</param>
    /// <returns>The absolute path to the public directory.</returns>
    public static string ResolvePublicDirectory(AtollConfig config, string projectRoot)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(projectRoot);

        return Path.IsPathRooted(config.PublicDir)
            ? config.PublicDir
            : Path.GetFullPath(Path.Combine(projectRoot, config.PublicDir));
    }

    /// <summary>
    /// Normalizes the base path to ensure it has a leading slash and no trailing slash.
    /// Returns <c>/</c> for root.
    /// </summary>
    /// <param name="basePath">The base path to normalize.</param>
    /// <returns>The normalized base path.</returns>
    public static string NormalizeBasePath(string basePath)
    {
        ArgumentNullException.ThrowIfNull(basePath);

        if (basePath.Length == 0 || basePath == "/")
        {
            return "/";
        }

        // Ensure leading slash
        if (!basePath.StartsWith('/'))
        {
            basePath = "/" + basePath;
        }

        // Remove trailing slash
        return basePath.TrimEnd('/');
    }
}
