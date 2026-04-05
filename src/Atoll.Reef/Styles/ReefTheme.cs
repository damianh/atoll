using Atoll.Components;
using Atoll.Css;

namespace Atoll.Reef.Styles;

/// <summary>
/// Provides the full CSS theme for <c>Atoll.Reef</c>. Apply this component once
/// in the article layout (or a shared head component) to inject all design tokens
/// and structural styles for the articles/blog site.
/// </summary>
/// <remarks>
/// Marked with <see cref="GlobalStyleAttribute"/> so the CSS is emitted without
/// a scope wrapper — the Reef theme must affect the full page, not just a subtree.
/// Sections: reset, light tokens, dark tokens, layout, typography, prose,
/// article card, list, grid, timeline, table, pagination, tag cloud, author card,
/// article meta, article nav, series, filter, and view toggle styles.
/// </remarks>
[GlobalStyle]
[Styles(Reset + LightTokens + DarkTokens + Layout + Typography + Prose +
        ArticleMetaStyles + ArticleCardStyles + ArticleListStyles + ArticleGridStyles +
        ArticleTimelineStyles + ArticleTableStyles + PaginationStyles + TagCloudStyles +
        AuthorCardStyles + ArticleNavStyles + SeriesStyles + FilterStyles + ViewToggleStyles)]
public sealed class ReefTheme : AtollComponent
{
    // -------------------------------------------------------------------------
    // CSS sections (constants so they can be referenced from the attribute)
    // -------------------------------------------------------------------------

    private const string Reset = """
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        html { scroll-behavior: smooth; }
        body {
            font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background: var(--reef-bg);
            color: var(--reef-text);
            line-height: 1.75;
            font-size: 1rem;
        }
        img, svg { display: block; max-width: 100%; }
        a { color: var(--reef-link); text-decoration: none; }
        a:hover { color: var(--reef-link-hover); text-decoration: underline; }
        """;

    private const string LightTokens = """
        :root {
            --reef-bg: #ffffff;
            --reef-bg-raised: #f9fafb;
            --reef-bg-subtle: #f3f4f6;
            --reef-text: #111827;
            --reef-text-muted: #6b7280;
            --reef-text-faint: #9ca3af;
            --reef-primary: #0f3460;
            --reef-primary-hover: #1e5fa8;
            --reef-accent: #e94560;
            --reef-link: #0f3460;
            --reef-link-hover: #e94560;
            --reef-border: #e5e7eb;
            --reef-tag-bg: #eff6ff;
            --reef-tag-text: #0f3460;
            --reef-header-height: 3.5rem;
            --reef-content-width: 65ch;
        }
        """;

    private const string DarkTokens = """
        [data-theme="dark"] {
            --reef-bg: #0d1117;
            --reef-bg-raised: #161b22;
            --reef-bg-subtle: #21262d;
            --reef-text: #e6edf3;
            --reef-text-muted: #8b949e;
            --reef-text-faint: #6e7681;
            --reef-primary: #58a6ff;
            --reef-primary-hover: #79c0ff;
            --reef-accent: #f78166;
            --reef-link: #58a6ff;
            --reef-link-hover: #79c0ff;
            --reef-border: #30363d;
            --reef-tag-bg: #161b22;
            --reef-tag-text: #58a6ff;
        }
        """;

    private const string Layout = """
        .reef-header {
            position: sticky;
            top: 0;
            z-index: 100;
            height: var(--reef-header-height);
            background: var(--reef-bg);
            border-bottom: 1px solid var(--reef-border);
        }
        .reef-header-inner {
            display: flex;
            align-items: center;
            gap: 1rem;
            max-width: 72rem;
            margin: 0 auto;
            padding: 0 1.5rem;
            height: 100%;
        }
        .reef-brand {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-weight: 600;
            font-size: 1.1rem;
            color: var(--reef-text);
            flex: 1;
        }
        .reef-brand:hover { text-decoration: none; }
        .reef-logo { height: 1.75rem; width: auto; }
        .reef-social { display: flex; gap: 0.75rem; }
        .reef-social-link { font-size: 0.875rem; color: var(--reef-text-muted); }
        .reef-main {
            max-width: 72rem;
            margin: 0 auto;
            padding: 2rem 1.5rem;
        }
        .reef-article { max-width: var(--reef-content-width); margin: 0 auto; }
        .reef-footer {
            border-top: 1px solid var(--reef-border);
            padding: 1.5rem;
            text-align: center;
            font-size: 0.875rem;
            color: var(--reef-text-muted);
        }
        .article-listing { max-width: 72rem; }
        """;

    private const string Typography = """
        h1, h2, h3, h4, h5, h6 {
            line-height: 1.25;
            font-weight: 600;
            color: var(--reef-text);
        }
        h1 { font-size: 2rem; margin-bottom: 1rem; }
        h2 { font-size: 1.5rem; margin-top: 2rem; margin-bottom: 0.75rem; }
        h3 { font-size: 1.25rem; margin-top: 1.5rem; margin-bottom: 0.5rem; }
        p { margin-bottom: 1rem; }
        """;

