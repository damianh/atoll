using System.Text.Json;
using Atoll.Build.Ssg;
using Atoll.Redirects;

namespace Atoll.Build.Tests.Ssg;

public sealed class RedirectsFileWriterTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _outputDir;

    public RedirectsFileWriterTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "atoll-redirects-test-" + Guid.NewGuid().ToString("N")[..8]);
        _outputDir = Path.Combine(_testDir, "dist");
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }

    private static RedirectMap CreateMap(params (string from, string to)[] entries)
    {
        return RedirectMap.Create(entries.Select(e => new KeyValuePair<string, string>(e.from, e.to)));
    }

    // ----------------------------------------------------------------
    // Serialize — structure and content
    // ----------------------------------------------------------------

    [Fact]
    public void SerializeShouldProduceValidJson()
    {
        var map = CreateMap(("/old", "/new"));

        var json = RedirectsFileWriter.Serialize(map);

        Should.NotThrow(() => JsonDocument.Parse(json));
    }

    [Fact]
    public void SerializeShouldIncludeAllEntries()
    {
        var map = CreateMap(("/old-a", "/new-a"), ("/old-b", "/new-b"));

        var json = RedirectsFileWriter.Serialize(map);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("redirects").GetArrayLength().ShouldBe(2);
    }

    [Fact]
    public void SerializeShouldUseCamelCasePropertyNames()
    {
        var map = CreateMap(("/old", "/new"));

        var json = RedirectsFileWriter.Serialize(map);
        var doc = JsonDocument.Parse(json);

        var entry = doc.RootElement.GetProperty("redirects")[0];
        entry.TryGetProperty("from", out _).ShouldBeTrue();
        entry.TryGetProperty("to", out _).ShouldBeTrue();
        entry.TryGetProperty("status", out _).ShouldBeTrue();
    }

    [Fact]
    public void SerializeShouldDefaultStatusTo301()
    {
        var map = CreateMap(("/old", "/new"));

        var json = RedirectsFileWriter.Serialize(map);
        var doc = JsonDocument.Parse(json);

        var entry = doc.RootElement.GetProperty("redirects")[0];
        entry.GetProperty("status").GetInt32().ShouldBe(301);
    }

    [Fact]
    public void SerializeShouldUseCustomStatusCode()
    {
        var map = CreateMap(("/old", "/new"));

        var json = RedirectsFileWriter.Serialize(map, 302);
        var doc = JsonDocument.Parse(json);

        var entry = doc.RootElement.GetProperty("redirects")[0];
        entry.GetProperty("status").GetInt32().ShouldBe(302);
    }

    [Fact]
    public void SerializeShouldIncludeCorrectFromAndToValues()
    {
        var map = CreateMap(("/old-page", "/new-page"));

        var json = RedirectsFileWriter.Serialize(map);
        var doc = JsonDocument.Parse(json);

        var entry = doc.RootElement.GetProperty("redirects")[0];
        entry.GetProperty("from").GetString().ShouldBe("/old-page");
        entry.GetProperty("to").GetString().ShouldBe("/new-page");
    }

    [Fact]
    public void SerializeShouldProduceEmptyArrayForEmptyMap()
    {
        var map = RedirectMap.Empty;

        var json = RedirectsFileWriter.Serialize(map);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("redirects").GetArrayLength().ShouldBe(0);
    }

    // ----------------------------------------------------------------
    // WriteAsync — file I/O
    // ----------------------------------------------------------------

    [Fact]
    public async Task WriteAsyncShouldWriteFileToOutputDirectory()
    {
        var map = CreateMap(("/old", "/new"));
        var writer = new RedirectsFileWriter(_outputDir);

        await writer.WriteAsync(map);

        File.Exists(Path.Combine(_outputDir, "redirects.json")).ShouldBeTrue();
    }

    [Fact]
    public async Task WriteAsyncShouldWriteValidJsonContent()
    {
        var map = CreateMap(("/old", "/new"));
        var writer = new RedirectsFileWriter(_outputDir);

        await writer.WriteAsync(map);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "redirects.json"));
        Should.NotThrow(() => JsonDocument.Parse(content));
    }

    [Fact]
    public async Task WriteAsyncWithStatusCodeShouldEmbedCustomStatus()
    {
        var map = CreateMap(("/old", "/new"));
        var writer = new RedirectsFileWriter(_outputDir);

        await writer.WriteAsync(map, 308);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "redirects.json"));
        var doc = JsonDocument.Parse(content);
        doc.RootElement.GetProperty("redirects")[0].GetProperty("status").GetInt32().ShouldBe(308);
    }
}
