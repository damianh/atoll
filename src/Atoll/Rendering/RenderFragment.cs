using System.Text;

namespace Atoll.Rendering;

/// <summary>
/// The fundamental unit of renderable content in Atoll.
/// This is the Atoll equivalent of Astro's <c>RenderTemplateResult</c>.
/// A <see cref="RenderFragment"/> encapsulates a function that writes content to an
/// <see cref="IRenderDestination"/>, possibly asynchronously.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RenderFragment"/> is designed to support both synchronous and asynchronous
/// rendering paths. Static HTML uses a fast synchronous path, while dynamic expressions
/// (e.g., from component rendering or async data fetching) use the async path.
/// </para>
/// <para>
/// Fragments compose naturally via <see cref="Concat"/> and can be rendered to either
/// a string (for SSG or testing) or a stream (for streaming SSR).
/// </para>
/// </remarks>
public readonly struct RenderFragment
{
    private readonly Func<IRenderDestination, ValueTask>? _renderer;

    private RenderFragment(Func<IRenderDestination, ValueTask> renderer)
    {
        _renderer = renderer;
    }

    /// <summary>
    /// A <see cref="RenderFragment"/> that produces no output.
    /// </summary>
    public static readonly RenderFragment Empty = new(static _ => default);

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from trusted HTML content.
    /// The HTML will not be escaped.
    /// </summary>
    /// <param name="html">The trusted HTML string.</param>
    /// <returns>A new <see cref="RenderFragment"/> that writes the HTML to a destination.</returns>
    public static RenderFragment FromHtml(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        if (html.Length == 0)
        {
            return Empty;
        }

        // Capture the HTML in a closure for the renderer delegate
        return new RenderFragment(destination =>
        {
            destination.Write(RenderChunk.Html(html));
            return default;
        });
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from plain text that will be HTML-escaped.
    /// </summary>
    /// <param name="text">The text to escape and render.</param>
    /// <returns>A new <see cref="RenderFragment"/> that writes the escaped text to a destination.</returns>
    public static RenderFragment FromText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
        {
            return Empty;
        }

        return new RenderFragment(destination =>
        {
            destination.Write(RenderChunk.Text(text));
            return default;
        });
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from an <see cref="HtmlString"/> (trusted HTML).
    /// </summary>
    /// <param name="htmlString">The trusted HTML string.</param>
    /// <returns>A new <see cref="RenderFragment"/> that writes the HTML to a destination.</returns>
    public static RenderFragment FromHtmlString(HtmlString htmlString)
    {
        if (htmlString.IsEmpty)
        {
            return Empty;
        }

        return FromHtml(htmlString.Value);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> from an async rendering function.
    /// </summary>
    /// <param name="renderer">The async function that writes content to a destination.</param>
    /// <returns>A new <see cref="RenderFragment"/>.</returns>
    public static RenderFragment FromAsync(Func<IRenderDestination, ValueTask> renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return new RenderFragment(renderer);
    }

    /// <summary>
    /// Creates a <see cref="RenderFragment"/> that concatenates multiple fragments in order.
    /// </summary>
    /// <param name="fragments">The fragments to concatenate.</param>
    /// <returns>A new <see cref="RenderFragment"/> that renders all fragments sequentially.</returns>
    public static RenderFragment Concat(params RenderFragment[] fragments)
    {
        ArgumentNullException.ThrowIfNull(fragments);
        if (fragments.Length == 0)
        {
            return Empty;
        }

        if (fragments.Length == 1)
        {
            return fragments[0];
        }

        // Capture the array for the closure
        var captured = fragments.ToArray();
        return new RenderFragment(async destination =>
        {
            foreach (var fragment in captured)
            {
                await fragment.RenderAsync(destination);
            }
        });
    }

    /// <summary>
    /// Renders this fragment to the specified destination.
    /// </summary>
    /// <param name="destination">The destination to write rendered content to.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous render operation.</returns>
    public ValueTask RenderAsync(IRenderDestination destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (_renderer is null)
        {
            return default;
        }

        return _renderer(destination);
    }

    /// <summary>
    /// Renders this fragment to a string. This buffers all output and returns
    /// the complete rendered content. Primarily used for testing and SSG.
    /// </summary>
    /// <returns>The complete rendered HTML string.</returns>
    public async ValueTask<string> RenderToStringAsync()
    {
        var destination = new StringRenderDestination();
        await RenderAsync(destination);
        return destination.GetOutput();
    }

    /// <summary>
    /// Gets a value indicating whether this fragment has a renderer.
    /// A default-constructed <see cref="RenderFragment"/> (without using a factory method)
    /// has no renderer and will produce no output.
    /// </summary>
    public bool IsEmpty => _renderer is null || ReferenceEquals(_renderer, Empty._renderer);
}
