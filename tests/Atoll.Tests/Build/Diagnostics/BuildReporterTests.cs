using Atoll.Build.Diagnostics;
using Atoll.Build.Pipeline;
using Atoll.Build.Ssg;

namespace Atoll.Build.Tests.Diagnostics;

public sealed class BuildReporterTests
{
    // ── Helper factories ─────────────────────────────────────────────

    private static SsgRoute CreateRoute(string urlPath)
    {
        return new SsgRoute(urlPath, typeof(object));
    }

    private static SsgPageResult CreateSuccessPage(string urlPath, string html, TimeSpan elapsed)
    {
        return new SsgPageResult(
            CreateRoute(urlPath),
            "dist" + urlPath + "/index.html",
            html,
            elapsed);
    }

    private static SsgPageResult CreateFailedPage(string urlPath, string errorMessage, TimeSpan elapsed)
    {
        return new SsgPageResult(
            CreateRoute(urlPath),
            new InvalidOperationException(errorMessage),
            elapsed);
    }

    private static SsgResult CreateSsgResult(IReadOnlyList<SsgPageResult> pages, TimeSpan totalElapsed)
    {
        return new SsgResult(pages, totalElapsed);
    }

    private static AssetPipelineResult CreateAssetResult(
        CssProcessResult css,
        JsProcessResult js,
        CopyResult? staticAssets,
        TimeSpan elapsed)
    {
        return new AssetPipelineResult(css, js, staticAssets, elapsed);
    }

    private static AssetPipelineResult CreateEmptyAssetResult()
    {
        return CreateAssetResult(
            CssProcessResult.Empty,
            JsProcessResult.Empty,
            null,
            TimeSpan.FromMilliseconds(10));
    }

    private static AssetPipelineResult CreateFullAssetResult()
    {
        var css = new CssProcessResult(
            "body{color:red}",
            "_atoll/styles.abc123.css",
            "styles.abc123.css",
            "abc123");
        var js = new JsProcessResult(
            "console.log('hi')",
            "_atoll/scripts.def456.js",
            "scripts.def456.js",
            "def456");
        var copyResult = new CopyResult(
        [
            new CopiedFile("favicon.ico", "dist/favicon.ico", 1024),
            new CopiedFile("robots.txt", "dist/robots.txt", 50),
        ]);
        return CreateAssetResult(css, js, copyResult, TimeSpan.FromMilliseconds(150));
    }

    // ── Empty reporter ───────────────────────────────────────────────

    [Fact]
    public void NewReporterShouldHaveNoDiagnostics()
    {
        var reporter = new BuildReporter();

        reporter.Diagnostics.Count.ShouldBe(0);
        reporter.WarningCount.ShouldBe(0);
        reporter.ErrorCount.ShouldBe(0);
        reporter.HasWarnings.ShouldBeFalse();
        reporter.HasErrors.ShouldBeFalse();
    }

    // ── Info methods ──────────────────────────────────────────────────

