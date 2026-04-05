namespace Atoll.Giscus;

/// <summary>
/// Specifies the loading strategy for the giscus iframe.
/// </summary>
public enum GiscusLoading
{
    /// <summary>
    /// The iframe is loaded lazily — deferred until the user scrolls near it.
    /// This is the recommended default for performance.
    /// </summary>
    Lazy,

    /// <summary>The iframe is loaded eagerly as soon as the page loads.</summary>
    Eager,
}
