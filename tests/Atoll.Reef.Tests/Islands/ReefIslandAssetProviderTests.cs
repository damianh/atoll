using Atoll.Reef.Islands;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Islands;

public sealed class ReefIslandAssetProviderTests
{
    private static readonly string[] ExpectedOutputPaths =
    [
        "scripts/atoll-reef-article-filter.js",
        "scripts/atoll-reef-view-toggle.js",
    ];

    [Fact]
    public void GetAssetsShouldReturnTwoDescriptors()
    {
        var provider = new ReefIslandAssetProvider();

        var assets = provider.GetAssets().ToList();

        assets.Count.ShouldBe(2);
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectOutputPaths()
    {
        var provider = new ReefIslandAssetProvider();

        var paths = provider.GetAssets().Select(a => a.OutputPath).ToList();

        foreach (var expected in ExpectedOutputPaths)
        {
            paths.ShouldContain(expected);
        }
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedResources()
    {
        var provider = new ReefIslandAssetProvider();

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
        var provider = new ReefIslandAssetProvider();

        foreach (var descriptor in provider.GetAssets())
        {
            using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;
            stream.Length.ShouldBeGreaterThan(0,
                customMessage: $"Resource '{descriptor.ResourceName}' is empty");
        }
    }
}
