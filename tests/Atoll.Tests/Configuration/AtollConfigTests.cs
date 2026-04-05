using System.Text.Json;
using Atoll.Configuration;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Configuration;

public sealed class AtollConfigTests
{
    // ── Default values ──

    [Fact]
    public void ShouldHaveEmptySiteByDefault()
    {
        var config = new AtollConfig();
        config.Site.ShouldBe("");
    }

    [Fact]
    public void ShouldHaveSlashBaseByDefault()
    {
        var config = new AtollConfig();
        config.Base.ShouldBe("/");
    }

    [Fact]
    public void ShouldHaveDistOutDirByDefault()
    {
        var config = new AtollConfig();
        config.OutDir.ShouldBe("dist");
    }

    [Fact]
    public void ShouldHaveSrcPagesSrcDirByDefault()
    {
        var config = new AtollConfig();
        config.SrcDir.ShouldBe("src/pages");
    }

    [Fact]
    public void ShouldHavePublicPublicDirByDefault()
    {
        var config = new AtollConfig();
        config.PublicDir.ShouldBe("public");
    }

    [Fact]
    public void ShouldHaveServerConfigByDefault()
    {
        var config = new AtollConfig();
        config.Server.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldHaveBuildConfigByDefault()
    {
        var config = new AtollConfig();
        config.Build.ShouldNotBeNull();
    }

    // ── Server defaults ──

    [Fact]
    public void ShouldHaveLocalhostHostByDefault()
    {
        var config = new AtollServerConfig();
        config.Host.ShouldBe("localhost");
    }

    [Fact]
    public void ShouldHavePort4321ByDefault()
    {
        var config = new AtollServerConfig();
        config.Port.ShouldBe(4321);
    }

    // ── Build defaults ──

    [Fact]
    public void ShouldHaveMinifyTrueByDefault()
    {
        var config = new AtollBuildConfig();
        config.Minify.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveFingerprintTrueByDefault()
    {
        var config = new AtollBuildConfig();
        config.Fingerprint.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveCleanTrueByDefault()
    {
        var config = new AtollBuildConfig();
        config.Clean.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveConcurrencyNegativeOneByDefault()
    {
        var config = new AtollBuildConfig();
        config.Concurrency.ShouldBe(-1);
    }

    // ── Property setters ──

    [Fact]
    public void ShouldSetSiteProperty()
    {
        var config = new AtollConfig { Site = "https://example.com" };
        config.Site.ShouldBe("https://example.com");
    }

    [Fact]
    public void ShouldSetBaseProperty()
    {
        var config = new AtollConfig { Base = "/docs" };
        config.Base.ShouldBe("/docs");
    }

    [Fact]
    public void ShouldSetOutDirProperty()
    {
        var config = new AtollConfig { OutDir = "build" };
        config.OutDir.ShouldBe("build");
    }

    [Fact]
    public void ShouldSetSrcDirProperty()
    {
        var config = new AtollConfig { SrcDir = "pages" };
        config.SrcDir.ShouldBe("pages");
    }

    [Fact]
    public void ShouldSetPublicDirProperty()
    {
        var config = new AtollConfig { PublicDir = "static" };
        config.PublicDir.ShouldBe("static");
    }

    [Fact]
    public void ShouldSetServerHostProperty()
    {
        var config = new AtollServerConfig { Host = "0.0.0.0" };
        config.Host.ShouldBe("0.0.0.0");
    }

    [Fact]
    public void ShouldSetServerPortProperty()
    {
        var config = new AtollServerConfig { Port = 8080 };
        config.Port.ShouldBe(8080);
    }

    [Fact]
    public void ShouldSetBuildConcurrencyProperty()
    {
        var config = new AtollBuildConfig { Concurrency = 4 };
        config.Concurrency.ShouldBe(4);
    }

    [Fact]
    public void ShouldSetBuildMinifyProperty()
    {
        var config = new AtollBuildConfig { Minify = false };
        config.Minify.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSetBuildFingerprintProperty()
    {
        var config = new AtollBuildConfig { Fingerprint = false };
        config.Fingerprint.ShouldBeFalse();
    }

    [Fact]
    public void ShouldSetBuildCleanProperty()
    {
        var config = new AtollBuildConfig { Clean = false };
        config.Clean.ShouldBeFalse();
    }

    // ── Cache config defaults ──

    [Fact]
    public void ShouldHaveCacheConfigByDefault()
    {
        var config = new AtollBuildConfig();
        config.Cache.ShouldNotBeNull();
    }

    [Fact]
    public void ShouldHaveGenerateHeadersFileTrueByDefault()
    {
        var config = new AtollCacheConfig();
        config.GenerateHeadersFile.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveEmptyCustomRulesByDefault()
    {
        var config = new AtollCacheConfig();
        config.CustomRules.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldSetGenerateHeadersFileProperty()
    {
        var config = new AtollCacheConfig { GenerateHeadersFile = false };
        config.GenerateHeadersFile.ShouldBeFalse();
    }

    [Fact]
    public void ShouldAddCustomRules()
    {
        var config = new AtollCacheConfig();
        config.CustomRules.Add(new AtollCacheHeaderRule
        {
            Path = "/api/*",
            Headers = new Dictionary<string, string> { ["Cache-Control"] = "no-store" }
        });

        config.CustomRules.Count.ShouldBe(1);
        config.CustomRules[0].Path.ShouldBe("/api/*");
        config.CustomRules[0].Headers["Cache-Control"].ShouldBe("no-store");
    }

    [Fact]
    public void ShouldHaveEmptyPathByDefaultInAtollCacheHeaderRule()
    {
        var rule = new AtollCacheHeaderRule();
        rule.Path.ShouldBe("");
        rule.Headers.ShouldBeEmpty();
    }

    // ── Cache config deserialization ──

    [Fact]
    public void ShouldDeserializeCacheConfigWithGenerateHeadersFileFalse()
    {
        var json = """{"build":{"cache":{"generateHeadersFile":false}}}""";
        var config = JsonSerializer.Deserialize<AtollConfig>(json)!;

        config.Build.Cache.GenerateHeadersFile.ShouldBeFalse();
    }

    [Fact]
    public void ShouldDeserializeCacheConfigWithCustomRules()
    {
        var json = """
            {
              "build": {
                "cache": {
                  "generateHeadersFile": true,
                  "customRules": [
                    { "path": "/api/*", "headers": { "Cache-Control": "no-store" } }
                  ]
                }
              }
            }
            """;
        var config = JsonSerializer.Deserialize<AtollConfig>(json)!;

        config.Build.Cache.GenerateHeadersFile.ShouldBeTrue();
        config.Build.Cache.CustomRules.Count.ShouldBe(1);
        config.Build.Cache.CustomRules[0].Path.ShouldBe("/api/*");
        config.Build.Cache.CustomRules[0].Headers["Cache-Control"].ShouldBe("no-store");
    }

    [Fact]
    public void ShouldDeserializeCacheConfigWhenCacheSectionAbsent()
    {
        var json = """{"build":{}}""";
        var config = JsonSerializer.Deserialize<AtollConfig>(json)!;

        // Defaults should apply
        config.Build.Cache.GenerateHeadersFile.ShouldBeTrue();
        config.Build.Cache.CustomRules.ShouldBeEmpty();
    }

    [Fact]
    public void ShouldDeserializeMultipleCustomRules()
    {
        var json = """
            {
              "build": {
                "cache": {
                  "customRules": [
                    { "path": "/api/*", "headers": { "Cache-Control": "no-store" } },
                    { "path": "/static/*", "headers": { "Cache-Control": "public, max-age=86400" } }
                  ]
                }
              }
            }
            """;
        var config = JsonSerializer.Deserialize<AtollConfig>(json)!;

        config.Build.Cache.CustomRules.Count.ShouldBe(2);
        config.Build.Cache.CustomRules[1].Path.ShouldBe("/static/*");
        config.Build.Cache.CustomRules[1].Headers["Cache-Control"].ShouldBe("public, max-age=86400");
    }

    [Fact]
    public void ShouldRoundTripCacheConfigViaSerialization()
    {
        var original = new AtollConfig
        {
            Build =
            {
                Cache =
                {
                    GenerateHeadersFile = false,
                    CustomRules =
                    {
                        new AtollCacheHeaderRule
                        {
                            Path = "/api/*",
                            Headers = new Dictionary<string, string> { ["Cache-Control"] = "no-store" }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<AtollConfig>(json)!;

        restored.Build.Cache.GenerateHeadersFile.ShouldBeFalse();
        restored.Build.Cache.CustomRules.Count.ShouldBe(1);
        restored.Build.Cache.CustomRules[0].Path.ShouldBe("/api/*");
        restored.Build.Cache.CustomRules[0].Headers["Cache-Control"].ShouldBe("no-store");
    }
}