    private const string Prose = """
        .prose { color: var(--reef-text); }
        .prose a { color: var(--reef-link); }
        .prose a:hover { color: var(--reef-link-hover); }
        .prose code {
            background: var(--reef-bg-subtle);
            border-radius: 0.25rem;
            padding: 0.1em 0.35em;
            font-size: 0.875em;
        }
        .prose pre {
            background: var(--reef-bg-raised);
            border: 1px solid var(--reef-border);
            border-radius: 0.5rem;
            padding: 1rem 1.25rem;
            overflow-x: auto;
            margin-bottom: 1.25rem;
        }
        .prose pre code { background: none; padding: 0; font-size: 0.875rem; }
        .prose ul, .prose ol { padding-left: 1.5rem; margin-bottom: 1rem; }
        .prose li { margin-bottom: 0.25rem; }
        .prose blockquote {
            border-left: 4px solid var(--reef-accent);
            padding-left: 1rem;
            color: var(--reef-text-muted);
            margin-bottom: 1rem;
        }
        .prose hr { border: none; border-top: 1px solid var(--reef-border); margin: 2rem 0; }
        .prose img { border-radius: 0.5rem; margin: 1.5rem 0; }
        """;

    private const string ArticleMetaStyles = """
        .article-meta {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 0.75rem;
            font-size: 0.875rem;
            color: var(--reef-text-muted);
            margin-bottom: 1.25rem;
        }
        .article-date, .article-author, .article-reading-time { display: inline; }
        .article-tags {
            display: flex;
            flex-wrap: wrap;
            gap: 0.375rem;
            list-style: none;
            padding: 0;
            margin: 0;
        }
        .tag-pill {
            display: inline-block;
            padding: 0.125rem 0.5rem;
            background: var(--reef-tag-bg);
            color: var(--reef-tag-text);
            border-radius: 9999px;
            font-size: 0.75rem;
            font-weight: 500;
        }
        .tag-pill:hover { text-decoration: none; opacity: 0.85; }
        """;

    private const string ArticleCardStyles = """
        .article-card {
            background: var(--reef-bg-raised);
            border: 1px solid var(--reef-border);
            border-radius: 0.75rem;
            overflow: hidden;
            display: flex;
            flex-direction: column;
            transition: box-shadow 0.15s;
        }
        .article-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
        .article-card-image-link { display: block; }
        .article-card-image { width: 100%; aspect-ratio: 16/9; object-fit: cover; }
        .article-card-body { padding: 1.25rem; flex: 1; }
        .article-card-title { font-size: 1.125rem; margin: 0 0 0.5rem; }
        .article-card-title a { color: var(--reef-text); }
        .article-card-title a:hover { color: var(--reef-link-hover); text-decoration: none; }
        .article-card-description { font-size: 0.9rem; color: var(--reef-text-muted); margin-bottom: 0.75rem; }
        """;

    private const string ArticleListStyles = """
        .article-list { display: flex; flex-direction: column; gap: 0; }
        .article-list-item {
            padding: 1rem 0;
            border-bottom: 1px solid var(--reef-border);
        }
        .article-list-item:last-child { border-bottom: none; }
        .article-list-item-header {
            display: flex;
            align-items: baseline;
            gap: 1rem;
            flex-wrap: wrap;
            margin-bottom: 0.25rem;
        }
        .article-list-item-title { font-size: 1.05rem; font-weight: 600; }
        .article-list-item-title a { color: var(--reef-text); }
        .article-list-item-title a:hover { color: var(--reef-link-hover); text-decoration: none; }
        .article-list-item-date { font-size: 0.8rem; color: var(--reef-text-muted); white-space: nowrap; }
        .article-list-item-description { font-size: 0.9rem; color: var(--reef-text-muted); }
        """;

    private const string ArticleGridStyles = """
        .article-grid {
            display: grid;
            grid-template-columns: repeat(var(--grid-cols, 3), 1fr);
            gap: 1.5rem;
        }
        @media (max-width: 900px) { .article-grid { grid-template-columns: repeat(2, 1fr); } }
        @media (max-width: 600px) { .article-grid { grid-template-columns: 1fr; } }
        """;

    private const string ArticleTimelineStyles = """
        .article-timeline { display: flex; flex-direction: column; gap: 2rem; }
        .timeline-year { }
        .timeline-year h2 {
            font-size: 1.5rem;
            color: var(--reef-text-muted);
            margin-bottom: 0.75rem;
            padding-bottom: 0.5rem;
            border-bottom: 1px solid var(--reef-border);
        }
        .timeline-entries { list-style: none; padding: 0; display: flex; flex-direction: column; gap: 0.5rem; }
        .timeline-entry {
            display: flex;
            align-items: baseline;
            gap: 1rem;
            padding: 0.5rem 0;
        }
        .timeline-entry__date { font-size: 0.8rem; color: var(--reef-text-muted); white-space: nowrap; min-width: 5rem; }
        .timeline-entry__title a { color: var(--reef-text); font-weight: 500; }
        .timeline-entry__title a:hover { color: var(--reef-link-hover); }
        """;

