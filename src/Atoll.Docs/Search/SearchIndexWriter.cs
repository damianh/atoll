using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atoll.Docs.Search;

/// <summary>
/// Serializes a <see cref="SearchIndex"/> to JSON and writes it as
/// <c>search-index.json</c> in an output directory.
/// </summary>
public sealed class SearchIndexWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <paramref name="index"/> to JSON and writes it to
    /// <c>{outputDirectory}/search-index.json</c>.
    /// </summary>
    /// <param name="index">The search index to write.</param>
    /// <param name="outputDirectory">The directory to write the file into.</param>
    public void Write(SearchIndex index, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(outputDirectory);
        var path = Path.Combine(outputDirectory, "search-index.json");
        var json = Serialize(index);
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Serializes the <paramref name="index"/> to JSON and writes it to
    /// <c>{outputDirectory}/search-index.json</c> asynchronously.
    /// </summary>
    /// <param name="index">The search index to write.</param>
    /// <param name="outputDirectory">The directory to write the file into.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public async Task WriteAsync(SearchIndex index, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(outputDirectory);
        var path = Path.Combine(outputDirectory, "search-index.json");
        var json = Serialize(index);
        Directory.CreateDirectory(outputDirectory);
        await File.WriteAllTextAsync(path, json);
    }

    /// <summary>
    /// Serializes the <paramref name="index"/> to a JSON string.
    /// </summary>
    /// <param name="index">The search index to serialize.</param>
    /// <returns>A JSON string representation of the index.</returns>
    public string Serialize(SearchIndex index)
    {
        ArgumentNullException.ThrowIfNull(index);
        return JsonSerializer.Serialize(index, JsonOptions);
    }
}
