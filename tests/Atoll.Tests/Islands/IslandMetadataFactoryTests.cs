using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;
using Atoll.Rendering;

namespace Atoll.Tests.Islands;

public sealed class IslandMetadataFactoryTests
{
    // ─── Test fixtures ─────────────────────────────────────────────────────

    [ClientIdle]
    private sealed class IdleVanillaIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/idle-widget.js";

        protected override Task RenderCoreAsync(RenderContext context) =>
            Task.CompletedTask;
    }

    [ClientLoad]
    private sealed class LoadVanillaIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/load-widget.js";
        public override string ClientExportName => "LoadWidget";

        protected override Task RenderCoreAsync(RenderContext context) =>
            Task.CompletedTask;
    }

    [ClientLoad]
    private sealed class LoadWebIsland : WebComponentIsland
    {
        public override string TagName => "load-web-island";
        public override string ClientModuleUrl => "/components/load-web-island.js";

        protected override Task RenderLightDomAsync(RenderContext context) =>
            Task.CompletedTask;
    }

    [ClientVisible(RootMargin = "100px")]
    private sealed class VisibleIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/visible.js";

        protected override Task RenderCoreAsync(RenderContext context) =>
            Task.CompletedTask;
    }

    [ClientMedia("(max-width: 640px)")]
    private sealed class MediaIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/media.js";

        protected override Task RenderCoreAsync(RenderContext context) =>
            Task.CompletedTask;
    }

    private sealed class NoDirectiveIsland : VanillaJsIsland
    {
        public override string ClientModuleUrl => "/scripts/no-directive.js";

        protected override Task RenderCoreAsync(RenderContext context) =>
            Task.CompletedTask;
    }

    // ─── Tests ─────────────────────────────────────────────────────────────

    [Fact]
    public void CreateShouldReturnMetadataForVanillaJsIslandWithIdleDirective()
    {
        var island = new IdleVanillaIsland();

        var metadata = IslandMetadataFactory.Create(island);

        metadata.ShouldNotBeNull();
        metadata.ComponentUrl.ShouldBe("/scripts/idle-widget.js");
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
        metadata.ComponentExport.ShouldBe("default");
        metadata.DirectiveValue.ShouldBeNull();
        metadata.DisplayName.ShouldBe("IdleVanillaIsland");
    }

    [Fact]
    public void CreateShouldReturnMetadataForVanillaJsIslandWithLoadDirective()
    {
        var island = new LoadVanillaIsland();

        var metadata = IslandMetadataFactory.Create(island);

        metadata.ShouldNotBeNull();
        metadata.ComponentUrl.ShouldBe("/scripts/load-widget.js");
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
        metadata.ComponentExport.ShouldBe("LoadWidget");
        metadata.DisplayName.ShouldBe("LoadVanillaIsland");
    }

    [Fact]
    public void CreateShouldReturnMetadataForWebComponentIslandWithLoadDirective()
    {
        var island = new LoadWebIsland();

        var metadata = IslandMetadataFactory.Create(island);

        metadata.ShouldNotBeNull();
        metadata.ComponentUrl.ShouldBe("/components/load-web-island.js");
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Load);
        metadata.DisplayName.ShouldBe("LoadWebIsland");
    }

    [Fact]
    public void CreateShouldReturnMetadataWithDirectiveValueForVisibleDirective()
    {
        var island = new VisibleIsland();

        var metadata = IslandMetadataFactory.Create(island);

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        metadata.DirectiveValue.ShouldBe("100px");
    }

    [Fact]
    public void CreateShouldReturnMetadataWithDirectiveValueForMediaDirective()
    {
        var island = new MediaIsland();

        var metadata = IslandMetadataFactory.Create(island);

        metadata.ShouldNotBeNull();
        metadata.DirectiveType.ShouldBe(ClientDirectiveType.Media);
        metadata.DirectiveValue.ShouldBe("(max-width: 640px)");
    }

    [Fact]
    public void CreateShouldReturnNullForComponentWithoutDirectiveAttribute()
    {
        var island = new NoDirectiveIsland();

        var metadata = IslandMetadataFactory.Create(island);

        metadata.ShouldBeNull();
    }

    [Fact]
    public void CreateShouldThrowArgumentNullExceptionForNullInput()
    {
        Should.Throw<ArgumentNullException>(() => IslandMetadataFactory.Create(null!));
    }
}