    [Fact]
    public void InfoShouldAddInfoDiagnostic()
    {
        var reporter = new BuildReporter();

        reporter.Info("Build started");

        reporter.Diagnostics.Count.ShouldBe(1);
        reporter.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Info);
        reporter.Diagnostics[0].Message.ShouldBe("Build started");
        reporter.Diagnostics[0].Source.ShouldBeNull();
    }

    [Fact]
    public void InfoWithSourceShouldAddInfoDiagnosticWithSource()
    {
        var reporter = new BuildReporter();

        reporter.Info("Page rendered", "/about");

        reporter.Diagnostics.Count.ShouldBe(1);
        reporter.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Info);
        reporter.Diagnostics[0].Message.ShouldBe("Page rendered");
        reporter.Diagnostics[0].Source.ShouldBe("/about");
    }

    [Fact]
    public void InfoShouldThrowOnNullMessage()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Info(null!));
    }

    [Fact]
    public void InfoWithSourceShouldThrowOnNullMessage()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Info(null!, "/page"));
    }

    [Fact]
    public void InfoWithSourceShouldThrowOnNullSource()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Info("msg", null!));
    }

    // ── Warn methods ─────────────────────────────────────────────────

    [Fact]
    public void WarnShouldAddWarningDiagnostic()
    {
        var reporter = new BuildReporter();

        reporter.Warn("Empty output");

        reporter.Diagnostics.Count.ShouldBe(1);
        reporter.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Warning);
        reporter.WarningCount.ShouldBe(1);
        reporter.HasWarnings.ShouldBeTrue();
        reporter.HasErrors.ShouldBeFalse();
    }

    [Fact]
    public void WarnWithSourceShouldAddWarningDiagnosticWithSource()
    {
        var reporter = new BuildReporter();

        reporter.Warn("Empty output", "/empty");

        reporter.Diagnostics[0].Source.ShouldBe("/empty");
        reporter.WarningCount.ShouldBe(1);
    }

    [Fact]
    public void WarnShouldThrowOnNullMessage()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Warn(null!));
    }

    [Fact]
    public void WarnWithSourceShouldThrowOnNullMessage()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Warn(null!, "/page"));
    }

    [Fact]
    public void WarnWithSourceShouldThrowOnNullSource()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Warn("msg", null!));
    }

    // ── Error methods ────────────────────────────────────────────────

    [Fact]
    public void ErrorShouldAddErrorDiagnostic()
    {
        var reporter = new BuildReporter();

        reporter.Error("Render failed");

        reporter.Diagnostics.Count.ShouldBe(1);
        reporter.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Error);
        reporter.ErrorCount.ShouldBe(1);
        reporter.HasErrors.ShouldBeTrue();
        reporter.HasWarnings.ShouldBeFalse();
    }

    [Fact]
    public void ErrorWithSourceShouldAddErrorDiagnosticWithSource()
    {
        var reporter = new BuildReporter();

        reporter.Error("Render failed", "/broken");

        reporter.Diagnostics[0].Source.ShouldBe("/broken");
        reporter.ErrorCount.ShouldBe(1);
    }

    [Fact]
    public void ErrorShouldThrowOnNullMessage()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Error(null!));
    }

    [Fact]
    public void ErrorWithSourceShouldThrowOnNullMessage()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Error(null!, "/page"));
    }

    [Fact]
    public void ErrorWithSourceShouldThrowOnNullSource()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.Error("msg", null!));
    }

    // ── Mixed diagnostics counting ──────────────────────────────────

    [Fact]
    public void ShouldCountMixedDiagnosticsCorrectly()
    {
        var reporter = new BuildReporter();

        reporter.Info("Starting");
        reporter.Warn("Empty page");
        reporter.Warn("Missing asset");
        reporter.Error("Render crash");
        reporter.Info("Done");

        reporter.Diagnostics.Count.ShouldBe(5);
        reporter.WarningCount.ShouldBe(2);
        reporter.ErrorCount.ShouldBe(1);
        reporter.HasWarnings.ShouldBeTrue();
        reporter.HasErrors.ShouldBeTrue();
    }

    // ── CollectFromSsgResult ─────────────────────────────────────────

    [Fact]
    public void CollectFromSsgResultShouldReportErrorsForFailedPages()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
            CreateFailedPage("/broken", "Component threw", TimeSpan.FromMilliseconds(10)),
            CreateFailedPage("/also-broken", "Null reference", TimeSpan.FromMilliseconds(5)),
        ],
        TimeSpan.FromMilliseconds(100));

        reporter.CollectFromSsgResult(ssgResult);

        reporter.ErrorCount.ShouldBe(2);
        reporter.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .All(d => d.Source is not null)
            .ShouldBeTrue();
    }

    [Fact]
    public void CollectFromSsgResultShouldReportWarningForEmptyContent()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
            new SsgPageResult(CreateRoute("/empty"), "dist/empty/index.html", "", TimeSpan.FromMilliseconds(5)),
        ],
        TimeSpan.FromMilliseconds(60));

        reporter.CollectFromSsgResult(ssgResult);

        reporter.WarningCount.ShouldBe(1);
        reporter.Diagnostics[0].Message.ShouldContain("empty content");
        reporter.Diagnostics[0].Source.ShouldBe("/empty");
    }

    [Fact]
    public void CollectFromSsgResultShouldNotReportAnythingForAllSuccess()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
            CreateSuccessPage("/about", "<html>about</html>", TimeSpan.FromMilliseconds(30)),
        ],
        TimeSpan.FromMilliseconds(80));

        reporter.CollectFromSsgResult(ssgResult);

        reporter.Diagnostics.Count.ShouldBe(0);
    }

    [Fact]
    public void CollectFromSsgResultShouldIncludeErrorMessageFromException()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateFailedPage("/fail", "Something went wrong", TimeSpan.FromMilliseconds(10)),
        ],
        TimeSpan.FromMilliseconds(10));

        reporter.CollectFromSsgResult(ssgResult);

        reporter.Diagnostics[0].Message.ShouldContain("Something went wrong");
    }

    [Fact]
    public void CollectFromSsgResultShouldHandleMixedSuccessFailureAndEmpty()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
            CreateFailedPage("/broken", "fail", TimeSpan.FromMilliseconds(10)),
            new SsgPageResult(CreateRoute("/empty"), "dist/empty/index.html", "", TimeSpan.FromMilliseconds(5)),
        ],
        TimeSpan.FromMilliseconds(65));

        reporter.CollectFromSsgResult(ssgResult);

        reporter.ErrorCount.ShouldBe(1);
        reporter.WarningCount.ShouldBe(1);
    }

    [Fact]
    public void CollectFromSsgResultShouldThrowOnNull()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.CollectFromSsgResult(null!));
    }

    // ── CollectFromAssetResult ───────────────────────────────────────

    [Fact]
    public void CollectFromAssetResultShouldReportInfoWhenNoAssets()
    {
        var reporter = new BuildReporter();
        var assetResult = CreateEmptyAssetResult();

        reporter.CollectFromAssetResult(assetResult);

        reporter.Diagnostics.Count.ShouldBe(1);
        reporter.Diagnostics[0].Severity.ShouldBe(DiagnosticSeverity.Info);
        reporter.Diagnostics[0].Message.ShouldContain("No CSS or JS");
    }

    [Fact]
    public void CollectFromAssetResultShouldNotReportWhenCssPresent()
    {
        var reporter = new BuildReporter();
        var cssResult = new CssProcessResult("body{}", "_atoll/styles.css", "styles.css", null);
        var assetResult = CreateAssetResult(cssResult, JsProcessResult.Empty, null, TimeSpan.FromMilliseconds(10));

        reporter.CollectFromAssetResult(assetResult);

        reporter.Diagnostics.Count.ShouldBe(0);
    }

    [Fact]
    public void CollectFromAssetResultShouldNotReportWhenJsPresent()
    {
        var reporter = new BuildReporter();
        var jsResult = new JsProcessResult("var x=1;", "_atoll/scripts.js", "scripts.js", null);
        var assetResult = CreateAssetResult(CssProcessResult.Empty, jsResult, null, TimeSpan.FromMilliseconds(10));

        reporter.CollectFromAssetResult(assetResult);

        reporter.Diagnostics.Count.ShouldBe(0);
    }

    [Fact]
    public void CollectFromAssetResultShouldNotReportWhenBothCssAndJsPresent()
    {
        var reporter = new BuildReporter();
        var assetResult = CreateFullAssetResult();

        reporter.CollectFromAssetResult(assetResult);

        reporter.Diagnostics.Count.ShouldBe(0);
    }

    [Fact]
    public void CollectFromAssetResultShouldThrowOnNull()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(() => reporter.CollectFromAssetResult(null!));
    }

    // ── FormatSummary ────────────────────────────────────────────────

    [Fact]
    public void FormatSummaryShouldIncludeBuildSummaryHeader()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult([], TimeSpan.Zero);
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.Zero);

        summary.ShouldContain("Build Summary");
    }

    [Fact]
    public void FormatSummaryShouldIncludePageCounts()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
            CreateSuccessPage("/about", "<html>about</html>", TimeSpan.FromMilliseconds(30)),
        ],
        TimeSpan.FromMilliseconds(80));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(100));

        summary.ShouldContain("2 rendered (2 total)");
    }

    [Fact]
    public void FormatSummaryShouldIncludeFailureCount()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
            CreateFailedPage("/fail", "Error", TimeSpan.FromMilliseconds(10)),
        ],
        TimeSpan.FromMilliseconds(60));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(70));

        summary.ShouldContain("1 rendered (2 total)");
        summary.ShouldContain("Failures: 1");
    }

    [Fact]
    public void FormatSummaryShouldNotIncludeFailureSectionWhenAllSucceed()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldNotContain("Failures:");
    }

    [Fact]
    public void FormatSummaryShouldIncludeRenderTimingStatistics()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(20)),
            CreateSuccessPage("/about", "<html>about</html>", TimeSpan.FromMilliseconds(40)),
            CreateSuccessPage("/contact", "<html>contact</html>", TimeSpan.FromMilliseconds(60)),
        ],
        TimeSpan.FromMilliseconds(120));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(150));

        summary.ShouldContain("Render:");
        summary.ShouldContain("avg");
        summary.ShouldContain("min");
        summary.ShouldContain("max");
    }

    [Fact]
    public void FormatSummaryShouldIncludeCssAssetInfo()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateFullAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(200));

        summary.ShouldContain("CSS:");
        summary.ShouldContain("styles.abc123.css");
    }

    [Fact]
    public void FormatSummaryShouldIncludeJsAssetInfo()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateFullAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(200));

        summary.ShouldContain("JS:");
        summary.ShouldContain("scripts.def456.js");
    }

    [Fact]
    public void FormatSummaryShouldIncludeStaticAssetCount()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateFullAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(200));

        summary.ShouldContain("Static:");
        summary.ShouldContain("2 files copied");
    }

    [Fact]
    public void FormatSummaryShouldIncludeTotalHtmlSize()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldContain("HTML:");
    }

    [Fact]
    public void FormatSummaryShouldIncludeTimingBreakdown()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(80));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(100));

        summary.ShouldContain("SSG:");
        summary.ShouldContain("Assets:");
        summary.ShouldContain("Total:");
    }

    [Fact]
    public void FormatSummaryShouldIncludeWarningsSection()
    {
        var reporter = new BuildReporter();
        reporter.Warn("Empty page", "/empty");
        reporter.Warn("Unused content");

        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldContain("Warnings:");
        summary.ShouldContain("WARN");
        summary.ShouldContain("Empty page");
        summary.ShouldContain("[/empty]");
        summary.ShouldContain("Unused content");
    }

    [Fact]
    public void FormatSummaryShouldIncludeErrorsSection()
    {
        var reporter = new BuildReporter();
        reporter.Error("Render crashed", "/broken");

        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldContain("Errors:");
        summary.ShouldContain("ERR");
        summary.ShouldContain("Render crashed");
        summary.ShouldContain("[/broken]");
    }

    [Fact]
    public void FormatSummaryShouldNotIncludeWarningSectionWhenNoWarnings()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldNotContain("Warnings:");
    }

    [Fact]
    public void FormatSummaryShouldNotIncludeErrorSectionWhenNoErrors()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldNotContain("Errors:");
    }

    [Fact]
    public void FormatSummaryShouldNotShowStaticSectionWhenNoStaticAssets()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldNotContain("Static:");
    }

    [Fact]
    public void FormatSummaryShouldNotShowCssSectionWhenNoCss()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldNotContain("CSS:");
    }

    [Fact]
    public void FormatSummaryShouldNotShowJsSectionWhenNoJs()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(50));
        var assetResult = CreateEmptyAssetResult();

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(60));

        summary.ShouldNotContain("JS:");
    }

    [Fact]
    public void FormatSummaryShouldThrowOnNullSsgResult()
    {
        var reporter = new BuildReporter();
        Should.Throw<ArgumentNullException>(
            () => reporter.FormatSummary(null!, CreateEmptyAssetResult(), TimeSpan.Zero));
    }

    [Fact]
    public void FormatSummaryShouldThrowOnNullAssetResult()
    {
        var reporter = new BuildReporter();
        var ssgResult = CreateSsgResult([], TimeSpan.Zero);
        Should.Throw<ArgumentNullException>(
            () => reporter.FormatSummary(ssgResult, null!, TimeSpan.Zero));
    }

    // ── FormatPageTimings ────────────────────────────────────────────

    [Fact]
    public void FormatPageTimingsShouldListSlowestPagesFirst()
    {
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/fast", "<html>fast</html>", TimeSpan.FromMilliseconds(10)),
            CreateSuccessPage("/slow", "<html>slow</html>", TimeSpan.FromMilliseconds(100)),
            CreateSuccessPage("/medium", "<html>medium</html>", TimeSpan.FromMilliseconds(50)),
        ],
        TimeSpan.FromMilliseconds(160));

        var timings = BuildReporter.FormatPageTimings(ssgResult);

        timings.ShouldContain("Page Timings");
        var slowIdx = timings.IndexOf("/slow", StringComparison.Ordinal);
        var medIdx = timings.IndexOf("/medium", StringComparison.Ordinal);
        var fastIdx = timings.IndexOf("/fast", StringComparison.Ordinal);
        slowIdx.ShouldBeLessThan(medIdx);
        medIdx.ShouldBeLessThan(fastIdx);
    }

    [Fact]
    public void FormatPageTimingsShouldRespectMaxPages()
    {
        var pages = Enumerable.Range(0, 20)
            .Select(i => CreateSuccessPage(
                "/page-" + i,
                "<html>page " + i + "</html>",
                TimeSpan.FromMilliseconds(i * 10)))
            .ToList();
        var ssgResult = CreateSsgResult(pages, TimeSpan.FromMilliseconds(1000));

        var timings = BuildReporter.FormatPageTimings(ssgResult, 5);

        var pageLines = timings.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.Contains("/page-", StringComparison.Ordinal))
            .ToList();
        pageLines.Count.ShouldBe(5);
    }

    [Fact]
    public void FormatPageTimingsDefaultOverloadShouldUseMaxTen()
    {
        var pages = Enumerable.Range(0, 15)
            .Select(i => CreateSuccessPage(
                "/page-" + i,
                "<html>page " + i + "</html>",
                TimeSpan.FromMilliseconds(i * 10)))
            .ToList();
        var ssgResult = CreateSsgResult(pages, TimeSpan.FromMilliseconds(1000));

        var timings = BuildReporter.FormatPageTimings(ssgResult);

        var pageLines = timings.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.Contains("/page-", StringComparison.Ordinal))
            .ToList();
        pageLines.Count.ShouldBe(10);
    }

    [Fact]
    public void FormatPageTimingsShouldReturnEmptyForNoSuccessPages()
    {
        var ssgResult = CreateSsgResult(
        [
            CreateFailedPage("/fail", "Error", TimeSpan.FromMilliseconds(10)),
        ],
        TimeSpan.FromMilliseconds(10));

        var timings = BuildReporter.FormatPageTimings(ssgResult);

        timings.ShouldBe("");
    }

    [Fact]
    public void FormatPageTimingsShouldReturnEmptyForEmptyResult()
    {
        var ssgResult = CreateSsgResult([], TimeSpan.Zero);

        var timings = BuildReporter.FormatPageTimings(ssgResult);

        timings.ShouldBe("");
    }

    [Fact]
    public void FormatPageTimingsShouldSkipFailedPages()
    {
        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/ok", "<html>ok</html>", TimeSpan.FromMilliseconds(50)),
            CreateFailedPage("/fail", "Error", TimeSpan.FromMilliseconds(10)),
        ],
        TimeSpan.FromMilliseconds(60));

        var timings = BuildReporter.FormatPageTimings(ssgResult);

        timings.ShouldContain("/ok");
        timings.ShouldNotContain("/fail");
    }

    [Fact]
    public void FormatPageTimingsShouldThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => BuildReporter.FormatPageTimings(null!));
    }

    [Fact]
    public void FormatPageTimingsWithMaxPagesShouldThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => BuildReporter.FormatPageTimings(null!, 5));
    }

    // ── FormatByteSize ──────────────────────────────────────────────

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1, "1 B")]
    [InlineData(512, "512 B")]
    [InlineData(1023, "1023 B")]
    public void FormatByteSizeShouldReturnBytesForSmallValues(long bytes, string expected)
    {
        BuildReporter.FormatByteSize(bytes).ShouldBe(expected);
    }

    [Theory]
    [InlineData(1024, "1.0 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(10240, "10.0 KB")]
    [InlineData(1048575, "1024.0 KB")]
    public void FormatByteSizeShouldReturnKilobytesForMediumValues(long bytes, string expected)
    {
        BuildReporter.FormatByteSize(bytes).ShouldBe(expected);
    }

    [Theory]
    [InlineData(1048576, "1.00 MB")]
    [InlineData(1572864, "1.50 MB")]
    [InlineData(10485760, "10.00 MB")]
    public void FormatByteSizeShouldReturnMegabytesForLargeValues(long bytes, string expected)
    {
        BuildReporter.FormatByteSize(bytes).ShouldBe(expected);
    }

    // ── Integration: Collect + Format ────────────────────────────────

    [Fact]
    public void ShouldProduceCompleteSummaryWithCollectedDiagnostics()
    {
        var reporter = new BuildReporter();

        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(40)),
            CreateSuccessPage("/about", "<html>about</html>", TimeSpan.FromMilliseconds(20)),
            CreateFailedPage("/broken", "NullReferenceException", TimeSpan.FromMilliseconds(5)),
        ],
        TimeSpan.FromMilliseconds(200));
        var assetResult = CreateFullAssetResult();

        reporter.CollectFromSsgResult(ssgResult);
        reporter.CollectFromAssetResult(assetResult);

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(400));

        summary.ShouldSatisfyAllConditions(
            () => summary.ShouldContain("Build Summary"),
            () => summary.ShouldContain("2 rendered (3 total)"),
            () => summary.ShouldContain("Failures: 1"),
            () => summary.ShouldContain("CSS:"),
            () => summary.ShouldContain("JS:"),
            () => summary.ShouldContain("Static:"),
            () => summary.ShouldContain("Errors:"),
            () => summary.ShouldContain("NullReferenceException"),
            () => summary.ShouldContain("[/broken]")
        );
    }

    [Fact]
    public void ShouldProduceCleanSummaryWhenEverythingSucceeds()
    {
        var reporter = new BuildReporter();

        var ssgResult = CreateSsgResult(
        [
            CreateSuccessPage("/", "<html>home</html>", TimeSpan.FromMilliseconds(30)),
            CreateSuccessPage("/about", "<html>about</html>", TimeSpan.FromMilliseconds(20)),
        ],
        TimeSpan.FromMilliseconds(100));
        var assetResult = CreateFullAssetResult();

        reporter.CollectFromSsgResult(ssgResult);
        reporter.CollectFromAssetResult(assetResult);

        var summary = reporter.FormatSummary(ssgResult, assetResult, TimeSpan.FromMilliseconds(300));

        summary.ShouldSatisfyAllConditions(
            () => summary.ShouldContain("Build Summary"),
            () => summary.ShouldContain("2 rendered (2 total)"),
            () => summary.ShouldNotContain("Failures:"),
            () => summary.ShouldNotContain("Warnings:"),
            () => summary.ShouldNotContain("Errors:")
        );
    }
}
