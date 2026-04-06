using System.Diagnostics;
using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Configuration;
using SkiaSharp;

namespace Atoll.Lagoon.OpenGraph;

/// <summary>
/// Generates OpenGraph PNG images for all documentation pages provided by an
/// <see cref="IOgImageConfiguration"/> implementation, writing them to
/// <c>{outputDirectory}/og/{slug}.png</c>.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// var generator = new OgImageGenerator(outputDirectory, projectRoot);
/// var result = await generator.GenerateAsync(query, config, ogConfig);
/// Console.WriteLine($"  OG:     {result.ImageCount} images generated");
/// </code>
/// </remarks>
public sealed class OgImageGenerator
{
    private readonly string _outputDirectory;
    private readonly string _projectRoot;

    /// <summary>
    /// Initializes a new instance of <see cref="OgImageGenerator"/>.
    /// </summary>
    /// <param name="outputDirectory">The SSG output directory where images will be written.</param>
    /// <param name="projectRoot">The project root directory, used to resolve relative asset paths.</param>
    public OgImageGenerator(string outputDirectory, string projectRoot)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        ArgumentNullException.ThrowIfNull(projectRoot);
        _outputDirectory = outputDirectory;
        _projectRoot = projectRoot;
    }

    /// <summary>
    /// Generates OG images for all documents returned by <paramref name="configuration"/>
    /// and writes them to the output directory. Retrieves rendering configuration from
    /// <see cref="IOgImageConfiguration.GetOpenGraphConfig"/>.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <param name="configuration">The OG image configuration providing documents and rendering config.</param>
    /// <returns>A <see cref="OgImageGenerationResult"/> with stats about the generated images.</returns>
    public Task<OgImageGenerationResult> GenerateAsync(
        CollectionQuery query,
        IOgImageConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return GenerateAsync(query, configuration, configuration.GetOpenGraphConfig());
    }

    /// <summary>
    /// Generates OG images for all documents returned by <paramref name="configuration"/>
    /// and writes them to the output directory.
    /// </summary>
    /// <param name="query">The content collection query for accessing content entries.</param>
    /// <param name="configuration">The OG image configuration providing documents to generate images for.</param>
    /// <param name="ogConfig">The OpenGraph rendering configuration (background image, fonts, colors).</param>
    /// <returns>A <see cref="OgImageGenerationResult"/> with stats about the generated images.</returns>
    public async Task<OgImageGenerationResult> GenerateAsync(
        CollectionQuery query,
        IOgImageConfiguration configuration,
        OpenGraphConfig ogConfig)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(ogConfig);

        var sw = Stopwatch.StartNew();

        var renderOptions = BuildRenderOptions(ogConfig);
        using var typeface = renderOptions.Typeface;
        using var renderer = new OgImageRenderer(renderOptions);

        var ogOutputDir = Path.Combine(_outputDirectory, "og");
        Directory.CreateDirectory(ogOutputDir);

        var count = 0;
        foreach (var document in configuration.GetDocuments(query))
        {
            var pngBytes = renderer.Render(document);
            var outputPath = ResolveOutputPath(ogOutputDir, document.Slug);

            // Ensure the subdirectory exists (slug may contain path segments)
            var outputDir = Path.GetDirectoryName(outputPath);
            if (outputDir is not null)
            {
                Directory.CreateDirectory(outputDir);
            }

            await File.WriteAllBytesAsync(outputPath, pngBytes);
            count++;
        }

        sw.Stop();
        return new OgImageGenerationResult(count, ogOutputDir, sw.Elapsed);
    }

    private OgImageRenderOptions BuildRenderOptions(OpenGraphConfig ogConfig)
    {
        byte[]? backgroundBytes = null;
        if (!string.IsNullOrEmpty(ogConfig.BackgroundImagePath))
        {
            var resolvedPath = Path.IsPathRooted(ogConfig.BackgroundImagePath)
                ? ogConfig.BackgroundImagePath
                : Path.GetFullPath(Path.Combine(_projectRoot, ogConfig.BackgroundImagePath));

            if (File.Exists(resolvedPath))
            {
                backgroundBytes = File.ReadAllBytes(resolvedPath);
            }
        }

        SKTypeface? typeface = null;
        if (!string.IsNullOrEmpty(ogConfig.FontPath))
        {
            var resolvedFontPath = Path.IsPathRooted(ogConfig.FontPath)
                ? ogConfig.FontPath
                : Path.GetFullPath(Path.Combine(_projectRoot, ogConfig.FontPath));

            if (File.Exists(resolvedFontPath))
            {
                typeface = SKTypeface.FromFile(resolvedFontPath);
            }
        }

        return new OgImageRenderOptions
        {
            BackgroundImageBytes = backgroundBytes,
            Typeface = typeface,
            TitleFontSize = ogConfig.TitleFontSize,
            DescriptionFontSize = ogConfig.DescriptionFontSize,
            CategoryFontSize = ogConfig.CategoryFontSize,
            TitleColor = ParseHexColor(ogConfig.TitleColor, SKColors.White),
            DescriptionColor = ParseHexColor(ogConfig.DescriptionColor, new SKColor(0xEE, 0xEE, 0xEE)),
            CategoryColor = ParseHexColor(ogConfig.CategoryColor, new SKColor(0xAA, 0xAA, 0xAA)),
            Categories = ogConfig.Categories,
        };
    }

    private static string ResolveOutputPath(string ogOutputDir, string slug)
    {
        // Normalize slug: strip leading slash, ensure .png suffix
        var normalized = slug.TrimStart('/');
        if (!normalized.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".png";
        }

        // Replace forward slashes with directory separators
        var relativePath = normalized.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(ogOutputDir, relativePath);
    }

    private static SKColor ParseHexColor(string hex, SKColor fallback)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return fallback;
        }

        var cleaned = hex.TrimStart('#');

        if (cleaned.Length == 6
            && uint.TryParse(cleaned, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
        {
            var r = (byte)((rgb >> 16) & 0xFF);
            var g = (byte)((rgb >> 8) & 0xFF);
            var b = (byte)(rgb & 0xFF);
            return new SKColor(r, g, b);
        }

        if (cleaned.Length == 8
            && uint.TryParse(cleaned, System.Globalization.NumberStyles.HexNumber, null, out var argb))
        {
            var a = (byte)((argb >> 24) & 0xFF);
            var r = (byte)((argb >> 16) & 0xFF);
            var g = (byte)((argb >> 8) & 0xFF);
            var b = (byte)(argb & 0xFF);
            return new SKColor(r, g, b, a);
        }

        return fallback;
    }
}
