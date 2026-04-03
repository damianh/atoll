using Atoll.Cli.Commands;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests;

public sealed class NewCommandHandlerTests : IDisposable
{
    private readonly string _tempDir;

    public NewCommandHandlerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "atoll-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    // ── Basic scaffolding ──

    [Fact]
    public async Task ShouldCreateProjectDirectory()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreatePagesDirectory()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site", "Pages")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateLayoutsDirectory()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site", "Layouts")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreatePublicDirectory()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site", "public")).ShouldBeTrue();
    }

    // ── Generated files ──

    [Fact]
    public async Task ShouldCreateCsprojFile()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var csproj = Path.Combine(_tempDir, "my-site", "my-site.csproj");
        File.Exists(csproj).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateCsprojWithCorrectContent()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "my-site.csproj"));
        content.ShouldContain("net10.0");
        content.ShouldContain("Atoll.Middleware");
        content.ShouldContain("Microsoft.NET.Sdk.Web");
        content.ShouldContain("my-site");
    }

    [Fact]
    public async Task ShouldCreateAtollJsonFile()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var configFile = Path.Combine(_tempDir, "my-site", "atoll.json");
        File.Exists(configFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateAtollJsonWithCorrectContent()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "atoll.json"));
        content.ShouldContain("\"site\"");
        content.ShouldContain("\"base\"");
        content.ShouldContain("4321");
        content.ShouldContain("localhost");
    }

    [Fact]
    public async Task ShouldCreateProgramFile()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var programFile = Path.Combine(_tempDir, "my-site", "Program.cs");
        File.Exists(programFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateProgramFileWithAtollSetup()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "Program.cs"));
        content.ShouldContain("AddAtoll");
        content.ShouldContain("UseAtoll");
        content.ShouldContain("WebApplication");
    }

    [Fact]
    public async Task ShouldCreateIndexPageFile()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var indexFile = Path.Combine(_tempDir, "my-site", "Pages", "Index.cs");
        File.Exists(indexFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateIndexPageWithCorrectContent()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "Pages", "Index.cs"));
        content.ShouldContain("IAtollPage");
        content.ShouldContain("AtollComponent");
        content.ShouldContain("Welcome to my-site");
        content.ShouldContain("my_site.Pages");
    }

    [Fact]
    public async Task ShouldCreateMainLayoutFile()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var layoutFile = Path.Combine(_tempDir, "my-site", "Layouts", "MainLayout.cs");
        File.Exists(layoutFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateMainLayoutWithCorrectContent()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "Layouts", "MainLayout.cs"));
        content.ShouldContain("MainLayout");
        content.ShouldContain("AtollComponent");
        content.ShouldContain("DOCTYPE html");
        content.ShouldContain("RenderSlotAsync");
    }

    [Fact]
    public async Task ShouldCreateGitIgnoreFile()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var gitignore = Path.Combine(_tempDir, "my-site", ".gitignore");
        File.Exists(gitignore).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateGitIgnoreWithCorrectContent()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", ".gitignore"));
        content.ShouldContain("bin/");
        content.ShouldContain("obj/");
        content.ShouldContain("dist/");
    }

    // ── Namespace sanitization ──

    [Fact]
    public async Task ShouldSanitizeNamespaceWithHyphens()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("my-awesome-site", _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "my-awesome-site", "Pages", "Index.cs"));
        content.ShouldContain("my_awesome_site.Pages");
    }

    [Fact]
    public async Task ShouldSanitizeNamespaceStartingWithDigit()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("123site", _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "123site", "Pages", "Index.cs"));
        content.ShouldContain("_123site.Pages");
    }

    // ── Error handling ──

    [Fact]
    public async Task ShouldNotOverwriteExistingNonEmptyDirectory()
    {
        var projectDir = Path.Combine(_tempDir, "existing");
        Directory.CreateDirectory(projectDir);
        await File.WriteAllTextAsync(Path.Combine(projectDir, "file.txt"), "content");

        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("existing", _tempDir);

        // Should not have created project files
        File.Exists(Path.Combine(projectDir, "atoll.json")).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldAllowCreationInExistingEmptyDirectory()
    {
        var projectDir = Path.Combine(_tempDir, "empty-dir");
        Directory.CreateDirectory(projectDir);

        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("empty-dir", _tempDir);

        File.Exists(Path.Combine(projectDir, "atoll.json")).ShouldBeTrue();
    }

    // ── XML escaping in csproj ──

    [Fact]
    public async Task ShouldEscapeXmlSpecialCharactersInProjectName()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("site&project", _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "site&project", "site&project.csproj"));
        content.ShouldContain("site&amp;project");
        content.ShouldNotContain("<RootNamespace>site&project</RootNamespace>");
    }

    // ── Complete scaffold verification ──

    [Fact]
    public async Task ShouldCreateCompleteProjectStructure()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("full-test", _tempDir);

        var projectDir = Path.Combine(_tempDir, "full-test");

        // Verify all expected files exist
        File.Exists(Path.Combine(projectDir, "full-test.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "atoll.json")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "Program.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "Pages", "Index.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "Layouts", "MainLayout.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, ".gitignore")).ShouldBeTrue();

        // Verify all expected directories exist
        Directory.Exists(Path.Combine(projectDir, "Pages")).ShouldBeTrue();
        Directory.Exists(Path.Combine(projectDir, "Layouts")).ShouldBeTrue();
        Directory.Exists(Path.Combine(projectDir, "public")).ShouldBeTrue();
    }

    // ── --template option: explicit starter ──

    [Fact]
    public async Task ShouldScaffoldStarterWhenTemplateIsExplicitlyStarter()
    {
        var handler = new NewCommandHandler();
        await handler.ExecuteAsync("explicit-starter", _tempDir, "starter");

        var projectDir = Path.Combine(_tempDir, "explicit-starter");
        File.Exists(Path.Combine(projectDir, "explicit-starter.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "Program.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "atoll.json")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "Pages", "Index.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(projectDir, "Layouts", "MainLayout.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldProduceIdenticalOutputForDefaultAndExplicitStarter()
    {
        var handler = new NewCommandHandler();

        var defaultDir = Path.Combine(_tempDir, "default-starter");
        Directory.CreateDirectory(defaultDir);
        await handler.ExecuteAsync("my-project", defaultDir);

        var explicitDir = Path.Combine(_tempDir, "explicit-starter");
        Directory.CreateDirectory(explicitDir);
        await handler.ExecuteAsync("my-project", explicitDir, "starter");

        var defaultCsproj = await File.ReadAllTextAsync(
            Path.Combine(defaultDir, "my-project", "my-project.csproj"));
        var explicitCsproj = await File.ReadAllTextAsync(
            Path.Combine(explicitDir, "my-project", "my-project.csproj"));

        defaultCsproj.ShouldBe(explicitCsproj);
    }
}
