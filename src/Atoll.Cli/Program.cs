using Atoll.Cli;

var rootCommand = CommandFactory.CreateRootCommand();
var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();
