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
}
