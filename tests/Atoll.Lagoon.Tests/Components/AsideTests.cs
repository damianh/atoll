using Atoll.Components;
using Atoll.Lagoon.Components;
using Atoll.Rendering;
using Atoll.Slots;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Components;

public sealed class AsideTests
{
    private static async Task<string> RenderAsideAsync(
        AsideType type = AsideType.Note,
        string? title = null,
        string slotContent = "<p>Content here</p>")
    {
        var destination = new StringRenderDestination();
        var props = new Dictionary<string, object?>
        {
            ["Type"] = type,
            ["Title"] = title,
        };
        var slots = SlotCollection.FromDefault(RenderFragment.FromHtml(slotContent));
        await ComponentRenderer.RenderComponentAsync<Aside>(destination, props, slots);
        return destination.GetOutput();
    }

    [Fact]
    public async Task ShouldRenderAsideElement()
    {
        var html = await RenderAsideAsync();

        html.ShouldContain("<aside ");
        html.ShouldContain("</aside>");
    }

    [Fact]
    public async Task ShouldHaveRoleNote()
    {
        var html = await RenderAsideAsync();

        html.ShouldContain("role=\"note\"");
    }

    [Fact]
    public async Task ShouldHaveAriaLabel()
    {
        var html = await RenderAsideAsync(AsideType.Note);

        html.ShouldContain("aria-label=\"Note\"");
    }

    [Fact]
    public async Task ShouldApplyNoteVariantClass()
    {
        var html = await RenderAsideAsync(AsideType.Note);

        html.ShouldContain("aside-note");
    }

    [Fact]
    public async Task ShouldApplyTipVariantClass()
    {
        var html = await RenderAsideAsync(AsideType.Tip);

        html.ShouldContain("aside-tip");
    }

    [Fact]
    public async Task ShouldApplyCautionVariantClass()
    {
        var html = await RenderAsideAsync(AsideType.Caution);

        html.ShouldContain("aside-caution");
    }

    [Fact]
    public async Task ShouldApplyDangerVariantClass()
    {
        var html = await RenderAsideAsync(AsideType.Danger);

        html.ShouldContain("aside-danger");
    }

    [Fact]
    public async Task ShouldUseDefaultTitleForNoteVariant()
    {
        var html = await RenderAsideAsync(AsideType.Note);

        html.ShouldContain("Note");
        html.ShouldContain("aside-title");
    }

    [Fact]
    public async Task ShouldUseDefaultTitleForTipVariant()
    {
        var html = await RenderAsideAsync(AsideType.Tip);

        html.ShouldContain("Tip");
    }

    [Fact]
    public async Task ShouldUseDefaultTitleForCautionVariant()
    {
        var html = await RenderAsideAsync(AsideType.Caution);

        html.ShouldContain("Caution");
    }

    [Fact]
    public async Task ShouldUseDefaultTitleForDangerVariant()
    {
        var html = await RenderAsideAsync(AsideType.Danger);

        html.ShouldContain("Danger");
    }

    [Fact]
    public async Task ShouldUseCustomTitleWhenProvided()
    {
        var html = await RenderAsideAsync(title: "Custom heading");

        html.ShouldContain("Custom heading");
        html.ShouldContain("aria-label=\"Custom heading\"");
    }

    [Fact]
    public async Task ShouldRenderSlotContent()
    {
        var html = await RenderAsideAsync(slotContent: "<p>This is the body</p>");

        html.ShouldContain("<p>This is the body</p>");
        html.ShouldContain("aside-content");
    }

    [Fact]
    public async Task ShouldHtmlEncodeTitleInAriaLabel()
    {
        var html = await RenderAsideAsync(title: "A \"quoted\" title");

        html.ShouldContain("aria-label=\"A &quot;quoted&quot; title\"");
    }

    [Fact]
    public async Task ShouldRenderIconForNoteVariant()
    {
        var html = await RenderAsideAsync(AsideType.Note);

        html.ShouldContain("<svg");
    }

    [Fact]
    public async Task ShouldRenderIconForAllVariants()
    {
        foreach (var type in Enum.GetValues<AsideType>())
        {
            var html = await RenderAsideAsync(type);
            html.ShouldContain("<svg");
        }
    }
}
