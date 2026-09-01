using Atoll.Lagoon.Islands;

namespace Atoll.Lagoon.Tests.Islands;

public sealed class TabsAssetTests
{
    [Fact]
    public void ShouldReadSyncKeyFromIslandRootOrRenderedTabsWrapper()
    {
        var asset = new LagoonIslandAssetProvider()
            .GetAssets()
            .Single(asset => asset.OutputPath == "scripts/atoll-docs-tabs.js");

        using var stream = asset.ResourceAssembly.GetManifestResourceStream(asset.ResourceName);
        stream.ShouldNotBeNull();
        using var reader = new StreamReader(stream);
        var script = reader.ReadToEnd();

        script.ShouldContain("element.dataset.syncKey");
        script.ShouldContain("element.querySelector('.tabs')?.dataset.syncKey");
    }
}
