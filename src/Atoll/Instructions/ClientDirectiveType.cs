namespace Atoll.Instructions;

/// <summary>
/// Specifies the type of client directive for island hydration.
/// Determines when and how a component is hydrated on the client side.
/// </summary>
/// <remarks>
/// This is the Atoll equivalent of Astro's <c>client:*</c> directive types.
/// </remarks>
public enum ClientDirectiveType
{
    /// <summary>
    /// Hydrate the component immediately on page load.
    /// Equivalent to Astro's <c>client:load</c>.
    /// </summary>
    Load,

    /// <summary>
    /// Hydrate the component when the browser is idle (using <c>requestIdleCallback</c>).
    /// Equivalent to Astro's <c>client:idle</c>.
    /// </summary>
    Idle,

    /// <summary>
    /// Hydrate the component when it enters the viewport (using <c>IntersectionObserver</c>).
    /// Equivalent to Astro's <c>client:visible</c>.
    /// </summary>
    Visible,

    /// <summary>
    /// Hydrate the component when a CSS media query matches.
    /// Equivalent to Astro's <c>client:media</c>.
    /// </summary>
    Media,
}
