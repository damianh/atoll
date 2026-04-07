using Atoll.Build.Pipeline;

namespace Atoll.Build.Tests.Pipeline;

public sealed class AssetFingerprinterTests
{
    [Fact]
    public void ComputeHashShouldReturnDeterministicHashForStringContent()
    {
        var hash1 = AssetFingerprinter.ComputeHash("body { color: red; }");
        var hash2 = AssetFingerprinter.ComputeHash("body { color: red; }");

        hash1.ShouldBe(hash2);
        hash1.Length.ShouldBe(8);
    }

    [Fact]
    public void ComputeHashShouldReturnDifferentHashForDifferentContent()
    {
        var hash1 = AssetFingerprinter.ComputeHash("body { color: red; }");
        var hash2 = AssetFingerprinter.ComputeHash("body { color: blue; }");

        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void ComputeHashShouldReturnLowercaseHex()
    {
        var hash = AssetFingerprinter.ComputeHash("test content");

        hash.ShouldSatisfyAllConditions(
            () => hash.Length.ShouldBe(8),
            () => hash.All(c => "0123456789abcdef".Contains(c)).ShouldBeTrue()
        );
    }

    [Fact]
    public void ComputeHashWithCustomLengthShouldRespectLength()
    {
        var hash = AssetFingerprinter.ComputeHash("test content", 12);

        hash.Length.ShouldBe(12);
    }

    [Fact]
    public void ComputeHashShouldThrowOnNullString()
    {
        Should.Throw<ArgumentNullException>(() => AssetFingerprinter.ComputeHash((string)null!));
    }

    [Fact]
    public void ComputeHashShouldThrowOnZeroLength()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => AssetFingerprinter.ComputeHash("test", 0));
    }

    [Fact]
    public void ComputeHashForBytesShoulDReturnDeterministicHash()
    {
        var bytes = "body { color: red; }"u8.ToArray();
        var hash1 = AssetFingerprinter.ComputeHash(bytes);
        var hash2 = AssetFingerprinter.ComputeHash(bytes);

        hash1.ShouldBe(hash2);
        hash1.Length.ShouldBe(8);
    }

    [Fact]
    public void ComputeHashForBytesShouldThrowOnNullArray()
    {
        Should.Throw<ArgumentNullException>(() => AssetFingerprinter.ComputeHash((byte[])null!));
    }

    [Fact]
    public void ComputeHashForBytesShouldMatchStringHash()
    {
        var text = "test content";
        var hashFromString = AssetFingerprinter.ComputeHash(text);
        var hashFromBytes = AssetFingerprinter.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));

        hashFromString.ShouldBe(hashFromBytes);
    }

    [Fact]
    public void CreateFingerprintedFileNameShouldInsertHashBeforeExtension()
    {
        var result = AssetFingerprinter.CreateFingerprintedFileName("styles.css", "a1b2c3d4");

        result.ShouldBe("styles.a1b2c3d4.css");
    }

    [Fact]
    public void CreateFingerprintedFileNameShouldHandleNoExtension()
    {
        var result = AssetFingerprinter.CreateFingerprintedFileName("styles", "a1b2c3d4");

        result.ShouldBe("styles.a1b2c3d4");
    }

    [Fact]
    public void CreateFingerprintedFileNameShouldHandleDoubleExtension()
    {
        var result = AssetFingerprinter.CreateFingerprintedFileName("bundle.min.js", "a1b2c3d4");

        result.ShouldBe("bundle.min.a1b2c3d4.js");
    }

    [Fact]
    public void CreateFingerprintedFileNameShouldThrowOnNull()
    {
        Should.Throw<ArgumentNullException>(() => AssetFingerprinter.CreateFingerprintedFileName(null!, "hash"));
        Should.Throw<ArgumentNullException>(() => AssetFingerprinter.CreateFingerprintedFileName("file.css", null!));
    }

    [Fact]
    public void CreateFingerprintedPathShouldPreserveDirectory()
    {
        var result = AssetFingerprinter.CreateFingerprintedPath(
            Path.Combine("_atoll", "styles.css"), "a1b2c3d4");

        result.ShouldBe(Path.Combine("_atoll", "styles.a1b2c3d4.css"));
    }

    [Fact]
    public void CreateFingerprintedPathShouldHandleFileNameOnly()
    {
        var result = AssetFingerprinter.CreateFingerprintedPath("styles.css", "a1b2c3d4");

        result.ShouldBe("styles.a1b2c3d4.css");
    }

    [Fact]
    public void FingerprintWithStringShouldReturnFileNameAndHash()
    {
        var (fileName, hash) = AssetFingerprinter.Fingerprint("styles.css", "body { color: red; }");

        hash.Length.ShouldBe(8);
        fileName.ShouldContain(hash);
        fileName.ShouldStartWith("styles.");
        fileName.ShouldEndWith(".css");
    }

    [Fact]
    public void FingerprintWithBytesShouldReturnFileNameAndHash()
    {
        var bytes = "body { color: red; }"u8.ToArray();
        var (fileName, hash) = AssetFingerprinter.Fingerprint("styles.css", bytes);

        hash.Length.ShouldBe(8);
        fileName.ShouldContain(hash);
        fileName.ShouldStartWith("styles.");
        fileName.ShouldEndWith(".css");
    }

    [Fact]
    public void FingerprintShouldBeConsistentWithComputeHash()
    {
        var content = "body { color: red; }";
        var expectedHash = AssetFingerprinter.ComputeHash(content);
        var (fileName, hash) = AssetFingerprinter.Fingerprint("styles.css", content);

        hash.ShouldBe(expectedHash);
        fileName.ShouldBe($"styles.{expectedHash}.css");
    }
}
