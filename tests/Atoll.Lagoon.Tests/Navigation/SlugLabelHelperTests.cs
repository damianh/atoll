using Atoll.Lagoon.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Navigation;

public sealed class SlugLabelHelperTests
{
    // --- StripNumericPrefix ---

    [Fact]
    public void StripNumericPrefixShouldStripLeadingZeroPaddedNumber()
    {
        SlugLabelHelper.StripNumericPrefix("01-intro").ShouldBe("intro");
    }

    [Fact]
    public void StripNumericPrefixShouldStripMultiDigitNumber()
    {
        SlugLabelHelper.StripNumericPrefix("123-advanced").ShouldBe("advanced");
    }

    [Fact]
    public void StripNumericPrefixShouldStripSingleDigitNumber()
    {
        SlugLabelHelper.StripNumericPrefix("1-start").ShouldBe("start");
    }

    [Fact]
    public void StripNumericPrefixShouldStripWhenResultIsEmpty()
    {
        SlugLabelHelper.StripNumericPrefix("123-").ShouldBe("");
    }

    [Fact]
    public void StripNumericPrefixShouldReturnUnchangedWhenNoPrefix()
    {
        SlugLabelHelper.StripNumericPrefix("no-prefix").ShouldBe("no-prefix");
    }

    [Fact]
    public void StripNumericPrefixShouldReturnUnchangedForPlainWord()
    {
        SlugLabelHelper.StripNumericPrefix("intro").ShouldBe("intro");
    }

    [Fact]
    public void StripNumericPrefixShouldReturnUnchangedWhenNumberNotAtStart()
    {
        SlugLabelHelper.StripNumericPrefix("page-01").ShouldBe("page-01");
    }

    [Fact]
    public void StripNumericPrefixShouldHandleEmptyString()
    {
        SlugLabelHelper.StripNumericPrefix("").ShouldBe("");
    }

    // --- Humanize ---

    [Fact]
    public void HumanizeShouldStripPrefixAndTitleCaseHyphenatedWords()
    {
        SlugLabelHelper.Humanize("02-getting-started").ShouldBe("Getting Started");
    }

    [Fact]
    public void HumanizeShouldReplaceUnderscoresWithSpaces()
    {
        SlugLabelHelper.Humanize("api_reference").ShouldBe("Api Reference");
    }

    [Fact]
    public void HumanizeShouldTitleCasePlainWord()
    {
        SlugLabelHelper.Humanize("intro").ShouldBe("Intro");
    }

    [Fact]
    public void HumanizeShouldHandleAlreadyCleanName()
    {
        SlugLabelHelper.Humanize("Guides").ShouldBe("Guides");
    }

    [Fact]
    public void HumanizeShouldHandleEmptyString()
    {
        SlugLabelHelper.Humanize("").ShouldBe("");
    }

    [Fact]
    public void HumanizeShouldHandleNumberOnlyPrefixWithNoLabel()
    {
        SlugLabelHelper.Humanize("01-").ShouldBe("");
    }

    [Fact]
    public void HumanizeShouldHandleMixedHyphensAndUnderscores()
    {
        SlugLabelHelper.Humanize("01-getting_started").ShouldBe("Getting Started");
    }

    // --- TryParseNumericPrefix ---

    [Fact]
    public void TryParseNumericPrefixShouldReturnTrueAndParsedValueForZeroPadded()
    {
        SlugLabelHelper.TryParseNumericPrefix("01-basics", out var prefix).ShouldBeTrue();
        prefix.ShouldBe(1);
    }

    [Fact]
    public void TryParseNumericPrefixShouldReturnTrueForMultiDigit()
    {
        SlugLabelHelper.TryParseNumericPrefix("10-advanced", out var prefix).ShouldBeTrue();
        prefix.ShouldBe(10);
    }

    [Fact]
    public void TryParseNumericPrefixShouldReturnFalseWhenNoPrefix()
    {
        SlugLabelHelper.TryParseNumericPrefix("no-prefix", out var prefix).ShouldBeFalse();
        prefix.ShouldBe(0);
    }

    [Fact]
    public void TryParseNumericPrefixShouldReturnFalseForEmptyString()
    {
        SlugLabelHelper.TryParseNumericPrefix("", out var prefix).ShouldBeFalse();
        prefix.ShouldBe(0);
    }

    [Fact]
    public void TryParseNumericPrefixShouldReturnFalseWhenNumberNotAtStart()
    {
        SlugLabelHelper.TryParseNumericPrefix("page-01", out var prefix).ShouldBeFalse();
        prefix.ShouldBe(0);
    }
}
