using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;
using Atoll.Core.Configuration;

namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll build</c> command. Runs the full SSG pipeline:
/// load config → discover routes → render pages → process assets → post-process HTML → write manifest.
/// </summary>
internal sealed class BuildCommandHandler
{
    /// <summary>
    /// Executes the build command.
    /// </summary>
    /// <param name="projectRoot">The project root directory.</param>
    public async Task ExecuteAsync(string projectRoot)
    {
        var startTime = DateTime.UtcNow;
        Console.WriteLine("Atoll — building site...");

        // Load configuration
        var config = await AtollConfigLoader.LoadAsync(projectRoot);
        var outputDir = AtollConfigLoader.ResolveOutputDirectory(config, projectRoot);
        var publicDir = AtollConfigLoader.ResolvePublicDirectory(config, projectRoot);
        var basePath = AtollConfigLoader.NormalizeBasePath(config.Base);
        var basePathForAssets = basePath == "/" ? "" : basePath;

        Console.WriteLine($"  Output: {outputDir}");
        Console.WriteLine($"  Site:   {(config.Site.Length > 0 ? config.Site : "(not configured)")}");
        if (basePath != "/")
        {
            Console.WriteLine($"  Base:   {basePath}");
        }

        // SSG options
        var ssgOptions = new SsgOptions(outputDir)
        {
            BaseUrl = config.Site,
            BasePath = basePathForAssets,
            MaxConcurrency = config.Build.Concurrency,
            CleanOutputDirectory = config.Build.Clean,
        };

        // Discover routes (placeholder: in real usage, routes come from assembly scanning)
        // For now, we render an empty route table — the caller provides routes via library mode.
        var generator = new StaticSiteGenerator(ssgOptions);
        var ssgResult = await generator.GenerateAsync([]);

        // Asset pipeline
        var pipelineOptions = new AssetPipelineOptions(outputDir)
        {
            BasePath = basePathForAssets,
            PublicDirectory = Directory.Exists(publicDir) ? publicDir : "",
            Minify = config.Build.Minify,
            Fingerprint = config.Build.Fingerprint,
        };

        var outputWriter = new OutputWriter(outputDir);
        var pipeline = new AssetPipeline(pipelineOptions, outputWriter);
        var assetResult = await pipeline.RunAsync(Array.Empty<Type>(), Array.Empty<string>());

        // Post-process HTML
        var cssHref = assetResult.Css.HasContent
            ? "/" + assetResult.Css.OutputPath.Replace('\\', '/')
            : "";
        var jsHref = assetResult.Js.HasContent
            ? "/" + assetResult.Js.OutputPath.Replace('\\', '/')
            : "";

        if (cssHref.Length > 0 || jsHref.Length > 0)
        {
            var postProcessorOptions = new HtmlPostProcessorOptions
            {
                CssHref = cssHref,
                JsHref = jsHref,
                BasePath = basePathForAssets,
                RemoveInlineStyles = true,
            };
            var postProcessor = new HtmlPostProcessor(postProcessorOptions);

            foreach (var pageResult in ssgResult.PageResults)
            {
                if (!pageResult.IsSuccess || pageResult.OutputPath.Length == 0)
                {
                    continue;
                }

                var processedHtml = postProcessor.Process(pageResult.Html);
                await File.WriteAllTextAsync(pageResult.OutputPath, processedHtml);
            }
        }

        // Write build manifest
        var manifestWriter = new BuildManifestWriter(outputDir);
        var manifest = BuildManifestWriter.BuildFrom(ssgResult, assetResult, ssgOptions);
        await manifestWriter.WriteAsync(manifest);

        // Report results
        var elapsed = DateTime.UtcNow - startTime;
        Console.WriteLine();
        Console.WriteLine($"  Pages:  {ssgResult.SuccessCount} rendered");
        if (ssgResult.FailureCount > 0)
        {
            Console.WriteLine($"  Errors: {ssgResult.FailureCount} failed");
            foreach (var failure in ssgResult.Failures)
            {
                Console.WriteLine($"    ✗ {failure.Route.UrlPath}: {failure.Error?.Message}");
            }
        }

        if (assetResult.StaticAssets is not null)
        {
            Console.WriteLine($"  Assets: {assetResult.StaticAssets.Count} static files copied");
        }

        Console.WriteLine($"  Time:   {elapsed.TotalMilliseconds:F0}ms");
        Console.WriteLine($"  Done!   {outputDir}");
    }
}
