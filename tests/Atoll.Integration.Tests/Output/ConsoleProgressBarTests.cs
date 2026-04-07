using System.IO;
using System.Text;
using Atoll.Cli.Output;
using Shouldly;
using Xunit;

namespace Atoll.Integration.Tests.Output;

public sealed class ConsoleProgressBarTests
{
    [Fact]
    public void AdvanceShouldWritePlainTextForNonInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Config", "Compile", "Routes"], 20, writer, isInteractive: false);

        bar.Advance();

        sb.ToString().ShouldBe("  Config (1/3)\r\n");
    }

    [Fact]
    public void AdvanceShouldWriteAllPhasesForNonInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Config", "Compile", "Routes"], 20, writer, isInteractive: false);

        bar.Advance();
        bar.Advance();
        bar.Advance();

        var lines = sb.ToString().Split(writer.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("  Config (1/3)");
        lines[1].ShouldBe("  Compile (2/3)");
        lines[2].ShouldBe("  Routes (3/3)");
    }

    [Fact]
    public void AdvanceShouldWriteCarriageReturnPrefixForInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Config", "Compile"], 20, writer, isInteractive: true);

        bar.Advance();

        var output = sb.ToString();
        output.ShouldStartWith("\r");
    }

    [Fact]
    public void AdvanceShouldIncludePhaseNameAndCounterForInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Config", "Compile"], 20, writer, isInteractive: true);

        bar.Advance();

        var output = sb.ToString();
        output.ShouldContain("Config");
        output.ShouldContain("(1/2)");
    }

    [Fact]
    public void AdvanceShouldIncludeUnicodeBlockCharsForInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Phase1"], 4, writer, isInteractive: true);

        bar.Advance();

        var output = sb.ToString();
        // After advancing 1/1 phase, bar should be fully filled
        output.ShouldContain("\u2588"); // filled block
    }

    [Fact]
    public void CompleteShouldClearLineForInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Config"], 20, writer, isInteractive: true);

        bar.Advance();
        var lineLength = sb.ToString().Length - 1; // subtract the leading \r
        sb.Clear();

        bar.Complete();

        var afterComplete = sb.ToString();
        // Complete should write \r + spaces (to erase the bar line) + \r
        afterComplete.ShouldStartWith("\r");
        afterComplete.ShouldContain(new string(' ', lineLength));
    }

    [Fact]
    public void CompleteShouldBeNoOpForNonInteractiveMode()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Config"], 20, writer, isInteractive: false);

        bar.Advance();
        var afterAdvance = sb.ToString();
        sb.Clear();

        bar.Complete();

        // No additional output in non-interactive mode
        sb.ToString().ShouldBeEmpty();
    }

    [Fact]
    public void AdvancePastTotalShouldBeNoOp()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Only"], 20, writer, isInteractive: false);

        bar.Advance(); // phase 1 (valid)
        var afterFirst = sb.ToString();

        bar.Advance(); // past end — should no-op
        bar.Advance(); // past end — should no-op

        // Only one line should have been written
        sb.ToString().ShouldBe(afterFirst);
    }

    [Fact]
    public void ShouldHandleSinglePhase()
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);
        var bar = new ConsoleProgressBar(["Done"], 10, writer, isInteractive: false);

        bar.Advance();
        bar.Complete();

        sb.ToString().ShouldContain("Done (1/1)");
    }
}
