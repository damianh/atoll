using Atoll.Build.Content.Markdown;
using Atoll.Components;
using Atoll.Rendering;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Content.Markdown;

public sealed class ComponentMapTests
{
    // ── Registration ──

    [Fact]
    public void ShouldRegisterAndResolveByName()
    {
        var map = new ComponentMap().Add<SimpleComponent>("simple");

        var type = map.Resolve("simple");

        type.ShouldBe(typeof(SimpleComponent));
    }

    [Fact]
    public void ShouldResolveMultipleRegistrations()
    {
        var map = new ComponentMap()
            .Add<SimpleComponent>("simple")
            .Add<GreetingComponent>("greeting");

        map.Resolve("simple").ShouldBe(typeof(SimpleComponent));
        map.Resolve("greeting").ShouldBe(typeof(GreetingComponent));
    }

    [Fact]
    public void ShouldResolveKebabCaseName()
    {
        var map = new ComponentMap().Add<SimpleComponent>("my-component");

        map.Resolve("my-component").ShouldBe(typeof(SimpleComponent));
    }

    // ── Case insensitivity ──

    [Fact]
    public void ShouldResolveCaseInsensitively()
    {
        var map = new ComponentMap().Add<SimpleComponent>("Counter");

        map.Resolve("counter").ShouldBe(typeof(SimpleComponent));
        map.Resolve("COUNTER").ShouldBe(typeof(SimpleComponent));
        map.Resolve("Counter").ShouldBe(typeof(SimpleComponent));
    }

    // ── TryResolve ──

    [Fact]
    public void TryResolveShouldReturnTrueForRegisteredName()
    {
        var map = new ComponentMap().Add<SimpleComponent>("simple");

        var found = map.TryResolve("simple", out var type);

        found.ShouldBeTrue();
        type.ShouldBe(typeof(SimpleComponent));
    }

    [Fact]
    public void TryResolveShouldReturnFalseForUnknownName()
    {
        var map = new ComponentMap();

        var found = map.TryResolve("unknown", out var type);

        found.ShouldBeFalse();
        type.ShouldBeNull();
    }

    // ── Error cases ──

    [Fact]
    public void ShouldThrowKeyNotFoundForUnknownName()
    {
        var map = new ComponentMap().Add<SimpleComponent>("simple");

        var ex = Should.Throw<KeyNotFoundException>(() => map.Resolve("unknown"));

        ex.Message.ShouldContain("unknown");
        ex.Message.ShouldContain("simple");
    }

    [Fact]
    public void ShouldIncludeRegisteredNamesInErrorMessage()
    {
        var map = new ComponentMap()
            .Add<SimpleComponent>("alpha")
            .Add<GreetingComponent>("beta");

        var ex = Should.Throw<KeyNotFoundException>(() => map.Resolve("gamma"));

        ex.Message.ShouldContain("alpha");
        ex.Message.ShouldContain("beta");
    }

    [Fact]
    public void ShouldIncludeNoneWhenNoComponentsRegistered()
    {
        var map = new ComponentMap();

        var ex = Should.Throw<KeyNotFoundException>(() => map.Resolve("anything"));

        ex.Message.ShouldContain("(none)");
    }

    [Fact]
    public void ShouldThrowOnDuplicateRegistration()
    {
        var map = new ComponentMap().Add<SimpleComponent>("counter");

        var ex = Should.Throw<ArgumentException>(() => map.Add<GreetingComponent>("counter"));

        ex.Message.ShouldContain("counter");
    }

    [Fact]
    public void ShouldThrowOnEmptyName()
    {
        var map = new ComponentMap();

        Should.Throw<ArgumentException>(() => map.Add<SimpleComponent>(""));
    }

    [Fact]
    public void ShouldThrowOnWhitespaceName()
    {
        var map = new ComponentMap();

        Should.Throw<ArgumentException>(() => map.Add<SimpleComponent>("   "));
    }

    [Fact]
    public void ShouldThrowOnNullName()
    {
        var map = new ComponentMap();

        Should.Throw<ArgumentNullException>(() => map.Add<SimpleComponent>(null!));
    }

    [Fact]
    public void ShouldThrowOnNullResolveArgument()
    {
        var map = new ComponentMap();

        Should.Throw<ArgumentNullException>(() => map.Resolve(null!));
    }

    [Fact]
    public void ShouldThrowOnNullTryResolveArgument()
    {
        var map = new ComponentMap();

        Should.Throw<ArgumentNullException>(() => map.TryResolve(null!, out _));
    }

    // ── RegisteredNames ──

    [Fact]
    public void ShouldExposeRegisteredNames()
    {
        var map = new ComponentMap()
            .Add<SimpleComponent>("alpha")
            .Add<GreetingComponent>("beta");

        map.RegisteredNames.ShouldContain("alpha");
        map.RegisteredNames.ShouldContain("beta");
        map.RegisteredNames.Count.ShouldBe(2);
    }

    [Fact]
    public void RegisteredNamesShouldBeEmptyByDefault()
    {
        var map = new ComponentMap();

        map.RegisteredNames.ShouldBeEmpty();
    }

    // ── Fixtures ──

    private sealed class SimpleComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<span>simple</span>");
            return Task.CompletedTask;
        }
    }

    private sealed class GreetingComponent : IAtollComponent
    {
        public Task RenderAsync(RenderContext context)
        {
            context.WriteHtml("<span>greeting</span>");
            return Task.CompletedTask;
        }
    }
}
