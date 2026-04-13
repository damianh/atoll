using System.Collections.Concurrent;
using Atoll.Mermaid;
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
    // Languages that auto-detect as a terminal frame.
    private static readonly HashSet<string> TerminalLanguages =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "bash", "sh", "shell", "powershell", "pwsh", "ps1",
            "cmd", "bat", "zsh", "fish", "terminal", "console",
        };

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
            ["zsh"] = "source.shell",
            ["fish"] = "source.shell",
            ["cmd"] = "source.shell",
            ["bat"] = "source.shell",
            ["terminal"] = "source.shell",
            ["console"] = "source.shell",
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

    // Copy-to-clipboard button — now placed in the frame header.
    // The JS walks up to '.ec-frame' or '.code-block-wrapper' (frameless fallback)
    // to find the <code> element and copy its text content.
    private const string CopyButtonHtml = """
        <button type="button" class="code-copy-btn" aria-label="Copy code" onclick="
            let w=this.closest('.ec-frame')||this.closest('.code-block-wrapper');
            let c=w&&w.querySelector('code');
            if(c)navigator.clipboard.writeText(c.innerText).then(()=>{
                this.classList.add('copied');
                setTimeout(()=>this.classList.remove('copied'),2000);
            })">
            <svg class="copy-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
                 stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <rect x="9" y="9" width="13" height="13" rx="2" ry="2"/>
                <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
            </svg>
            <svg class="check-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
                 stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="20 6 9 17 4 12"/>
            </svg>
        </button>
        """;

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
            fenced.Info is { Length: > 0 } lang)
        {
            // Parse expressive-code metadata from the fence info string regardless of
            // whether we can syntax-highlight — the meta drives frame, markers, etc.
            var meta = CodeBlockMetaParser.Parse(fenced.Info, fenced.Arguments?.ToString());

            // For diff blocks, try to highlight using the underlying diff language.
            var effectiveLang = (lang.Equals("diff", StringComparison.OrdinalIgnoreCase) &&
                                 meta.DiffLang is { Length: > 0 })
                ? meta.DiffLang
                : lang;

            if (LanguageScopes.TryGetValue(effectiveLang!, out var scopeId))
            {
                var grammar = GrammarCache
                    .GetOrAdd(scopeId, static id => new Lazy<IGrammar?>(
                        () => LoadGrammarSafe(id),
                        LazyThreadSafetyMode.ExecutionAndPublication))
                    .Value;

            if (grammar is not null)
            {
                var isDiff = lang.Equals("diff", StringComparison.OrdinalIgnoreCase);
                WriteHighlightedBlock(renderer, fenced, grammar, effectiveLang!, meta, isDiff);
                return;
            }
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

    /// <summary>
    /// Resolves the effective frame type for a code block, applying auto-detection
    /// rules when <see cref="CodeFrameType.Auto"/> is specified.
    /// </summary>
    private static CodeFrameType ResolveFrameType(CodeBlockMeta meta, string lang)
    {
        if (meta.Frame != CodeFrameType.Auto)
        {
            return meta.Frame;
        }

        // A title implies an editor frame.
        if (meta.Title is { Length: > 0 })
        {
            return CodeFrameType.Code;
        }

        // Terminal-like languages auto-detect as a terminal frame.
        if (TerminalLanguages.Contains(lang))
        {
            return CodeFrameType.Terminal;
        }

        // Default: editor frame.
        return CodeFrameType.Code;
    }

    /// <summary>
    /// Returns the CSS extra class name for a line marker (e.g., <c>"ec-mark"</c>),
    /// or <c>null</c> when the line has no marker. Precedence: ins &gt; del &gt; mark.
    /// </summary>
    private static string? ResolveLineMarkerClass(CodeBlockMeta meta, int lineNumber)
    {
        string? result = null;

        foreach (var marker in meta.LineMarkers)
        {
            foreach (var range in marker.Ranges)
            {
                if (!range.Contains(lineNumber))
                {
                    continue;
                }

                // ins has highest precedence — short-circuit immediately.
                if (marker.Type == LineMarkerType.Ins)
                {
                    return "ec-ins";
                }

                // del overrides mark.
                if (marker.Type == LineMarkerType.Del)
                {
                    result = "ec-del";
                }
                else if (result is null)
                {
                    result = "ec-mark";
                }
            }
        }

        return result;
    }

    private static void WriteHighlightedBlock(
        HtmlRenderer renderer,
        FencedCodeBlock block,
        IGrammar grammar,
        string lang,
        CodeBlockMeta meta,
        bool isDiff)
    {
        var frameType = ResolveFrameType(meta, lang);

        if (frameType == CodeFrameType.None)
        {
            // Frameless: keep backward-compatible wrapper div.
            renderer.Write("<div class=\"code-block-wrapper\"");
            if (meta.Wrap)
            {
                renderer.Write(" data-wrap");
            }

            if (meta.ShowLineNumbers.HasValue)
            {
                renderer.Write(" data-line-numbers");
                if (meta.ShowLineNumbers.Value != 1)
                {
                    renderer.Write(" style=\"counter-reset:ec-line-num ");
                    renderer.Write((meta.ShowLineNumbers.Value - 1).ToString());
                    renderer.Write("\"");
                }
            }

            renderer.Write(">");
        }
        else
        {
            // Frame wrapper.
            renderer.Write("<figure class=\"ec-frame\" data-frame=\"");
            renderer.Write(frameType == CodeFrameType.Terminal ? "terminal" : "code");
            renderer.Write("\"");
            if (meta.Wrap)
            {
                renderer.Write(" data-wrap");
            }

            if (meta.ShowLineNumbers.HasValue)
            {
                renderer.Write(" data-line-numbers");
                if (meta.ShowLineNumbers.Value != 1)
                {
                    renderer.Write(" style=\"counter-reset:ec-line-num ");
                    renderer.Write((meta.ShowLineNumbers.Value - 1).ToString());
                    renderer.Write("\"");
                }
            }

            renderer.Write(">");

            // No header bar — copy button is placed after the <pre> block,
            // positioned absolute and shown on hover over the frame.
        }

        renderer.Write("<pre class=\"highlight\"><code class=\"language-");
        renderer.Write(lang.ToLowerInvariant());
        renderer.Write("\">");

        var lines = CollectLines(block);

        // Pre-sort collapse ranges for efficient lookup.
        var sortedCollapseRanges = meta.CollapseRanges.Count > 0
            ? meta.CollapseRanges.OrderBy(static r => r.Start).ToList()
            : null;

        var insideCollapse = false;

        IStateStack ruleStack = StateStack.NULL;
        for (var i = 0; i < lines.Count; i++)
        {
            var lineNumber = i + 1; // 1-based

            // ── Collapse section handling ──────────────────────────────────────

            if (sortedCollapseRanges is not null)
            {
                // Close an active collapse section when we've passed its end.
                if (insideCollapse)
                {
                    // Find the active range containing the previous line.
                    var prevLine = lineNumber - 1;
                    var wasInRange = sortedCollapseRanges.Any(r => r.Contains(prevLine));
                    var isInRange = sortedCollapseRanges.Any(r => r.Contains(lineNumber));
                    if (!isInRange && wasInRange)
                    {
                        renderer.Write("</div></details>");
                        insideCollapse = false;
                        renderer.WriteLine();
                    }
                }

                // Open a collapse section when we enter a new range.
                if (!insideCollapse && sortedCollapseRanges.Any(r => r.Contains(lineNumber)))
                {
                    var range = sortedCollapseRanges.First(r => r.Contains(lineNumber));
                    var lineCount = range.End - range.Start + 1;
                    var label = lineCount == 1 ? "1 collapsed line" : $"{lineCount} collapsed lines";
                    renderer.Write("<details class=\"ec-collapse-group\"><summary class=\"ec-collapse-summary\">");
                    renderer.Write(label);
                    renderer.Write("</summary><div class=\"ec-collapse-content\">");
                    insideCollapse = true;
                }
            }

            var line = lines[i];

            // Diff mode: detect prefix and strip it before syntax highlighting.
            string? diffMarkerClass = null;
            string tokenizeLine = line;
            if (isDiff && line.Length > 0)
            {
                switch (line[0])
                {
                    case '+':
                        diffMarkerClass = "ec-ins";
                        tokenizeLine = line[1..];
                        break;
                    case '-':
                        diffMarkerClass = "ec-del";
                        tokenizeLine = line[1..];
                        break;
                    default:
                        // Space or other — neutral, strip leading space if present.
                        tokenizeLine = line.Length > 0 && line[0] == ' ' ? line[1..] : line;
                        break;
                }
            }

            ITokenizeLineResult result;
            lock (GrammarLoadLock)
            {
                result = grammar.TokenizeLine(
                    new LineText(tokenizeLine),
                    ruleStack,
                    TimeSpan.FromSeconds(5));
            }

            ruleStack = result.RuleStack;

            // Diff marker class takes precedence over explicit line markers (ins > del > mark).
            var markerClass = diffMarkerClass ?? ResolveLineMarkerClass(meta, lineNumber);
            if (markerClass is null)
            {
                renderer.Write("<div class=\"ec-line\">");
            }
            else
            {
                renderer.Write("<div class=\"ec-line ");
                renderer.Write(markerClass);
                renderer.Write("\">");
            }

            // Word-wrap indent: measure leading whitespace and set CSS variable.
            if (meta.Wrap && meta.PreserveIndent)
            {
                var indentChars = CountLeadingWhitespace(tokenizeLine);
                renderer.Write("<div class=\"ec-line-content\" style=\"--ec-indent:");
                renderer.Write(indentChars.ToString());
                renderer.Write("ch\">");
            }
            else
            {
                renderer.Write("<div class=\"ec-line-content\">");
            }

            var fragments = BuildTokenFragments(tokenizeLine, result.Tokens);
            if (meta.InlineMarkers.Count > 0)
            {
                renderer.Write(InlineMarkerApplier.Apply(tokenizeLine, fragments, meta.InlineMarkers));
            }
            else
            {
                WriteFragments(renderer, fragments);
            }

            renderer.Write("</div></div>");

            if (i < lines.Count - 1)
            {
                renderer.WriteLine();
            }
        }

        // Close any trailing collapse section.
        if (insideCollapse)
        {
            renderer.Write("</div></details>");
        }

        renderer.Write("</code></pre>");
        renderer.Write(CopyButtonHtml);

        if (frameType == CodeFrameType.None)
        {
            renderer.Write("</div>");
        }
        else
        {
            renderer.Write("</figure>");
        }

        renderer.WriteLine();
    }

    private static List<TokenFragment> BuildTokenFragments(
        string line,
        IEnumerable<IToken> tokens)
    {
        var fragments = new List<TokenFragment>();

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
            fragments.Add(new TokenFragment(start, end, text, cssClass));
        }

        return fragments;
    }

    private static void WriteFragments(HtmlRenderer renderer, List<TokenFragment> fragments)
    {
        foreach (var fragment in fragments)
        {
            if (fragment.CssClass is null)
            {
                WriteEscaped(renderer, fragment.Text);
            }
            else
            {
                renderer.Write("<span class=\"");
                renderer.Write(fragment.CssClass);
                renderer.Write("\">");
                WriteEscaped(renderer, fragment.Text);
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

    /// <summary>
    /// Counts the number of leading space/tab characters on a line for use as
    /// the <c>--ec-indent</c> CSS variable when word-wrap + preserveIndent is active.
    /// Tabs are counted as one character each (the CSS <c>ch</c> unit approximation).
    /// </summary>
    private static int CountLeadingWhitespace(string line)
    {
        var count = 0;
        foreach (var ch in line)
        {
            if (ch == ' ' || ch == '\t')
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }
}
