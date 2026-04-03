using Shouldly;
using System.Diagnostics;
using Xunit;

namespace Atoll.Integration.Tests;

/// <summary>
/// Tests that install the Atoll.Templates pack from the local templates directory
/// and verify that each template can be instantiated with correct file structure
/// and namespace substitution. These tests do not compile the generated projects
/// (that requires Atoll.Middleware to be published to a NuGet feed).
/// Run with: dotnet test --filter "Category=Template"
/// </summary>
[Trait("Category", "Template")]
public sealed class TemplateInstantiationTests : IAsyncLifetime
{
    private static readonly string TemplatesDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "templates"));

    private string _tempDir = string.Empty;
    private bool _templatesInstalled;

    public async Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-template-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        // Install templates from local source
        var exitCode = await RunDotnetAsync($"new install \"{TemplatesDir}\"", workingDir: _tempDir);
        _templatesInstalled = exitCode == 0;
    }

    public async Task DisposeAsync()
    {
        if (_templatesInstalled)
        {
            // Uninstall templates to leave the system clean
            await RunDotnetAsync($"new uninstall \"{TemplatesDir}\"", workingDir: _tempDir);
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

    // ── atoll-empty ──

    [Fact]
    public async Task AtollEmptyShouldCreateCsprojFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-empty -n MyApp -o \"{_tempDir}/MyApp\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyApp", "MyApp.csproj")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollEmptyShouldCreateProgramFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-empty -n MyApp -o \"{_tempDir}/MyApp\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyApp", "Program.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollEmptyShouldCreateAtollJsonFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-empty -n MyApp -o \"{_tempDir}/MyApp\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyApp", "atoll.json")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollEmptyShouldSubstituteNamespace()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-empty -n MyApp -o \"{_tempDir}/MyApp\"", workingDir: _tempDir);

        var csprojPath = Path.Combine(_tempDir, "MyApp", "MyApp.csproj");
        File.Exists(csprojPath).ShouldBeTrue();
        var content = await File.ReadAllTextAsync(csprojPath);
        content.ShouldNotContain("AtollEmpty");
    }

    // ── atoll-starter ──

    [Fact]
    public async Task AtollStarterShouldCreateCsprojFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{_tempDir}/MySite\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MySite", "MySite.csproj")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollStarterShouldCreateProgramFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{_tempDir}/MySite\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MySite", "Program.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollStarterShouldCreateAtollJsonFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{_tempDir}/MySite\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MySite", "atoll.json")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollStarterShouldCreateIndexPage()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{_tempDir}/MySite\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MySite", "Pages", "Index.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollStarterShouldCreateMainLayout()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{_tempDir}/MySite\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MySite", "Layouts", "MainLayout.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollStarterShouldSubstituteNamespaceInIndexPage()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-starter -n MySite -o \"{_tempDir}/MySite\"", workingDir: _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "MySite", "Pages", "Index.cs"));
        content.ShouldNotContain("AtollStarter");
        content.ShouldContain("MySite");
    }

    // ── atoll-blog ──

    [Fact]
    public async Task AtollBlogShouldCreateCsprojFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{_tempDir}/MyBlog\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyBlog", "MyBlog.csproj")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollBlogShouldCreateProgramFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{_tempDir}/MyBlog\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyBlog", "Program.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollBlogShouldCreateThemeToggleJsStub()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{_tempDir}/MyBlog\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyBlog", "public", "scripts", "theme-toggle.js")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollBlogShouldCreateBlogPostPages()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{_tempDir}/MyBlog\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyBlog", "Pages", "BlogIndexPage.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(_tempDir, "MyBlog", "Pages", "BlogPostPage.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollBlogShouldCreateMarkdownContent()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{_tempDir}/MyBlog\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyBlog", "Content", "blog", "welcome.md")).ShouldBeTrue();
        File.Exists(Path.Combine(_tempDir, "MyBlog", "Content", "blog", "getting-started.md")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollBlogShouldSubstituteNamespaceInPages()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-blog -n MyBlog -o \"{_tempDir}/MyBlog\"", workingDir: _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "MyBlog", "Pages", "BlogIndexPage.cs"));
        content.ShouldNotContain("AtollBlog");
        content.ShouldContain("MyBlog");
    }

    // ── atoll-portfolio ──

    [Fact]
    public async Task AtollPortfolioShouldCreateCsprojFile()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-portfolio -n MyPortfolio -o \"{_tempDir}/MyPortfolio\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyPortfolio", "MyPortfolio.csproj")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollPortfolioShouldCreateContactFormJsStub()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-portfolio -n MyPortfolio -o \"{_tempDir}/MyPortfolio\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyPortfolio", "public", "scripts", "contact-form.js")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollPortfolioShouldCreateMobileNavJsStub()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-portfolio -n MyPortfolio -o \"{_tempDir}/MyPortfolio\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyPortfolio", "public", "scripts", "mobile-nav.js")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollPortfolioShouldCreateIslandFiles()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-portfolio -n MyPortfolio -o \"{_tempDir}/MyPortfolio\"", workingDir: _tempDir);

        File.Exists(Path.Combine(_tempDir, "MyPortfolio", "Islands", "ContactForm.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(_tempDir, "MyPortfolio", "Islands", "MobileNav.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AtollPortfolioShouldSubstituteNamespaceInIslands()
    {
        if (!_templatesInstalled) return;
        await RunDotnetAsync($"new atoll-portfolio -n MyPortfolio -o \"{_tempDir}/MyPortfolio\"", workingDir: _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "MyPortfolio", "Islands", "ContactForm.cs"));
        content.ShouldNotContain("AtollPortfolio");
        content.ShouldContain("MyPortfolio");
    }

    // ── Helpers ──

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

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
