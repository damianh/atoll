using Atoll.Components;
using Atoll.Instructions;
using Atoll.Islands;

namespace Atoll.Tests.Islands;

public sealed class DirectiveExtractorTests
{
    // ── ClientLoadAttribute tests ──

    [Fact]
    public void ClientLoadShouldHaveLoadDirectiveType()
    {
        var attr = new ClientLoadAttribute();

        attr.DirectiveType.ShouldBe(ClientDirectiveType.Load);
    }

    [Fact]
    public void ClientLoadShouldHaveNullValue()
    {
        var attr = new ClientLoadAttribute();

        attr.Value.ShouldBeNull();
    }

    // ── ClientIdleAttribute tests ──

    [Fact]
    public void ClientIdleShouldHaveIdleDirectiveType()
    {
        var attr = new ClientIdleAttribute();

        attr.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    [Fact]
    public void ClientIdleShouldHaveNullValue()
    {
        var attr = new ClientIdleAttribute();

        attr.Value.ShouldBeNull();
    }

    // ── ClientVisibleAttribute tests ──

    [Fact]
    public void ClientVisibleShouldHaveVisibleDirectiveType()
    {
        var attr = new ClientVisibleAttribute();

        attr.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
    }

    [Fact]
    public void ClientVisibleWithoutRootMarginShouldHaveNullValue()
    {
        var attr = new ClientVisibleAttribute();

        attr.Value.ShouldBeNull();
        attr.RootMargin.ShouldBeNull();
    }

    [Fact]
    public void ClientVisibleWithRootMarginShouldReturnRootMarginAsValue()
    {
        var attr = new ClientVisibleAttribute { RootMargin = "200px" };

        attr.Value.ShouldBe("200px");
        attr.RootMargin.ShouldBe("200px");
    }

    [Fact]
    public void ClientVisibleWithComplexRootMarginShouldReturnFullMarginAsValue()
    {
        var attr = new ClientVisibleAttribute { RootMargin = "0px 0px 200px 0px" };

        attr.Value.ShouldBe("0px 0px 200px 0px");
    }

    // ── ClientMediaAttribute tests ──

    [Fact]
    public void ClientMediaShouldHaveMediaDirectiveType()
    {
        var attr = new ClientMediaAttribute("(max-width: 768px)");

        attr.DirectiveType.ShouldBe(ClientDirectiveType.Media);
    }

    [Fact]
    public void ClientMediaShouldStoreMediaQuery()
    {
        var attr = new ClientMediaAttribute("(max-width: 768px)");

        attr.MediaQuery.ShouldBe("(max-width: 768px)");
    }

    [Fact]
    public void ClientMediaShouldReturnMediaQueryAsValue()
    {
        var attr = new ClientMediaAttribute("(max-width: 768px)");

        attr.Value.ShouldBe("(max-width: 768px)");
    }

    [Fact]
    public void ClientMediaShouldThrowWhenMediaQueryIsNull()
    {
        Should.Throw<ArgumentNullException>(() => new ClientMediaAttribute(null!));
    }

    [Fact]
    public void ClientMediaShouldThrowWhenMediaQueryIsEmpty()
    {
        Should.Throw<ArgumentException>(() => new ClientMediaAttribute(""));
    }

    [Fact]
    public void ClientMediaShouldThrowWhenMediaQueryIsWhitespace()
    {
        Should.Throw<ArgumentException>(() => new ClientMediaAttribute("   "));
    }

    // ── DirectiveExtractor.GetDirective tests ──

    [Fact]
    public void GetDirectiveShouldReturnNullForComponentWithoutDirective()
    {
        var result = DirectiveExtractor.GetDirective(typeof(NoDirectiveComponent));

        result.ShouldBeNull();
    }

    [Fact]
    public void GetDirectiveShouldReturnLoadForClientLoadComponent()
    {
        var result = DirectiveExtractor.GetDirective(typeof(LoadComponent));

        result.ShouldNotBeNull();
        result.DirectiveType.ShouldBe(ClientDirectiveType.Load);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void GetDirectiveShouldReturnIdleForClientIdleComponent()
    {
        var result = DirectiveExtractor.GetDirective(typeof(IdleComponent));

        result.ShouldNotBeNull();
        result.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void GetDirectiveShouldReturnVisibleForClientVisibleComponent()
    {
        var result = DirectiveExtractor.GetDirective(typeof(VisibleComponent));

        result.ShouldNotBeNull();
        result.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void GetDirectiveShouldReturnVisibleWithRootMarginForClientVisibleComponentWithMargin()
    {
        var result = DirectiveExtractor.GetDirective(typeof(VisibleWithMarginComponent));

        result.ShouldNotBeNull();
        result.DirectiveType.ShouldBe(ClientDirectiveType.Visible);
        result.Value.ShouldBe("200px");
    }

    [Fact]
    public void GetDirectiveShouldReturnMediaWithQueryForClientMediaComponent()
    {
        var result = DirectiveExtractor.GetDirective(typeof(MediaComponent));

        result.ShouldNotBeNull();
        result.DirectiveType.ShouldBe(ClientDirectiveType.Media);
        result.Value.ShouldBe("(max-width: 768px)");
    }

    [Fact]
    public void GetDirectiveShouldThrowForNullComponentType()
    {
        Should.Throw<ArgumentNullException>(() => DirectiveExtractor.GetDirective(null!));
    }

    [Fact]
    public void GetDirectiveShouldReturnNullForNonComponentType()
    {
        var result = DirectiveExtractor.GetDirective(typeof(string));

        result.ShouldBeNull();
    }

    [Fact]
    public void GetDirectiveShouldReturnNullForAbstractComponentWithoutDirective()
    {
        var result = DirectiveExtractor.GetDirective(typeof(AtollComponent));

        result.ShouldBeNull();
    }

    // ── DirectiveExtractor.HasDirective tests ──

    [Fact]
    public void HasDirectiveShouldReturnFalseForComponentWithoutDirective()
    {
        DirectiveExtractor.HasDirective(typeof(NoDirectiveComponent)).ShouldBeFalse();
    }

    [Fact]
    public void HasDirectiveShouldReturnTrueForClientLoadComponent()
    {
        DirectiveExtractor.HasDirective(typeof(LoadComponent)).ShouldBeTrue();
    }

    [Fact]
    public void HasDirectiveShouldReturnTrueForClientIdleComponent()
    {
        DirectiveExtractor.HasDirective(typeof(IdleComponent)).ShouldBeTrue();
    }

    [Fact]
    public void HasDirectiveShouldReturnTrueForClientVisibleComponent()
    {
        DirectiveExtractor.HasDirective(typeof(VisibleComponent)).ShouldBeTrue();
    }

    [Fact]
    public void HasDirectiveShouldReturnTrueForClientMediaComponent()
    {
        DirectiveExtractor.HasDirective(typeof(MediaComponent)).ShouldBeTrue();
    }

    [Fact]
    public void HasDirectiveShouldThrowForNullComponentType()
    {
        Should.Throw<ArgumentNullException>(() => DirectiveExtractor.HasDirective(null!));
    }

    [Fact]
    public void HasDirectiveShouldReturnFalseForNonComponentType()
    {
        DirectiveExtractor.HasDirective(typeof(string)).ShouldBeFalse();
    }

    // ── Inheritance behavior tests ──

    [Fact]
    public void GetDirectiveShouldNotInheritDirectiveFromBaseClass()
    {
        // ClientDirectiveAttribute has Inherited = false, so derived classes
        // should NOT inherit the directive from their base class
        var result = DirectiveExtractor.GetDirective(typeof(DerivedFromLoadComponent));

        result.ShouldBeNull();
    }

    [Fact]
    public void HasDirectiveShouldReturnFalseForDerivedClassWithoutOwnDirective()
    {
        DirectiveExtractor.HasDirective(typeof(DerivedFromLoadComponent)).ShouldBeFalse();
    }

    [Fact]
    public void GetDirectiveShouldReturnOwnDirectiveOnDerivedClass()
    {
        var result = DirectiveExtractor.GetDirective(typeof(DerivedWithOwnDirective));

        result.ShouldNotBeNull();
        result.DirectiveType.ShouldBe(ClientDirectiveType.Idle);
    }

    // ── DirectiveInfo record equality tests ──

    [Fact]
    public void DirectiveInfoShouldSupportValueEquality()
    {
        var info1 = new DirectiveInfo(ClientDirectiveType.Load, null);
        var info2 = new DirectiveInfo(ClientDirectiveType.Load, null);

        info1.ShouldBe(info2);
    }

    [Fact]
    public void DirectiveInfoShouldNotBeEqualWhenDirectiveTypeDiffers()
    {
        var info1 = new DirectiveInfo(ClientDirectiveType.Load, null);
        var info2 = new DirectiveInfo(ClientDirectiveType.Idle, null);

        info1.ShouldNotBe(info2);
    }

    [Fact]
    public void DirectiveInfoShouldNotBeEqualWhenValueDiffers()
    {
        var info1 = new DirectiveInfo(ClientDirectiveType.Media, "(max-width: 768px)");
        var info2 = new DirectiveInfo(ClientDirectiveType.Media, "(min-width: 1024px)");

        info1.ShouldNotBe(info2);
    }

    [Fact]
    public void DirectiveInfoShouldBeEqualWhenBothHaveSameMediaQuery()
    {
        var info1 = new DirectiveInfo(ClientDirectiveType.Media, "(max-width: 768px)");
        var info2 = new DirectiveInfo(ClientDirectiveType.Media, "(max-width: 768px)");

        info1.ShouldBe(info2);
    }

    // ── Test component fixtures ──

    private sealed class NoDirectiveComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>No directive</div>");
            return Task.CompletedTask;
        }
    }

    [ClientLoad]
    private sealed class LoadComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Load</div>");
            return Task.CompletedTask;
        }
    }

    [ClientIdle]
    private sealed class IdleComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Idle</div>");
            return Task.CompletedTask;
        }
    }

    [ClientVisible]
    private sealed class VisibleComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Visible</div>");
            return Task.CompletedTask;
        }
    }

    [ClientVisible(RootMargin = "200px")]
    private sealed class VisibleWithMarginComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Visible with margin</div>");
            return Task.CompletedTask;
        }
    }

    [ClientMedia("(max-width: 768px)")]
    private sealed class MediaComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Media</div>");
            return Task.CompletedTask;
        }
    }

    // For inheritance tests - base class with directive
    [ClientLoad]
    private class BaseLoadComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Base load</div>");
            return Task.CompletedTask;
        }
    }

    // Derived without its own directive — should NOT inherit
    private sealed class DerivedFromLoadComponent : BaseLoadComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Derived from load</div>");
            return Task.CompletedTask;
        }
    }

    // Derived with its own directive — should use its own
    [ClientIdle]
    private sealed class DerivedWithOwnDirective : BaseLoadComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            WriteHtml("<div>Derived with own directive</div>");
            return Task.CompletedTask;
        }
    }
}
