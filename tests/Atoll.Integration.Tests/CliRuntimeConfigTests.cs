using System.Text.Json;

namespace Atoll.Integration.Tests;

public sealed class CliRuntimeConfigTests
{
    [Fact]
    public void ShouldRollForwardToLatestInstalledMajorRuntime()
    {
        var runtimeConfigPath = Path.Combine(AppContext.BaseDirectory, "Atoll.Cli.runtimeconfig.json");
        using var runtimeConfig = JsonDocument.Parse(File.ReadAllText(runtimeConfigPath));

        runtimeConfig.RootElement
            .GetProperty("runtimeOptions")
            .GetProperty("rollForward")
            .GetString()
            .ShouldBe("LatestMajor");
    }
}
