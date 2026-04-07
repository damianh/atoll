using System.Diagnostics;
using System.Text;
using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.LlmsTxt;

/// <summary>
/// Generates <c>llms.txt</c> and <c>llms-full.txt</c> files from content collections following
/// the <see href="https://llmstxt.org/">llms.txt specification</see>.
/// Follows the caller-orchestrated post-processing pattern used by <c>LagoonSearchIndexGenerator</c>
/// and <c>LagoonRedirectGenerator</c> — call this after <c>StaticSiteGenerator.GenerateAsync</c>.
/// </summary>
/// <remarks>
/// <para>
/// The generated <c>llms.txt</c> file is a concise markdown index of all documentation pages,
/// structured with an H1 title, optional blockquote summary, and H2-grouped link lists.
/// AI agents can fetch this single file to understand the available documentation.
/// </para>
/// <para>
/// When documents provide <see cref="LlmsTxtDocumentInput.MarkdownBody"/>, an additional
/// <c>llms-full.txt</c> file is generated with the full content of every page inlined —
/// suitable for ingestion into an LLM context window.
/// </para>
/// Usage:
/// <code>
/// var generator = new LlmsTxtGenerator(outputDirectory);
/// var result = await generator.GenerateAsync(query, config);
/// Console.WriteLine($"  LLMs:    {result.DocumentCount} documents exported");
/// </code>
/// </remarks>
public sealed class LlmsTxtGenerator
{
    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new instance of <see cref="LlmsTxtGenerator"/>.
    /// </summary>
    /// <param name="outputDirectory">The SSG output directory where <c>llms.txt</c> will be written.</param>
    public LlmsTxtGenerator(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Generates <c>llms.txt</c> (and optionally <c>llms-full.txt</c>) from documents provided
    /// by the <paramref name="configuration"/> and writes them to the output directory.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <param name="configuration">The LLM export configuration that provides site info and documents.</param>
    /// <returns>A <see cref="LlmsTxtGenerationResult"/> with stats about the generated files.</returns>
    public async Task<LlmsTxtGenerationResult> GenerateAsync(
        CollectionQuery query,
        ILlmsTxtConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configuration);

        var sw = Stopwatch.StartNew();

        var siteInfo = configuration.GetSiteInfo();
        var documents = configuration.GetDocuments(query).ToList();

        Directory.CreateDirectory(_outputDirectory);

        var llmsTxtPath = Path.Combine(_outputDirectory, "llms.txt");
        var llmsTxtContent = BuildLlmsTxt(siteInfo, documents);
        await File.WriteAllTextAsync(llmsTxtPath, llmsTxtContent, Utf8NoBom);

        string? llmsFullTxtPath = null;
        if (documents.Any(d => d.MarkdownBody is not null))
        {
            llmsFullTxtPath = Path.Combine(_outputDirectory, "llms-full.txt");
            var llmsFullContent = BuildLlmsFullTxt(siteInfo, documents);
            await File.WriteAllTextAsync(llmsFullTxtPath, llmsFullContent, Utf8NoBom);
        }

        sw.Stop();
        return new LlmsTxtGenerationResult(documents.Count, llmsTxtPath, llmsFullTxtPath, sw.Elapsed);
    }

    /// <summary>
    /// Generates <c>llms.txt</c> (and optionally <c>llms-full.txt</c>) from an explicit list
    /// of documents with the given site info.
    /// </summary>
    /// <param name="siteInfo">The site title and optional description.</param>
    /// <param name="documents">The documents to include in the output.</param>
    /// <returns>A <see cref="LlmsTxtGenerationResult"/> with stats about the generated files.</returns>
    public async Task<LlmsTxtGenerationResult> GenerateAsync(
        LlmsTxtSiteInfo siteInfo,
        IEnumerable<LlmsTxtDocumentInput> documents)
    {
        ArgumentNullException.ThrowIfNull(siteInfo);
        ArgumentNullException.ThrowIfNull(documents);

        var sw = Stopwatch.StartNew();

        var documentList = documents.ToList();

        Directory.CreateDirectory(_outputDirectory);

        var llmsTxtPath = Path.Combine(_outputDirectory, "llms.txt");
        var llmsTxtContent = BuildLlmsTxt(siteInfo, documentList);
        await File.WriteAllTextAsync(llmsTxtPath, llmsTxtContent, Utf8NoBom);

        string? llmsFullTxtPath = null;
        if (documentList.Any(d => d.MarkdownBody is not null))
        {
            llmsFullTxtPath = Path.Combine(_outputDirectory, "llms-full.txt");
            var llmsFullContent = BuildLlmsFullTxt(siteInfo, documentList);
            await File.WriteAllTextAsync(llmsFullTxtPath, llmsFullContent, Utf8NoBom);
        }

        sw.Stop();
        return new LlmsTxtGenerationResult(documentList.Count, llmsTxtPath, llmsFullTxtPath, sw.Elapsed);
    }