    private const string ArticleTableStyles = """
        .article-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
        .article-table th {
            text-align: left;
            padding: 0.5rem 0.75rem;
            border-bottom: 2px solid var(--reef-border);
            color: var(--reef-text-muted);
            font-weight: 600;
        }
        .article-table td {
            padding: 0.625rem 0.75rem;
            border-bottom: 1px solid var(--reef-border);
            vertical-align: top;
        }
        .article-table tr:last-child td { border-bottom: none; }
        .article-table a { color: var(--reef-link); }
        """;

    private const string PaginationStyles = """
        .pagination {
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.25rem;
            padding: 2rem 0;
            flex-wrap: wrap;
        }
        .pagination-link {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 2rem;
            height: 2rem;
            padding: 0 0.5rem;
            border: 1px solid var(--reef-border);
            border-radius: 0.375rem;
            font-size: 0.875rem;
            color: var(--reef-text);
        }
        .pagination-link:hover { background: var(--reef-bg-subtle); text-decoration: none; }
        .pagination-link[aria-current="page"] {
            background: var(--reef-primary);
            color: #fff;
            border-color: var(--reef-primary);
        }
        .pagination-ellipsis { padding: 0 0.25rem; color: var(--reef-text-muted); }
        """;

    private const string TagCloudStyles = """
        .tag-cloud {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            padding: 1rem 0;
        }
        .tag-count { font-size: 0.75em; color: var(--reef-text-muted); margin-left: 0.2em; }
        """;

    private const string AuthorCardStyles = """
        .author-card {
            display: flex;
            gap: 1rem;
            align-items: flex-start;
            background: var(--reef-bg-raised);
            border: 1px solid var(--reef-border);
            border-radius: 0.75rem;
            padding: 1.25rem;
            margin: 2rem 0;
        }
        .author-card__avatar {
            width: 3.5rem;
            height: 3.5rem;
            border-radius: 50%;
            object-fit: cover;
            flex-shrink: 0;
        }
        .author-card__info { flex: 1; }
        .author-card__name { font-weight: 600; margin-bottom: 0.25rem; }
        .author-card__bio { font-size: 0.9rem; color: var(--reef-text-muted); }
        """;

    private const string ArticleNavStyles = """
        .article-nav {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 1rem;
            padding: 2rem 0;
            border-top: 1px solid var(--reef-border);
        }
        .article-nav-link {
            display: flex;
            flex-direction: column;
            gap: 0.25rem;
            padding: 0.75rem 1rem;
            border: 1px solid var(--reef-border);
            border-radius: 0.5rem;
        }
        .article-nav-link:hover { background: var(--reef-bg-subtle); text-decoration: none; }
        .article-nav-label { font-size: 0.75rem; color: var(--reef-text-muted); text-transform: uppercase; letter-spacing: 0.05em; }
        .article-nav-title { font-weight: 500; color: var(--reef-text); }
        .article-nav-next { text-align: right; grid-column: 2; }
        .article-nav-prev { grid-column: 1; }
        """;

    private const string SeriesStyles = """
        .article-series {
            background: var(--reef-bg-raised);
            border: 1px solid var(--reef-border);
            border-left: 4px solid var(--reef-primary);
            border-radius: 0.5rem;
            padding: 1rem 1.25rem;
            margin: 1.5rem 0;
        }
        .series-header { font-weight: 600; margin-bottom: 0.75rem; }
        .series-parts { padding-left: 1.25rem; }
        .series-parts li { padding: 0.2rem 0; }
        .series-part--current { font-weight: 600; color: var(--reef-primary); }
        """;

    private const string FilterStyles = """
        .article-filter {
            display: flex;
            flex-wrap: wrap;
            gap: 0.5rem;
            align-items: center;
            padding: 1rem 0;
        }
        .filter-label { font-size: 0.875rem; color: var(--reef-text-muted); margin-right: 0.25rem; }
        .filter-pill {
            cursor: pointer;
            padding: 0.25rem 0.75rem;
            border: 1px solid var(--reef-border);
            border-radius: 9999px;
            font-size: 0.8rem;
            background: none;
            color: var(--reef-text);
        }
        .filter-pill:hover, .filter-pill[aria-pressed="true"] {
            background: var(--reef-primary);
            color: #fff;
            border-color: var(--reef-primary);
        }
        """;

    private const string ViewToggleStyles = """
        .view-toggle {
            display: flex;
            gap: 0.25rem;
            align-items: center;
        }
        .view-toggle__btn {
            cursor: pointer;
            padding: 0.375rem 0.625rem;
            border: 1px solid var(--reef-border);
            border-radius: 0.375rem;
            background: none;
            color: var(--reef-text-muted);
            font-size: 0.875rem;
        }
        .view-toggle__btn:hover { background: var(--reef-bg-subtle); }
        .view-toggle__btn[aria-pressed="true"] {
            background: var(--reef-primary);
            color: #fff;
            border-color: var(--reef-primary);
        }
        """;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context) => Task.CompletedTask;
}
