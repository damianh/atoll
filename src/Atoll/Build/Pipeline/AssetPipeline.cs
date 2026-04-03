using System.Diagnostics;
using Atoll.Build.Ssg;
using Atoll.Css;

namespace Atoll.Build.Pipeline;

/// <summary>
/// Orchestrates the complete asset pipeline for static site generation:
/// CSS processing, JS processing, static asset copying, and asset writing.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline runs as a post-SSG step after pages are rendered:
/// </para>
/// <list type="number">
/// <item>Process CSS from component types (aggregate → scope → minify → fingerprint)</item>
/// <item>Process JS from island scripts (concatenate → minify → fingerprint)</item>
/// <item>Copy static assets from <c>public/</c></item>
/// <item>Write processed CSS/JS to output directory</item>
/// <item>Return an <see cref="AssetPipelineResult"/> with all asset references</item>
/// </list>
/// </remarks>
public sealed class AssetPipeline
{
    private readonly AssetPipelineOptions _options;
    private readonly OutputWriter _outputWriter;

    /// <summary>
    /// Initializes a new <see cref="AssetPipeline"/> with the specified options.
    /// </summary>
    /// <param name="options">The pipeline configuration.</param>
    /// <param name="outputWriter">The output writer for writing processed assets.</param>
    public AssetPipeline(AssetPipelineOptions options, OutputWriter outputWriter)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(outputWriter);
        _options = options;
        _outputWriter = outputWriter;
    }

    /// <summary>
    /// Runs the full asset pipeline: process CSS, JS, and static assets.
    /// </summary>
    /// <param name="componentTypes">The page/component types rendered during SSG (for CSS extraction).</param>
    /// <param name="jsSources">JavaScript source strings to process (island scripts, etc.).</param>
    /// <returns>An <see cref="AssetPipelineResult"/> with all processed asset references.</returns>
    public async Task<AssetPipelineResult> RunAsync(
        IEnumerable<Type> componentTypes,
        IEnumerable<string> jsSources)
    {
        ArgumentNullException.ThrowIfNull(componentTypes);
        ArgumentNullException.ThrowIfNull(jsSources);

        var stopwatch = Stopwatch.StartNew();

        // Process CSS
        var cssResult = ProcessCss(componentTypes);

        // Process JS
        var jsResult = ProcessJs(jsSources);

        // Write processed CSS to output
        if (cssResult.HasContent)
        {
            await _outputWriter.WriteFileAsync(cssResult.OutputPath, cssResult.Css);
        }

        // Write processed JS to output
        if (jsResult.HasContent)
        {
            await _outputWriter.WriteFileAsync(jsResult.OutputPath, jsResult.Js);
        }

        // Copy static assets
        CopyResult? copyResult = null;
        if (_options.PublicDirectory.Length > 0)
        {
            var copier = new StaticAssetCopier(_options.OutputDirectory);
            copyResult = await copier.CopyAsync(_options.PublicDirectory);
        }

        stopwatch.Stop();

        return new AssetPipelineResult(cssResult, jsResult, copyResult, stopwatch.Elapsed);
    }

    /// <summary>
    /// Runs the asset pipeline with CSS from a pre-populated aggregator.
    /// </summary>
    /// <param name="cssAggregator">The pre-populated CSS aggregator.</param>
    /// <param name="jsSources">JavaScript source strings to process.</param>
    /// <returns>An <see cref="AssetPipelineResult"/> with all processed asset references.</returns>
    public async Task<AssetPipelineResult> RunAsync(
        CssAggregator cssAggregator,
        IEnumerable<string> jsSources)
    {
        ArgumentNullException.ThrowIfNull(cssAggregator);
        ArgumentNullException.ThrowIfNull(jsSources);

        var stopwatch = Stopwatch.StartNew();

        // Process CSS
        var cssProcessor = CreateCssProcessor();
        var cssResult = cssProcessor.Process(cssAggregator);

        // Process JS
        var jsResult = ProcessJs(jsSources);

        // Write processed CSS to output
        if (cssResult.HasContent)
        {
            await _outputWriter.WriteFileAsync(cssResult.OutputPath, cssResult.Css);
        }

        // Write processed JS to output
        if (jsResult.HasContent)
        {
            await _outputWriter.WriteFileAsync(jsResult.OutputPath, jsResult.Js);
        }

        // Copy static assets
        CopyResult? copyResult = null;
        if (_options.PublicDirectory.Length > 0)
        {
            var copier = new StaticAssetCopier(_options.OutputDirectory);
            copyResult = await copier.CopyAsync(_options.PublicDirectory);
        }

        stopwatch.Stop();

        return new AssetPipelineResult(cssResult, jsResult, copyResult, stopwatch.Elapsed);
    }

    private CssProcessResult ProcessCss(IEnumerable<Type> componentTypes)
    {
        var processor = CreateCssProcessor();
        return processor.Process(componentTypes);
    }

    private JsProcessResult ProcessJs(IEnumerable<string> jsSources)
    {
        var processor = CreateJsProcessor();
        return processor.Process(jsSources);
    }

    private CssProcessor CreateCssProcessor()
    {
        return new CssProcessor(new CssProcessorOptions
        {
            Minify = _options.Minify,
            BasePath = _options.BasePath,
            Fingerprint = _options.Fingerprint,
            OutputFileName = _options.CssOutputFileName,
            OutputSubdirectory = _options.AssetSubdirectory,
        });
    }

    private JsProcessor CreateJsProcessor()
    {
        return new JsProcessor(new JsProcessorOptions
        {
            Minify = _options.Minify,
            Fingerprint = _options.Fingerprint,
            OutputFileName = _options.JsOutputFileName,
            OutputSubdirectory = _options.AssetSubdirectory,
        });
    }
}