    /// <summary>
    /// Builds the <c>llms.txt</c> index content following the llms.txt specification:
    /// H1 title, optional blockquote, H2-grouped link lists.
    /// </summary>
    internal static string BuildLlmsTxt(LlmsTxtSiteInfo siteInfo, IReadOnlyList<LlmsTxtDocumentInput> documents)
    {
        var sb = new StringBuilder();

        // H1 title
        sb.Append("# ").AppendLine(siteInfo.Title);

        // Optional blockquote summary
        if (!string.IsNullOrWhiteSpace(siteInfo.Description))
        {
            sb.AppendLine();
            sb.Append("> ").AppendLine(siteInfo.Description);
        }

        // Group documents by section
        var grouped = GroupBySection(documents);

        foreach (var (section, docs) in grouped)
        {
            sb.AppendLine();
            sb.Append("## ").AppendLine(section);
            sb.AppendLine();

            foreach (var doc in docs)
            {
                sb.Append("- [").Append(doc.Title).Append("](").Append(doc.Href).Append(')');

                if (!string.IsNullOrWhiteSpace(doc.Description))
                {
                    sb.Append(": ").Append(doc.Description);
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds the <c>llms-full.txt</c> content: same header as <c>llms.txt</c> but with each
    /// document's full markdown body inlined below its heading.
    /// </summary>
    internal static string BuildLlmsFullTxt(LlmsTxtSiteInfo siteInfo, IReadOnlyList<LlmsTxtDocumentInput> documents)
    {
        var sb = new StringBuilder();

        // H1 title
        sb.Append("# ").AppendLine(siteInfo.Title);

        // Optional blockquote summary
        if (!string.IsNullOrWhiteSpace(siteInfo.Description))
        {
            sb.AppendLine();
            sb.Append("> ").AppendLine(siteInfo.Description);
        }

        // Each document as a section with its full content
        foreach (var doc in documents)
        {
            sb.AppendLine();
            sb.Append("## ").AppendLine(doc.Title);
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(doc.Description))
            {
                sb.AppendLine(doc.Description);
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(doc.MarkdownBody))
            {
                sb.AppendLine(doc.MarkdownBody.TrimEnd());
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Groups documents by their <see cref="LlmsTxtDocumentInput.Section"/> property.
    /// Documents without a section are placed under a "Documentation" default group.
    /// </summary>
    internal static IReadOnlyList<(string Section, IReadOnlyList<LlmsTxtDocumentInput> Documents)> GroupBySection(
        IReadOnlyList<LlmsTxtDocumentInput> documents)
    {
        const string defaultSection = "Documentation";

        var groups = new List<(string Section, IReadOnlyList<LlmsTxtDocumentInput> Documents)>();
        var sectionOrder = new List<string>();
        var sectionMap = new Dictionary<string, List<LlmsTxtDocumentInput>>(StringComparer.Ordinal);

        foreach (var doc in documents)
        {
            var section = string.IsNullOrWhiteSpace(doc.Section) ? defaultSection : doc.Section;

            if (!sectionMap.TryGetValue(section, out var list))
            {
                list = [];
                sectionMap[section] = list;
                sectionOrder.Add(section);
            }

            list.Add(doc);
        }

        foreach (var section in sectionOrder)
        {
            groups.Add((section, sectionMap[section]));
        }

        return groups;
    }

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
}
