using Atoll.Core.Css;

namespace Atoll.Build.Pipeline;

/// <summary>
/// Processes CSS for static site generation by aggregating component styles,
/// applying scoping, minifying, and optionally fingerprinting the output.
/// </summary>
/// <remarks>
/// <para>
/// The CSS processing pipeline:
/// </para>
/// <list type="number">
/// <item>Collect CSS from component types via <see cref="CssAggregator"/></item>
/// <item>Combine all CSS into a single string</item>
/// <item>Optionally minify via <see cref="CssMinifier"/></item>
/// <item>Optionally rewrite URLs via <see cref="CssUrlRewriter"/></item>
/// <item>Optionally fingerprint the output filename</item>
/// </list>
/// </remarks>
public sealed class CssProcessor
{
    private readonly CssProcessorOptions _options;

    /// <summary>
    /// Initializes a new <see cref="CssProcessor"/> with default options.
    /// </summary>
    public CssProcessor()
        : this(new CssProcessorOptions())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="CssProcessor"/> with the specified options.
    /// </summary>
    /// <param name="options">The processing options.</param>
    public CssProcessor(CssProcessorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Processes CSS from the specified component types.
    /// </summary>
    /// <param name="componentTypes">The component types to extract CSS from.</param>
    /// <returns>A <see cref="CssProcessResult"/> with the processed CSS and metadata.</returns>
    public CssProcessResult Process(IEnumerable<Type> componentTypes)
    {
        ArgumentNullException.ThrowIfNull(componentTypes);

        var aggregator = new CssAggregator();
        foreach (var type in componentTypes)
        {
            aggregator.Add(type);
        }

        return ProcessAggregated(aggregator);
    }

    /// <summary>
    /// Processes CSS from a pre-populated <see cref="CssAggregator"/>.
    /// </summary>
    /// <param name="aggregator">The aggregator containing collected CSS.</param>
    /// <returns>A <see cref="CssProcessResult"/> with the processed CSS and metadata.</returns>
    public CssProcessResult Process(CssAggregator aggregator)
    {
        ArgumentNullException.ThrowIfNull(aggregator);
        return ProcessAggregated(aggregator);
    }

    /// <summary>
    /// Processes a raw CSS string.
    /// </summary>
    /// <param name="css">The CSS text to process.</param>
    /// <returns>A <see cref="CssProcessResult"/> with the processed CSS and metadata.</returns>
    public CssProcessResult Process(string css)
    {
        ArgumentNullException.ThrowIfNull(css);
        return ProcessRawCss(css);
    }

    private CssProcessResult ProcessAggregated(CssAggregator aggregator)
    {
        var css = aggregator.GetCombinedCss();
        if (css.Length == 0)
        {
            return CssProcessResult.Empty;
        }

        return ProcessRawCss(css);
    }

    private CssProcessResult ProcessRawCss(string css)
    {
        if (css.Length == 0)
        {
            return CssProcessResult.Empty;
        }

        var processedCss = css;

        // Rewrite URLs if a base path is configured
        if (_options.BasePath.Length > 0)
        {
            processedCss = CssUrlRewriter.Rewrite(processedCss, _options.BasePath);
        }

        // Minify if enabled
        if (_options.Minify)
        {
            processedCss = CssMinifier.Minify(processedCss);
        }

        // Compute filename (with or without fingerprint)
        var outputFileName = _options.OutputFileName;
        string? hash = null;

        if (_options.Fingerprint)
        {
            hash = AssetFingerprinter.ComputeHash(processedCss);
            outputFileName = AssetFingerprinter.CreateFingerprintedFileName(outputFileName, hash);
        }

        var outputPath = _options.OutputSubdirectory.Length > 0
            ? Path.Combine(_options.OutputSubdirectory, outputFileName)
            : outputFileName;

        return new CssProcessResult(processedCss, outputPath, outputFileName, hash);
    }
}

/// <summary>
/// Configuration options for the <see cref="CssProcessor"/>.
/// </summary>
public sealed class CssProcessorOptions
{
    /// <summary>
    /// Gets or sets whether to minify the CSS output. Defaults to <c>true</c>.
    /// </summary>
    public bool Minify { get; set; } = true;

    /// <summary>
    /// Gets or sets the base path for URL rewriting (e.g., <c>/docs</c>).
    /// An empty string disables URL rewriting. Defaults to empty.
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets whether to fingerprint the output filename. Defaults to <c>true</c>.
    /// </summary>
    public bool Fingerprint { get; set; } = true;

    /// <summary>
    /// Gets or sets the base output filename (before fingerprinting). Defaults to <c>styles.css</c>.
    /// </summary>
    public string OutputFileName { get; set; } = "styles.css";

    /// <summary>
    /// Gets or sets the subdirectory within the output directory to write CSS files to.
    /// Defaults to <c>_astro</c> (following Astro convention).
    /// </summary>
    public string OutputSubdirectory { get; set; } = "_astro";
}

/// <summary>
/// Represents the result of CSS processing.
/// </summary>
public sealed class CssProcessResult
{
    /// <summary>
    /// An empty result representing no CSS output.
    /// </summary>
    public static readonly CssProcessResult Empty = new("", "", "", null);

    /// <summary>
    /// Initializes a new <see cref="CssProcessResult"/>.
    /// </summary>
    /// <param name="css">The processed CSS content.</param>
    /// <param name="outputPath">The relative output path (e.g., <c>_astro/styles.a1b2c3d4.css</c>).</param>
    /// <param name="fileName">The output filename (e.g., <c>styles.a1b2c3d4.css</c>).</param>
    /// <param name="hash">The content hash, or <c>null</c> if fingerprinting was disabled.</param>
    public CssProcessResult(string css, string outputPath, string fileName, string? hash)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(fileName);
        Css = css;
        OutputPath = outputPath;
        FileName = fileName;
        Hash = hash;
    }

    /// <summary>
    /// Gets the processed CSS content.
    /// </summary>
    public string Css { get; }

    /// <summary>
    /// Gets the relative output path including subdirectory (e.g., <c>_astro/styles.a1b2c3d4.css</c>).
    /// </summary>
    public string OutputPath { get; }

    /// <summary>
    /// Gets the output filename (e.g., <c>styles.a1b2c3d4.css</c>).
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the content hash, or <c>null</c> if fingerprinting was disabled.
    /// </summary>
    public string? Hash { get; }

    /// <summary>
    /// Gets whether this result contains any CSS content.
    /// </summary>
    public bool HasContent => Css.Length > 0;
}
