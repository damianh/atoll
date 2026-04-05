using System.Text.RegularExpressions;

namespace Atoll.Build.Pipeline;

/// <summary>
/// Detects whether a file path refers to a fingerprinted (content-hashed) asset
/// produced by <see cref="AssetFingerprinter"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AssetFingerprinter"/> inserts an 8-character lowercase hex hash before
/// the file extension, producing names such as <c>styles.a1b2c3d4.css</c>.
/// Assets are placed under the <c>_atoll/</c> subdirectory by default.
/// </para>
/// </remarks>
public static class FingerprintDetector
{
    // Matches filenames where the hash segment is 8 lowercase hex chars immediately
    // before a single non-dot extension — e.g. "styles.a1b2c3d4.css".
    // Anchored to end-of-string so "styles.a1b2c3d4.min.css" does NOT match.
    private static readonly Regex FingerprintPattern =
        new(@"\.[0-9a-f]{8}\.[^.]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="path"/> refers to a file inside the
    /// <c>_atoll/</c> subdirectory whose filename contains a fingerprint hash segment.
    /// </summary>
    /// <param name="path">The URL or file path to test (e.g., <c>/_atoll/styles.a1b2c3d4.css</c>).</param>
    public static bool IsFingerprintedAsset(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        // Normalise to forward slashes for consistent matching regardless of OS path separator
        var normalised = path.Replace('\\', '/');

        // Must be under the _atoll/ directory
        if (!normalised.Contains("/_atoll/") && !normalised.StartsWith("_atoll/", StringComparison.Ordinal))
        {
            return false;
        }

        return HasFingerprintedFileName(System.IO.Path.GetFileName(normalised));
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="fileName"/> contains a fingerprint hash
    /// segment — i.e. matches the pattern <c>name.XXXXXXXX.ext</c> where
    /// <c>XXXXXXXX</c> is exactly 8 lowercase hex characters.
    /// </summary>
    /// <param name="fileName">The bare filename to test (e.g., <c>styles.a1b2c3d4.css</c>).</param>
    public static bool HasFingerprintedFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        return FingerprintPattern.IsMatch(fileName);
    }
}
