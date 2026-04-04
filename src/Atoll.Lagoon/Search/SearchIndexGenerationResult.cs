namespace Atoll.Lagoon.Search;

/// <summary>
/// The result of a search index generation operation.
/// </summary>
public sealed class SearchIndexGenerationResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="SearchIndexGenerationResult"/>.
    /// </summary>
    /// <param name="entryCount">The number of entries indexed.</param>
    /// <param name="outputPath">The full path of the written <c>search-index.json</c> file.</param>
    /// <param name="elapsed">The time taken to generate and write the index.</param>
    public SearchIndexGenerationResult(int entryCount, string outputPath, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(outputPath);
        EntryCount = entryCount;
        OutputPath = outputPath;
        Elapsed = elapsed;
    }

    /// <summary>Gets the number of entries indexed.</summary>
    public int EntryCount { get; }

    /// <summary>Gets the full path of the written <c>search-index.json</c> file.</summary>
    public string OutputPath { get; }

    /// <summary>Gets the time taken to generate and write the index.</summary>
    public TimeSpan Elapsed { get; }
}
