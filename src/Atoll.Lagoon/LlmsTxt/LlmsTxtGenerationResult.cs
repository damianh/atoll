namespace Atoll.Lagoon.LlmsTxt;

/// <summary>
/// The result of an <c>llms.txt</c> / <c>llms-full.txt</c> generation operation.
/// </summary>
public sealed class LlmsTxtGenerationResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="LlmsTxtGenerationResult"/>.
    /// </summary>
    /// <param name="documentCount">The number of documents included in the output.</param>
    /// <param name="llmsTxtPath">The full path of the written <c>llms.txt</c> file.</param>
    /// <param name="llmsFullTxtPath">
    /// The full path of the written <c>llms-full.txt</c> file, or <c>null</c> if no documents
    /// provided markdown bodies.
    /// </param>
    /// <param name="elapsed">The time taken to generate and write the files.</param>
    public LlmsTxtGenerationResult(int documentCount, string llmsTxtPath, string? llmsFullTxtPath, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(llmsTxtPath);
        DocumentCount = documentCount;
        LlmsTxtPath = llmsTxtPath;
        LlmsFullTxtPath = llmsFullTxtPath;
        Elapsed = elapsed;
    }

    /// <summary>Gets the number of documents included in the output.</summary>
    public int DocumentCount { get; }

    /// <summary>Gets the full path of the written <c>llms.txt</c> file.</summary>
    public string LlmsTxtPath { get; }

    /// <summary>
    /// Gets the full path of the written <c>llms-full.txt</c> file, or <c>null</c> if not generated.
    /// </summary>
    public string? LlmsFullTxtPath { get; }

    /// <summary>Gets the time taken to generate and write the files.</summary>
    public TimeSpan Elapsed { get; }
}
