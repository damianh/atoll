using System.Text;
using System.Text.RegularExpressions;

namespace Atoll.Core.Css;

/// <summary>
/// Applies hash-based CSS class scoping using the <c>:where(.atoll-HASH)</c> strategy.
/// Each CSS rule's selectors are wrapped so they only match elements within a component
/// that has the corresponding scope class.
/// </summary>
/// <remarks>
/// <para>
/// The scoping strategy uses <c>:where()</c> to avoid increasing specificity, which
/// mirrors Astro's approach. For example, <c>.card { color: blue; }</c> becomes
/// <c>:where(.atoll-a1b2c3d4) .card { color: blue; }</c>.
/// </para>
/// <para>
/// Special selectors are handled:
/// <list type="bullet">
///   <item><c>:root</c>, <c>html</c>, <c>body</c> — not scoped (they're global by nature)</item>
///   <item><c>@keyframes</c>, <c>@font-face</c> — not scoped (at-rules that don't target elements)</item>
///   <item><c>@media</c>, <c>@supports</c>, <c>@layer</c> — scoping applied to inner rules</item>
/// </list>
/// </para>
/// </remarks>
public static class StyleScoper
{
    private static readonly HashSet<string> GlobalSelectors = new(StringComparer.OrdinalIgnoreCase)
    {
        ":root",
        "html",
        "body",
    };

    private static readonly HashSet<string> PassThroughAtRules = new(StringComparer.OrdinalIgnoreCase)
    {
        "@keyframes",
        "@font-face",
        "@import",
        "@charset",
        "@namespace",
    };

    private static readonly HashSet<string> NestedAtRules = new(StringComparer.OrdinalIgnoreCase)
    {
        "@media",
        "@supports",
        "@layer",
        "@container",
    };

    /// <summary>
    /// Scopes the specified CSS to the given component type using
    /// <c>:where(.atoll-HASH)</c> wrapping.
    /// </summary>
    /// <param name="css">The CSS text to scope.</param>
    /// <param name="componentType">The component type whose hash determines the scope.</param>
    /// <returns>The scoped CSS text.</returns>
    public static string Scope(string css, Type componentType)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(componentType);

