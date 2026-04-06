namespace Atoll.Annotations;

/// <summary>
/// Specifies where annotation feedback is submitted on GitHub.
/// </summary>
public enum AnnotationTarget
{
    /// <summary>
    /// Feedback is submitted as a new GitHub Issue.
    /// </summary>
    Issue = 0,

    /// <summary>
    /// Feedback is submitted as a new GitHub Discussion.
    /// </summary>
    Discussion = 1,
}
