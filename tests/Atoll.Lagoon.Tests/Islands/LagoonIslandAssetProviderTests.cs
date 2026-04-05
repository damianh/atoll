using Atoll.Lagoon.Islands;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Islands;

public sealed class LagoonIslandAssetProviderTests
{
    private static readonly string[] ExpectedOutputPaths =
    [
        "scripts/atoll-docs-search-dialog.js",
        "scripts/atoll-theme-toggle.js",
        "scripts/atoll-docs-mobile-nav.js",
    ];

    [Fact]
    public void GetAssetsShouldReturnThreeDescriptors()
    {
        var provider = new LagoonIslandAssetProvider();

        var assets = provider.GetAssets().ToList();

        assets.Count.ShouldBe(3);
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectOutputPaths()
    {
        var provider = new LagoonIslandAssetProvider();

        var paths = provider.GetAssets().Select(a => a.OutputPath).ToList();

        foreach (var expected in ExpectedOutputPaths)
        {
            paths.ShouldContain(expected);
        }
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedResources()
    {
        var provider = new LagoonIslandAssetProvider();

        foreach (var descriptor in provider.GetAssets())
        {
            var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName);
            stream.ShouldNotBeNull(
                customMessage: $"Embedded resource '{descriptor.ResourceName}' not found in assembly " +
                               $"'{descriptor.ResourceAssembly.GetName().Name}'");
            stream!.Dispose();
        }
    }

    [Fact]
    public void GetAssetsShouldHaveNonEmptyResourceContent()
    {
        var provider = new LagoonIslandAssetProvider();

        foreach (var descriptor in provider.GetAssets())
        {
            using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;
            stream.Length.ShouldBeGreaterThan(0,
                customMessage: $"Resource '{descriptor.ResourceName}' is empty");
        }
    }
}
