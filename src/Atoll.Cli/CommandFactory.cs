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
        var noCacheOption = new Option<bool>("--no-cache")
        {
            Description = "Skip the incremental build cache and force a full rebuild",
            DefaultValueFactory = _ => false,
        };

        var command = new Command("build", "Build the site for production (static site generation)");
        command.Add(noCacheOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var noCache = parseResult.GetValue(noCacheOption);
            var handler = new Commands.BuildCommandHandler();
            await handler.ExecuteAsync(root, noCache, cancellationToken);
        });

        return command;
    }

    /// <summary>
    /// Creates the <c>dev</c> subcommand for starting the development server.
    /// </summary>
    public static Command CreateDevCommand()
    {
        var portOption = CreatePortOption();
        var writeDistOption = CreateWriteDistOption();

        var command = new Command("dev", "Start the development server with live reload");
        command.Add(portOption);
        command.Add(writeDistOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var port = parseResult.GetValue(portOption);
            var writeDist = parseResult.GetValue(writeDistOption);
            var handler = new Commands.DevCommandHandler();
            await handler.ExecuteAsync(root, port, writeDist, cancellationToken);
        });

        return command;
    }

    /// <summary>
    /// Creates the <c>--write-dist</c> option for the <c>dev</c> command.
    /// </summary>
    public static Option<bool> CreateWriteDistOption()
    {
        return new Option<bool>("--write-dist")
        {
            Description = "Write all pages and assets to the output directory after each rebuild (useful for AppHost or external static file servers)",
            DefaultValueFactory = _ => false,
        };
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
            await handler.ExecuteAsync(root, port, cancellationToken);
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

        var templateOption = new Option<string>("--template")
        {
            Description = "The template to use: empty, starter, blog, or portfolio (defaults to starter)",
            DefaultValueFactory = _ => "starter",
        };
        templateOption.Aliases.Add("-t");
        templateOption.AcceptOnlyFromAmong("empty", "starter", "blog", "portfolio");

        var command = new Command("new", "Create a new Atoll project from a template");
        command.Add(nameArgument);
        command.Add(templateOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var root = parseResult.GetValue<string>("--root")!;
            var name = parseResult.GetRequiredValue(nameArgument);
            var template = parseResult.GetValue(templateOption)!;
            var handler = new Commands.NewCommandHandler();
            await handler.ExecuteAsync(name, root, template);
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
