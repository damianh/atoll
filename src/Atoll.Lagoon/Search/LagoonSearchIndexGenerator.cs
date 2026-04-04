using System.Diagnostics;
using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Search;

/// <summary>
/// Generates a <c>search-index.json</c> file from content collections using the
/// <see cref="SearchIndexBuilder"/> and <see cref="SearchIndexWriter"/> infrastructure.
/// Follows the caller-orchestrated post-processing pattern used by <c>AssetPipeline</c>
/// and <c>BuildManifestWriter</c> — call this after <c>StaticSiteGenerator.GenerateAsync</c>.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// var generator = new LagoonSearchIndexGenerator(outputDirectory);
/// var result = await generator.GenerateAsync(query, config);
/// Console.WriteLine($"  Search:  {result.EntryCount} entries indexed");
/// </code>
/// </remarks>
public sealed class LagoonSearchIndexGenerator
{
    private readonly string _outputDirectory;
    private readonly SearchIndexWriter _writer;

    /// <summary>
    /// Initializes a new instance of <see cref="LagoonSearchIndexGenerator"/>.
    /// </summary>
    /// <param name="outputDirectory">The SSG output directory where <c>search-index.json</c> will be written.</param>
    public LagoonSearchIndexGenerator(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
        _writer = new SearchIndexWriter();
    }

    /// <summary>
    /// Generates the search index from documents provided by the <paramref name="configuration"/> and
    /// writes <c>search-index.json</c> to the output directory.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <param name="configuration">The search index configuration that provides documents to index.</param>
    /// <returns>A <see cref="SearchIndexGenerationResult"/> with stats about the generated index.</returns>
    public async Task<SearchIndexGenerationResult> GenerateAsync(
        CollectionQuery query,
        ISearchIndexConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configuration);

        var sw = Stopwatch.StartNew();

        var builder = new SearchIndexBuilder();
        foreach (var document in configuration.GetDocuments(query))
        {
            builder.Add(document);
        }

        var index = builder.Build();
        await _writer.WriteAsync(index, _outputDirectory);

        sw.Stop();
        var outputPath = Path.Combine(_outputDirectory, "search-index.json");
        return new SearchIndexGenerationResult(index.Entries.Count, outputPath, sw.Elapsed);
    }

    /// <summary>
    /// Generates the search index from a collection of <see cref="SearchDocumentInput"/> items and
    /// writes <c>search-index.json</c> to the output directory.
    /// </summary>
    /// <param name="documents">The documents to include in the search index.</param>
    /// <returns>A <see cref="SearchIndexGenerationResult"/> with stats about the generated index.</returns>
    public async Task<SearchIndexGenerationResult> GenerateAsync(
        IEnumerable<SearchDocumentInput> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);
        return await GenerateFromDocumentsAsync(documents, "");
    }

    /// <summary>
    /// Generates the search index from a collection of <see cref="SearchDocumentInput"/> items and
    /// writes it to a locale-specific subdirectory (e.g., <c>fr/search-index.json</c>).
    /// </summary>
    /// <param name="documents">The documents to include in the search index.</param>
    /// <param name="localePrefix">
    /// The locale prefix (e.g., <c>"fr"</c>). When non-empty, the index is written to
    /// <c>{outputDirectory}/{localePrefix}/search-index.json</c>.
    /// When empty, the index is written to <c>{outputDirectory}/search-index.json</c>.
    /// </param>
    /// <returns>A <see cref="SearchIndexGenerationResult"/> with stats about the generated index.</returns>
    public async Task<SearchIndexGenerationResult> GenerateAsync(
        IEnumerable<SearchDocumentInput> documents,
        string localePrefix)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(localePrefix);
        return await GenerateFromDocumentsAsync(documents, localePrefix);
    }

    private async Task<SearchIndexGenerationResult> GenerateFromDocumentsAsync(
        IEnumerable<SearchDocumentInput> documents,
        string localePrefix)
    {
        var sw = Stopwatch.StartNew();

        var builder = new SearchIndexBuilder();
        foreach (var document in documents)
        {
            builder.Add(document);
        }

        var index = builder.Build();

        var targetDirectory = ResolveOutputDirectory(localePrefix);
        await _writer.WriteAsync(index, targetDirectory);

        sw.Stop();
        var outputPath = Path.Combine(targetDirectory, "search-index.json");
        return new SearchIndexGenerationResult(index.Entries.Count, outputPath, sw.Elapsed);
    }

    private string ResolveOutputDirectory(string localePrefix)
    {
        if (string.IsNullOrEmpty(localePrefix))
        {
            return _outputDirectory;
        }

        var normalized = localePrefix.Trim('/');
        return Path.Combine(_outputDirectory, normalized);
    }
}
