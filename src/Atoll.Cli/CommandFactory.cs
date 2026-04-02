using System.CommandLine;

namespace Atoll.Cli;

/// <summary>
/// Creates the Atoll CLI command tree. Separating command construction from
/// execution allows integration tests to verify parsing and structure
/// without spawning a child process.
/// </summary>
public static class CommandFactory
{
    /// <summary>
    /// Creates the root option for specifying the project root directory.
    /// This option is recursive (available on all subcommands).
    /// </summary>
    public static Option<string> CreateRootOption()
    {
        return new Option<string>("--root")
        {
            Description = "The project root directory (defaults to current directory)",
            DefaultValueFactory = _ => Directory.GetCurrentDirectory(),
            Recursive = true,
        };
    }

    /// <summary>
    /// Creates the <c>build</c> subcommand for static site generation.
    /// </summary>
    public static Command CreateBuildCommand()
    {
        var command = new Command("build", "Build the site for production (static site generation)");

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var handler = new Commands.BuildCommandHandler();
            await handler.ExecuteAsync(root);
        });

        return command;
    }

    /// <summary>
    /// Creates the <c>dev</c> subcommand for starting the development server.
    /// </summary>
    public static Command CreateDevCommand()
    {
        var portOption = CreatePortOption();

        var command = new Command("dev", "Start the development server with live reload");
        command.Add(portOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var port = parseResult.GetValue(portOption);
            var handler = new Commands.DevCommandHandler();
            await handler.ExecuteAsync(root, port);
        });

        return command;
    }

    /// <summary>
    /// Creates the <c>preview</c> subcommand for previewing the built site.
    /// </summary>
    public static Command CreatePreviewCommand()
    {
        var portOption = CreatePortOption();

        var command = new Command("preview", "Preview the built site locally");
        command.Add(portOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var port = parseResult.GetValue(portOption);
            var handler = new Commands.PreviewCommandHandler();
            await handler.ExecuteAsync(root, port);
        });

        return command;
    }

    /// <summary>
    /// Creates the <c>new</c> subcommand for scaffolding a new Atoll project.
    /// </summary>
    public static Command CreateNewCommand()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "The name of the new project (also used as the directory name)",
        };

        var command = new Command("new", "Create a new Atoll project from a template");
        command.Add(nameArgument);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var name = parseResult.GetRequiredValue(nameArgument);
            var handler = new Commands.NewCommandHandler();
            await handler.ExecuteAsync(name, root);
        });

        return command;
    }

    /// <summary>
    /// Builds the complete root command with all subcommands registered.
    /// </summary>
    public static RootCommand CreateRootCommand()
    {
        var rootOption = CreateRootOption();

        var rootCommand = new RootCommand("Atoll — the .NET-native Astro-inspired framework");
        rootCommand.Add(rootOption);
        rootCommand.Add(CreateBuildCommand());
        rootCommand.Add(CreateDevCommand());
        rootCommand.Add(CreatePreviewCommand());
        rootCommand.Add(CreateNewCommand());

        return rootCommand;
    }

    private static Option<int> CreatePortOption()
    {
        return new Option<int>("--port")
        {
            Description = "The port to listen on (0 = use config or default 4321)",
            DefaultValueFactory = _ => 0,
        };
    }
}
