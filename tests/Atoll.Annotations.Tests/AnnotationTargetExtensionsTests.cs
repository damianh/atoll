
namespace Atoll.Annotations.Tests;

public sealed class AnnotationTargetExtensionsTests
{
    [Fact]
    public void ShouldReturnIssueForIssueTarget()
    {
        AnnotationTarget.Issue.ToDataValue().ShouldBe("issue");
    }

    [Fact]
    public void ShouldReturnDiscussionForDiscussionTarget()
    {
        AnnotationTarget.Discussion.ToDataValue().ShouldBe("discussion");
    }

    [Fact]
    public void ShouldThrowForUndefinedTarget()
    {
        var undefined = (AnnotationTarget)99;

        Should.Throw<ArgumentOutOfRangeException>(() => undefined.ToDataValue());
    }
}
