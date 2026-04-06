using System.Text.RegularExpressions;

namespace Atoll.Lagoon.Markdown;

/// <summary>
/// Parses the info string and arguments of a Markdig <c>FencedCodeBlock</c> into a
/// <see cref="CodeBlockMeta"/> instance.
/// </summary>
/// <remarks>
/// The grammar supported (informally):
/// <code>
/// info-string  := language [SP arguments]
/// arguments    := attribute*
/// attribute    :=
///     | 'title=' QUOTED-STRING
///     | 'frame=' ('code' | 'terminal' | 'none' | 'auto')
///     | 'lang='  QUOTED-STRING
///     | 'wrap'
///     | 'preserveIndent' ['=' ('true'|'false')]
///     | 'showLineNumbers'
///     | 'startLineNumber=' NUMBER
///     | '{' range-list '}'                  -- mark ranges
///     | ('ins'|'del'|'mark'|'collapse') '=' '{' range-list '}'
///     | QUOTED-STRING                       -- literal inline marker
///     | '/' REGEX '/'                       -- regex inline marker
/// range-list   := range (',' range)*
/// range        := NUMBER ['-' NUMBER]
/// QUOTED-STRING := '"' [^"]* '"'
/// </code>
/// Unknown attributes are silently ignored.
/// </remarks>
internal static class CodeBlockMetaParser
{
    // Languages that auto-detect as terminal frames.
    private static readonly HashSet<string> TerminalLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "bash", "sh", "shell", "zsh", "fish", "powershell", "pwsh", "ps1",
        "cmd", "bat", "terminal", "console",
    };

    /// <summary>
    /// Parses a fenced code block's info string and arguments into a <see cref="CodeBlockMeta"/>.
    /// </summary>
    /// <param name="info">
    /// The language identifier (Markdig's <c>FencedCodeBlock.Info</c> — first token of the
    /// opening fence line).
    /// </param>
    /// <param name="arguments">
    /// Everything after the language identifier on the opening fence line
    /// (Markdig's <c>FencedCodeBlock.Arguments</c>).
    /// </param>
    internal static CodeBlockMeta Parse(string? info, string? arguments)
    {
        var language = string.IsNullOrWhiteSpace(info) ? null : info.Trim();

        if (string.IsNullOrWhiteSpace(arguments))
        {
            return CodeBlockMeta.ForLanguage(language);
        }

        return ParseArguments(language, arguments.Trim());
    }

    private static CodeBlockMeta ParseArguments(string? language, string args)
    {
        string? title = null;
        var frame = CodeFrameType.Auto;
        string? diffLang = null;
        var wrap = false;
        var preserveIndent = true;
        int? showLineNumbers = null;

        var markRanges = new List<LineRange>();
        var insRanges = new List<LineRange>();
        var delRanges = new List<LineRange>();
        var collapseRanges = new List<LineRange>();
        var inlineMarkers = new List<InlineMarker>();

        var pos = 0;

        while (pos < args.Length)
        {
            SkipWhitespace(args, ref pos);
            if (pos >= args.Length)
            {
                break;
            }

            var ch = args[pos];

            // Quoted string → literal inline marker  "text"
            if (ch == '"')
            {
                var text = ReadQuotedString(args, ref pos);
                if (text is not null)
                {
                    inlineMarkers.Add(new InlineMarker { IsRegex = false, Pattern = text });
                }

                continue;
            }

            // Regex marker  /pattern/  or  /pattern with (group)/
            if (ch == '/')
            {
                var (pattern, hasGroup) = ReadRegexMarker(args, ref pos);
                if (pattern is not null)
                {
                    inlineMarkers.Add(new InlineMarker
                    {
                        IsRegex = true,
                        Pattern = pattern,
                        HasCaptureGroup = hasGroup,
                    });
                }

                continue;
            }

            // Bare brace  {range-list}  → mark ranges
            if (ch == '{')
            {
                var ranges = ReadBracedRanges(args, ref pos);
                markRanges.AddRange(ranges);
                continue;
            }

            // Named attribute (keyword possibly followed by '=' value)
            var keyword = ReadIdentifier(args, ref pos);
            if (keyword.Length == 0)
            {
                // Unrecognised character — skip it
                pos++;
                continue;
            }

            switch (keyword.ToLowerInvariant())
            {
                case "title":
                    title = ReadEqualQuotedString(args, ref pos);
                    break;

                case "frame":
                    var frameStr = ReadEqualQuotedString(args, ref pos)
                                   ?? ReadEqualBareValue(args, ref pos);
                    frame = frameStr?.ToLowerInvariant() switch
                    {
                        "code" => CodeFrameType.Code,
                        "terminal" => CodeFrameType.Terminal,
                        "none" => CodeFrameType.None,
                        _ => CodeFrameType.Auto,
                    };
                    break;

                case "lang":
                    diffLang = ReadEqualQuotedString(args, ref pos)
                               ?? ReadEqualBareValue(args, ref pos);
                    break;

                case "wrap":
                    wrap = true;
                    // Allow optional  =true / =false
                    TryReadBooleanFlag(args, ref pos, ref wrap);
                    break;

                case "preserveindent":
                    preserveIndent = true;
                    TryReadBooleanFlag(args, ref pos, ref preserveIndent);
                    break;

                case "showlinenumbers":
                    showLineNumbers = 1; // default start
                    break;

                case "startlinenumber":
                {
                    if (TryReadEqualInt(args, ref pos, out var start))
                    {
                        showLineNumbers = start;
                    }

                    break;
                }

                case "ins":
                {
                    var ranges = ReadEqualBracedRanges(args, ref pos);
                    insRanges.AddRange(ranges);
                    break;
                }

                case "del":
                {
                    var ranges = ReadEqualBracedRanges(args, ref pos);
                    delRanges.AddRange(ranges);
                    break;
                }

                case "mark":
                {
                    var ranges = ReadEqualBracedRanges(args, ref pos);
                    markRanges.AddRange(ranges);
                    break;
                }

                case "collapse":
                {
                    var ranges = ReadEqualBracedRanges(args, ref pos);
                    collapseRanges.AddRange(ranges);
                    break;
                }

                // Unknown — ignore
            }
        }

        // Build the line markers list
        var lineMarkers = new List<LineMarker>();
        if (markRanges.Count > 0)
        {
            lineMarkers.Add(new LineMarker(LineMarkerType.Mark, markRanges));
        }

        if (insRanges.Count > 0)
        {
            lineMarkers.Add(new LineMarker(LineMarkerType.Ins, insRanges));
        }

        if (delRanges.Count > 0)
        {
            lineMarkers.Add(new LineMarker(LineMarkerType.Del, delRanges));
        }

        return new CodeBlockMeta
        {
            Language = language,
            Title = title,
            Frame = frame,
            DiffLang = diffLang,
            Wrap = wrap,
            PreserveIndent = preserveIndent,
            ShowLineNumbers = showLineNumbers,
            LineMarkers = lineMarkers,
            InlineMarkers = inlineMarkers,
            CollapseRanges = collapseRanges,
        };
    }

    // -------------------------------------------------------------------------
    // Helper: determines the auto-detected frame type for a language
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the resolved <see cref="CodeFrameType"/> for the given metadata, applying
    /// auto-detection rules when <see cref="CodeBlockMeta.Frame"/> is
    /// <see cref="CodeFrameType.Auto"/>.
    /// </summary>
    internal static CodeFrameType ResolveFrameType(CodeBlockMeta meta)
    {
        if (meta.Frame != CodeFrameType.Auto)
        {
            return meta.Frame;
        }

        // Explicit title → editor (code) frame
        if (meta.Title is not null)
        {
            return CodeFrameType.Code;
        }

        // Terminal languages auto-detect as terminal frame
        if (meta.Language is not null && TerminalLanguages.Contains(meta.Language))
        {
            return CodeFrameType.Terminal;
        }

        // Default to code frame
        return CodeFrameType.Code;
    }

    // -------------------------------------------------------------------------
    // Low-level tokenizer helpers
    // -------------------------------------------------------------------------

    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos]))
        {
            pos++;
        }
    }

    private static string ReadIdentifier(string s, ref int pos)
    {
        var start = pos;
        while (pos < s.Length && (char.IsLetterOrDigit(s[pos]) || s[pos] == '-' || s[pos] == '_'))
        {
            pos++;
        }

        return s[start..pos];
    }

    /// <summary>Reads a quoted string starting at the current position.  The leading <c>"</c>
    /// must be present at <paramref name="pos"/>.</summary>
    private static string? ReadQuotedString(string s, ref int pos)
    {
        if (pos >= s.Length || s[pos] != '"')
        {
            return null;
        }

        pos++; // consume opening quote
        var start = pos;
        while (pos < s.Length && s[pos] != '"')
        {
            if (s[pos] == '\\' && pos + 1 < s.Length)
            {
                pos++; // skip escape char
            }

            pos++;
        }

        var value = s[start..pos];
        if (pos < s.Length)
        {
            pos++; // consume closing quote
        }

        return value;
    }

    /// <summary>Reads a regex marker of the form <c>/pattern/</c>.  Returns the pattern and
    /// whether it contains a capture group.</summary>
    private static (string? Pattern, bool HasGroup) ReadRegexMarker(string s, ref int pos)
    {
        if (pos >= s.Length || s[pos] != '/')
        {
            return (null, false);
        }

        pos++; // consume opening slash
        var sb = new System.Text.StringBuilder();
        while (pos < s.Length && s[pos] != '/')
        {
            if (s[pos] == '\\' && pos + 1 < s.Length)
            {
                sb.Append(s[pos]);
                pos++;
            }

            sb.Append(s[pos]);
            pos++;
        }

        if (pos < s.Length)
        {
            pos++; // consume closing slash
        }

        var pattern = sb.ToString();
        if (pattern.Length == 0)
        {
            return (null, false);
        }

        // Detect explicit capturing group (non-escaped '(' not followed by '?')
        var hasGroup = HasCapturingGroup(pattern);
        return (pattern, hasGroup);
    }

    private static bool HasCapturingGroup(string pattern)
    {
        for (var i = 0; i < pattern.Length; i++)
        {
            if (pattern[i] == '\\')
            {
                i++;
                continue;
            }

            if (pattern[i] == '(' && (i + 1 >= pattern.Length || pattern[i + 1] != '?'))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads <c>={range-list}</c> (or <c>= {range-list}</c>) at the current position.</summary>
    private static IReadOnlyList<LineRange> ReadEqualBracedRanges(string s, ref int pos)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '=')
        {
            return [];
        }

        pos++; // consume '='
        SkipWhitespace(s, ref pos);
        return ReadBracedRanges(s, ref pos);
    }

    /// <summary>Reads <c>{range-list}</c> at the current position.</summary>
    private static IReadOnlyList<LineRange> ReadBracedRanges(string s, ref int pos)
    {
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '{')
        {
            return [];
        }

        pos++; // consume '{'
        var ranges = new List<LineRange>();

        while (pos < s.Length && s[pos] != '}')
        {
            SkipWhitespace(s, ref pos);
            if (pos >= s.Length || s[pos] == '}')
            {
                break;
            }

            if (!TryReadInt(s, ref pos, out var start))
            {
                // Skip unexpected character
                pos++;
                continue;
            }

            SkipWhitespace(s, ref pos);

            var end = start;
            if (pos < s.Length && s[pos] == '-')
            {
                pos++; // consume '-'
                if (TryReadInt(s, ref pos, out var rangeEnd))
                {
                    end = rangeEnd;
                }
            }

            if (start > 0 && end >= start)
            {
                ranges.Add(new LineRange(start, end));
            }

            SkipWhitespace(s, ref pos);
            if (pos < s.Length && s[pos] == ',')
            {
                pos++; // consume ','
            }
        }

        if (pos < s.Length && s[pos] == '}')
        {
            pos++; // consume '}'
        }

        return ranges;
    }

    /// <summary>Reads <c>="quoted"</c> after a keyword, returning the unquoted value.</summary>
    private static string? ReadEqualQuotedString(string s, ref int pos)
    {
        var saved = pos;
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '=')
        {
            pos = saved;
            return null;
        }

        pos++; // consume '='
        SkipWhitespace(s, ref pos);

        if (pos >= s.Length || s[pos] != '"')
        {
            pos = saved;
            return null;
        }

        return ReadQuotedString(s, ref pos);
    }

    /// <summary>Reads <c>=bareword</c> (no quotes) after a keyword.</summary>
    private static string? ReadEqualBareValue(string s, ref int pos)
    {
        var saved = pos;
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '=')
        {
            pos = saved;
            return null;
        }

        pos++; // consume '='
        SkipWhitespace(s, ref pos);

        var value = ReadIdentifier(s, ref pos);
        if (value.Length == 0)
        {
            pos = saved;
            return null;
        }

        return value;
    }

    /// <summary>
    /// Optionally reads <c>=true</c> or <c>=false</c> after a boolean flag keyword, updating
    /// <paramref name="flag"/> accordingly.  If no <c>=value</c> is present the flag is
    /// left unchanged.
    /// </summary>
    private static void TryReadBooleanFlag(string s, ref int pos, ref bool flag)
    {
        var saved = pos;
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '=')
        {
            pos = saved;
            return;
        }

        pos++; // consume '='
        SkipWhitespace(s, ref pos);

        var value = ReadIdentifier(s, ref pos);
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            flag = true;
        }
        else if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            flag = false;
        }
        else
        {
            // Unknown value — restore
            pos = saved;
        }
    }

    private static bool TryReadEqualInt(string s, ref int pos, out int value)
    {
        value = 0;
        var saved = pos;
        SkipWhitespace(s, ref pos);
        if (pos >= s.Length || s[pos] != '=')
        {
            pos = saved;
            return false;
        }

        pos++; // consume '='
        SkipWhitespace(s, ref pos);

        if (!TryReadInt(s, ref pos, out value))
        {
            pos = saved;
            return false;
        }

        return true;
    }

    private static bool TryReadInt(string s, ref int pos, out int value)
    {
        value = 0;
        var start = pos;
        while (pos < s.Length && char.IsAsciiDigit(s[pos]))
        {
            pos++;
        }

        if (pos == start)
        {
            return false;
        }

        return int.TryParse(s[start..pos], out value);
    }
}
