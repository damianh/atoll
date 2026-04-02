namespace Atoll.Cli;

/// <summary>
/// Entry point for the Atoll CLI tool.
/// </summary>
internal sealed class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = CommandFactory.CreateRootCommand();
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }
}
