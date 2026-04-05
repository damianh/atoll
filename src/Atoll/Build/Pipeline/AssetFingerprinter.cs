using System.Security.Cryptography;
using System.Text;

namespace Atoll.Build.Pipeline;

/// <summary>
/// Generates content-hash fingerprinted filenames for cache busting.
/// Uses SHA-256 to produce a truncated hex hash that is inserted into
/// the filename before the extension.
/// </summary>
/// <remarks>
/// <para>
/// For example, <c>styles.css</c> with hash <c>a1b2c3d4</c> becomes
/// <c>styles.a1b2c3d4.css</c>. This ensures browsers cache-bust when content changes.
/// </para>
/// </remarks>
public static class AssetFingerprinter
{
    private const int DefaultHashLength = 8;

    /// <summary>
    /// Computes a content hash from the specified text content.
    /// </summary>
    /// <param name="content">The text content to hash.</param>
    /// <returns>A lowercase hex hash string of <paramref name="hashLength"/> characters.</returns>
    /// <param name="hashLength">The number of hex characters to include. Defaults to 8.</param>
    public static string ComputeHash(string content, int hashLength)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hashLength);

        var bytes = Encoding.UTF8.GetBytes(content);
        return ComputeHash(bytes, hashLength);
    }

    /// <summary>
    /// Computes a content hash from the specified text content using the default hash length.
    /// </summary>
    /// <param name="content">The text content to hash.</param>
    /// <returns>A lowercase hex hash string of 8 characters.</returns>
    public static string ComputeHash(string content)
    {
        return ComputeHash(content, DefaultHashLength);
    }

    /// <summary>
    /// Computes a content hash from the specified binary content.
    /// </summary>
    /// <param name="content">The binary content to hash.</param>
    /// <param name="hashLength">The number of hex characters to include. Defaults to 8.</param>
    /// <returns>A lowercase hex hash string of <paramref name="hashLength"/> characters.</returns>
    public static string ComputeHash(byte[] content, int hashLength)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hashLength);

        var hashBytes = SHA256.HashData(content);
        var fullHex = Convert.ToHexStringLower(hashBytes);

        // Clamp to available hex length
        var effectiveLength = Math.Min(hashLength, fullHex.Length);
        return fullHex[..effectiveLength];
    }

    /// <summary>
    /// Computes a content hash from the specified binary content using the default hash length.
    /// </summary>
    /// <param name="content">The binary content to hash.</param>
    /// <returns>A lowercase hex hash string of 8 characters.</returns>
    public static string ComputeHash(byte[] content)
    {
        return ComputeHash(content, DefaultHashLength);
    }

    /// <summary>
    /// Creates a fingerprinted filename by inserting the content hash before the file extension.
    /// </summary>
    /// <param name="fileName">The original filename (e.g., <c>styles.css</c>).</param>
    /// <param name="hash">The content hash string.</param>
    /// <returns>The fingerprinted filename (e.g., <c>styles.a1b2c3d4.css</c>).</returns>
    public static string CreateFingerprintedFileName(string fileName, string hash)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(hash);

        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        return $"{nameWithoutExtension}.{hash}{extension}";
    }

    /// <summary>
    /// Creates a fingerprinted path by replacing the filename portion with a fingerprinted version.
    /// </summary>
    /// <param name="filePath">The original file path (e.g., <c>_atoll/styles.css</c>).</param>
    /// <param name="hash">The content hash string.</param>
    /// <returns>The fingerprinted path (e.g., <c>_atoll/styles.a1b2c3d4.css</c>).</returns>
    public static string CreateFingerprintedPath(string filePath, string hash)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(hash);

        var directory = Path.GetDirectoryName(filePath) ?? "";
        var fingerprintedName = CreateFingerprintedFileName(Path.GetFileName(filePath), hash);

        return directory.Length > 0
            ? Path.Combine(directory, fingerprintedName)
            : fingerprintedName;
    }

    /// <summary>
    /// Computes a hash and creates a fingerprinted filename in a single operation.
    /// </summary>
    /// <param name="fileName">The original filename.</param>
    /// <param name="content">The text content to hash.</param>
    /// <returns>A tuple of (fingerprinted filename, hash).</returns>
    public static (string FileName, string Hash) Fingerprint(string fileName, string content)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var hash = ComputeHash(content);
        var fingerprintedName = CreateFingerprintedFileName(fileName, hash);
        return (fingerprintedName, hash);
    }

    /// <summary>
    /// Computes a hash and creates a fingerprinted filename in a single operation.
    /// </summary>
    /// <param name="fileName">The original filename.</param>
    /// <param name="content">The binary content to hash.</param>
    /// <returns>A tuple of (fingerprinted filename, hash).</returns>
    public static (string FileName, string Hash) Fingerprint(string fileName, byte[] content)
    {
        ArgumentNullException.ThrowIfNull(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var hash = ComputeHash(content);
        var fingerprintedName = CreateFingerprintedFileName(fileName, hash);
        return (fingerprintedName, hash);
    }
}
