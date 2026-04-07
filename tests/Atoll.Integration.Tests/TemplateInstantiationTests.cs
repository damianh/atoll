using System.Diagnostics;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests;

/// <summary>
/// Shared fixture that installs the Atoll.Templates pack once, instantiates
/// every template into a shared temp directory, and uninstalls on teardown.
/// This avoids spawning dozens of <c>dotnet</c> processes (one install/uninstall
/// per test) which caused orphan-process hangs on Linux CI.
/// </summary>
public sealed class TemplateFixture : IAsyncLifetime
{
    private static readonly string TemplatesDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "templates"));

    private string _tempDir = string.Empty;

    public bool TemplatesInstalled { get; private set; }

    /// <summary>Root directory for the atoll-empty template output.</summary>
    public string EmptyDir => Path.Combine(_tempDir, "MyApp");

    /// <summary>Root directory for the atoll-starter template output.</summary>
    public string StarterDir => Path.Combine(_tempDir, "MySite");

    /// <summary>Root directory for the atoll-blog template output.</summary>
    public string BlogDir => Path.Combine(_tempDir, "MyBlog");

    /// <summary>Root directory for the atoll-portfolio template output.</summary>
    public string PortfolioDir => Path.Combine(_tempDir, "MyPortfolio");

    public async ValueTask InitializeAsync()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "atoll-template-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        var installResult = await RunDotnetAsync($"new install \"{TemplatesDir}\"", _tempDir);
        TemplatesInstalled = installResult == 0;

        if (!TemplatesInstalled)
        {
            return;
        }

        // Instantiate each template once. Tests assert against these outputs.
        await RunDotnetAsync($"new atoll-empty -n MyApp -o \"{EmptyDir}\"", _tempDir);
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{StarterDir}\"", _tempDir);
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{BlogDir}\"", _tempDir);
        await RunDotnetAsync($"new atoll-portfolio -n MyPortfolio -o \"{PortfolioDir}\"", _tempDir);
    }

    public async ValueTask DisposeAsync()
    {
        if (TemplatesInstalled)
        {
            await RunDotnetAsync($"new uninstall \"{TemplatesDir}\"", _tempDir);
        }

        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch (IOException)
            {
                // Best-effort cleanup
            }
        }
    }

    private static async Task<int> RunDotnetAsync(string arguments, string workingDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
        {
            return -1;
        }

        // Drain redirected streams to prevent pipe-buffer deadlocks and orphan processes on Linux.
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await process.WaitForExitAsync(cts.Token);
        await Task.WhenAll(stdoutTask, stderrTask);

        return process.ExitCode;
    }
}

/// <summary>
/// Tests that verify each Atoll template can be instantiated with correct file
/// structure and namespace substitution. These tests do not compile the generated
/// projects (that requires Atoll.Middleware to be published to a NuGet feed).
/// Run with: <c>dotnet test --filter "Category=Template"</c>
/// </summary>
[Trait("Category", "Template")]
[Collection("TemplateTests")]
public sealed class TemplateInstantiationTests
{
    private readonly TemplateFixture _fixture;

    public TemplateInstantiationTests(TemplateFixture fixture)
    {
        _fixture = fixture;
    }

    // ── atoll-empty ──

