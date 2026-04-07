using Atoll.Build.Ssg;
using Atoll.Configuration;

namespace Atoll.Tests.Build.Ssg;

public sealed class HeadersFileGeneratorTests
{
    // ── Default rules ──

    [Fact]
    public void ShouldContainFingerprintedAssetsRule()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        content.ShouldContain("/_atoll/*");
        content.ShouldContain("Cache-Control: public, max-age=31536000, immutable");
    }

    [Fact]
    public void ShouldContainHtmlRule()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        content.ShouldContain("/*.html");
        content.ShouldContain("Cache-Control: public, max-age=0, must-revalidate");
    }

    [Fact]
    public void ShouldContainRootRule()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        // Root rule path must appear as its own line (platform-agnostic newline check)
        var lines = content.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();
        lines.ShouldContain("/");
    }

    [Fact]
    public void ShouldContainSearchIndexRule()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        content.ShouldContain("/search-index.json");
    }

    // ── Custom rules ──

    [Fact]
    public void ShouldAppendCustomRulesAfterDefaults()
    {
        var config = new AtollCacheConfig
        {
            CustomRules =
            [
                new AtollCacheHeaderRule
                {
                    Path = "/api/*",
                    Headers = new Dictionary<string, string> { ["Cache-Control"] = "no-store" }
                }
            ]
        };
        var generator = new HeadersFileGenerator(config);
        var content = generator.Generate();

        // Custom rule is present
        content.ShouldContain("/api/*");
        content.ShouldContain("Cache-Control: no-store");

        // Default rules still present
        content.ShouldContain("/_atoll/*");
        content.ShouldContain("Cache-Control: public, max-age=31536000, immutable");

        // Custom rule appears after defaults
        var defaultIndex = content.IndexOf("/_atoll/*", StringComparison.Ordinal);
        var customIndex = content.IndexOf("/api/*", StringComparison.Ordinal);
        customIndex.ShouldBeGreaterThan(defaultIndex);
    }

    [Fact]
    public void ShouldSupportMultipleHeadersInCustomRule()
    {
        var config = new AtollCacheConfig
        {
            CustomRules =
            [
                new AtollCacheHeaderRule
                {
                    Path = "/api/*",
                    Headers = new Dictionary<string, string>
                    {
                        ["Cache-Control"] = "no-store",
                        ["X-Custom-Header"] = "value"
                    }
                }
            ]
        };
        var generator = new HeadersFileGenerator(config);
        var content = generator.Generate();

        content.ShouldContain("Cache-Control: no-store");
        content.ShouldContain("X-Custom-Header: value");
    }

    [Fact]
    public void ShouldSkipCustomRuleWithEmptyPath()
    {
        var config = new AtollCacheConfig
        {
            CustomRules =
            [
                new AtollCacheHeaderRule
                {
                    Path = "",
                    Headers = new Dictionary<string, string> { ["Cache-Control"] = "no-store" }
                }
            ]
        };
        var generator = new HeadersFileGenerator(config);
        var content = generator.Generate();

        // no-store must NOT appear (the rule was skipped)
        content.ShouldNotContain("no-store");
    }

    [Fact]
    public void ShouldSkipCustomRuleWithNoHeaders()
    {
        var config = new AtollCacheConfig
        {
            CustomRules =
            [
                new AtollCacheHeaderRule
                {
                    Path = "/api/*",
                    Headers = new Dictionary<string, string>()
                }
            ]
        };
        var generator = new HeadersFileGenerator(config);
        var content = generator.Generate();

        content.ShouldNotContain("/api/*");
    }

    // ── Format validation ──

    [Fact]
    public void ShouldIndentHeadersWithTwoSpaces()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        // Every Cache-Control header line should be indented with exactly 2 spaces
        var lines = content.Split('\n');
        var headerLines = lines.Where(l => l.TrimStart().StartsWith("Cache-Control:", StringComparison.Ordinal)).ToList();

        headerLines.ShouldNotBeEmpty();
        foreach (var line in headerLines)
        {
            line.ShouldStartWith("  Cache-Control:");
        }
    }

    [Fact]
    public void ShouldSeparateRulesWithBlankLines()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        // Content should contain blank lines between rules (platform-agnostic)
        var normalized = content.Replace("\r\n", "\n");
        normalized.ShouldContain("\n\n");
    }

    // ── File writing ──

    [Fact]
    public async Task ShouldWriteHeadersFileToOutputDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "atoll-headers-test-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDir);
            var generator = new HeadersFileGenerator();

            await generator.WriteAsync(tempDir);

            var headersPath = Path.Combine(tempDir, "_headers");
            File.Exists(headersPath).ShouldBeTrue();

            var content = await File.ReadAllTextAsync(headersPath);
            content.ShouldContain("/_atoll/*");
            content.ShouldContain("Cache-Control: public, max-age=31536000, immutable");
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best-effort */ }
        }
    }

    [Fact]
    public async Task ShouldWriteUtf8EncodedFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "atoll-headers-utf8-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(tempDir);

            var config = new AtollCacheConfig
            {
                CustomRules =
                [
                    new AtollCacheHeaderRule
                    {
                        Path = "/api/*",
                        Headers = new Dictionary<string, string> { ["Cache-Control"] = "no-store" }
                    }
                ]
            };
            var generator = new HeadersFileGenerator(config);

            await generator.WriteAsync(tempDir);

            var bytes = await File.ReadAllBytesAsync(Path.Combine(tempDir, "_headers"));
            // UTF-8 BOM is NOT expected (File.WriteAllTextAsync without BOM)
            bytes.ShouldNotBeEmpty();
            // No UTF-8 BOM (0xEF, 0xBB, 0xBF)
            if (bytes.Length >= 3)
            {
                (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF).ShouldBeFalse();
            }
        }
        finally
        {
            try { Directory.Delete(tempDir, recursive: true); } catch { /* best-effort */ }
        }
    }

    // ── Default config ──

    [Fact]
    public void DefaultConstructorShouldUseEmptyCustomRules()
    {
        var generator = new HeadersFileGenerator();
        var content = generator.Generate();

        // Default output has exactly the 4 default rule paths (lines not starting with space or empty)
        var lines = content.Split('\n')
            .Select(l => l.TrimEnd('\r'))
            .Where(l => l.Length > 0 && !l.StartsWith(' '))
            .ToList();
        lines.Count.ShouldBe(4);
    }

    [Fact]
    public void ShouldThrowWhenConfigIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new HeadersFileGenerator(null!));
    }

    [Fact]
    public void ShouldThrowWhenOutputDirectoryIsNull()
    {
        var generator = new HeadersFileGenerator();
        Should.Throw<ArgumentNullException>(() => generator.WriteAsync(null!));
    }
}
