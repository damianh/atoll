using System.Diagnostics;
using System.Text;
using Atoll.Build.Content.Collections;

namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Generates a Netlify / Cloudflare Pages compatible <c>_redirects</c> file
/// from redirect rules provided by an <see cref="IRedirectConfiguration"/> implementation.
/// </summary>
/// <remarks>
/// The generated file contains one rule per line in the format:
/// <code>
/// /old/url /new/url 301
/// </code>
/// Rules where <see cref="RedirectRule.From"/> or <see cref="RedirectRule.To"/> is null or
/// whitespace are skipped.
/// </remarks>
public sealed class LagoonRedirectGenerator
{
    private readonly string _outputDirectory;

    /// <summary>
    /// Initializes a new instance targeting the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">
    /// The directory in which to write the <c>_redirects</c> file.
    /// </param>
    public LagoonRedirectGenerator(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Enumerates redirect rules from <paramref name="configuration"/>, writes a <c>_redirects</c>
    /// file to the output directory, and returns a result summarising the outcome.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <param name="configuration">The user-provided redirect configuration.</param>
    /// <param name="cancellationToken">A token to cancel the generation operation.</param>
    /// <returns>A <see cref="RedirectGenerationResult"/> describing the generation outcome.</returns>
    public async Task<RedirectGenerationResult> GenerateAsync(
        CollectionQuery query,
        IRedirectConfiguration configuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configuration);

        var stopwatch = Stopwatch.StartNew();

        var rules = new List<RedirectRule>();
        foreach (var rule in configuration.GetRedirects(query))
        {
            if (string.IsNullOrWhiteSpace(rule.From) || string.IsNullOrWhiteSpace(rule.To))
            {
                continue;
            }

            rules.Add(rule);
        }

        await WriteRedirectsFileAsync(rules, cancellationToken);

        stopwatch.Stop();
        return new RedirectGenerationResult(rules.Count, stopwatch.Elapsed);
    }

    private async Task WriteRedirectsFileAsync(IReadOnlyList<RedirectRule> rules, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        foreach (var rule in rules)
        {
            sb.Append(rule.From).Append(' ').Append(rule.To).Append(' ').AppendLine(rule.StatusCode.ToString());
        }

        var outputPath = Path.Combine(_outputDirectory, "_redirects");
        await File.WriteAllTextAsync(outputPath, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);
    }
}
