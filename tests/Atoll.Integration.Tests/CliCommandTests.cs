using System.CommandLine;
using Atoll.Cli;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests;

public sealed class CliCommandTests
{
    // ── Root command structure ──

    [Fact]
    public void ShouldCreateRootCommandWithDescription()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        rootCommand.Description.ShouldNotBeNull();
        rootCommand.Description.ShouldContain("Atoll");
    }

    [Fact]
    public void ShouldHaveThreeSubcommands()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        rootCommand.Subcommands.Count.ShouldBe(3);
    }

    [Fact]
    public void ShouldHaveBuildSubcommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        rootCommand.Subcommands.ShouldContain(c => c.Name == "build");
    }

    [Fact]
    public void ShouldHaveDevSubcommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        rootCommand.Subcommands.ShouldContain(c => c.Name == "dev");
    }

    [Fact]
    public void ShouldHavePreviewSubcommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        rootCommand.Subcommands.ShouldContain(c => c.Name == "preview");
    }

    // ── Root option ──

    [Fact]
    public void ShouldHaveRecursiveRootOption()
    {
        var rootOption = CommandFactory.CreateRootOption();
        rootOption.Name.ShouldBe("--root");
        rootOption.Recursive.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveRootOptionWithDefaultValue()
    {
        var rootOption = CommandFactory.CreateRootOption();
        rootOption.HasDefaultValue.ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveRootOptionOnRootCommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        rootCommand.Options.ShouldContain(o => o.Name == "--root");
    }

    // ── Build command ──

    [Fact]
    public void ShouldCreateBuildCommandWithDescription()
    {
        var command = CommandFactory.CreateBuildCommand();
        command.Name.ShouldBe("build");
        command.Description.ShouldNotBeNull();
        command.Description!.ShouldContain("production");
    }

    [Fact]
    public void ShouldCreateBuildCommandWithAction()
    {
        var command = CommandFactory.CreateBuildCommand();
        command.Action.ShouldNotBeNull();
    }

    // ── Dev command ──

    [Fact]
    public void ShouldCreateDevCommandWithDescription()
    {
        var command = CommandFactory.CreateDevCommand();
        command.Name.ShouldBe("dev");
        command.Description.ShouldNotBeNull();
        command.Description!.ShouldContain("development");
    }

    [Fact]
    public void ShouldCreateDevCommandWithPortOption()
    {
        var command = CommandFactory.CreateDevCommand();
        command.Options.ShouldContain(o => o.Name == "--port");
    }

    [Fact]
    public void ShouldCreateDevCommandWithAction()
    {
        var command = CommandFactory.CreateDevCommand();
        command.Action.ShouldNotBeNull();
    }

    // ── Preview command ──

    [Fact]
    public void ShouldCreatePreviewCommandWithDescription()
    {
        var command = CommandFactory.CreatePreviewCommand();
        command.Name.ShouldBe("preview");
        command.Description.ShouldNotBeNull();
        command.Description!.ShouldContain("Preview");
    }

    [Fact]
    public void ShouldCreatePreviewCommandWithPortOption()
    {
        var command = CommandFactory.CreatePreviewCommand();
        command.Options.ShouldContain(o => o.Name == "--port");
    }

    [Fact]
    public void ShouldCreatePreviewCommandWithAction()
    {
        var command = CommandFactory.CreatePreviewCommand();
        command.Action.ShouldNotBeNull();
    }

    // ── Parsing ──

    [Fact]
    public void ShouldParseBuildCommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("build");

        result.Errors.Count.ShouldBe(0);
        result.CommandResult.Command.Name.ShouldBe("build");
    }

    [Fact]
    public void ShouldParseDevCommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("dev");

        result.Errors.Count.ShouldBe(0);
        result.CommandResult.Command.Name.ShouldBe("dev");
    }

    [Fact]
    public void ShouldParsePreviewCommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("preview");

        result.Errors.Count.ShouldBe(0);
        result.CommandResult.Command.Name.ShouldBe("preview");
    }

    [Fact]
    public void ShouldParseRootOptionOnBuildCommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("build --root /my/project");

        result.Errors.Count.ShouldBe(0);
        result.GetValue<string>("--root").ShouldBe("/my/project");
    }

    [Fact]
    public void ShouldParseRootOptionBeforeSubcommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("--root /my/project build");

        result.Errors.Count.ShouldBe(0);
        result.GetValue<string>("--root").ShouldBe("/my/project");
        result.CommandResult.Command.Name.ShouldBe("build");
    }

    [Fact]
    public void ShouldParseDevPortOption()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("dev --port 8080");

        result.Errors.Count.ShouldBe(0);
        result.GetValue<int>("--port").ShouldBe(8080);
    }

    [Fact]
    public void ShouldParsePreviewPortOption()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("preview --port 3000");

        result.Errors.Count.ShouldBe(0);
        result.GetValue<int>("--port").ShouldBe(3000);
    }

    [Fact]
    public void ShouldParseDevWithRootAndPort()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("dev --root /project --port 5000");

        result.Errors.Count.ShouldBe(0);
        result.CommandResult.Command.Name.ShouldBe("dev");
        result.GetValue<string>("--root").ShouldBe("/project");
        result.GetValue<int>("--port").ShouldBe(5000);
    }

    [Fact]
    public void ShouldReportErrorForUnknownCommand()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("unknown");

        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ShouldUseDefaultRootWhenNotSpecified()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("build");

        // Should use the current directory as default
        var rootValue = result.GetValue<string>("--root");
        rootValue.ShouldNotBeNullOrEmpty();
        rootValue.ShouldBe(Directory.GetCurrentDirectory());
    }

    [Fact]
    public void ShouldUseDefaultPortWhenNotSpecified()
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var result = rootCommand.Parse("dev");

        result.GetValue<int>("--port").ShouldBe(0);
    }

    // ── Port option on build should not exist ──

    [Fact]
    public void ShouldNotHavePortOptionOnBuildCommand()
    {
        var command = CommandFactory.CreateBuildCommand();
        command.Options.ShouldNotContain(o => o.Name == "--port");
    }

    // ── Description content ──

    [Fact]
    public void ShouldHaveBuildDescriptionAboutStaticSiteGeneration()
    {
        var command = CommandFactory.CreateBuildCommand();
        command.Description!.ShouldContain("static site generation", Case.Insensitive);
    }

    [Fact]
    public void ShouldHaveDevDescriptionAboutDevelopment()
    {
        var command = CommandFactory.CreateDevCommand();
        command.Description!.ShouldContain("development", Case.Insensitive);
    }

    [Fact]
    public void ShouldHavePreviewDescriptionAboutPreview()
    {
        var command = CommandFactory.CreatePreviewCommand();
        command.Description!.ShouldContain("preview", Case.Insensitive);
    }
}
