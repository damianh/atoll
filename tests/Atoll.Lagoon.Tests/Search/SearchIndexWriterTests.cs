using System.Text.Json;
using Atoll.Lagoon.Search;
using Shouldly;
using Xunit;

namespace Atoll.Lagoon.Tests.Search;

public sealed class SearchIndexWriterTests
{
    private readonly SearchIndexWriter _writer = new SearchIndexWriter();

    // ── Serialize ──

    [Fact]
    public void ShouldSerializeEmptyIndex()
    {
        var timestamp = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var index = new SearchIndex([], timestamp);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("entries").GetArrayLength().ShouldBe(0);
        doc.RootElement.GetProperty("generatedAt").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ShouldSerializeEntriesWithCamelCase()
    {
        var entry = new SearchEntry("Getting Started", "/docs/getting-started/", null, null, [], "body text");
        var index = new SearchIndex([entry], DateTimeOffset.UtcNow);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        var first = doc.RootElement.GetProperty("entries")[0];
        first.TryGetProperty("title", out _).ShouldBeTrue("title property should be camelCase");
        first.TryGetProperty("href", out _).ShouldBeTrue("href property should be camelCase");
        first.TryGetProperty("body", out _).ShouldBeTrue("body property should be camelCase");
        first.TryGetProperty("headings", out _).ShouldBeTrue("headings property should be camelCase");

        // PascalCase variants should NOT exist
        first.TryGetProperty("Title", out _).ShouldBeFalse("PascalCase Title should not exist");
        first.TryGetProperty("Href", out _).ShouldBeFalse("PascalCase Href should not exist");
    }

    [Fact]
    public void ShouldOmitNullDescriptionAndSection()
    {
        var entry = new SearchEntry("Title", "/path/", null, null, [], "body");
        var index = new SearchIndex([entry], DateTimeOffset.UtcNow);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        var first = doc.RootElement.GetProperty("entries")[0];
        first.TryGetProperty("description", out _).ShouldBeFalse("null description should be omitted");
        first.TryGetProperty("section", out _).ShouldBeFalse("null section should be omitted");
        first.TryGetProperty("topics", out _).ShouldBeFalse("null topics should be omitted");
    }

    [Fact]
    public void ShouldSerializeTopicsArrayWhenPopulated()
    {
        var entry = new SearchEntry(
            "Guide",
            "/docs/guide/",
            null,
            null,
            [],
            "body",
            ["IdentityServer", "Security"]);
        var index = new SearchIndex([entry], DateTimeOffset.UtcNow);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        var first = doc.RootElement.GetProperty("entries")[0];
        var topics = first.GetProperty("topics");
        topics.GetArrayLength().ShouldBe(2);
        topics[0].GetString().ShouldBe("IdentityServer");
        topics[1].GetString().ShouldBe("Security");
    }

    [Fact]
    public void ShouldSerializeTopicsAlongsideSection()
    {
        var entry = new SearchEntry(
            "Guide",
            "/docs/guide/",
            null,
            "Getting Started",
            [],
            "body",
            ["IdentityServer"]);
        var index = new SearchIndex([entry], DateTimeOffset.UtcNow);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        var first = doc.RootElement.GetProperty("entries")[0];
        first.GetProperty("section").GetString().ShouldBe("Getting Started");
        first.GetProperty("topics")[0].GetString().ShouldBe("IdentityServer");
    }

    [Fact]
    public void ShouldOmitTopicsWhenNull()
    {
        var entry = new SearchEntry("Title", "/path/", null, "Section", [], "body");
        var index = new SearchIndex([entry], DateTimeOffset.UtcNow);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        // The six-arg constructor leaves Topics null, so it should be omitted
        var first = doc.RootElement.GetProperty("entries")[0];
        first.TryGetProperty("topics", out _).ShouldBeFalse("topics should be omitted when null");
    }

    [Fact]
    public void ShouldPreserveAllEntryFields()
    {
        var headings = new List<string> { "Introduction", "Setup" };
        var entry = new SearchEntry(
            "Guide",
            "/docs/guide/",
            "A helpful guide",
            "Getting Started",
            headings,
            "plain body text");
        var index = new SearchIndex([entry], DateTimeOffset.UtcNow);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        var first = doc.RootElement.GetProperty("entries")[0];
        first.GetProperty("title").GetString().ShouldBe("Guide");
        first.GetProperty("href").GetString().ShouldBe("/docs/guide/");
        first.GetProperty("description").GetString().ShouldBe("A helpful guide");
        first.GetProperty("section").GetString().ShouldBe("Getting Started");
        first.GetProperty("body").GetString().ShouldBe("plain body text");

        var jsonHeadings = first.GetProperty("headings");
        jsonHeadings.GetArrayLength().ShouldBe(2);
        jsonHeadings[0].GetString().ShouldBe("Introduction");
        jsonHeadings[1].GetString().ShouldBe("Setup");
    }

    [Fact]
    public void ShouldSerializeGeneratedAtTimestamp()
    {
        var timestamp = new DateTimeOffset(2025, 3, 15, 9, 30, 0, TimeSpan.Zero);
        var index = new SearchIndex([], timestamp);

        var json = _writer.Serialize(index);
        using var doc = JsonDocument.Parse(json);

        // The generatedAt should round-trip as an ISO 8601 string
        var generatedAt = doc.RootElement.GetProperty("generatedAt").GetDateTimeOffset();
        generatedAt.ShouldBe(timestamp);
    }

    // ── WriteAsync ──

    [Fact]
    public async Task ShouldWriteSearchIndexJsonToOutputDirectory()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "atoll-writer-test-" + Guid.NewGuid().ToString("N")[..8]);
        try
        {
            var index = new SearchIndex([], DateTimeOffset.UtcNow);

            await _writer.WriteAsync(index, outputDir);

            var expectedPath = Path.Combine(outputDir, "search-index.json");
            File.Exists(expectedPath).ShouldBeTrue();
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ShouldCreateOutputDirectoryIfNotExists()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "atoll-writer-test-" + Guid.NewGuid().ToString("N")[..8]);
        try
        {
            Directory.Exists(outputDir).ShouldBeFalse("directory should not exist yet");

            var index = new SearchIndex([], DateTimeOffset.UtcNow);
            await _writer.WriteAsync(index, outputDir);

            Directory.Exists(outputDir).ShouldBeTrue("directory should be created");
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }

    [Fact]
    public void ShouldWriteSynchronouslyToOutputDirectory()
    {
        var outputDir = Path.Combine(Path.GetTempPath(), "atoll-writer-test-" + Guid.NewGuid().ToString("N")[..8]);
        try
        {
            var index = new SearchIndex([], DateTimeOffset.UtcNow);

            _writer.Write(index, outputDir);

            var expectedPath = Path.Combine(outputDir, "search-index.json");
            File.Exists(expectedPath).ShouldBeTrue();
        }
        finally
        {
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }
        }
    }
}
