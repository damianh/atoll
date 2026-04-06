using System.Text.Json;
using Atoll.Configuration;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Configuration;

public sealed class AtollConfigTests
{
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