        var scopeHash = ScopeHashGenerator.Generate(componentType);
        return Scope(css, scopeHash);
    }

    /// <summary>
    /// Scopes the specified CSS using the given scope hash string
    /// (e.g., <c>atoll-a1b2c3d4</c>).
    /// </summary>
    /// <param name="css">The CSS text to scope.</param>
    /// <param name="scopeHash">The scope hash (e.g., <c>atoll-a1b2c3d4</c>).</param>
    /// <returns>The scoped CSS text.</returns>
    public static string Scope(string css, string scopeHash)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(scopeHash);

        if (css.Length == 0 || scopeHash.Length == 0)
        {
            return css;
        }

        var scopeSelector = $":where(.{scopeHash})";
        return ScopeInternal(css, scopeSelector);
    }

    /// <summary>
    /// Extracts CSS from a component type's <see cref="StylesAttribute"/> declarations
    /// and returns the scoped CSS. If the component has <see cref="GlobalStyleAttribute"/>,
    /// the CSS is returned unscoped.
    /// </summary>
    /// <param name="componentType">The component type to extract and scope styles for.</param>
    /// <returns>The processed CSS text, or an empty string if the component has no styles.</returns>
    public static string ExtractAndScope(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var stylesAttributes = componentType
            .GetCustomAttributes(typeof(StylesAttribute), true)
            .Cast<StylesAttribute>()
            .ToList();

        if (stylesAttributes.Count == 0)
        {
            return string.Empty;
        }

        var isGlobal = componentType
            .GetCustomAttributes(typeof(GlobalStyleAttribute), true)
            .Length > 0;

        var combinedCss = new StringBuilder();
        foreach (var attr in stylesAttributes)
        {
            if (combinedCss.Length > 0)
            {
                combinedCss.Append('\n');
            }
            combinedCss.Append(attr.Css);
        }

        var css = combinedCss.ToString();
        if (isGlobal)
        {
            return css;
        }

        return Scope(css, componentType);
    }

    /// <summary>
    /// Checks whether the specified component type has any <see cref="StylesAttribute"/> declarations.
    /// </summary>
    /// <param name="componentType">The component type to check.</param>
    /// <returns><c>true</c> if the type has styles; otherwise, <c>false</c>.</returns>
    public static bool HasStyles(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return componentType
            .GetCustomAttributes(typeof(StylesAttribute), true)
            .Length > 0;
    }

    /// <summary>
    /// Checks whether the specified component type uses global (unscoped) styles.
    /// </summary>
    /// <param name="componentType">The component type to check.</param>
    /// <returns><c>true</c> if the type has the <see cref="GlobalStyleAttribute"/>; otherwise, <c>false</c>.</returns>
    public static bool IsGlobal(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        return componentType
            .GetCustomAttributes(typeof(GlobalStyleAttribute), true)
            .Length > 0;
    }

    private static string ScopeInternal(string css, string scopeSelector)
    {
        var result = new StringBuilder(css.Length + 128);
        var position = 0;

        while (position < css.Length)
        {
            SkipWhitespace(css, ref position);
            if (position >= css.Length)
            {
                break;
            }

            if (css[position] == '@')
            {
                ProcessAtRule(css, ref position, scopeSelector, result);
            }
            else if (css[position] == '/' && position + 1 < css.Length && css[position + 1] == '*')
            {
                ProcessComment(css, ref position, result);
            }
            else
            {
                ProcessRule(css, ref position, scopeSelector, result);
            }
        }

        return result.ToString();
    }

    private static void ProcessAtRule(string css, ref int position, string scopeSelector, StringBuilder result)
    {
        var atRuleStart = position;
        var atRuleName = ReadAtRuleName(css, ref position);

        // Check for pass-through at-rules (no scoping needed)
        if (IsPassThroughAtRule(atRuleName))
        {
            // Copy everything up to the closing brace or semicolon
            if (atRuleName.Equals("@keyframes", StringComparison.OrdinalIgnoreCase))
            {
                CopyAtRuleBlock(css, atRuleStart, ref position, result);
            }
            else
            {
                CopyUntilSemicolon(css, atRuleStart, ref position, result);
            }
            return;
        }

        // Check for nested at-rules (scope inner rules)
        if (IsNestedAtRule(atRuleName))
        {
            // Copy the at-rule preamble (e.g., "@media (max-width: 768px)")
            SkipWhitespace(css, ref position);
            var preambleEnd = css.IndexOf('{', position);
            if (preambleEnd < 0)
            {
                // Malformed — copy rest as-is
                result.Append(css, atRuleStart, css.Length - atRuleStart);
                position = css.Length;
                return;
            }

            result.Append(css, atRuleStart, preambleEnd - atRuleStart + 1);
            position = preambleEnd + 1;

            // Process inner rules with scoping
            var depth = 1;
            while (position < css.Length && depth > 0)
            {
                SkipWhitespace(css, ref position);
                if (position >= css.Length)
                {
                    break;
                }

                if (css[position] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        result.Append('}');
                        position++;
                    }
                }
                else if (css[position] == '@')
                {
                    ProcessAtRule(css, ref position, scopeSelector, result);
                }
                else if (css[position] == '/' && position + 1 < css.Length && css[position + 1] == '*')
                {
                    ProcessComment(css, ref position, result);
                }
                else
                {
                    ProcessRule(css, ref position, scopeSelector, result);
                }
            }

            return;
        }

        // Unknown at-rule — copy the block or statement as-is
        if (HasBlock(css, position))
        {
            CopyAtRuleBlock(css, atRuleStart, ref position, result);
        }
        else
        {
            CopyUntilSemicolon(css, atRuleStart, ref position, result);
        }
    }

    private static void ProcessComment(string css, ref int position, StringBuilder result)
    {
        var commentStart = position;
        position += 2; // Skip /*
        var endIndex = css.IndexOf("*/", position, StringComparison.Ordinal);
        if (endIndex < 0)
        {
            result.Append(css, commentStart, css.Length - commentStart);
            position = css.Length;
        }
        else
        {
            result.Append(css, commentStart, endIndex + 2 - commentStart);
            position = endIndex + 2;
        }
    }

    private static void ProcessRule(string css, ref int position, string scopeSelector, StringBuilder result)
    {
        // Read selectors until we hit '{'
        var selectorStart = position;
        var braceIndex = FindUnquotedChar(css, '{', position);
        if (braceIndex < 0)
        {
            // Malformed — copy rest as-is
            result.Append(css, position, css.Length - position);
            position = css.Length;
            return;
        }

        var selectorText = css.Substring(selectorStart, braceIndex - selectorStart).Trim();
        position = braceIndex + 1;

        // Find matching closing brace
        var bodyStart = position;
        var depth = 1;
        while (position < css.Length && depth > 0)
        {
            if (css[position] == '{')
            {
                depth++;
            }
            else if (css[position] == '}')
            {
                depth--;
            }
            else if (css[position] == '\'' || css[position] == '"')
            {
                SkipString(css, ref position);
                continue;
            }

            position++;
        }

        var bodyEnd = position - 1; // points to '}'
        var body = css.Substring(bodyStart, bodyEnd - bodyStart);

        // Scope the selectors
        var scopedSelectors = ScopeSelectors(selectorText, scopeSelector);
        result.Append(scopedSelectors);
        result.Append('{');
        result.Append(body);
        result.Append('}');
    }

    private static string ScopeSelectors(string selectorText, string scopeSelector)
    {
        var selectors = SplitSelectors(selectorText);
        var result = new StringBuilder();

        for (var i = 0; i < selectors.Count; i++)
        {
            if (i > 0)
            {
                result.Append(',');
            }

            var selector = selectors[i].Trim();
            if (IsGlobalSelector(selector))
            {
                result.Append(selector);
            }
            else
            {
                result.Append(scopeSelector);
                result.Append(' ');
                result.Append(selector);
            }
        }

        return result.ToString();
    }

    private static List<string> SplitSelectors(string selectorText)
    {
        var selectors = new List<string>();
        var current = new StringBuilder();
        var parenDepth = 0;

        foreach (var c in selectorText)
        {
            if (c == '(')
            {
                parenDepth++;
                current.Append(c);
            }
            else if (c == ')')
            {
                parenDepth--;
                current.Append(c);
            }
            else if (c == ',' && parenDepth == 0)
            {
                selectors.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            selectors.Add(current.ToString());
        }

        return selectors;
    }

    private static bool IsGlobalSelector(string selector)
    {
        // Check if the base part of the selector is a global one
        foreach (var global in GlobalSelectors)
        {
            if (selector.Equals(global, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Also match when the global selector is followed by combinators/pseudo-classes
            // e.g., "html > body", ":root::before"
            if (selector.StartsWith(global, StringComparison.OrdinalIgnoreCase)
                && selector.Length > global.Length)
            {
                var nextChar = selector[global.Length];
                if (nextChar == ' ' || nextChar == '>' || nextChar == '+' || nextChar == '~'
                    || nextChar == ':' || nextChar == '[' || nextChar == '.')
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string ReadAtRuleName(string css, ref int position)
    {
        var start = position;
        position++; // skip '@'
        while (position < css.Length && !char.IsWhiteSpace(css[position])
               && css[position] != '{' && css[position] != '(' && css[position] != ';')
        {
            position++;
        }

        return css[start..position];
    }

    private static bool IsPassThroughAtRule(string atRuleName)
    {
        foreach (var rule in PassThroughAtRules)
        {
            if (atRuleName.Equals(rule, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNestedAtRule(string atRuleName)
    {
        foreach (var rule in NestedAtRules)
        {
            if (atRuleName.Equals(rule, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasBlock(string css, int position)
    {
        while (position < css.Length)
        {
            if (css[position] == '{')
            {
                return true;
            }
            if (css[position] == ';')
            {
                return false;
            }
            position++;
        }

        return false;
    }

    private static void CopyAtRuleBlock(string css, int start, ref int position, StringBuilder result)
    {
        var braceIndex = css.IndexOf('{', position);
        if (braceIndex < 0)
        {
            result.Append(css, start, css.Length - start);
            position = css.Length;
            return;
        }

        position = braceIndex + 1;
        var depth = 1;
        while (position < css.Length && depth > 0)
        {
            if (css[position] == '{')
            {
                depth++;
            }
            else if (css[position] == '}')
            {
                depth--;
            }

            position++;
        }

        result.Append(css, start, position - start);
    }

    private static void CopyUntilSemicolon(string css, int start, ref int position, StringBuilder result)
    {
        var semiIndex = css.IndexOf(';', position);
        if (semiIndex < 0)
        {
            result.Append(css, start, css.Length - start);
            position = css.Length;
        }
        else
        {
            result.Append(css, start, semiIndex + 1 - start);
            position = semiIndex + 1;
        }
    }

    private static int FindUnquotedChar(string css, char target, int position)
    {
        while (position < css.Length)
        {
            if (css[position] == target)
            {
                return position;
            }

            if (css[position] == '\'' || css[position] == '"')
            {
                SkipString(css, ref position);
                continue;
            }

            if (css[position] == '/' && position + 1 < css.Length && css[position + 1] == '*')
            {
                var end = css.IndexOf("*/", position + 2, StringComparison.Ordinal);
                position = end < 0 ? css.Length : end + 2;
                continue;
            }

            position++;
        }

        return -1;
    }

    private static void SkipString(string css, ref int position)
    {
        var quote = css[position];
        position++;
        while (position < css.Length)
        {
            if (css[position] == '\\')
            {
                position += 2;
                continue;
            }

            if (css[position] == quote)
            {
                position++;
                return;
            }

            position++;
        }
    }

    private static void SkipWhitespace(string css, ref int position)
    {
        while (position < css.Length && char.IsWhiteSpace(css[position]))
        {
            position++;
        }
    }
}
