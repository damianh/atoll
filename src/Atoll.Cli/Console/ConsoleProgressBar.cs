using System.IO;

namespace Atoll.Cli.Output;

/// <summary>
/// Renders a Unicode progress bar on the current console line using carriage return
/// to overwrite in-place. Degrades to plain-text output when the console is redirected
/// (CI pipelines, piped output, etc.).
/// </summary>
internal sealed class ConsoleProgressBar
{
    private readonly string[] _phaseNames;
    private readonly int _totalPhases;
    private readonly bool _isInteractive;
    private readonly int _barWidth;
    private readonly TextWriter _writer;
    private int _currentPhase;
    private int _lastLineLength;

    /// <summary>
    /// Initializes a new <see cref="ConsoleProgressBar"/> using <see cref="System.Console.Out"/>
    /// and auto-detecting whether the output is interactive.
    /// </summary>
    /// <param name="phaseNames">Ordered names for each phase (e.g. "Config", "Build", "Routes").</param>
    /// <param name="barWidth">Width of the bar in characters.</param>
    public ConsoleProgressBar(string[] phaseNames, int barWidth)
        : this(phaseNames, barWidth, System.Console.Out, !System.Console.IsOutputRedirected)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConsoleProgressBar"/> using <see cref="System.Console.Out"/>
    /// and auto-detecting whether the output is interactive.
    /// Uses a default bar width of 20 characters.
    /// </summary>
    /// <param name="phaseNames">Ordered names for each phase (e.g. "Config", "Build", "Routes").</param>
    public ConsoleProgressBar(string[] phaseNames)
        : this(phaseNames, 20)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConsoleProgressBar"/> with an explicit writer and interactivity flag.
    /// Intended for testing.
    /// </summary>
    /// <param name="phaseNames">Ordered names for each phase.</param>
    /// <param name="barWidth">Width of the bar in characters.</param>
    /// <param name="writer">The <see cref="TextWriter"/> to write output to.</param>
    /// <param name="isInteractive">Whether to render the animated bar (<c>true</c>) or plain text (<c>false</c>).</param>
    internal ConsoleProgressBar(string[] phaseNames, int barWidth, TextWriter writer, bool isInteractive)
    {
        ArgumentNullException.ThrowIfNull(phaseNames);
        ArgumentNullException.ThrowIfNull(writer);

        _phaseNames = phaseNames;
        _totalPhases = phaseNames.Length;
        _barWidth = barWidth;
        _writer = writer;
        _isInteractive = isInteractive;
    }

    /// <summary>
    /// Advances to the next phase, updating the console display.
    /// Interactive mode rewrites the same line using <c>\r</c>.
    /// Non-interactive mode writes one line per phase.
    /// </summary>
    public void Advance()
    {
        if (_currentPhase >= _totalPhases)
        {
            return;
        }

        var phaseName = _phaseNames[_currentPhase];
        _currentPhase++;

        if (_isInteractive)
        {
            WriteInteractiveLine(phaseName);
        }
        else
        {
            _writer.WriteLine($"  {phaseName} ({_currentPhase}/{_totalPhases})");
        }
    }

    /// <summary>
    /// Completes the progress bar. In interactive mode, clears the bar line and
    /// moves to the next line. In non-interactive mode, this is a no-op.
    /// </summary>
    public void Complete()
    {
        if (!_isInteractive)
        {
            return;
        }

        // Clear the bar line entirely, then move to the next line.
        if (_lastLineLength > 0)
        {
            _writer.Write('\r');
            _writer.Write(new string(' ', _lastLineLength));
            _writer.Write('\r');
        }
    }

    private void WriteInteractiveLine(string phaseName)
    {
        // _currentPhase is already incremented in Advance() before this call.
        var filled = _currentPhase;

        // Clamp to bar width
        var filledWidth = _barWidth > 0
            ? (int)Math.Round((double)filled / _totalPhases * _barWidth)
            : 0;
        var emptyWidth = _barWidth - filledWidth;

        var bar = new string('\u2588', filledWidth) + new string('\u2591', emptyWidth);
        var line = $"  [{bar}] {phaseName} ({filled}/{_totalPhases})";

        // Pad with trailing spaces to overwrite any leftover characters from longer lines.
        var padding = _lastLineLength > line.Length ? new string(' ', _lastLineLength - line.Length) : "";
        _writer.Write($"\r{line}{padding}");
        _lastLineLength = line.Length;
    }
}
