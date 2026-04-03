using Atoll.Instructions;

namespace Atoll.Islands;

/// <summary>
/// Marks a component for client-side hydration when a CSS media query matches.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>client:media</c> directive. The component
/// is server-side rendered and hydrated only when the specified CSS media query evaluates
/// to <c>true</c>, using <c>matchMedia</c>.
/// </para>
/// <para>
/// Use this for interactive components that should only be hydrated for certain
/// viewport sizes or device capabilities (e.g., a mobile navigation menu that
/// only needs JavaScript on small screens).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ClientMedia("(max-width: 768px)")]
/// public sealed class MobileMenu : AtollComponent
/// {
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;nav class=\"mobile-menu\"&gt;...&lt;/nav&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ClientMediaAttribute : ClientDirectiveAttribute
{
    /// <summary>
    /// Initializes a new <see cref="ClientMediaAttribute"/> with the specified CSS media query.
    /// </summary>
    /// <param name="mediaQuery">
    /// The CSS media query that must match for hydration to occur
    /// (e.g., <c>"(max-width: 768px)"</c>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="mediaQuery"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="mediaQuery"/> is empty or whitespace.
    /// </exception>
    public ClientMediaAttribute(string mediaQuery) : base(ClientDirectiveType.Media)
    {
        ArgumentNullException.ThrowIfNull(mediaQuery);

        if (string.IsNullOrWhiteSpace(mediaQuery))
        {
            throw new ArgumentException("Media query must not be empty or whitespace.", nameof(mediaQuery));
        }

        MediaQuery = mediaQuery;
    }

    /// <summary>
    /// Gets the CSS media query that must match for hydration to occur.
    /// </summary>
    public string MediaQuery { get; }

    /// <inheritdoc />
    public override string? Value => MediaQuery;
}
