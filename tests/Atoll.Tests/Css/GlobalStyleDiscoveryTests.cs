using System.Reflection;
using Atoll.Css;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Css;

public sealed class GlobalStyleDiscoveryTests
{
    // ── Fixture types ──

    [GlobalStyle]
    [Styles(".global-thing { color: red; }")]
    private sealed class GlobalStyledType
    {
    }

    [Styles(".scoped-thing { color: blue; }")]
    private sealed class ScopedStyledType
    {
    }

    [GlobalStyle]
    private sealed class GlobalNoStylesType
    {
    }

    private sealed class PlainType
    {
    }

    // ── Single assembly ──

    [Fact]
    public void DiscoverGlobalStylesShouldFindGlobalStyledTypes()
    {
        var assembly = typeof(GlobalStyleDiscoveryTests).Assembly;

        var result = GlobalStyleDiscovery.DiscoverGlobalStyles(assembly);

        result.ShouldContain(typeof(GlobalStyledType));
    }

    [Fact]
    public void DiscoverGlobalStylesShouldSkipScopedStyles()
    {
        var assembly = typeof(GlobalStyleDiscoveryTests).Assembly;

        var result = GlobalStyleDiscovery.DiscoverGlobalStyles(assembly);

        result.ShouldNotContain(typeof(ScopedStyledType));
    }

    [Fact]
    public void DiscoverGlobalStylesShouldSkipTypesWithoutStyles()
    {
        var assembly = typeof(GlobalStyleDiscoveryTests).Assembly;

        var result = GlobalStyleDiscovery.DiscoverGlobalStyles(assembly);

        result.ShouldNotContain(typeof(GlobalNoStylesType));
    }

    [Fact]
    public void DiscoverGlobalStylesShouldSkipPlainTypes()
    {
        var assembly = typeof(GlobalStyleDiscoveryTests).Assembly;

        var result = GlobalStyleDiscovery.DiscoverGlobalStyles(assembly);

        result.ShouldNotContain(typeof(PlainType));
    }

    // ── Multiple assemblies ──

    [Fact]
    public void DiscoverGlobalStylesShouldDeduplicateAcrossAssemblies()
    {
        var assembly = typeof(GlobalStyleDiscoveryTests).Assembly;

        // Pass the same assembly twice — should not duplicate results
        var result = GlobalStyleDiscovery.DiscoverGlobalStyles([assembly, assembly]);

        result.Count(t => t == typeof(GlobalStyledType)).ShouldBe(1);
    }

    [Fact]
    public void DiscoverGlobalStylesShouldReturnEmptyForAssemblyWithNoGlobalStyles()
    {
        // System.Runtime has no Atoll attributes
        var assembly = typeof(object).Assembly;

        var result = GlobalStyleDiscovery.DiscoverGlobalStyles(assembly);

        result.ShouldBeEmpty();
    }

    // ── Null guards ──

    [Fact]
    public void DiscoverGlobalStylesShouldThrowOnNullAssembly()
    {
        Should.Throw<ArgumentNullException>(() =>
            GlobalStyleDiscovery.DiscoverGlobalStyles((Assembly)null!));
    }

    [Fact]
    public void DiscoverGlobalStylesShouldThrowOnNullAssembliesEnumerable()
    {
        Should.Throw<ArgumentNullException>(() =>
            GlobalStyleDiscovery.DiscoverGlobalStyles((IEnumerable<Assembly>)null!));
    }
}
