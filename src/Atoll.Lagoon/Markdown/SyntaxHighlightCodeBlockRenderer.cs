using System.Collections.Concurrent;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// HTML renderer for code blocks that applies server-side syntax highlighting using
/// TextMate grammars. Fenced code blocks with a recognized language identifier are
/// tokenized and rendered with <c>&lt;span&gt;</c> elements carrying semantic CSS
/// class names (e.g., <c>tm-keyword</c>, <c>tm-string</c>).
/// Code blocks with unrecognized or absent language identifiers are forwarded to
/// a fallback renderer.
/// </summary>
internal sealed class SyntaxHighlightCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    // Maps fenced code block language identifiers (lower-case) to TextMate scope IDs.
    private static readonly Dictionary<string, string> LanguageScopes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["csharp"] = "source.cs",
            ["cs"] = "source.cs",
            ["javascript"] = "source.js",
            ["js"] = "source.js",
            ["typescript"] = "source.ts",
            ["ts"] = "source.ts",
            ["html"] = "text.html.basic",
            ["css"] = "source.css",
            ["json"] = "source.json",
            ["xml"] = "text.xml",
            ["yaml"] = "source.yaml",
            ["yml"] = "source.yaml",
            ["bash"] = "source.shell",
            ["shell"] = "source.shell",
            ["sh"] = "source.shell",
            ["python"] = "source.python",
            ["py"] = "source.python",
            ["sql"] = "source.sql",
            ["java"] = "source.java",
            ["go"] = "source.go",
            ["ruby"] = "source.ruby",
            ["rb"] = "source.ruby",
            ["rust"] = "source.rust",
            ["rs"] = "source.rust",
            ["cpp"] = "source.cpp",
            ["c++"] = "source.cpp",
            ["c"] = "source.c",
            ["php"] = "source.php",
            ["swift"] = "source.swift",
            ["kotlin"] = "source.kotlin",
            ["kt"] = "source.kotlin",
            ["dockerfile"] = "source.dockerfile",
            ["powershell"] = "source.powershell",
            ["pwsh"] = "source.powershell",
            ["ps1"] = "source.powershell",
        };

    // Lazily-initialized shared Registry — expensive to construct, safe to share.
    private static readonly Lazy<Registry> SharedRegistry = new(
        static () => new Registry(new RegistryOptions(ThemeName.DarkPlus)),
        LazyThreadSafetyMode.ExecutionAndPublication);

    // Grammar cache keyed by scope ID.  Lazy<T> ensures that only one thread ever
    // invokes Registry.LoadGrammar for a given scope — the registry is not thread-safe.
    private static readonly ConcurrentDictionary<string, Lazy<IGrammar?>> GrammarCache = new();

    // Lock to serialize ALL grammar loads AND tokenization calls — the registry's
    // internal SyncRegistry and Grammar.RegisterRule use non-concurrent Dictionary,
    // making both LoadGrammar and TokenizeLine unsafe for concurrent access.
    private static readonly object GrammarLoadLock = new();

    private readonly IMarkdownObjectRenderer _fallback;

    /// <summary>
    /// Initializes a new instance of <see cref="SyntaxHighlightCodeBlockRenderer"/>.
    /// </summary>
    /// <param name="fallback">
    /// The renderer to delegate to when the code block has no recognized language
    /// identifier (e.g., the default <see cref="CodeBlockRenderer"/> or
    /// <see cref="MermaidCodeBlockRenderer"/>).
    /// </param>
    internal SyntaxHighlightCodeBlockRenderer(IMarkdownObjectRenderer fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        _fallback = fallback;
    }

    /// <inheritdoc />
    protected override void Write(HtmlRenderer renderer, CodeBlock block)
    {
        if (block is FencedCodeBlock fenced &&
            fenced.Info is { Length: > 0 } lang &&
            LanguageScopes.TryGetValue(lang, out var scopeId))
        {
            var grammar = GrammarCache
                .GetOrAdd(scopeId, static id => new Lazy<IGrammar?>(
                    () => LoadGrammarSafe(id),
                    LazyThreadSafetyMode.ExecutionAndPublication))
                .Value;

            if (grammar is not null)
            {
                WriteHighlightedBlock(renderer, fenced, grammar, lang);
                return;
            }
        }

        _fallback.Write(renderer, block);
    }

    /// <summary>
    /// Attempts to load a TextMate grammar by scope ID. Returns <c>null</c> instead
    /// of throwing when the grammar definition cannot be located (e.g., missing
    /// embedded resources in certain assembly-loading contexts).
    /// </summary>
    private static IGrammar? LoadGrammarSafe(string scopeId)
    {
        try
        {
            lock (GrammarLoadLock)
            {
                return SharedRegistry.Value.LoadGrammar(scopeId);
            }
        }
        catch
        {
            // Grammar definition unavailable in this assembly-loading context — fall
            // back to plain-text rendering for this language.
            return null;
        }
    }

    private static void WriteHighlightedBlock(
        HtmlRenderer renderer,
        FencedCodeBlock block,
        IGrammar grammar,
        string lang)
    {
        renderer.Write("<pre class=\"highlight\"><code class=\"language-");
        renderer.Write(lang.ToLowerInvariant());
        renderer.Write("\">");

        var lines = CollectLines(block);

        IStateStack ruleStack = StateStack.NULL;
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            ITokenizeLineResult result;
            lock (GrammarLoadLock)
            {
                result = grammar.TokenizeLine(
                    new LineText(line),
                    ruleStack,
                    TimeSpan.FromSeconds(5));
            }

            ruleStack = result.RuleStack;

            WriteTokenizedLine(renderer, line, result.Tokens);

            if (i < lines.Count - 1)
            {
                renderer.WriteLine();
            }
        }

        renderer.Write("</code></pre>");
        renderer.WriteLine();
    }

    private static void WriteTokenizedLine(
        HtmlRenderer renderer,
        string line,
        IEnumerable<IToken> tokens)
    {
        foreach (var token in tokens)
        {
            var start = token.StartIndex;
            var end = token.EndIndex;

            if (start >= end || start >= line.Length)
            {
                continue;
            }

            end = Math.Min(end, line.Length);
            var text = line[start..end];

            var cssClass = MapScopesToClass(token.Scopes);

            if (cssClass is null)
            {
                WriteEscaped(renderer, text);
            }
            else
            {
                renderer.Write("<span class=\"");
                renderer.Write(cssClass);
                renderer.Write("\">");
                WriteEscaped(renderer, text);
                renderer.Write("</span>");
            }
        }
    }

    private static void WriteEscaped(HtmlRenderer renderer, string text)
    {
        // HtmlRenderer.WriteEscape encodes <, >, &, ", '
        renderer.WriteEscape(text);
    }

    private static string? MapScopesToClass(List<string> scopes)
    {
        // Scopes are ordered from least to most specific; iterate in reverse to find
        // the most specific matching rule first.
        for (var i = scopes.Count - 1; i >= 0; i--)
        {
            var scope = scopes[i];
            var mapped = MapSingleScope(scope);
            if (mapped is not null)
            {
                return mapped;
            }
        }

        return null;
    }

    private static string? MapSingleScope(string scope)
    {
        if (scope.StartsWith("keyword", StringComparison.Ordinal) ||
            scope.StartsWith("storage.modifier", StringComparison.Ordinal))
        {
            return "tm-keyword";
        }

        if (scope.StartsWith("string", StringComparison.Ordinal))
        {
            return "tm-string";
        }

        if (scope.StartsWith("comment", StringComparison.Ordinal))
        {
            return "tm-comment";
        }

        if (scope.StartsWith("constant.numeric", StringComparison.Ordinal))
        {
            return "tm-number";
        }

        if (scope.StartsWith("entity.name.function", StringComparison.Ordinal) ||
            scope.StartsWith("support.function", StringComparison.Ordinal))
        {
            return "tm-function";
        }

        if (scope.StartsWith("entity.name.type", StringComparison.Ordinal) ||
            scope.StartsWith("entity.name.class", StringComparison.Ordinal) ||
            scope.StartsWith("storage.type", StringComparison.Ordinal) ||
            scope.StartsWith("support.type", StringComparison.Ordinal) ||
            scope.StartsWith("support.class", StringComparison.Ordinal))
        {
            return "tm-type";
        }

        if (scope.StartsWith("entity.name.namespace", StringComparison.Ordinal))
        {
            return "tm-namespace";
        }

        if (scope.StartsWith("variable", StringComparison.Ordinal))
        {
            return "tm-variable";
        }

        if (scope.StartsWith("constant", StringComparison.Ordinal))
        {
            return "tm-constant";
        }

        if (scope.StartsWith("punctuation", StringComparison.Ordinal))
        {
            return "tm-punctuation";
        }

        if (scope.StartsWith("meta.preprocessor", StringComparison.Ordinal) ||
            scope.StartsWith("keyword.preprocessor", StringComparison.Ordinal))
        {
            return "tm-preprocessor";
        }

        return null;
    }

    private static List<string> CollectLines(FencedCodeBlock block)
    {
        var lines = new List<string>();
        var slices = block.Lines;

        for (var i = 0; i < slices.Count; i++)
        {
            var line = slices.Lines[i];
            var slice = line.Slice;
            // Reconstruct the string for this line from the slice.
            var text = slice.Text is null
                ? string.Empty
                : slice.Text.Substring(slice.Start, slice.Length);
            lines.Add(text);
        }

        return lines;
    }
}
