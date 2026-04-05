using Atoll.DrawIo.Islands;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Islands;

public sealed class DrawIoIslandAssetProviderTests
{
    [Fact]
    public void GetAssetsShouldReturnOneDescriptor()
    {
        var provider = new DrawIoIslandAssetProvider();

        var assets = provider.GetAssets().ToList();

        assets.Count.ShouldBe(1);
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectOutputPath()
    {
        var provider = new DrawIoIslandAssetProvider();

        var asset = provider.GetAssets().Single();

        asset.OutputPath.ShouldBe("scripts/atoll-drawio-interactive.js");
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectResourceName()
    {
        var provider = new DrawIoIslandAssetProvider();

        var asset = provider.GetAssets().Single();

        asset.ResourceName.ShouldBe("Atoll.DrawIo.Islands.Assets.drawio-interactive.js");
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedResource()
    {
        var provider = new DrawIoIslandAssetProvider();
        var descriptor = provider.GetAssets().Single();

        var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName);

        stream.ShouldNotBeNull(
            customMessage: $"Embedded resource '{descriptor.ResourceName}' not found in assembly " +
                           $"'{descriptor.ResourceAssembly.GetName().Name}'");
        stream!.Dispose();
    }

    [Fact]
    public void GetAssetsShouldHaveNonEmptyResourceContent()
    {
        var provider = new DrawIoIslandAssetProvider();
        var descriptor = provider.GetAssets().Single();

        using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;

        stream.Length.ShouldBeGreaterThan(0,
            customMessage: $"Resource '{descriptor.ResourceName}' is empty");
    }
}
