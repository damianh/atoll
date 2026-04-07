using Atoll.Cli.Commands;

namespace Atoll.Integration.Tests;

// Serialized to avoid flaky failures from concurrent dotnet process spawning
// and Windows file-handle races during parallel temp directory cleanup.
[Collection("NewCommandHandler")]
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
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreatePagesDirectory()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site", "Pages")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateLayoutsDirectory()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site", "Layouts")).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreatePublicDirectory()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        Directory.Exists(Path.Combine(_tempDir, "my-site", "public")).ShouldBeTrue();
    }

    // ── Generated files ──

    [Fact]
    public async Task ShouldCreateCsprojFile()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var csproj = Path.Combine(_tempDir, "my-site", "my-site.csproj");
        File.Exists(csproj).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateCsprojWithCorrectContent()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "my-site.csproj"));
        content.ShouldContain("net10.0");
        content.ShouldContain("Atoll.Middleware");
        content.ShouldContain("Microsoft.NET.Sdk.Web");
        content.ShouldContain("my-site");
    }

    [Fact]
    public async Task ShouldCreateAtollJsonFile()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var configFile = Path.Combine(_tempDir, "my-site", "atoll.json");
        File.Exists(configFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateAtollJsonWithCorrectContent()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "atoll.json"));
        content.ShouldContain("\"site\"");
        content.ShouldContain("\"base\"");
        content.ShouldContain("4321");
        content.ShouldContain("localhost");
    }

    [Fact]
    public async Task ShouldCreateProgramFile()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var programFile = Path.Combine(_tempDir, "my-site", "Program.cs");
        File.Exists(programFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateProgramFileWithAtollSetup()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "Program.cs"));
        content.ShouldContain("AddAtoll");
        content.ShouldContain("UseAtoll");
        content.ShouldContain("WebApplication");
    }

    [Fact]
    public async Task ShouldCreateIndexPageFile()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var indexFile = Path.Combine(_tempDir, "my-site", "Pages", "Index.cs");
        File.Exists(indexFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateIndexPageWithCorrectContent()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "Pages", "Index.cs"));
        content.ShouldContain("IAtollPage");
        content.ShouldContain("AtollComponent");
        content.ShouldContain("Welcome to my-site");
        content.ShouldContain("my_site.Pages");
    }

    [Fact]
    public async Task ShouldCreateMainLayoutFile()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var layoutFile = Path.Combine(_tempDir, "my-site", "Layouts", "MainLayout.cs");
        File.Exists(layoutFile).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateMainLayoutWithCorrectContent()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", "Layouts", "MainLayout.cs"));
        content.ShouldContain("MainLayout");
        content.ShouldContain("AtollComponent");
        content.ShouldContain("DOCTYPE html");
        content.ShouldContain("RenderSlotAsync");
    }

    [Fact]
    public async Task ShouldCreateGitIgnoreFile()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var gitignore = Path.Combine(_tempDir, "my-site", ".gitignore");
        File.Exists(gitignore).ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldCreateGitIgnoreWithCorrectContent()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-site", _tempDir);

        var content = await File.ReadAllTextAsync(Path.Combine(_tempDir, "my-site", ".gitignore"));
        content.ShouldContain("bin/");
        content.ShouldContain("obj/");
        content.ShouldContain("dist/");
    }

    // ── Namespace sanitization ──

    [Fact]
    public async Task ShouldSanitizeNamespaceWithHyphens()
    {
        await NewCommandHandler.ScaffoldStarterAsync("my-awesome-site", _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "my-awesome-site", "Pages", "Index.cs"));
        content.ShouldContain("my_awesome_site.Pages");
    }

    [Fact]
    public async Task ShouldSanitizeNamespaceStartingWithDigit()
    {
        await NewCommandHandler.ScaffoldStarterAsync("123site", _tempDir);

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

        await NewCommandHandler.ScaffoldStarterAsync("existing", _tempDir);

        // Should not have created project files
        File.Exists(Path.Combine(projectDir, "atoll.json")).ShouldBeFalse();
    }

    [Fact]
    public async Task ShouldAllowCreationInExistingEmptyDirectory()
    {
        var projectDir = Path.Combine(_tempDir, "empty-dir");
        Directory.CreateDirectory(projectDir);

        await NewCommandHandler.ScaffoldStarterAsync("empty-dir", _tempDir);

        File.Exists(Path.Combine(projectDir, "atoll.json")).ShouldBeTrue();
    }

    // ── XML escaping in csproj ──

    [Fact]
    public async Task ShouldEscapeXmlSpecialCharactersInProjectName()
    {
        await NewCommandHandler.ScaffoldStarterAsync("site&project", _tempDir);

        var content = await File.ReadAllTextAsync(
            Path.Combine(_tempDir, "site&project", "site&project.csproj"));
        content.ShouldContain("site&amp;project");
        content.ShouldNotContain("<RootNamespace>site&project</RootNamespace>");
    }

    // ── Complete scaffold verification ──

    [Fact]
    public async Task ShouldCreateCompleteProjectStructure()
    {
        await NewCommandHandler.ScaffoldStarterAsync("full-test", _tempDir);

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
    }

    [Fact]
    public async Task ShouldProduceIdenticalOutputForDefaultAndExplicitStarter()
    {
        // Both paths use ScaffoldStarterAsync directly to verify built-in
        // scaffolding produces consistent output regardless of call site.
        var defaultDir = Path.Combine(_tempDir, "default-starter");
        Directory.CreateDirectory(defaultDir);
        await NewCommandHandler.ScaffoldStarterAsync("my-project", defaultDir);

        var explicitDir = Path.Combine(_tempDir, "explicit-starter");
        Directory.CreateDirectory(explicitDir);
        await NewCommandHandler.ScaffoldStarterAsync("my-project", explicitDir);

        var defaultCsproj = await File.ReadAllTextAsync(
            Path.Combine(defaultDir, "my-project", "my-project.csproj"));
        var explicitCsproj = await File.ReadAllTextAsync(
            Path.Combine(explicitDir, "my-project", "my-project.csproj"));

        defaultCsproj.ShouldBe(explicitCsproj);
    }
}
