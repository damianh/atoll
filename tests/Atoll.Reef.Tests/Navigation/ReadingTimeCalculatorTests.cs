using Atoll.Reef.Navigation;
using Shouldly;
using Xunit;

namespace Atoll.Reef.Tests.Navigation;

public sealed class ReadingTimeCalculatorTests
{
    [Fact]
    public void ShouldReturnOneMinuteForEmptyString()
    {
        ReadingTimeCalculator.Calculate("").ShouldBe(1);
    }

    [Fact]
    public void ShouldReturnOneMinuteForWhitespaceOnly()
    {
        ReadingTimeCalculator.Calculate("   ").ShouldBe(1);
    }

    [Fact]
    public void ShouldReturnFiveMinutesForOneThousandWords()
    {
        var body = string.Join(" ", Enumerable.Repeat("word", 1000));
        ReadingTimeCalculator.Calculate(body).ShouldBe(5);
    }

    [Fact]
    public void ShouldReturnOneMinuteForFewerThanTwoHundredWords()
    {
        var body = string.Join(" ", Enumerable.Repeat("word", 50));
        ReadingTimeCalculator.Calculate(body).ShouldBe(1);
    }

    [Fact]
    public void ShouldRoundUpPartialMinutes()
    {
        // 201 words → ceil(201/200) = 2 minutes
        var body = string.Join(" ", Enumerable.Repeat("word", 201));
        ReadingTimeCalculator.Calculate(body).ShouldBe(2);
    }

    [Fact]
    public void ShouldReturnOneMinuteForExactlyTwoHundredWords()
    {
        var body = string.Join(" ", Enumerable.Repeat("word", 200));
        ReadingTimeCalculator.Calculate(body).ShouldBe(1);
    }
}
