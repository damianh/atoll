namespace Atoll.Annotations;

/// <summary>
/// Extension methods for <see cref="AnnotationTarget"/>.
/// </summary>
public static class AnnotationTargetExtensions
{
    /// <summary>
    /// Returns the HTML data attribute value for the given <see cref="AnnotationTarget"/>.
    /// </summary>
    /// <param name="target">The annotation target.</param>
    /// <returns>
    /// <c>"issue"</c> for <see cref="AnnotationTarget.Issue"/>;
    /// <c>"discussion"</c> for <see cref="AnnotationTarget.Discussion"/>.
    /// </returns>
    public static string ToDataValue(this AnnotationTarget target) => target switch
    {
        AnnotationTarget.Issue => "issue",
        AnnotationTarget.Discussion => "discussion",
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, $"Unsupported annotation target: {target}"),
    };
}
