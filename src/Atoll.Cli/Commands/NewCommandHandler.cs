namespace Atoll.Cli.Commands;

/// <summary>
/// Handles the <c>atoll new</c> command. Scaffolds a new Atoll project
/// with a directory structure, sample page, layout, and configuration.
/// </summary>
public sealed class NewCommandHandler
{
    /// <summary>
    /// Executes the new command.
    /// </summary>
    /// <param name="name">The project name (also used as the directory name).</param>
    /// <param name="parentDirectory">The parent directory to create the project in.</param>
    public async Task ExecuteAsync(string name, string parentDirectory)
    {
        var projectDir = Path.Combine(parentDirectory, name);

        if (Directory.Exists(projectDir) && Directory.GetFileSystemEntries(projectDir).Length > 0)
        {
            Console.WriteLine($"Error: Directory '{projectDir}' already exists and is not empty.");
            return;
        }

        Console.WriteLine($"Atoll — creating new project '{name}'...");

        // Create directory structure
        Directory.CreateDirectory(projectDir);
        Directory.CreateDirectory(Path.Combine(projectDir, "Pages"));
        Directory.CreateDirectory(Path.Combine(projectDir, "Layouts"));
        Directory.CreateDirectory(Path.Combine(projectDir, "public"));

        // Write project files
        await WriteProjectFileAsync(projectDir, name);
        await WriteConfigFileAsync(projectDir);
        await WriteProgramFileAsync(projectDir, name);
        await WriteHomePageAsync(projectDir, name);
        await WriteMainLayoutAsync(projectDir);
        await WriteGitIgnoreAsync(projectDir);

        Console.WriteLine();
        Console.WriteLine($"  Created project at {projectDir}");
        Console.WriteLine();
        Console.WriteLine("  Next steps:");
        Console.WriteLine($"    cd {name}");
        Console.WriteLine("    dotnet run");
        Console.WriteLine();
    }

    private static async Task WriteProjectFileAsync(string projectDir, string name)
    {
        var escapedName = EscapeXml(name);
        var content = $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <RootNamespace>{{escapedName}}</RootNamespace>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Atoll.Middleware" Version="*" />
              </ItemGroup>
            </Project>
            """;

        await File.WriteAllTextAsync(Path.Combine(projectDir, name + ".csproj"), content);
    }

    private static async Task WriteConfigFileAsync(string projectDir)
    {
        var content = """
            {
              "site": "",
              "base": "/",
              "outDir": "dist",
              "server": {
                "host": "localhost",
                "port": 4321
              }
            }
            """;

        await File.WriteAllTextAsync(Path.Combine(projectDir, "atoll.json"), content);
    }

    private static async Task WriteProgramFileAsync(string projectDir, string name)
    {
        var content = """
            using Atoll.Middleware.Server.Hosting;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAtoll(options =>
            {
                options.Assemblies.Add(typeof(Program).Assembly);
            });

            var app = builder.Build();

            app.UseAtoll();

            app.Run();
            """;

        await File.WriteAllTextAsync(Path.Combine(projectDir, "Program.cs"), content);
    }

    private static async Task WriteHomePageAsync(string projectDir, string name)
    {
        var namespaceName = SanitizeNamespace(name);
        var escapedName = name.Replace("\"", "\\\"");
        var content = $$"""
            using Atoll.Core.Components;
            using Atoll.Routing;

            namespace {{namespaceName}}.Pages;

            [Layout(typeof(Layouts.MainLayout))]
            public sealed class Index : AtollComponent, IAtollPage
            {
                protected override Task RenderCoreAsync(RenderContext context)
                {
                    WriteHtml("<h1>Welcome to {{escapedName}}</h1>");
                    WriteHtml("<p>This is your new Atoll site. Edit <code>Pages/Index.cs</code> to get started.</p>");
                    return Task.CompletedTask;
                }
            }
            """;

        await File.WriteAllTextAsync(Path.Combine(projectDir, "Pages", "Index.cs"), content);
    }

    private static async Task WriteMainLayoutAsync(string projectDir)
    {
        var content = """
            using Atoll.Core.Components;

            namespace Layouts;

            public sealed class MainLayout : AtollComponent
            {
                protected override async Task RenderCoreAsync(RenderContext context)
                {
                    WriteHtml("<!DOCTYPE html>");
                    WriteHtml("<html lang=\"en\">");
                    WriteHtml("<head>");
                    WriteHtml("  <meta charset=\"UTF-8\" />");
                    WriteHtml("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
                    WriteHtml("  <title>Atoll Site</title>");
                    WriteHtml("</head>");
                    WriteHtml("<body>");
                    await RenderSlotAsync(context);
                    WriteHtml("</body>");
                    WriteHtml("</html>");
                }
            }
            """;

        await File.WriteAllTextAsync(Path.Combine(projectDir, "Layouts", "MainLayout.cs"), content);
    }

    private static async Task WriteGitIgnoreAsync(string projectDir)
    {
        var content = """
            bin/
            obj/
            dist/
            .vs/
            *.user
            """;

        await File.WriteAllTextAsync(Path.Combine(projectDir, ".gitignore"), content);
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private static string SanitizeNamespace(string name)
    {
        // Replace characters that are invalid for C# identifiers
        var chars = new char[name.Length];
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                chars[i] = c;
            }
            else
            {
                chars[i] = '_';
            }
        }

        var result = new string(chars);

        // Ensure the namespace doesn't start with a digit
        if (result.Length > 0 && char.IsDigit(result[0]))
        {
            result = "_" + result;
        }

        return result.Length == 0 ? "AtollProject" : result;
    }
}
