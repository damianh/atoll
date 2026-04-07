using Atoll.Charts.Islands;

namespace Atoll.Charts.Tests.Islands;

public sealed class ChartIslandAssetProviderTests
{
    [Fact]
    public void ShouldReturnTwoAssets()
    {
        var provider = new ChartIslandAssetProvider();

        provider.GetAssets().Count().ShouldBe(2);
    }

    [Fact]
    public void ShouldContainVendorAsset()
    {
        var provider = new ChartIslandAssetProvider();

        provider.GetAssets().ShouldContain(a => a.OutputPath == "scripts/atoll-charts-vendor.min.js");
    }

    [Fact]
    public void ShouldContainInitAsset()
    {
        var provider = new ChartIslandAssetProvider();

        provider.GetAssets().ShouldContain(a => a.OutputPath == "scripts/atoll-charts-init.js");
    }

    [Fact]
    public void EmbeddedResourcesShouldBeResolvable()
    {
        var provider = new ChartIslandAssetProvider();

        foreach (var asset in provider.GetAssets())
        {
            using var stream = asset.ResourceAssembly.GetManifestResourceStream(asset.ResourceName);
            stream.ShouldNotBeNull($"Embedded resource '{asset.ResourceName}' could not be resolved from assembly '{asset.ResourceAssembly.GetName().Name}'");
        }
    }
}
