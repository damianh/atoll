namespace Atoll.Core.Rendering;

/// <summary>
/// Renders interleaved static HTML parts and dynamic expression fragments,
/// preserving output order even when expressions are asynchronous.
/// This is the Atoll equivalent of Astro's <c>RenderTemplateResult</c>.
/// </summary>
/// <remarks>
/// <para>
/// The rendering algorithm has two modes:
/// </para>
/// <list type="number">
/// <item>
/// <description>
/// <strong>Fast sync path:</strong> HTML parts and expression fragments are written
/// directly to the destination as long as all expressions complete synchronously.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Buffered async path:</strong> Once any expression returns an incomplete
/// <see cref="ValueTask"/>, all remaining expressions are eagerly started in
/// <see cref="BufferedRenderer"/> instances. After the triggering expression completes,
/// the remaining parts/expressions are flushed sequentially to preserve output order.
/// </description>
/// </item>
/// </list>
/// <para>
/// Invariant: <c>htmlParts.Length == expressions.Length + 1</c>. The template is structured
/// as: htmlParts[0], expressions[0], htmlParts[1], expressions[1], ..., htmlParts[N].
/// </para>
/// </remarks>
public sealed class InterpolatedTemplate
{
    private readonly string[] _htmlParts;
    private readonly RenderFragment[] _expressions;

    /// <summary>
    /// Initializes a new <see cref="InterpolatedTemplate"/> with the specified HTML parts
    /// and expression fragments.
    /// </summary>
    /// <param name="htmlParts">
    /// The static HTML parts. Must have exactly one more element than <paramref name="expressions"/>.
    /// </param>
    /// <param name="expressions">The dynamic expression fragments interleaved between HTML parts.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="htmlParts"/> length does not equal <paramref name="expressions"/> length plus one.
    /// </exception>
    public InterpolatedTemplate(string[] htmlParts, RenderFragment[] expressions)
    {
        ArgumentNullException.ThrowIfNull(htmlParts);
        ArgumentNullException.ThrowIfNull(expressions);

        if (htmlParts.Length != expressions.Length + 1)
        {
            throw new ArgumentException(
                $"htmlParts must have exactly one more element than expressions. Got {htmlParts.Length} parts and {expressions.Length} expressions.",
                nameof(htmlParts));
        }

        _htmlParts = htmlParts;
        _expressions = expressions;
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from this interpolated template.
    /// The returned fragment implements the fast-sync / buffered-async rendering algorithm.
    /// </summary>
    /// <returns>A <see cref="RenderFragment"/> that renders this template.</returns>
    public RenderFragment ToRenderFragment()
    {
        if (_expressions.Length == 0)
        {
            // No expressions — just static HTML
            var html = _htmlParts[0];
            return string.IsNullOrEmpty(html) ? RenderFragment.Empty : RenderFragment.FromHtml(html);
        }

        // Capture fields for the closure
        var htmlParts = _htmlParts;
        var expressions = _expressions;

        return RenderFragment.FromAsync(destination => RenderCore(destination, htmlParts, expressions));
    }

    private static ValueTask RenderCore(IRenderDestination destination, string[] htmlParts, RenderFragment[] expressions)
    {
        for (var i = 0; i < expressions.Length; i++)
        {
            // Write the HTML part that precedes this expression
            WriteHtmlPart(destination, htmlParts[i]);

            // Render the expression
            var expression = expressions[i];
            if (expression.IsEmpty)
            {
                continue;
            }

            var renderTask = expression.RenderAsync(destination);

            if (!renderTask.IsCompletedSuccessfully)
            {
                // Async encountered — switch to buffered mode for remaining expressions
                return RenderBufferedAsync(destination, htmlParts, expressions, i, renderTask);
            }
        }

        // Write the final HTML part (after the last expression)
        WriteHtmlPart(destination, htmlParts[htmlParts.Length - 1]);
        return default;
    }

    private static async ValueTask RenderBufferedAsync(
        IRenderDestination destination,
        string[] htmlParts,
        RenderFragment[] expressions,
        int asyncIndex,
        ValueTask pendingTask)
    {
        // Start buffered renderers for all remaining expressions (after the async one)
        var startIndex = asyncIndex + 1;
        var remaining = expressions.Length - startIndex;
        var flushers = new BufferedRenderer[remaining];

        for (var j = 0; j < remaining; j++)
        {
            var expr = expressions[startIndex + j];
            flushers[j] = new BufferedRenderer(expr);
            flushers[j].Start();
        }

        // Wait for the expression that triggered async mode
        await pendingTask.ConfigureAwait(false);

        // Sequentially flush remaining expressions, interleaving with their HTML parts
        for (var k = 0; k < flushers.Length; k++)
        {
            WriteHtmlPart(destination, htmlParts[startIndex + k]);
            await flushers[k].FlushAsync(destination).ConfigureAwait(false);
        }

        // Write the final HTML part
        WriteHtmlPart(destination, htmlParts[htmlParts.Length - 1]);
    }

    private static void WriteHtmlPart(IRenderDestination destination, string html)
    {
        if (!string.IsNullOrEmpty(html))
        {
            destination.Write(RenderChunk.Html(html));
        }
    }
}
