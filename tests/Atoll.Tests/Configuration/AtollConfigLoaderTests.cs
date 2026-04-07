using Atoll.Configuration;

namespace Atoll.Tests.Configuration;

public sealed class AtollConfigLoaderTests
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    // ── Deserialization ──

    [Fact]
    public void ShouldDeserializeFullConfig()
    {
        var json = """
            {
              "site": "https://example.com",
              "base": "/docs",
              "outDir": "build",
              "srcDir": "pages",
              "publicDir": "static",
              "server": { "host": "0.0.0.0", "port": 8080 },
              "build": { "minify": false, "fingerprint": false, "clean": false, "concurrency": 4 }
            }
            """;

        var config = AtollConfigLoader.Deserialize(json);

        config.Site.ShouldBe("https://example.com");
        config.Base.ShouldBe("/docs");
        config.OutDir.ShouldBe("build");
        config.SrcDir.ShouldBe("pages");
        config.PublicDir.ShouldBe("static");
        config.Server.Host.ShouldBe("0.0.0.0");
        config.Server.Port.ShouldBe(8080);
        config.Build.Minify.ShouldBeFalse();
        config.Build.Fingerprint.ShouldBeFalse();
        config.Build.Clean.ShouldBeFalse();
        config.Build.Concurrency.ShouldBe(4);
    }

    [Fact]
    public void ShouldDeserializePartialConfig()
    {
        var json = """{ "site": "https://example.com" }""";

        var config = AtollConfigLoader.Deserialize(json);

        config.Site.ShouldBe("https://example.com");
        config.Base.ShouldBe("/");
        config.OutDir.ShouldBe("dist");
        config.SrcDir.ShouldBe("src/pages");
        config.PublicDir.ShouldBe("public");
    }

    [Fact]
    public void ShouldDeserializeEmptyJsonToDefaults()
    {
        var config = AtollConfigLoader.Deserialize("{}");

        config.Site.ShouldBe("");
        config.Base.ShouldBe("/");
        config.OutDir.ShouldBe("dist");
    }

    [Fact]
    public void ShouldHandleJsonWithComments()
    {
        var json = """
            {
              // This is a comment
              "site": "https://example.com"
            }
            """;

        var config = AtollConfigLoader.Deserialize(json);
        config.Site.ShouldBe("https://example.com");
    }

    [Fact]
    public void ShouldHandleJsonWithTrailingCommas()
    {
        var json = """
            {
              "site": "https://example.com",
              "base": "/docs",
            }
            """;

        var config = AtollConfigLoader.Deserialize(json);
        config.Site.ShouldBe("https://example.com");
        config.Base.ShouldBe("/docs");
    }

    [Fact]
    public void ShouldBeCaseInsensitiveForPropertyNames()
    {
        var json = """{ "Site": "https://example.com", "OutDir": "build" }""";

        var config = AtollConfigLoader.Deserialize(json);
        config.Site.ShouldBe("https://example.com");
        config.OutDir.ShouldBe("build");
    }

    [Fact]
    public void ShouldThrowOnInvalidJson()
    {
        Should.Throw<InvalidOperationException>(() =>
            AtollConfigLoader.Deserialize("not valid json"));
    }

    [Fact]
    public void ShouldThrowOnNullJsonInput()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.Deserialize(null!));
    }

    [Fact]
    public void ShouldDeserializeServerSectionOnly()
    {
        var json = """{ "server": { "host": "0.0.0.0", "port": 9000 } }""";

        var config = AtollConfigLoader.Deserialize(json);
        config.Server.Host.ShouldBe("0.0.0.0");
        config.Server.Port.ShouldBe(9000);
    }

    [Fact]
    public void ShouldDeserializeBuildSectionOnly()
    {
        var json = """{ "build": { "minify": false, "concurrency": 2 } }""";

        var config = AtollConfigLoader.Deserialize(json);
        config.Build.Minify.ShouldBeFalse();
        config.Build.Concurrency.ShouldBe(2);
        config.Build.Fingerprint.ShouldBeTrue(); // default preserved
        config.Build.Clean.ShouldBeTrue(); // default preserved
    }

    // ── Serialization ──

    [Fact]
    public void ShouldSerializeConfig()
    {
        var config = new AtollConfig { Site = "https://example.com", Base = "/docs" };
        var json = AtollConfigLoader.Serialize(config);

        json.ShouldContain("\"site\"");
        json.ShouldContain("https://example.com");
        json.ShouldContain("\"base\"");
        json.ShouldContain("/docs");
    }

    [Fact]
    public void ShouldSerializeAndDeserializeRoundTrip()
    {
        var original = new AtollConfig
        {
            Site = "https://example.com",
            Base = "/blog",
            OutDir = "output",
            SrcDir = "content",
            PublicDir = "assets",
            Server = new AtollServerConfig { Host = "127.0.0.1", Port = 5000 },
            Build = new AtollBuildConfig { Minify = false, Fingerprint = true, Clean = false, Concurrency = 8 },
        };

        var json = AtollConfigLoader.Serialize(original);
        var deserialized = AtollConfigLoader.Deserialize(json);

        deserialized.Site.ShouldBe(original.Site);
        deserialized.Base.ShouldBe(original.Base);
        deserialized.OutDir.ShouldBe(original.OutDir);
        deserialized.SrcDir.ShouldBe(original.SrcDir);
        deserialized.PublicDir.ShouldBe(original.PublicDir);
        deserialized.Server.Host.ShouldBe(original.Server.Host);
        deserialized.Server.Port.ShouldBe(original.Server.Port);
        deserialized.Build.Minify.ShouldBe(original.Build.Minify);
        deserialized.Build.Fingerprint.ShouldBe(original.Build.Fingerprint);
        deserialized.Build.Clean.ShouldBe(original.Build.Clean);
        deserialized.Build.Concurrency.ShouldBe(original.Build.Concurrency);
    }

    [Fact]
    public void ShouldThrowOnNullSerializeInput()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.Serialize(null!));
    }

    // ── LoadAsync ──

    [Fact]
    public async Task ShouldReturnDefaultConfigWhenFileDoesNotExist()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var config = await AtollConfigLoader.LoadAsync(tempDir, _ct);

            config.ShouldNotBeNull();
            config.Site.ShouldBe("");
            config.OutDir.ShouldBe("dist");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldLoadConfigFromFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var json = """{ "site": "https://test.com", "outDir": "output" }""";
            await File.WriteAllTextAsync(Path.Combine(tempDir, "atoll.json"), json);

            var config = await AtollConfigLoader.LoadAsync(tempDir, _ct);

            config.Site.ShouldBe("https://test.com");
            config.OutDir.ShouldBe("output");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ShouldThrowOnNullDirectoryForLoadAsync()
    {
        await Should.ThrowAsync<ArgumentNullException>(() =>
            AtollConfigLoader.LoadAsync(null!, _ct));
    }

    // ── LoadFromFileAsync ──

    [Fact]
    public async Task ShouldReturnDefaultConfigWhenSpecificFileDoesNotExist()
    {
        var config = await AtollConfigLoader.LoadFromFileAsync(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "atoll.json"), _ct);

        config.ShouldNotBeNull();
        config.Site.ShouldBe("");
    }

    [Fact]
    public async Task ShouldLoadFromSpecificFile()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            await File.WriteAllTextAsync(tempFile,
                """{ "site": "https://file-test.com" }""");

            var config = await AtollConfigLoader.LoadFromFileAsync(tempFile, _ct);
            config.Site.ShouldBe("https://file-test.com");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task ShouldThrowOnNullFilePathForLoadFromFileAsync()
    {
        await Should.ThrowAsync<ArgumentNullException>(() =>
            AtollConfigLoader.LoadFromFileAsync(null!, _ct));
    }

    // ── DefaultFileName ──

    [Fact]
    public void ShouldHaveCorrectDefaultFileName()
    {
        AtollConfigLoader.DefaultFileName.ShouldBe("atoll.json");
    }

    // ── ResolveOutputDirectory ──

    [Fact]
    public void ShouldResolveRelativeOutputDirectory()
    {
        var config = new AtollConfig { OutDir = "dist" };
        var root = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

        var result = AtollConfigLoader.ResolveOutputDirectory(config, root);

        result.ShouldBe(Path.GetFullPath(Path.Combine(root, "dist")));
    }

    [Fact]
    public void ShouldResolveAbsoluteOutputDirectory()
    {
        var absolutePath = Path.Combine(Path.GetTempPath(), "my-output");
        var config = new AtollConfig { OutDir = absolutePath };

        var result = AtollConfigLoader.ResolveOutputDirectory(config, "C:\\projects\\myapp");

        result.ShouldBe(absolutePath);
    }

    [Fact]
    public void ShouldThrowOnNullConfigForResolveOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.ResolveOutputDirectory(null!, "C:\\root"));
    }

    [Fact]
    public void ShouldThrowOnNullRootForResolveOutputDirectory()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.ResolveOutputDirectory(new AtollConfig(), null!));
    }

    // ── ResolveSrcDirectory ──

    [Fact]
    public void ShouldResolveRelativeSrcDirectory()
    {
        var config = new AtollConfig { SrcDir = "src/pages" };
        var root = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

        var result = AtollConfigLoader.ResolveSrcDirectory(config, root);

        result.ShouldBe(Path.GetFullPath(Path.Combine(root, "src/pages")));
    }

    [Fact]
    public void ShouldResolveAbsoluteSrcDirectory()
    {
        var absolutePath = Path.Combine(Path.GetTempPath(), "my-src");
        var config = new AtollConfig { SrcDir = absolutePath };

        var result = AtollConfigLoader.ResolveSrcDirectory(config, "C:\\projects\\myapp");

        result.ShouldBe(absolutePath);
    }

    [Fact]
    public void ShouldThrowOnNullConfigForResolveSrcDirectory()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.ResolveSrcDirectory(null!, "C:\\root"));
    }

    [Fact]
    public void ShouldThrowOnNullRootForResolveSrcDirectory()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.ResolveSrcDirectory(new AtollConfig(), null!));
    }

    // ── ResolvePublicDirectory ──

    [Fact]
    public void ShouldResolveRelativePublicDirectory()
    {
        var config = new AtollConfig { PublicDir = "public" };
        var root = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

        var result = AtollConfigLoader.ResolvePublicDirectory(config, root);

        result.ShouldBe(Path.GetFullPath(Path.Combine(root, "public")));
    }

    [Fact]
    public void ShouldResolveAbsolutePublicDirectory()
    {
        var absolutePath = Path.Combine(Path.GetTempPath(), "my-public");
        var config = new AtollConfig { PublicDir = absolutePath };

        var result = AtollConfigLoader.ResolvePublicDirectory(config, "C:\\projects\\myapp");

        result.ShouldBe(absolutePath);
    }

    [Fact]
    public void ShouldThrowOnNullConfigForResolvePublicDirectory()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.ResolvePublicDirectory(null!, "C:\\root"));
    }

    [Fact]
    public void ShouldThrowOnNullRootForResolvePublicDirectory()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.ResolvePublicDirectory(new AtollConfig(), null!));
    }

    // ── NormalizeBasePath ──

    [Fact]
    public void ShouldReturnSlashForEmptyBasePath()
    {
        AtollConfigLoader.NormalizeBasePath("").ShouldBe("/");
    }

    [Fact]
    public void ShouldReturnSlashForSlashBasePath()
    {
        AtollConfigLoader.NormalizeBasePath("/").ShouldBe("/");
    }

    [Fact]
    public void ShouldAddLeadingSlash()
    {
        AtollConfigLoader.NormalizeBasePath("docs").ShouldBe("/docs");
    }

    [Fact]
    public void ShouldRemoveTrailingSlash()
    {
        AtollConfigLoader.NormalizeBasePath("/docs/").ShouldBe("/docs");
    }

    [Fact]
    public void ShouldPreserveCorrectBasePath()
    {
        AtollConfigLoader.NormalizeBasePath("/docs").ShouldBe("/docs");
    }

    [Fact]
    public void ShouldAddLeadingAndRemoveTrailingSlash()
    {
        AtollConfigLoader.NormalizeBasePath("blog/v2/").ShouldBe("/blog/v2");
    }

    [Fact]
    public void ShouldRemoveMultipleTrailingSlashes()
    {
        AtollConfigLoader.NormalizeBasePath("/docs///").ShouldBe("/docs");
    }

    [Fact]
    public void ShouldThrowOnNullBasePath()
    {
        Should.Throw<ArgumentNullException>(() =>
            AtollConfigLoader.NormalizeBasePath(null!));
    }

    // ── Edge cases ──

    [Fact]
    public void ShouldDeserializeNestedServerWithPartialProperties()
    {
        var json = """{ "server": { "port": 9999 } }""";

        var config = AtollConfigLoader.Deserialize(json);

        config.Server.Port.ShouldBe(9999);
        config.Server.Host.ShouldBe("localhost"); // default preserved
    }

    [Fact]
    public void ShouldDeserializeNestedBuildWithPartialProperties()
    {
        var json = """{ "build": { "minify": false } }""";

        var config = AtollConfigLoader.Deserialize(json);

        config.Build.Minify.ShouldBeFalse();
        config.Build.Fingerprint.ShouldBeTrue(); // default preserved
        config.Build.Clean.ShouldBeTrue(); // default preserved
        config.Build.Concurrency.ShouldBe(-1); // default preserved
    }

    [Fact]
    public async Task ShouldLoadConfigWithCommentsAndTrailingCommas()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var json = """
                {
                  // Site URL
                  "site": "https://test.com",
                  "outDir": "output",
                }
                """;
            await File.WriteAllTextAsync(Path.Combine(tempDir, "atoll.json"), json);

            var config = await AtollConfigLoader.LoadAsync(tempDir, _ct);

            config.Site.ShouldBe("https://test.com");
            config.OutDir.ShouldBe("output");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
