using Atoll.Css;
using Shouldly;
using Xunit;

namespace Atoll.Tests.Css;

public sealed class ScopeHashGeneratorTests
{
    // ── Generate from Type ──

    [Fact]
    public void ShouldGenerateHashFromType()
    {
        var hash = ScopeHashGenerator.Generate(typeof(ScopeHashGeneratorTests));

        hash.ShouldStartWith("atoll-");
        hash.Length.ShouldBe(14); // "atoll-" (6) + 8 hex chars
    }

    [Fact]
    public void ShouldGenerateDeterministicHashForSameType()
    {
        var hash1 = ScopeHashGenerator.Generate(typeof(ScopeHashGeneratorTests));
        var hash2 = ScopeHashGenerator.Generate(typeof(ScopeHashGeneratorTests));

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void ShouldGenerateDifferentHashesForDifferentTypes()
    {
        var hash1 = ScopeHashGenerator.Generate(typeof(ScopeHashGeneratorTests));
        var hash2 = ScopeHashGenerator.Generate(typeof(StyleScopingTests));

        hash1.ShouldNotBe(hash2);
    }

    // ── Generate from string ──

    [Fact]
    public void ShouldGenerateHashFromString()
    {
        var hash = ScopeHashGenerator.Generate("MyNamespace.MyComponent");

        hash.ShouldStartWith("atoll-");
        hash.Length.ShouldBe(14);
    }

    [Fact]
    public void ShouldGenerateDeterministicHashForSameString()
    {
        var hash1 = ScopeHashGenerator.Generate("MyComponent");
        var hash2 = ScopeHashGenerator.Generate("MyComponent");

        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void ShouldGenerateDifferentHashesForDifferentStrings()
    {
        var hash1 = ScopeHashGenerator.Generate("ComponentA");
        var hash2 = ScopeHashGenerator.Generate("ComponentB");

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void HashShouldBeLowercaseHex()
    {
        var hash = ScopeHashGenerator.Generate("TestComponent");
        var hexPart = hash.Substring(6); // Remove "atoll-" prefix

        foreach (var c in hexPart)
        {
            (char.IsAsciiDigit(c) || (c >= 'a' && c <= 'f')).ShouldBeTrue(
                $"Expected hex character but got '{c}'");
        }
    }

    // ── GenerateClassSelector ──

    [Fact]
    public void ShouldGenerateClassSelectorFromType()
    {
        var selector = ScopeHashGenerator.GenerateClassSelector(typeof(ScopeHashGeneratorTests));

        selector.ShouldStartWith(".atoll-");
        selector.Length.ShouldBe(15); // "." + "atoll-" (6) + 8 hex chars
    }

    [Fact]
    public void ShouldGenerateClassSelectorFromString()
    {
        var selector = ScopeHashGenerator.GenerateClassSelector("MyComponent");

        selector.ShouldStartWith(".atoll-");
    }

    [Fact]
    public void ClassSelectorShouldCorrespondToHash()
    {
        var hash = ScopeHashGenerator.Generate("TestComponent");
        var selector = ScopeHashGenerator.GenerateClassSelector("TestComponent");

        selector.ShouldBe("." + hash);
    }

    // ── Null argument validation ──

    [Fact]
    public void GenerateFromTypeShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ScopeHashGenerator.Generate((Type)null!));
    }

    [Fact]
    public void GenerateFromStringShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ScopeHashGenerator.Generate((string)null!));
    }

    [Fact]
    public void GenerateFromStringShouldThrowForEmpty()
    {
        Should.Throw<ArgumentException>(
            () => ScopeHashGenerator.Generate(string.Empty));
    }

    [Fact]
    public void GenerateClassSelectorFromTypeShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ScopeHashGenerator.GenerateClassSelector((Type)null!));
    }

    [Fact]
    public void GenerateClassSelectorFromStringShouldThrowForNull()
    {
        Should.Throw<ArgumentNullException>(
            () => ScopeHashGenerator.GenerateClassSelector((string)null!));
    }
}
