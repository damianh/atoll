using Shouldly;
using Xunit;

namespace Atoll.Giscus.Tests;

public sealed class GiscusIslandAssetProviderTests
{
    [Fact]
    public void GetAssetsShouldReturnOneDescriptor()
    {
        var provider = new GiscusIslandAssetProvider();

        var assets = provider.GetAssets().ToList();

        assets.Count.ShouldBe(1);
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectOutputPath()
    {
        var provider = new GiscusIslandAssetProvider();

        var paths = provider.GetAssets().Select(a => a.OutputPath).ToList();

        paths.ShouldContain("scripts/atoll-giscus.js");
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedResource()
    {
        var provider = new GiscusIslandAssetProvider();

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
        var provider = new GiscusIslandAssetProvider();

        foreach (var descriptor in provider.GetAssets())
        {
            using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;
            stream.Length.ShouldBeGreaterThan(0,
                customMessage: $"Resource '{descriptor.ResourceName}' is empty");
        }
    }
}
