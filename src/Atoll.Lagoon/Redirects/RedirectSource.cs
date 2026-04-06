using Atoll.Redirects;

namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Represents a single content entry that declares redirect source paths.
/// Used by <see cref="RedirectCollector"/> to build a <see cref="RedirectMap"/>.
/// </summary>
/// <param name="CanonicalUrl">The canonical (current) URL of the content page.</param>
/// <param name="RedirectFrom">The list of old source paths that should redirect to <paramref name="CanonicalUrl"/>.</param>
public sealed record RedirectSource(string CanonicalUrl, IReadOnlyList<string> RedirectFrom);
