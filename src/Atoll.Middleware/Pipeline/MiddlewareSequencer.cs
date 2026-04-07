namespace Atoll.Middleware.Pipeline;

/// <summary>
/// Composes multiple <see cref="MiddlewareHandler"/> delegates into a single handler
/// that executes them in order, forming a chain-of-responsibility pipeline.
/// </summary>
/// <remarks>
/// <para>
/// This is Atoll's equivalent of Astro's <c>sequence()</c> function. Each middleware
/// in the sequence receives a <c>next</c> delegate that calls the subsequent middleware.
/// The final middleware's <c>next</c> invokes the terminal route handler.
/// </para>
/// <para>
/// Middleware can:
/// <list type="bullet">
/// <item><description><b>Pass through</b>: call <c>next()</c> and return the response.</description></item>
/// <item><description><b>Modify</b>: call <c>next()</c>, then modify the response.</description></item>
/// <item><description><b>Short-circuit</b>: return a response without calling <c>next()</c>.</description></item>
/// </list>
/// </para>
/// </remarks>
public static class MiddlewareSequencer
{
    /// <summary>
    /// Composes multiple middleware handlers into a single handler that executes
    /// them in order (left to right).
    /// </summary>
    /// <param name="handlers">The middleware handlers to compose.</param>
    /// <returns>
    /// A single <see cref="MiddlewareHandler"/> that executes all handlers in sequence.
    /// If no handlers are provided, returns a pass-through handler that calls <c>next()</c>.
    /// </returns>
    public static MiddlewareHandler Sequence(params MiddlewareHandler[] handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        // Filter out nulls
        var filtered = handlers.Where(h => h is not null).ToArray();

        if (filtered.Length == 0)
        {
            return (_, next) => next();
        }

        return (context, next) => ApplyHandler(0, context, next, filtered);
    }

    /// <summary>
    /// Composes middleware handlers from a list into a single handler.
    /// </summary>
    /// <param name="handlers">The middleware handlers to compose.</param>
    /// <returns>A single <see cref="MiddlewareHandler"/>.</returns>
    public static MiddlewareHandler Sequence(IReadOnlyList<MiddlewareHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        return Sequence(handlers.ToArray());
    }

    private static Task<Routing.AtollResponse> ApplyHandler(
        int index,
        MiddlewareContext context,
        MiddlewareNext terminalNext,
        MiddlewareHandler[] handlers)
    {
        var handler = handlers[index];

        MiddlewareNext next = () =>
        {
            if (index < handlers.Length - 1)
            {
                return ApplyHandler(index + 1, context, terminalNext, handlers);
            }

            // Last middleware in the chain — call the terminal handler
            return terminalNext();
        };

        return handler(context, next);
    }
}
