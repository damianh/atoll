using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Atoll.Mermaid.Islands;

namespace Atoll.Mermaid.Tests;

/// <summary>
/// Guards the bundled Mermaid build against accidental edits and incomplete upgrades.
/// Regenerate the bundle with <c>eng/update-mermaid.ps1</c> rather than editing files by hand.
/// </summary>
public sealed partial class MermaidAssetTests
{
    private static readonly Assembly ResourceAssembly = typeof(MermaidIslandAssetProvider).Assembly;

    [GeneratedRegex(@"\b(?:from|import)\s*\(?\s*[""'](?<spec>[^""']+)[""']")]
    private static partial Regex ImportSpecifierRegex();

    [GeneratedRegex(@"const BUNDLED_VERSION = '(?<version>[^']*)';")]
    private static partial Regex BundledVersionRegex();

    private static byte[] ReadResource(string name)
    {
        using var stream = ResourceAssembly.GetManifestResourceStream(name);
        stream.ShouldNotBeNull($"Embedded resource '{name}' is missing.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static string ReadResourceText(string name) =>
        System.Text.Encoding.UTF8.GetString(ReadResource(name));

    [Fact]
    public void ManifestShouldDescribePinnedMermaidPackage()
    {
        var manifest = MermaidIslandAssetProvider.Manifest;

        manifest.Package.ShouldBe("mermaid");
        manifest.Version.ShouldMatch(@"^\d+\.\d+\.\d+$");
        manifest.Tarball.ShouldBe($"https://registry.npmjs.org/mermaid/-/mermaid-{manifest.Version}.tgz");
        manifest.Integrity.ShouldStartWith("sha512-");
        manifest.Files.ShouldContainKey("mermaid.esm.min.mjs");
    }

    [Fact]
    public void BundledFilesShouldMatchManifestHashes()
    {
        var mismatches = new List<string>();
        foreach (var (path, expectedHash) in MermaidIslandAssetProvider.Manifest.Files)
        {
            var bytes = ReadResource(MermaidIslandAssetProvider.VendorResourcePrefix + path);
            var actualHash = Convert.ToHexStringLower(SHA256.HashData(bytes));
            if (actualHash != expectedHash)
            {
                mismatches.Add($"{path}: expected {expectedHash}, got {actualHash}");
            }
        }

        mismatches.ShouldBeEmpty();
    }

    [Fact]
    public void EmbeddedBundleShouldContainOnlyManifestFiles()
    {
        var prefix = MermaidIslandAssetProvider.VendorResourcePrefix;
        var embedded = ResourceAssembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix, StringComparison.Ordinal))
            .Select(n => n[prefix.Length..])
            .Where(n => n is not "manifest.json" and not "LICENSE")
            .Order(StringComparer.Ordinal)
            .ToList();

        embedded.ShouldBe(MermaidIslandAssetProvider.Manifest.Files.Keys.Order(StringComparer.Ordinal).ToList());
        ResourceAssembly.GetManifestResourceNames().ShouldContain(MermaidIslandAssetProvider.LicenseResourceName);
    }

    [Fact]
    public void BundledImportsShouldResolveToBundledFiles()
    {
        var files = MermaidIslandAssetProvider.Manifest.Files.Keys.ToHashSet(StringComparer.Ordinal);
        var problems = new List<string>();
        var relativeImports = 0;

        foreach (var path in files)
        {
            var source = ReadResourceText(MermaidIslandAssetProvider.VendorResourcePrefix + path);
            foreach (Match match in ImportSpecifierRegex().Matches(source))
            {
                var spec = match.Groups["spec"].Value;
                if (spec.StartsWith("./", StringComparison.Ordinal) || spec.StartsWith("../", StringComparison.Ordinal))
                {
                    relativeImports++;
                    var resolved = ResolveRelative(path, spec);
                    if (!files.Contains(resolved))
                    {
                        problems.Add($"{path} imports missing file '{spec}' ({resolved})");
                    }
                }
                else if (spec.Contains("://", StringComparison.Ordinal) || spec.StartsWith("//", StringComparison.Ordinal))
                {
                    problems.Add($"{path} imports external URL '{spec}'");
                }
            }
        }

        problems.ShouldBeEmpty();
        relativeImports.ShouldBeGreaterThan(files.Count, "the chunked build should import its chunks");
    }

    [Fact]
    public void InitScriptShouldReferenceBundledVersion()
    {
        var script = ReadResourceText(MermaidIslandAssetProvider.InitScriptResourceName);

        var match = BundledVersionRegex().Match(script);
        match.Success.ShouldBeTrue();
        match.Groups["version"].Value.ShouldBe(MermaidIslandAssetProvider.Manifest.Version);
        script.ShouldContain("./atoll-mermaid/${BUNDLED_VERSION}/mermaid.esm.min.mjs");
    }

    [Fact]
    public void InitScriptShouldNotLoadFromThirdPartyOrigins()
    {
        var script = ReadResourceText(MermaidIslandAssetProvider.InitScriptResourceName);

        script.ShouldNotContain("jsdelivr", Case.Insensitive);
        script.ShouldNotContain("https://");
        script.ShouldNotContain("http://");
    }

    [Fact]
    public void InitScriptShouldUseStrictSecurityLevel()
    {
        var script = ReadResourceText(MermaidIslandAssetProvider.InitScriptResourceName);

        script.ShouldContain("securityLevel: 'strict'");
        script.ShouldContain("startOnLoad: false");
    }

    [Fact]
    public void ProviderShouldServeInitScriptAndBundledFiles()
    {
        var manifest = MermaidIslandAssetProvider.Manifest;
        var assets = new MermaidIslandAssetProvider().GetAssets().ToList();

        assets.ShouldContain(a =>
            a.OutputPath == "scripts/atoll-docs-mermaid-init.js" &&
            a.ResourceName == MermaidIslandAssetProvider.InitScriptResourceName);
        assets.ShouldContain(a => a.OutputPath == $"scripts/atoll-mermaid/{manifest.Version}/mermaid.esm.min.mjs");
        assets.ShouldContain(a => a.OutputPath == $"scripts/atoll-mermaid/{manifest.Version}/LICENSE");

        foreach (var path in manifest.Files.Keys)
        {
            assets.ShouldContain(a =>
                a.OutputPath == $"scripts/atoll-mermaid/{manifest.Version}/{path}" &&
                a.ResourceName == MermaidIslandAssetProvider.VendorResourcePrefix + path);
        }

        assets.Count.ShouldBe(manifest.Files.Count + 2);
        assets.Select(a => a.OutputPath).ShouldBeUnique();
        assets.ShouldAllBe(a => a.ResourceAssembly == ResourceAssembly);
    }

    private static string ResolveRelative(string fromPath, string spec)
    {
        var segments = fromPath.Split('/').SkipLast(1).ToList();
        foreach (var part in spec.Split('/'))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (segments.Count == 0)
                {
                    return "<outside bundle>/" + spec;
                }

                segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(part);
        }

        return string.Join('/', segments);
    }
}
