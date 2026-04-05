using Atoll.Build.Pipeline;
using Shouldly;
using Xunit;

namespace Atoll.Build.Tests.Pipeline;

public sealed class FingerprintDetectorTests
{
    // ── HasFingerprintedFileName ──

    [Fact]
    public void ShouldDetectStandardFingerprintedCssFile()
    {
        FingerprintDetector.HasFingerprintedFileName("styles.a1b2c3d4.css").ShouldBeTrue();
    }

    [Fact]
    public void ShouldDetectStandardFingerprintedJsFile()
    {
        FingerprintDetector.HasFingerprintedFileName("scripts.00aabbcc.js").ShouldBeTrue();
    }

    [Fact]
    public void ShouldDetectFingerprintWithAllHexDigits()
    {
        FingerprintDetector.HasFingerprintedFileName("app.0123456789abcdef.js").ShouldBeFalse(); // 16 chars — too long
    }

    [Fact]
    public void ShouldRejectPlainCssFile()
    {
        FingerprintDetector.HasFingerprintedFileName("styles.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectPlainJsFile()
    {
        FingerprintDetector.HasFingerprintedFileName("scripts.js").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectFileWithNoExtension()
    {
        FingerprintDetector.HasFingerprintedFileName("noextension").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectFileWithSevenHexChars()
    {
        // Hash must be exactly 8 hex chars
        FingerprintDetector.HasFingerprintedFileName("styles.a1b2c3d.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectFileWithNineHexChars()
    {
        FingerprintDetector.HasFingerprintedFileName("styles.a1b2c3d4e.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectFileWithNonHexCharInHash()
    {
        FingerprintDetector.HasFingerprintedFileName("styles.a1b2c3z4.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectFileWithUppercaseHexInHash()
    {
        // Hash must be lowercase per AssetFingerprinter convention
        FingerprintDetector.HasFingerprintedFileName("styles.A1B2C3D4.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectDoubleExtensionAfterHash()
    {
        // "styles.a1b2c3d4.min.css" has a dot in the extension segment — must NOT match
        // because the pattern anchors to end-of-string with a single extension
        FingerprintDetector.HasFingerprintedFileName("styles.a1b2c3d4.min.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldThrowOnNullFileName()
    {
        Should.Throw<ArgumentNullException>(() =>
            FingerprintDetector.HasFingerprintedFileName(null!));
    }

    // ── IsFingerprintedAsset ──

    [Fact]
    public void ShouldDetectFingerprintedAssetUnderAstroDirectory()
    {
        FingerprintDetector.IsFingerprintedAsset("/_astro/styles.a1b2c3d4.css").ShouldBeTrue();
    }

    [Fact]
    public void ShouldDetectFingerprintedAssetWithForwardSlashPrefix()
    {
        FingerprintDetector.IsFingerprintedAsset("/_astro/scripts.00aabbcc.js").ShouldBeTrue();
    }

    [Fact]
    public void ShouldDetectFingerprintedAssetWithoutLeadingSlash()
    {
        FingerprintDetector.IsFingerprintedAsset("_astro/styles.a1b2c3d4.css").ShouldBeTrue();
    }

    [Fact]
    public void ShouldRejectNonFingerprintedAssetUnderAstroDirectory()
    {
        FingerprintDetector.IsFingerprintedAsset("/_astro/image.png").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectFingerprintedFileOutsideAstroDirectory()
    {
        FingerprintDetector.IsFingerprintedAsset("/public/styles.a1b2c3d4.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectHtmlFileUnderAstroDirectory()
    {
        FingerprintDetector.IsFingerprintedAsset("/_astro/index.html").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectRootHtmlFile()
    {
        FingerprintDetector.IsFingerprintedAsset("/index.html").ShouldBeFalse();
    }

    [Fact]
    public void ShouldRejectPlainCssAtRoot()
    {
        FingerprintDetector.IsFingerprintedAsset("/styles.css").ShouldBeFalse();
    }

    [Fact]
    public void ShouldHandleBackslashPathSeparator()
    {
        // Windows-style path separator should be normalised
        FingerprintDetector.IsFingerprintedAsset(@"\_astro\styles.a1b2c3d4.css").ShouldBeTrue();
    }

    [Fact]
    public void ShouldThrowOnNullPath()
    {
        Should.Throw<ArgumentNullException>(() =>
            FingerprintDetector.IsFingerprintedAsset(null!));
    }
}
