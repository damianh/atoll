using NUglify;

namespace Atoll.Css;

/// <summary>
/// Minifies CSS using NUglify. Provides a thin wrapper over the NUglify CSS
/// minification engine with sensible defaults for production output.
/// </summary>
public static class CssMinifier
{
    /// <summary>
    /// Minifies the specified CSS text.
    /// </summary>
    /// <param name="css">The CSS text to minify.</param>
    /// <returns>The minified CSS text. If minification fails, the original CSS is returned.</returns>
    public static string Minify(string css)
    {
        ArgumentNullException.ThrowIfNull(css);

        if (css.Length == 0)
        {
            return string.Empty;
        }

        var result = Uglify.Css(css);
        if (result.HasErrors)
        {
            // If minification fails, return the original CSS rather than breaking the build.
            return css;
        }

        return result.Code;
    }

    /// <summary>
    /// Minifies the specified CSS text and returns a result that includes
    /// any errors or warnings from the minification process.
    /// </summary>
    /// <param name="css">The CSS text to minify.</param>
    /// <returns>A <see cref="CssMinifyResult"/> containing the output and any diagnostics.</returns>
    public static CssMinifyResult MinifyWithDiagnostics(string css)
    {
        ArgumentNullException.ThrowIfNull(css);

        if (css.Length == 0)
        {
            return new CssMinifyResult(string.Empty, true, []);
        }

        var result = Uglify.Css(css);

        var diagnostics = new List<string>();
        foreach (var error in result.Errors)
        {
            diagnostics.Add(error.ToString());
        }

        return new CssMinifyResult(
            result.HasErrors ? css : result.Code,
            !result.HasErrors,
            diagnostics);
    }
}

/// <summary>
/// Represents the result of a CSS minification operation.
/// </summary>
/// <param name="Css">The output CSS (minified if successful, original if failed).</param>
/// <param name="Success">Whether minification succeeded without errors.</param>
/// <param name="Diagnostics">Any error or warning messages from the minifier.</param>
public sealed record CssMinifyResult(string Css, bool Success, IReadOnlyList<string> Diagnostics);
