using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atoll.Redirects;

namespace Atoll.Build.Ssg;

/// <summary>
/// Writes a <c>redirects.json</c> file to the SSG output directory, containing all
/// redirect entries from a <see cref="RedirectMap"/>.
/// </summary>
/// <remarks>
/// The JSON format is an object with a <c>redirects</c> array:
/// <code>
/// {
///   "redirects": [
///     { "from": "/old", "to": "/new", "status": 301 }
///   ]
/// }
/// </code>
/// This file can be consumed by the ASP.NET Core hosting layer at startup to register
/// server-side redirects without requiring a running Atoll instance.
/// </remarks>
public sealed class RedirectsFileWriter
{
    private const string FileName = "redirects.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new <see cref="RedirectsFileWriter"/> with the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">The root output directory (e.g., <c>dist/</c>).</param>
    public RedirectsFileWriter(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Serializes a <see cref="RedirectMap"/> to a JSON string using the default status code 301.
    /// </summary>
    /// <param name="redirectMap">The redirect map to serialize.</param>
    /// <returns>The JSON string representation.</returns>
    public static string Serialize(RedirectMap redirectMap)
    {
        return Serialize(redirectMap, 301);
    }

    /// <summary>
    /// Serializes a <see cref="RedirectMap"/> to a JSON string using the specified status code.
    /// </summary>
    /// <param name="redirectMap">The redirect map to serialize.</param>
    /// <param name="statusCode">The HTTP redirect status code to embed in each entry.</param>
    /// <returns>The JSON string representation.</returns>
    public static string Serialize(RedirectMap redirectMap, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(redirectMap);

        var entries = redirectMap.Entries
            .Select(kvp => new RedirectEntry(kvp.Key, kvp.Value, statusCode))
            .ToList();

        var document = new RedirectDocument(entries);
        return JsonSerializer.Serialize(document, SerializerOptions);
    }

    /// <summary>
    /// Writes a <c>redirects.json</c> file to the output directory using the default status code 301.
    /// </summary>
    /// <param name="redirectMap">The redirect map to write.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    public Task WriteAsync(RedirectMap redirectMap)
    {
        return WriteAsync(redirectMap, 301);
    }

    /// <summary>
    /// Writes a <c>redirects.json</c> file to the output directory using the specified status code.
    /// </summary>
    /// <param name="redirectMap">The redirect map to write.</param>
    /// <param name="statusCode">The HTTP redirect status code to embed in each entry.</param>
    /// <returns>A task that completes when the file has been written.</returns>
    public async Task WriteAsync(RedirectMap redirectMap, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(redirectMap);

        var json = Serialize(redirectMap, statusCode);
        var filePath = Path.Combine(_outputDirectory, FileName);
        await File.WriteAllTextAsync(filePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private sealed record RedirectEntry(string From, string To, int Status);
    private sealed record RedirectDocument(List<RedirectEntry> Redirects);
}
