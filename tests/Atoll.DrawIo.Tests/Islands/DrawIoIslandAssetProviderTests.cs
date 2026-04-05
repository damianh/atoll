using Atoll.DrawIo.Islands;
using Shouldly;
using Xunit;

namespace Atoll.DrawIo.Tests.Islands;

public sealed class DrawIoIslandAssetProviderTests
{
    [Fact]
    public void GetAssetsShouldReturnTwoDescriptors()
    {
        var provider = new DrawIoIslandAssetProvider();

        var assets = provider.GetAssets().ToList();

        assets.Count.ShouldBe(2);
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectViewerOutputPath()
    {
        var provider = new DrawIoIslandAssetProvider();

        var asset = provider.GetAssets().First();

        asset.OutputPath.ShouldBe("scripts/atoll-drawio-viewer.min.js");
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectInitOutputPath()
    {
        var provider = new DrawIoIslandAssetProvider();

        var asset = provider.GetAssets().Skip(1).First();

        asset.OutputPath.ShouldBe("scripts/atoll-drawio-viewer-init.js");
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectViewerResourceName()
    {
        var provider = new DrawIoIslandAssetProvider();

        var asset = provider.GetAssets().First();

        asset.ResourceName.ShouldBe("Atoll.DrawIo.Islands.Assets.viewer-static.min.js");
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectInitResourceName()
    {
        var provider = new DrawIoIslandAssetProvider();

        var asset = provider.GetAssets().Skip(1).First();

        asset.ResourceName.ShouldBe("Atoll.DrawIo.Islands.Assets.drawio-viewer-init.js");
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedViewerResource()
    {
        var provider = new DrawIoIslandAssetProvider();
        var descriptor = provider.GetAssets().First();

        var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName);

        stream.ShouldNotBeNull(
            customMessage: $"Embedded resource '{descriptor.ResourceName}' not found in assembly " +
                           $"'{descriptor.ResourceAssembly.GetName().Name}'");
        stream!.Dispose();
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedInitResource()
    {
        var provider = new DrawIoIslandAssetProvider();
        var descriptor = provider.GetAssets().Skip(1).First();

        var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName);

        stream.ShouldNotBeNull(
            customMessage: $"Embedded resource '{descriptor.ResourceName}' not found in assembly " +
                           $"'{descriptor.ResourceAssembly.GetName().Name}'");
        stream!.Dispose();
    }

    [Fact]
    public void GetAssetsShouldHaveNonEmptyViewerResourceContent()
    {
        var provider = new DrawIoIslandAssetProvider();
        var descriptor = provider.GetAssets().First();

        using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;

        stream.Length.ShouldBeGreaterThan(0,
            customMessage: $"Resource '{descriptor.ResourceName}' is empty");
    }

    [Fact]
    public void GetAssetsShouldHaveNonEmptyInitResourceContent()
    {
        var provider = new DrawIoIslandAssetProvider();
        var descriptor = provider.GetAssets().Skip(1).First();

        using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;

        stream.Length.ShouldBeGreaterThan(0,
            customMessage: $"Resource '{descriptor.ResourceName}' is empty");
    }
}
