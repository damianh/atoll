using Atoll.Core.Rendering;
using Atoll.Core.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Core.Tests.Slots;

public sealed class SlotCollectionTests
{
    [Fact]
    public void EmptyShouldHaveNoSlots()
    {
        var slots = SlotCollection.Empty;

        slots.Count.ShouldBe(0);
        slots.HasDefaultSlot.ShouldBeFalse();
    }

    [Fact]
    public void FromDefaultShouldCreateCollectionWithDefaultSlot()
    {
        var content = RenderFragment.FromHtml("<p>content</p>");
        var slots = SlotCollection.FromDefault(content);

        slots.Count.ShouldBe(1);
        slots.HasDefaultSlot.ShouldBeTrue();
        slots.HasSlot("default").ShouldBeTrue();
    }

    [Fact]
    public void HasSlotShouldReturnFalseForMissingSlot()
    {
        var slots = SlotCollection.Empty;

        slots.HasSlot("header").ShouldBeFalse();
    }

    [Fact]
    public void HasSlotShouldReturnTrueForExistingSlot()
    {
        var dict = new Dictionary<string, RenderFragment>
        {
            ["header"] = RenderFragment.FromHtml("<h1>Header</h1>"),
        };
        var slots = new SlotCollection(dict);

        slots.HasSlot("header").ShouldBeTrue();
    }

    [Fact]
    public async Task RenderSlotAsyncShouldRenderExistingSlot()
    {
        var dict = new Dictionary<string, RenderFragment>
        {
            ["default"] = RenderFragment.FromHtml("<p>Hello</p>"),
        };
        var slots = new SlotCollection(dict);
        var dest = new StringRenderDestination();

        await slots.RenderSlotAsync("default", dest);

        dest.GetOutput().ShouldBe("<p>Hello</p>");
    }

    [Fact]
    public async Task RenderSlotAsyncShouldDoNothingForMissingSlot()
    {
        var slots = SlotCollection.Empty;
        var dest = new StringRenderDestination();

        await slots.RenderSlotAsync("missing", dest);

        dest.GetOutput().ShouldBe("");
    }

    [Fact]
    public async Task RenderSlotAsyncWithFallbackShouldUseFallbackForMissingSlot()
    {
        var slots = SlotCollection.Empty;
        var dest = new StringRenderDestination();
        var fallback = RenderFragment.FromHtml("<em>Fallback</em>");

        await slots.RenderSlotAsync("missing", dest, fallback);

        dest.GetOutput().ShouldBe("<em>Fallback</em>");
    }

    [Fact]
    public async Task RenderSlotAsyncWithFallbackShouldRenderSlotWhenPresent()
    {
        var dict = new Dictionary<string, RenderFragment>
        {
            ["sidebar"] = RenderFragment.FromHtml("<nav>Links</nav>"),
        };
        var slots = new SlotCollection(dict);
        var dest = new StringRenderDestination();
        var fallback = RenderFragment.FromHtml("<em>Fallback</em>");

        await slots.RenderSlotAsync("sidebar", dest, fallback);

        dest.GetOutput().ShouldBe("<nav>Links</nav>");
    }

    [Fact]
    public void GetSlotFragmentShouldReturnEmptyForMissingSlot()
    {
        var slots = SlotCollection.Empty;

        var fragment = slots.GetSlotFragment("missing");

        fragment.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public async Task GetSlotFragmentShouldReturnFragmentForExistingSlot()
    {
        var dict = new Dictionary<string, RenderFragment>
        {
            ["content"] = RenderFragment.FromHtml("<p>Content</p>"),
        };
        var slots = new SlotCollection(dict);

        var fragment = slots.GetSlotFragment("content");
        var output = await fragment.RenderToStringAsync();

        output.ShouldBe("<p>Content</p>");
    }

    [Fact]
    public void ShouldSupportDictionaryEnumeration()
    {
        var dict = new Dictionary<string, RenderFragment>
        {
            ["header"] = RenderFragment.FromHtml("<h1>H</h1>"),
            ["footer"] = RenderFragment.FromHtml("<footer>F</footer>"),
        };
        var slots = new SlotCollection(dict);

        slots.Keys.ShouldContain("header");
        slots.Keys.ShouldContain("footer");
        slots.ContainsKey("header").ShouldBeTrue();
        slots.TryGetValue("header", out _).ShouldBeTrue();
        slots.TryGetValue("missing", out _).ShouldBeFalse();
    }

    [Fact]
    public void IndexerShouldThrowForMissingSlot()
    {
        var slots = SlotCollection.Empty;

        Should.Throw<KeyNotFoundException>(() => _ = slots["missing"]);
    }

    [Fact]
    public void HasSlotShouldThrowForNullName()
    {
        var slots = SlotCollection.Empty;

        Should.Throw<ArgumentNullException>(() => slots.HasSlot(null!));
    }

    [Fact]
    public void ConstructorShouldThrowForNullDictionary()
    {
        Should.Throw<ArgumentNullException>(() => new SlotCollection(null!));
    }
}
