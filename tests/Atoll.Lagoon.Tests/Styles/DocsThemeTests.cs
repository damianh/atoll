using Atoll.Css;
using Atoll.Lagoon.Styles;

namespace Atoll.Lagoon.Tests.Styles;

public sealed class DocsThemeTests
{
    private static readonly string Css = StyleScoper.ExtractAndScope(typeof(DocsTheme));

    [Fact]
    public void ShouldIncludeSharedLinkButtonStyles()
    {
        var declarations = GetDeclarations(".link-button");

        declarations.ShouldContain("display: inline-flex;");
        declarations.ShouldContain("gap: 0.5rem;");
        Css.ShouldContain(".link-button:focus-visible {");
        Css.ShouldContain(".link-button svg {");
    }

    [Theory]
    [InlineData(".link-button-primary", "background: var(--docs-primary);")]
    [InlineData(".link-button-secondary", "border-color: var(--docs-border);")]
    [InlineData(".link-button-minimal", "background: transparent;")]
    public void ShouldIncludeLinkButtonVariantStyles(string selector, string distinguishingDeclaration)
    {
        GetDeclarations(selector).ShouldContain(distinguishingDeclaration);
    }

    private static string GetDeclarations(string selector)
    {
        var selectorIndex = Css.IndexOf($"{selector} {{", StringComparison.Ordinal);
        selectorIndex.ShouldBeGreaterThanOrEqualTo(0);

        var blockStart = Css.IndexOf('{', selectorIndex);
        var blockEnd = Css.IndexOf('}', blockStart);
        blockEnd.ShouldBeGreaterThan(blockStart);

        return Css[(blockStart + 1)..blockEnd];
    }
}
