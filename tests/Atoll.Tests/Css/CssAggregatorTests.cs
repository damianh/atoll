using Atoll.Components;
using Atoll.Css;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Css;

public sealed class CssAggregatorTests
{
    // ── Add by type ──

    [Fact]
    public void ShouldAddCssFromStyledComponent()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(CardComponent)).ShouldBeTrue();
        aggregator.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldReturnFalseForComponentWithoutStyles()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(UnstyledComponent)).ShouldBeFalse();
        aggregator.Count.ShouldBe(0);
    }

    [Fact]
    public void ShouldDeduplicateSameComponentType()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(CardComponent)).ShouldBeTrue();
        aggregator.Add(typeof(CardComponent)).ShouldBeFalse();
        aggregator.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldAllowDifferentComponentTypes()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(CardComponent)).ShouldBeTrue();
        aggregator.Add(typeof(PanelComponent)).ShouldBeTrue();
        aggregator.Count.ShouldBe(2);
    }

    // ── Add by identifier ──

    [Fact]
    public void ShouldAddRawCssWithIdentifier()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("reset", "* { margin: 0; }", true).ShouldBeTrue();
        aggregator.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldDeduplicateSameIdentifier()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("reset", "* { margin: 0; }", true).ShouldBeTrue();
        aggregator.Add("reset", "* { padding: 0; }", true).ShouldBeFalse();
        aggregator.Count.ShouldBe(1);
    }

    [Fact]
    public void ShouldReturnFalseForEmptyCss()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("empty", "", true).ShouldBeFalse();
        aggregator.Count.ShouldBe(0);
    }

    // ── GetCombinedCss ──

    [Fact]
    public void ShouldReturnEmptyStringWhenNoCss()
    {
        var aggregator = new CssAggregator();

        aggregator.GetCombinedCss().ShouldBe(string.Empty);
    }

    [Fact]
    public void ShouldCombineMultipleEntries()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("a", ".a { color: red; }", false);
        aggregator.Add("b", ".b { color: blue; }", false);

        var combined = aggregator.GetCombinedCss();
        combined.ShouldContain(".a { color: red; }");
        combined.ShouldContain(".b { color: blue; }");
    }

    // ── GetEntries ──

    [Fact]
    public void ShouldReturnEntriesInOrder()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("first", ".first { }", false);
        aggregator.Add("second", ".second { }", true);

        var entries = aggregator.GetEntries();
        entries.Count.ShouldBe(2);
        entries[0].Css.ShouldContain(".first");
        entries[0].IsGlobal.ShouldBeFalse();
        entries[1].Css.ShouldContain(".second");
        entries[1].IsGlobal.ShouldBeTrue();
    }

    [Fact]
    public void EntryFromTypeShouldHaveComponentType()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(CardComponent));

        var entries = aggregator.GetEntries();
        entries[0].ComponentType.ShouldBe(typeof(CardComponent));
    }

    [Fact]
    public void EntryFromIdentifierShouldHaveNullComponentType()
    {
        var aggregator = new CssAggregator();

        aggregator.Add("raw", ".raw { }", false);

        var entries = aggregator.GetEntries();
        entries[0].ComponentType.ShouldBeNull();
    }

    // ── Clear ──

    [Fact]
    public void ClearShouldRemoveAllEntries()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(CardComponent));
        aggregator.Add("raw", ".raw { }", false);

        aggregator.Clear();

        aggregator.Count.ShouldBe(0);
        aggregator.GetCombinedCss().ShouldBe(string.Empty);
    }

    [Fact]
    public void ClearShouldAllowReaddingSameType()
    {
        var aggregator = new CssAggregator();

        aggregator.Add(typeof(CardComponent));
        aggregator.Clear();
        aggregator.Add(typeof(CardComponent)).ShouldBeTrue();
    }

    // ── Null argument validation ──

    [Fact]
    public void AddTypeShouldThrowForNull()
    {
        var aggregator = new CssAggregator();

        Should.Throw<ArgumentNullException>(
            () => aggregator.Add((Type)null!));
    }

    [Fact]
    public void AddIdentifierShouldThrowForNullIdentifier()
    {
        var aggregator = new CssAggregator();

        Should.Throw<ArgumentNullException>(
            () => aggregator.Add(null!, ".a { }", false));
    }

    [Fact]
    public void AddIdentifierShouldThrowForNullCss()
    {
        var aggregator = new CssAggregator();

        Should.Throw<ArgumentNullException>(
            () => aggregator.Add("id", null!, false));
    }

    // ── Test component types ──

    [Styles(".card { padding: 1rem; }")]
    private sealed class CardComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"card\">Card</div>");
            return Task.CompletedTask;
        }
    }

    [Styles(".panel { padding: 2rem; }")]
    private sealed class PanelComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div class=\"panel\">Panel</div>");
            return Task.CompletedTask;
        }
    }

    private sealed class UnstyledComponent : AtollComponent
    {
        protected override Task RenderCoreAsync(RenderContext context)
        {
            context.WriteHtml("<div>Unstyled</div>");
            return Task.CompletedTask;
        }
    }
}
