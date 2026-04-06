namespace Atoll.Lagoon.Redirects;

/// <summary>
/// Thrown when two redirect entries share the same source path, or when a redirect
/// source path conflicts with an existing content page URL.
/// </summary>
public sealed class RedirectConflictException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="RedirectConflictException"/> with the specified message.
    /// </summary>
    /// <param name="message">A message describing the conflict.</param>
    public RedirectConflictException(string message)
        : base(message)
    {
    }
}