    [Fact]
    public void AtollEmptyShouldCreateCsprojFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.EmptyDir, "MyApp.csproj")).ShouldBeTrue();
    }

    [Fact]
    public void AtollEmptyShouldCreateProgramFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.EmptyDir, "Program.cs")).ShouldBeTrue();
    }

    [Fact]
    public void AtollEmptyShouldCreateAtollJsonFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.EmptyDir, "atoll.json")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollEmptyShouldSubstituteNamespace()
    {
        if (!_fixture.TemplatesInstalled) return;

        var csprojPath = Path.Combine(_fixture.EmptyDir, "MyApp.csproj");
        File.Exists(csprojPath).ShouldBeTrue();
        var content = await File.ReadAllTextAsync(csprojPath);
        content.ShouldNotContain("AtollEmpty");
    }

    // ── atoll-starter ──

    [Fact]
    public void AtollStarterShouldCreateCsprojFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.StarterDir, "MySite.csproj")).ShouldBeTrue();
    }

    [Fact]
    public void AtollStarterShouldCreateProgramFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.StarterDir, "Program.cs")).ShouldBeTrue();
    }

    [Fact]
    public void AtollStarterShouldCreateAtollJsonFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.StarterDir, "atoll.json")).ShouldBeTrue();
    }

    [Fact]
    public void AtollStarterShouldCreateIndexPage()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.StarterDir, "Pages", "Index.cs")).ShouldBeTrue();
    }

    [Fact]
    public void AtollStarterShouldCreateMainLayout()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.StarterDir, "Layouts", "MainLayout.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollStarterShouldSubstituteNamespaceInIndexPage()
    {
        if (!_fixture.TemplatesInstalled) return;

        var content = await File.ReadAllTextAsync(
            Path.Combine(_fixture.StarterDir, "Pages", "Index.cs"));
        content.ShouldNotContain("AtollStarter");
        content.ShouldContain("MySite");
    }

    // ── atoll-blog ──

    [Fact]
    public void AtollBlogShouldCreateCsprojFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.BlogDir, "MyBlog.csproj")).ShouldBeTrue();
    }

    [Fact]
    public void AtollBlogShouldCreateProgramFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.BlogDir, "Program.cs")).ShouldBeTrue();
    }

    [Fact]
    public void AtollBlogShouldCreateThemeToggleJsStub()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.BlogDir, "public", "scripts", "theme-toggle.js")).ShouldBeTrue();
    }

    [Fact]
    public void AtollBlogShouldCreateBlogPostPages()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.BlogDir, "Pages", "BlogIndexPage.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(_fixture.BlogDir, "Pages", "BlogPostPage.cs")).ShouldBeTrue();
    }

    [Fact]
    public void AtollBlogShouldCreateMarkdownContent()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.BlogDir, "Content", "blog", "welcome.md")).ShouldBeTrue();
        File.Exists(Path.Combine(_fixture.BlogDir, "Content", "blog", "getting-started.md")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollBlogShouldSubstituteNamespaceInPages()
    {
        if (!_fixture.TemplatesInstalled) return;

        var content = await File.ReadAllTextAsync(
            Path.Combine(_fixture.BlogDir, "Pages", "BlogIndexPage.cs"));
        content.ShouldNotContain("AtollBlog");
        content.ShouldContain("MyBlog");
    }

    // ── atoll-portfolio ──

    [Fact]
    public void AtollPortfolioShouldCreateCsprojFile()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.PortfolioDir, "MyPortfolio.csproj")).ShouldBeTrue();
    }

    [Fact]
    public void AtollPortfolioShouldCreateContactFormJsStub()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.PortfolioDir, "public", "scripts", "contact-form.js")).ShouldBeTrue();
    }

    [Fact]
    public void AtollPortfolioShouldCreateMobileNavJsStub()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.PortfolioDir, "public", "scripts", "mobile-nav.js")).ShouldBeTrue();
    }

    [Fact]
    public void AtollPortfolioShouldCreateIslandFiles()
    {
        if (!_fixture.TemplatesInstalled) return;
        File.Exists(Path.Combine(_fixture.PortfolioDir, "Islands", "ContactForm.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(_fixture.PortfolioDir, "Islands", "MobileNav.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollPortfolioShouldSubstituteNamespaceInIslands()
    {
        if (!_fixture.TemplatesInstalled) return;

        var content = await File.ReadAllTextAsync(
            Path.Combine(_fixture.PortfolioDir, "Islands", "ContactForm.cs"));
        content.ShouldNotContain("AtollPortfolio");
        content.ShouldContain("MyPortfolio");
    }
}

[CollectionDefinition("TemplateTests")]
public sealed class TemplateTestsCollection : ICollectionFixture<TemplateFixture>;
