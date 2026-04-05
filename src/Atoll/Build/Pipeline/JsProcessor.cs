using NUglify;

namespace Atoll.Build.Pipeline;

/// <summary>
/// Processes JavaScript for static site generation by minifying and optionally
/// fingerprinting the output. Full bundling via esbuild is deferred to Story 48.
/// </summary>
/// <remarks>
/// <para>
/// The JS processing pipeline:
/// </para>
/// <list type="number">
/// <item>Accept one or more JS source strings</item>
/// <item>Concatenate if multiple sources</item>
/// <item>Optionally minify via NUglify</item>
/// <item>Optionally fingerprint the output filename</item>
/// </list>
/// </remarks>
public sealed class JsProcessor
{
    private readonly JsProcessorOptions _options;

    /// <summary>
    /// Initializes a new <see cref="JsProcessor"/> with default options.
    /// </summary>
    public JsProcessor()
        : this(new JsProcessorOptions())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="JsProcessor"/> with the specified options.
    /// </summary>
    /// <param name="options">The processing options.</param>
    public JsProcessor(JsProcessorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>
    /// Processes a single JS source string.
    /// </summary>
    /// <param name="js">The JavaScript source code.</param>
    /// <returns>A <see cref="JsProcessResult"/> with the processed JS and metadata.</returns>
    public JsProcessResult Process(string js)
    {
        ArgumentNullException.ThrowIfNull(js);
        return ProcessInternal(js);
    }

    /// <summary>
    /// Processes multiple JS source strings by concatenating them.
    /// </summary>
    /// <param name="sources">The JavaScript source strings to concatenate and process.</param>
    /// <returns>A <see cref="JsProcessResult"/> with the processed JS and metadata.</returns>
    public JsProcessResult Process(IEnumerable<string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var combined = string.Join("\n", sources);
        return ProcessInternal(combined);
    }

    /// <summary>
    /// Processes a named JS source with a specific output filename.
    /// </summary>
    /// <param name="js">The JavaScript source code.</param>
    /// <param name="outputFileName">The desired output filename (overrides the default).</param>
    /// <returns>A <see cref="JsProcessResult"/> with the processed JS and metadata.</returns>
    public JsProcessResult Process(string js, string outputFileName)
    {
        ArgumentNullException.ThrowIfNull(js);
        ArgumentNullException.ThrowIfNull(outputFileName);
        return ProcessInternal(js, outputFileName);
    }

    private JsProcessResult ProcessInternal(string js)
    {
        return ProcessInternal(js, _options.OutputFileName);
    }

    private JsProcessResult ProcessInternal(string js, string outputFileName)
    {
        if (js.Length == 0)
        {
            return JsProcessResult.Empty;
        }

        var processedJs = js;

        // Minify if enabled
        if (_options.Minify)
        {
            var result = Uglify.Js(processedJs);
            if (!result.HasErrors)
            {
                processedJs = result.Code;
            }
            // If minification fails, keep the original JS
        }

        // Compute filename (with or without fingerprint)
        string? hash = null;

        if (_options.Fingerprint)
        {
            hash = AssetFingerprinter.ComputeHash(processedJs);
            outputFileName = AssetFingerprinter.CreateFingerprintedFileName(outputFileName, hash);
        }

        var outputPath = _options.OutputSubdirectory.Length > 0
            ? Path.Combine(_options.OutputSubdirectory, outputFileName)
            : outputFileName;

        return new JsProcessResult(processedJs, outputPath, outputFileName, hash);
    }
}

/// <summary>
/// Configuration options for the <see cref="JsProcessor"/>.
/// </summary>
public sealed class JsProcessorOptions
{
    /// <summary>
    /// Gets or sets whether to minify the JS output. Defaults to <c>true</c>.
    /// </summary>
    public bool Minify { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to fingerprint the output filename. Defaults to <c>true</c>.
    /// </summary>
    public bool Fingerprint { get; set; } = true;

    /// <summary>
    /// Gets or sets the base output filename (before fingerprinting). Defaults to <c>scripts.js</c>.
    /// </summary>
    public string OutputFileName { get; set; } = "scripts.js";

    /// <summary>
    /// Gets or sets the subdirectory within the output directory to write JS files to.
    /// Defaults to <c>_atoll</c>.
    /// </summary>
    public string OutputSubdirectory { get; set; } = "_atoll";
}

/// <summary>
/// Represents the result of JS processing.
/// </summary>
public sealed class JsProcessResult
{
    /// <summary>
    /// An empty result representing no JS output.
    /// </summary>
    public static readonly JsProcessResult Empty = new("", "", "", null);

    /// <summary>
    /// Initializes a new <see cref="JsProcessResult"/>.
    /// </summary>
    /// <param name="js">The processed JS content.</param>
    /// <param name="outputPath">The relative output path (e.g., <c>_atoll/scripts.a1b2c3d4.js</c>).</param>
    /// <param name="fileName">The output filename (e.g., <c>scripts.a1b2c3d4.js</c>).</param>
    /// <param name="hash">The content hash, or <c>null</c> if fingerprinting was disabled.</param>
    public JsProcessResult(string js, string outputPath, string fileName, string? hash)
    {
        ArgumentNullException.ThrowIfNull(js);
        ArgumentNullException.ThrowIfNull(outputPath);
        ArgumentNullException.ThrowIfNull(fileName);
        Js = js;
        OutputPath = outputPath;
        FileName = fileName;
        Hash = hash;
    }

    /// <summary>
    /// Gets the processed JS content.
    /// </summary>
    public string Js { get; }

    /// <summary>
    /// Gets the relative output path including subdirectory (e.g., <c>_atoll/scripts.a1b2c3d4.js</c>).
    /// </summary>
    public string OutputPath { get; }

    /// <summary>
    /// Gets the output filename (e.g., <c>scripts.a1b2c3d4.js</c>).
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the content hash, or <c>null</c> if fingerprinting was disabled.
    /// </summary>
    public string? Hash { get; }

    /// <summary>
    /// Gets whether this result contains any JS content.
    /// </summary>
    public bool HasContent => Js.Length > 0;
}
