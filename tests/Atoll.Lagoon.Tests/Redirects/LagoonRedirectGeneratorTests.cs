using Atoll.Build.Content.Collections;
using Atoll.Lagoon.Redirects;

namespace Atoll.Lagoon.Tests.Redirects;

public sealed class LagoonRedirectGeneratorTests : IDisposable
{
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly string _outputDir;

    public LagoonRedirectGeneratorTests()
    {
        _outputDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_outputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDir))
        {
            Directory.Delete(_outputDir, recursive: true);
        }
    }

    // ── File content ──

    [Fact]
    public async Task GeneratesEmptyFile_WhenNoRules()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig([]);

        await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "_redirects"));
        content.ShouldBeEmpty();
    }

    [Fact]
    public async Task GeneratesPermanentRedirect_WhenStatusCode301()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig([new RedirectRule("/old", "/new", 301)]);

        await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "_redirects"));
        content.ShouldContain("/old /new 301");
    }

    [Fact]
    public async Task GeneratesTemporaryRedirect_WhenStatusCode302()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig([new RedirectRule("/temp", "/destination", 302)]);

        await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "_redirects"));
        content.ShouldContain("/temp /destination 302");
    }

    [Fact]
    public async Task GeneratesMultipleRules_InOrder()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig(
        [
            new RedirectRule("/first", "/a", 301),
            new RedirectRule("/second", "/b", 302),
            new RedirectRule("/third", "/c", 301),
        ]);

        await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        var lines = (await File.ReadAllTextAsync(Path.Combine(_outputDir, "_redirects")))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        lines.Length.ShouldBe(3);
        lines[0].Trim().ShouldBe("/first /a 301");
        lines[1].Trim().ShouldBe("/second /b 302");
        lines[2].Trim().ShouldBe("/third /c 301");
    }

    [Fact]
    public async Task SkipsRule_WhenFromIsEmpty()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig(
        [
            new RedirectRule("", "/new", 301),
            new RedirectRule("/valid", "/dest", 301),
        ]);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        result.RuleCount.ShouldBe(1);
        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "_redirects"));
        content.ShouldNotContain("/new");
        content.ShouldContain("/valid /dest 301");
    }

    [Fact]
    public async Task SkipsRule_WhenToIsEmpty()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig(
        [
            new RedirectRule("/old", "", 301),
            new RedirectRule("/valid", "/dest", 301),
        ]);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        result.RuleCount.ShouldBe(1);
        var content = await File.ReadAllTextAsync(Path.Combine(_outputDir, "_redirects"));
        content.ShouldContain("/valid /dest 301");
    }

    // ── Output path ──

    [Fact]
    public async Task WritesFile_ToOutputDirectory()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig([new RedirectRule("/old", "/new")]);

        await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        File.Exists(Path.Combine(_outputDir, "_redirects")).ShouldBeTrue();
    }

    // ── Result properties ──

    [Fact]
    public async Task RedirectGenerationResult_HasCorrectRuleCount()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig(
        [
            new RedirectRule("/a", "/x"),
            new RedirectRule("/b", "/y"),
            new RedirectRule("/c", "/z"),
        ]);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        result.RuleCount.ShouldBe(3);
    }

    [Fact]
    public async Task RedirectGenerationResult_ElapsedIsPositive()
    {
        var generator = new LagoonRedirectGenerator(_outputDir);
        var config = new StubRedirectConfig([new RedirectRule("/old", "/new")]);

        var result = await generator.GenerateAsync(CreateEmptyQuery(), config, _ct);

        result.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.Zero);
    }

    // ── RedirectRule defaults ──

    [Fact]
    public void DefaultStatusCode_Is301()
    {
        var rule = new RedirectRule("/old", "/new");

        rule.StatusCode.ShouldBe(301);
        rule.From.ShouldBe("/old");
        rule.To.ShouldBe("/new");
    }

    // ── Helpers ──

    private static CollectionQuery CreateEmptyQuery()
    {
        var config = new CollectionConfig("content");
        var provider = new InMemoryFileProvider();
        var loader = new CollectionLoader(config, provider);
        return new CollectionQuery(loader);
    }

    private sealed class StubRedirectConfig : IRedirectConfiguration
    {
        private readonly IReadOnlyList<RedirectRule> _rules;

        public StubRedirectConfig(IReadOnlyList<RedirectRule> rules)
        {
            _rules = rules;
        }

        public IEnumerable<RedirectRule> GetRedirects(CollectionQuery query) => _rules;
    }
}