/// <summary>
/// Configuration options for the <see cref="AssetPipeline"/>.
/// </summary>
public sealed class AssetPipelineOptions
{
    /// <summary>
    /// Initializes a new <see cref="AssetPipelineOptions"/> with the specified output directory.
    /// </summary>
    /// <param name="outputDirectory">The root output directory (e.g., <c>dist/</c>).</param>
    public AssetPipelineOptions(string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        OutputDirectory = outputDirectory;
    }

    /// <summary>
    /// Gets the root output directory.
    /// </summary>
    public string OutputDirectory { get; }

    /// <summary>
    /// Gets or sets the base path for URL rewriting. Defaults to empty.
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Gets or sets the public directory containing static assets to copy.
    /// An empty string disables static asset copying.
    /// </summary>
    public string PublicDirectory { get; set; } = "";

    /// <summary>
    /// Gets or sets whether to minify CSS and JS output. Defaults to <c>true</c>.
    /// </summary>
    public bool Minify { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to fingerprint asset filenames. Defaults to <c>true</c>.
    /// </summary>
    public bool Fingerprint { get; set; } = true;

    /// <summary>
    /// Gets or sets the subdirectory for processed assets. Defaults to <c>_astro</c>.
    /// </summary>
    public string AssetSubdirectory { get; set; } = "_astro";

    /// <summary>
    /// Gets or sets the CSS output filename (before fingerprinting). Defaults to <c>styles.css</c>.
    /// </summary>
    public string CssOutputFileName { get; set; } = "styles.css";

    /// <summary>
    /// Gets or sets the JS output filename (before fingerprinting). Defaults to <c>scripts.js</c>.
    /// </summary>
    public string JsOutputFileName { get; set; } = "scripts.js";
}

/// <summary>
/// Represents the result of running the complete asset pipeline.
/// </summary>
public sealed class AssetPipelineResult
{
    /// <summary>
    /// Initializes a new <see cref="AssetPipelineResult"/>.
    /// </summary>
    /// <param name="css">The CSS processing result.</param>
    /// <param name="js">The JS processing result.</param>
    /// <param name="staticAssets">The static asset copy result, or <c>null</c> if no static assets were copied.</param>
    /// <param name="elapsed">The total time for the asset pipeline.</param>
    public AssetPipelineResult(
        CssProcessResult css,
        JsProcessResult js,
        CopyResult? staticAssets,
        TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(js);
        Css = css;
        Js = js;
        StaticAssets = staticAssets;
        Elapsed = elapsed;
    }

    /// <summary>
    /// Gets the CSS processing result.
    /// </summary>
    public CssProcessResult Css { get; }

    /// <summary>
    /// Gets the JS processing result.
    /// </summary>
    public JsProcessResult Js { get; }

    /// <summary>
    /// Gets the static asset copy result, or <c>null</c> if no static assets were copied.
    /// </summary>
    public CopyResult? StaticAssets { get; }

    /// <summary>
    /// Gets the total time for the asset pipeline.
    /// </summary>
    public TimeSpan Elapsed { get; }
}
