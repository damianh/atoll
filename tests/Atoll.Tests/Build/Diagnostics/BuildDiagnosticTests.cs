using Atoll.Build.Diagnostics;

namespace Atoll.Build.Tests.Diagnostics;

public sealed class BuildDiagnosticTests
{
    [Fact]
    public void ConstructorShouldSetSeverityAndMessage()
    {
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Warning, "Something happened");

        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Warning);
        diagnostic.Message.ShouldBe("Something happened");
        diagnostic.Source.ShouldBeNull();
    }

    [Fact]
    public void ConstructorWithSourceShouldSetAllProperties()
    {
        var diagnostic = new BuildDiagnostic(DiagnosticSeverity.Error, "Render failed", "/about");

        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.Message.ShouldBe("Render failed");
        diagnostic.Source.ShouldBe("/about");
    }

    [Fact]
    public void ConstructorShouldThrowOnNullMessage()
    {
        Should.Throw<ArgumentNullException>(
            () => new BuildDiagnostic(DiagnosticSeverity.Info, null!));
    }

    [Fact]
    public void ConstructorWithSourceShouldThrowOnNullMessage()
    {
        Should.Throw<ArgumentNullException>(
            () => new BuildDiagnostic(DiagnosticSeverity.Info, null!, "/page"));
    }

    [Fact]
    public void ConstructorWithSourceShouldThrowOnNullSource()
    {
        Should.Throw<ArgumentNullException>(
            () => new BuildDiagnostic(DiagnosticSeverity.Info, "message", null!));
    }

    [Theory]
    [InlineData(DiagnosticSeverity.Info)]
    [InlineData(DiagnosticSeverity.Warning)]
    [InlineData(DiagnosticSeverity.Error)]
    public void ConstructorShouldAcceptAllSeverityLevels(DiagnosticSeverity severity)
    {
        var diagnostic = new BuildDiagnostic(severity, "test");

        diagnostic.Severity.ShouldBe(severity);
    }
}
