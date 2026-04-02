using Atoll.Core.Islands;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Islands;

public sealed class IslandScriptProviderTests
{
    [Fact]
    public void GetIslandScriptShouldReturnNonEmptyContent()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetIslandScriptShouldContainCustomElementDefinition()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldContain("atoll-island");
        script.ShouldContain("customElements.define");
    }

    [Fact]
    public void GetIslandScriptShouldContainPropTypeConstants()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldContain("PROP_TYPE");
        script.ShouldContain("Value: 0");
        script.ShouldContain("Date: 3");
        script.ShouldContain("URL: 7");
    }

    [Fact]
    public void GetIslandScriptShouldContainHydrationLifecycle()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldContain("connectedCallback");
        script.ShouldContain("hydrate");
        script.ShouldContain("atoll:hydrate");
    }

    [Fact]
    public void GetIslandScriptShouldContainSlotCollection()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldContain("atoll-slot");
        script.ShouldContain("slots");
    }

    [Fact]
    public void GetIslandScriptShouldReturnCachedInstance()
    {
        var first = IslandScriptProvider.GetIslandScript();
        var second = IslandScriptProvider.GetIslandScript();

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void GetDirectivesScriptShouldReturnNonEmptyContent()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        script.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetDirectivesScriptShouldContainLoadDirective()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        script.ShouldContain("Atoll.load");
        script.ShouldContain("requestAnimationFrame");
    }

    [Fact]
    public void GetDirectivesScriptShouldContainIdleDirective()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        script.ShouldContain("Atoll.idle");
        script.ShouldContain("requestIdleCallback");
    }

    [Fact]
    public void GetDirectivesScriptShouldContainVisibleDirective()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        script.ShouldContain("Atoll.visible");
        script.ShouldContain("IntersectionObserver");
    }

    [Fact]
    public void GetDirectivesScriptShouldContainMediaDirective()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        script.ShouldContain("Atoll.media");
        script.ShouldContain("matchMedia");
    }

    [Fact]
    public void GetDirectivesScriptShouldDispatchReadyEvents()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        // Directives dispatch events via template literal: `atoll:${directive}`
        script.ShouldContain("dispatchEvent");
        script.ShouldContain("atoll:");
        // All four directive names are listed in the forEach array
        script.ShouldContain("\"load\"");
        script.ShouldContain("\"idle\"");
        script.ShouldContain("\"visible\"");
        script.ShouldContain("\"media\"");
    }

    [Fact]
    public void GetDirectivesScriptShouldReturnCachedInstance()
    {
        var first = IslandScriptProvider.GetDirectivesScript();
        var second = IslandScriptProvider.GetDirectivesScript();

        ReferenceEquals(first, second).ShouldBeTrue();
    }

    [Fact]
    public void GetScriptShouldReturnIslandScriptByResourceName()
    {
        var script = IslandScriptProvider.GetScript(IslandScriptProvider.IslandScriptResourceName);

        script.ShouldNotBeNullOrWhiteSpace();
        script.ShouldContain("atoll-island");
    }

    [Fact]
    public void GetScriptShouldReturnDirectivesScriptByResourceName()
    {
        var script = IslandScriptProvider.GetScript(IslandScriptProvider.DirectivesScriptResourceName);

        script.ShouldNotBeNullOrWhiteSpace();
        script.ShouldContain("Atoll.load");
    }

    [Fact]
    public void GetScriptShouldThrowForNullResourceName()
    {
        Should.Throw<ArgumentNullException>(() => IslandScriptProvider.GetScript(null!));
    }

    [Fact]
    public void GetScriptShouldThrowForUnknownResourceName()
    {
        var exception = Should.Throw<InvalidOperationException>(
            () => IslandScriptProvider.GetScript("NonExistent.Resource.js"));

        exception.Message.ShouldContain("not found");
        exception.Message.ShouldContain("NonExistent.Resource.js");
    }

    [Fact]
    public void IslandScriptResourceNameShouldMatchExpectedValue()
    {
        IslandScriptProvider.IslandScriptResourceName
            .ShouldBe("Atoll.Core.Islands.Assets.atoll-island.js");
    }

    [Fact]
    public void DirectivesScriptResourceNameShouldMatchExpectedValue()
    {
        IslandScriptProvider.DirectivesScriptResourceName
            .ShouldBe("Atoll.Core.Islands.Assets.atoll-directives.js");
    }

    [Fact]
    public void EmbeddedResourcesShouldBeDiscoverable()
    {
        var assembly = typeof(IslandScriptProvider).Assembly;
        var resources = assembly.GetManifestResourceNames();

        resources.ShouldContain(IslandScriptProvider.IslandScriptResourceName);
        resources.ShouldContain(IslandScriptProvider.DirectivesScriptResourceName);
    }

    [Fact]
    public void IslandScriptShouldContainPropDeserialization()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldContain("reviveObject");
        script.ShouldContain("reviveTuple");
    }

    [Fact]
    public void IslandScriptShouldContainParentCoordination()
    {
        var script = IslandScriptProvider.GetIslandScript();

        // Top-down parent coordination: wait for parent island to hydrate first
        script.ShouldContain("parentElement");
        script.ShouldContain("closest");
    }

    [Fact]
    public void IslandScriptShouldContainUnmountHandling()
    {
        var script = IslandScriptProvider.GetIslandScript();

        script.ShouldContain("atoll:unmount");
        script.ShouldContain("disconnectedCallback");
    }

    [Fact]
    public void DirectivesScriptShouldContainIdleCallbackFallback()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        // Fallback for browsers without requestIdleCallback
        script.ShouldContain("setTimeout");
    }

    [Fact]
    public void DirectivesScriptShouldContainVisibleRootMarginSupport()
    {
        var script = IslandScriptProvider.GetDirectivesScript();

        script.ShouldContain("rootMargin");
    }
}
