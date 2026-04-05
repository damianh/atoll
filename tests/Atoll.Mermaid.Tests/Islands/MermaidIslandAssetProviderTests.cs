using Atoll.Mermaid.Islands;
using Shouldly;
using Xunit;

namespace Atoll.Mermaid.Tests.Islands;

public sealed class MermaidIslandAssetProviderTests
{
    [Fact]
    public void GetAssetsShouldReturnOneDescriptor()
    {
        var provider = new MermaidIslandAssetProvider();

        var assets = provider.GetAssets().ToList();

        assets.Count.ShouldBe(1);
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectOutputPath()
    {
        var provider = new MermaidIslandAssetProvider();

        var asset = provider.GetAssets().Single();

        asset.OutputPath.ShouldBe("scripts/atoll-docs-mermaid-init.js");
    }

    [Fact]
    public void GetAssetsShouldHaveCorrectResourceName()
    {
        var provider = new MermaidIslandAssetProvider();

        var asset = provider.GetAssets().Single();

        asset.ResourceName.ShouldBe("Atoll.Mermaid.Islands.Assets.mermaid-init.js");
    }

    [Fact]
    public void GetAssetsShouldReferenceValidEmbeddedResource()
    {
        var provider = new MermaidIslandAssetProvider();
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
        var provider = new MermaidIslandAssetProvider();
        var descriptor = provider.GetAssets().Single();

        using var stream = descriptor.ResourceAssembly.GetManifestResourceStream(descriptor.ResourceName)!;

        stream.Length.ShouldBeGreaterThan(0,
            customMessage: $"Resource '{descriptor.ResourceName}' is empty");
    }
}
