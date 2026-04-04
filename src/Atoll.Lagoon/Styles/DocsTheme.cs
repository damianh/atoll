using Atoll.Components;
using Atoll.Css;

namespace Atoll.Lagoon.Styles;

/// <summary>
/// Provides the full CSS theme for <c>Atoll.Lagoon</c>. Apply this component once
/// in the docs layout (or a shared head component) to inject all design tokens
/// and structural styles for the documentation site.
/// </summary>
/// <remarks>
/// Marked with <see cref="GlobalStyleAttribute"/> so the CSS is emitted without
/// a scope wrapper — the docs theme must affect the full page, not just a subtree.
/// Sections: reset, light tokens, dark tokens (via <c>[data-theme="dark"]</c>),
/// layout (header / body / sidebar / TOC), typography, prose, code blocks,
/// sidebar nav, TOC, pagination, breadcrumbs, hero, and search dialog.
/// </remarks>
[GlobalStyle]
[Styles(Reset + LightTokens + DarkTokens + Layout + Typography + Prose + CodeBlocks +
        SidebarNav + TocNav + PaginationStyles + BreadcrumbStyles + HeroStyles + SplashStyles + SearchStyles +
        LanguagePickerStyles + UntranslatedNoticeStyles)]
public sealed class DocsTheme : AtollComponent
{
    // -------------------------------------------------------------------------
    // CSS sections (constants so they can be referenced from the attribute)
    // -------------------------------------------------------------------------

    private const string Reset = """
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        html { scroll-behavior: smooth; }
        body {
            font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background: var(--docs-bg);
            color: var(--docs-text);
            line-height: 1.75;
            font-size: 1rem;
        }
        img, svg { display: block; max-width: 100%; }
        a { color: var(--docs-link); text-decoration: none; }
        a:hover { color: var(--docs-link-hover); text-decoration: underline; }
        """;

    private const string LightTokens = """
        :root {
            /* Backgrounds */
            --docs-bg: #ffffff;
            --docs-bg-raised: #f9fafb;
            --docs-bg-subtle: #f3f4f6;
            /* Text */
            --docs-text: #111827;
            --docs-text-muted: #6b7280;
            --docs-text-faint: #9ca3af;
            /* Accent */
            --docs-primary: #0f3460;
            --docs-primary-hover: #1e5fa8;
            --docs-accent: #e94560;
            /* Links */
            --docs-link: #0f3460;
            --docs-link-hover: #e94560;
            /* Borders */
            --docs-border: #e5e7eb;
            /* Sidebar */
            --docs-sidebar-bg: #f9fafb;
            --docs-sidebar-link-active-bg: #eff6ff;
            --docs-sidebar-link-active-text: #0f3460;
            /* Code */
            --docs-code-bg: #1e293b;
            --docs-code-text: #e2e8f0;
            --docs-code-inline-bg: #f3f4f6;
            --docs-code-inline-text: #be123c;
            /* Dimensions */
            --docs-sidebar-width: 16rem;
            --docs-toc-width: 14rem;
            --docs-header-height: 3.5rem;
        }
        """;

    private const string DarkTokens = """
        [data-theme="dark"] {
            /* Backgrounds */
            --docs-bg: #0f172a;
            --docs-bg-raised: #1e293b;
            --docs-bg-subtle: #1e293b;
            /* Text */
            --docs-text: #f1f5f9;
            --docs-text-muted: #94a3b8;
            --docs-text-faint: #64748b;
            /* Accent */
            --docs-primary: #7dd3fc;
            --docs-primary-hover: #bae6fd;
            --docs-accent: #f472b6;
            /* Links */
            --docs-link: #7dd3fc;
            --docs-link-hover: #f472b6;
            /* Borders */
            --docs-border: #334155;
            /* Sidebar */
            --docs-sidebar-bg: #1e293b;
            --docs-sidebar-link-active-bg: #0f172a;
            --docs-sidebar-link-active-text: #7dd3fc;
            /* Code */
            --docs-code-bg: #020617;
            --docs-code-text: #e2e8f0;
            --docs-code-inline-bg: #1e293b;
            --docs-code-inline-text: #f9a8d4;
        }
        """;

    private const string Layout = """
        /* ---- Header ---- */
        .docs-header {
            position: sticky;
            top: 0;
            z-index: 100;
            background: var(--docs-bg);
            border-bottom: 1px solid var(--docs-border);
            height: var(--docs-header-height);
        }
        .docs-header-inner {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 0 1.5rem;
            height: 100%;
            max-width: 100%;
        }
        .docs-brand {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            font-size: 1.125rem;
            font-weight: 700;
            color: var(--docs-text);
            white-space: nowrap;
            flex-shrink: 0;
        }
        .docs-brand:hover { color: var(--docs-link-hover); text-decoration: none; }
        .docs-logo { height: 1.75rem; width: auto; }
        .docs-header-actions {
            display: flex;
            align-items: center;
            gap: 0.75rem;
            margin-left: auto;
        }
        .docs-social-link {
            font-size: 0.875rem;
            color: var(--docs-text-muted);
        }
        .docs-social-link:hover { color: var(--docs-link-hover); text-decoration: none; }

        /* ---- Body layout ---- */
        .docs-body {
            display: grid;
            grid-template-columns: var(--docs-sidebar-width) 1fr var(--docs-toc-width);
            min-height: calc(100vh - var(--docs-header-height));
        }
        .docs-sidebar {
            grid-column: 1;
            background: var(--docs-sidebar-bg);
            border-right: 1px solid var(--docs-border);
            padding: 1.5rem 0.75rem;
            overflow-y: auto;
            position: sticky;
            top: var(--docs-header-height);
            height: calc(100vh - var(--docs-header-height));
        }
        .docs-main {
            grid-column: 2;
            min-width: 0;
            padding: 2.5rem 3rem;
        }
        .docs-toc {
            grid-column: 3;
            padding: 2.5rem 1rem;
            position: sticky;
            top: var(--docs-header-height);
            height: calc(100vh - var(--docs-header-height));
            overflow-y: auto;
            border-left: 1px solid var(--docs-border);
        }
        .docs-footer {
            grid-column: 1 / -1;
            border-top: 1px solid var(--docs-border);
            padding: 1.25rem 1.5rem;
            text-align: center;
            color: var(--docs-text-muted);
            font-size: 0.8rem;
        }

        /* ---- Responsive ---- */
        @media (max-width: 1024px) {
            .docs-body {
                grid-template-columns: var(--docs-sidebar-width) 1fr;
            }
            .docs-toc { display: none; }
        }
        @media (max-width: 768px) {
            .docs-body {
                grid-template-columns: 1fr;
            }
            .docs-sidebar {
                display: none;
                position: fixed;
                inset: 0;
                z-index: 50;
                width: 80vw;
                max-width: 20rem;
                overflow-y: auto;
            }
            .docs-sidebar[aria-hidden="false"] { display: block; }
            .docs-main { padding: 1.5rem; }
        }
        """;

    private const string Typography = """
        /* ---- Typography ---- */
        h1, h2, h3, h4, h5, h6 {
            font-weight: 700;
            line-height: 1.3;
            color: var(--docs-text);
        }
        h1 { font-size: 2rem; margin-bottom: 1rem; }
        h2 { font-size: 1.5rem; margin-top: 2.5rem; margin-bottom: 0.75rem; }
        h3 { font-size: 1.25rem; margin-top: 2rem; margin-bottom: 0.5rem; }
        p { margin-bottom: 1rem; }
        """;

    private const string Prose = """
        /* ---- Prose (Markdown article content) ---- */
        .prose h1 { font-size: 2rem; font-weight: 800; margin-bottom: 0.75rem; color: var(--docs-primary); }
        .prose h2 {
            font-size: 1.375rem;
            font-weight: 700;
            margin-top: 2.5rem;
            margin-bottom: 0.75rem;
            border-bottom: 1px solid var(--docs-border);
            padding-bottom: 0.375rem;
        }
        .prose h2:first-child { margin-top: 0; }
        .prose h3 { font-size: 1.125rem; font-weight: 600; margin-top: 1.75rem; margin-bottom: 0.5rem; }
        .prose h4 { font-size: 1rem; font-weight: 600; margin-top: 1.25rem; margin-bottom: 0.375rem; }
        .prose p { margin-bottom: 1rem; color: var(--docs-text); }
        .prose ul, .prose ol { margin-bottom: 1rem; padding-left: 1.5rem; }
        .prose li { margin-bottom: 0.3rem; }
        .prose table { width: 100%; border-collapse: collapse; margin-bottom: 1.25rem; font-size: 0.9rem; }
        .prose th, .prose td { border: 1px solid var(--docs-border); padding: 0.5rem 0.875rem; text-align: left; }
        .prose th { background: var(--docs-bg-raised); font-weight: 600; }
        .prose a { color: var(--docs-link); text-decoration: underline; }
        .prose a:hover { color: var(--docs-link-hover); }
        .prose blockquote {
            border-left: 3px solid var(--docs-border);
            padding-left: 1rem;
            color: var(--docs-text-muted);
            margin-bottom: 1rem;
            font-style: italic;
        }
        .prose hr { border: none; border-top: 1px solid var(--docs-border); margin: 2rem 0; }
        .prose img { border-radius: 0.375rem; margin: 1.25rem 0; }
        .prose details { border: 1px solid var(--docs-border); border-radius: 0.375rem; padding: 0.75rem 1rem; margin-bottom: 1rem; }
        .prose summary { cursor: pointer; font-weight: 600; }
        """;

    private const string CodeBlocks = """
        /* ---- Code ---- */
        .prose code {
            background: var(--docs-code-inline-bg);
            color: var(--docs-code-inline-text);
            padding: 0.15rem 0.35rem;
            border-radius: 0.25rem;
            font-size: 0.875em;
            font-family: ui-monospace, "Cascadia Code", "Fira Code", monospace;
        }
        .prose pre {
            background: var(--docs-code-bg);
            color: var(--docs-code-text);
            padding: 1.25rem 1.5rem;
            border-radius: 0.5rem;
            overflow-x: auto;
            margin-bottom: 1.25rem;
            font-size: 0.875rem;
            line-height: 1.6;
        }
        .prose pre code {
            background: none;
            padding: 0;
            color: inherit;
            font-size: inherit;
            border-radius: 0;
        }
        /* Mermaid diagrams */
        .prose pre.mermaid {
            background: var(--docs-bg-subtle);
            display: flex;
            justify-content: center;
            padding: 2rem;
        }
        """;

    private const string SidebarNav = """
        /* ---- Sidebar navigation ---- */
        .docs-sidebar nav ul { list-style: none; }
        .docs-sidebar nav li { margin: 0.1rem 0; }
        .docs-sidebar nav a {
            display: block;
            padding: 0.3rem 0.75rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            color: var(--docs-text);
            transition: background 0.1s, color 0.1s;
        }
        .docs-sidebar nav a:hover {
            background: var(--docs-sidebar-link-active-bg);
            color: var(--docs-primary);
            text-decoration: none;
        }
        .docs-sidebar nav a[aria-current="page"] {
            background: var(--docs-sidebar-link-active-bg);
            color: var(--docs-sidebar-link-active-text);
            font-weight: 600;
        }
        .sidebar-group-heading {
            font-size: 0.7rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.08em;
            color: var(--docs-text-muted);
            padding: 0.75rem 0.75rem 0.25rem;
        }
        .sidebar-badge {
            display: inline-block;
            font-size: 0.65rem;
            font-weight: 700;
            padding: 0.1rem 0.4rem;
            border-radius: 999px;
            background: var(--docs-accent);
            color: #fff;
            margin-left: 0.4rem;
            vertical-align: middle;
        }
        details > summary { list-style: none; cursor: pointer; }
        details > summary::-webkit-details-marker { display: none; }
        """;

    private const string TocNav = """
        /* ---- Table of contents ---- */
        .docs-toc nav ul { list-style: none; }
        .docs-toc nav > ul > li { margin-bottom: 0.35rem; }
        .docs-toc nav a {
            font-size: 0.8125rem;
            color: var(--docs-text-muted);
            transition: color 0.1s;
        }
        .docs-toc nav a:hover { color: var(--docs-link-hover); text-decoration: none; }
        .docs-toc nav ul ul { padding-left: 0.875rem; }
        .docs-toc nav ul ul a { font-size: 0.78125rem; }
        .docs-toc-heading {
            display: block;
            font-size: 0.7rem;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.08em;
            color: var(--docs-text-muted);
            margin-bottom: 0.75rem;
        }
        """;

    private const string PaginationStyles = """
        /* ---- Pagination ---- */
        .docs-pagination {
            display: flex;
            justify-content: space-between;
            gap: 1rem;
            margin-top: 3rem;
            padding-top: 1.5rem;
            border-top: 1px solid var(--docs-border);
        }
        .docs-pagination a {
            display: flex;
            flex-direction: column;
            padding: 0.75rem 1rem;
            border: 1px solid var(--docs-border);
            border-radius: 0.5rem;
            max-width: 48%;
            transition: border-color 0.1s, background 0.1s;
        }
        .docs-pagination a:hover {
            border-color: var(--docs-primary);
            background: var(--docs-bg-subtle);
            text-decoration: none;
        }
        .docs-pagination a[rel="next"] {
            margin-left: auto;
            text-align: right;
        }
        .pagination-direction {
            font-size: 0.75rem;
            color: var(--docs-text-muted);
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.04em;
        }
        .pagination-label {
            font-size: 0.9375rem;
            font-weight: 600;
            color: var(--docs-link);
            margin-top: 0.2rem;
        }
        """;

    private const string BreadcrumbStyles = """
        /* ---- Breadcrumbs ---- */
        .docs-breadcrumbs {
            margin-bottom: 1.5rem;
        }
        .docs-breadcrumbs ol {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            gap: 0.25rem;
            list-style: none;
            font-size: 0.8125rem;
            color: var(--docs-text-muted);
        }
        .docs-breadcrumbs ol li + li::before {
            content: "/";
            margin-right: 0.25rem;
            color: var(--docs-text-faint);
        }
        .docs-breadcrumbs a { color: var(--docs-text-muted); }
        .docs-breadcrumbs a:hover { color: var(--docs-link-hover); text-decoration: none; }
        .docs-breadcrumbs [aria-current="page"] { color: var(--docs-text); font-weight: 500; }
        """;

    private const string HeroStyles = """
        /* ---- Hero section ---- */
        .hero {
            padding: 4rem 0 2.5rem;
            display: grid;
            gap: 2rem;
        }
        @media (min-width: 768px) {
            .hero { grid-template-columns: 1fr auto; align-items: center; }
        }
        .hero-title {
            font-size: 2.75rem;
            font-weight: 800;
            line-height: 1.15;
            color: var(--docs-primary);
            margin-bottom: 1rem;
        }
        .hero-tagline {
            font-size: 1.125rem;
            color: var(--docs-text-muted);
            max-width: 36rem;
            margin-bottom: 2rem;
        }
        .hero-actions {
            display: flex;
            flex-wrap: wrap;
            gap: 0.75rem;
        }
        .hero-action-primary {
            display: inline-block;
            background: var(--docs-primary);
            color: #fff;
            padding: 0.75rem 1.5rem;
            border-radius: 0.375rem;
            font-weight: 600;
            font-size: 0.9375rem;
            transition: background 0.1s;
        }
        .hero-action-primary:hover {
            background: var(--docs-accent);
            color: #fff;
            text-decoration: none;
        }
        .hero-action-secondary {
            display: inline-block;
            border: 2px solid var(--docs-border);
            color: var(--docs-text);
            padding: 0.6875rem 1.5rem;
            border-radius: 0.375rem;
            font-weight: 600;
            font-size: 0.9375rem;
            transition: border-color 0.1s, color 0.1s;
        }
        .hero-action-secondary:hover {
            border-color: var(--docs-primary);
            color: var(--docs-primary);
            text-decoration: none;
        }
        .hero-image { border-radius: 0.5rem; max-height: 24rem; object-fit: cover; }
        """;

    private const string SplashStyles = """
        /* ---- Splash layout ---- */
        .splash-main {
            max-width: 72rem;
            margin: 0 auto;
            padding: 2rem 1.5rem;
            min-height: calc(100vh - var(--docs-header-height));
        }
        .splash-content {
            max-width: 65rem;
            margin: 0 auto;
        }

        /* Hero in splash context — larger, more prominent */
        .splash-content .hero {
            padding: 6rem 0 4rem;
            text-align: center;
        }
        .splash-content .hero-title {
            font-size: 3.5rem;
        }
        .splash-content .hero-tagline {
            font-size: 1.25rem;
            max-width: 42rem;
            margin-left: auto;
            margin-right: auto;
        }
        .splash-content .hero-actions {
            justify-content: center;
        }
        .splash-content .hero-image {
            margin: 0 auto;
        }

        /* On splash pages, when hero has an image, revert to side-by-side on desktop */
        @media (min-width: 768px) {
            .splash-content .hero {
                text-align: left;
            }
            .splash-content .hero-tagline {
                margin-left: 0;
            }
            .splash-content .hero-actions {
                justify-content: flex-start;
            }
        }

        /* Splash responsive */
        @media (max-width: 768px) {
            .splash-content .hero {
                padding: 3rem 0 2rem;
            }
            .splash-content .hero-title {
                font-size: 2.25rem;
            }
        }
        """;

    private const string SearchStyles = """
        /* ---- Search ---- */
        .search-wrapper { position: relative; }
        #search-trigger {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.4rem 0.75rem;
            border: 1px solid var(--docs-border);
            border-radius: 0.375rem;
            background: var(--docs-bg-subtle);
            color: var(--docs-text-muted);
            font-size: 0.875rem;
            cursor: pointer;
            white-space: nowrap;
            transition: border-color 0.1s;
        }
        #search-trigger:hover { border-color: var(--docs-primary); color: var(--docs-text); }
        #search-trigger kbd {
            display: inline-block;
            padding: 0.1rem 0.3rem;
            border: 1px solid var(--docs-border);
            border-radius: 0.2rem;
            font-size: 0.7rem;
            color: var(--docs-text-faint);
            background: var(--docs-bg);
        }
        #search-dialog {
            position: fixed;
            inset: 0;
            z-index: 200;
            display: none;
            align-items: flex-start;
            justify-content: center;
            padding-top: 10vh;
            background: rgba(0,0,0,0.4);
        }
        #search-dialog[open] { display: flex; }
        .search-dialog-inner {
            background: var(--docs-bg);
            border-radius: 0.75rem;
            box-shadow: 0 25px 50px rgba(0,0,0,0.25);
            width: 100%;
            max-width: 36rem;
            overflow: hidden;
        }
        .search-dialog-header {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1rem;
            border-bottom: 1px solid var(--docs-border);
        }
        #search-input {
            flex: 1;
            border: none;
            outline: none;
            font-size: 1rem;
            background: transparent;
            color: var(--docs-text);
        }
        #search-close {
            background: none;
            border: none;
            font-size: 1.25rem;
            cursor: pointer;
            color: var(--docs-text-muted);
            line-height: 1;
        }
        #search-results {
            max-height: 60vh;
            overflow-y: auto;
            padding: 0.5rem;
        }
        .search-result-item {
            padding: 0.6rem 0.75rem;
            border-radius: 0.375rem;
            cursor: pointer;
            color: var(--docs-text);
        }
        .search-result-item:hover,
        .search-result-item[aria-selected="true"] {
            background: var(--docs-sidebar-link-active-bg);
        }
        .search-result-title { font-weight: 600; font-size: 0.9375rem; margin-bottom: 0.2rem; }
        .search-result-section { font-size: 0.75rem; color: var(--docs-text-muted); margin-bottom: 0.2rem; }
        .search-result-snippet { font-size: 0.8125rem; color: var(--docs-text-muted); }
        .search-result-snippet mark { background: transparent; color: var(--docs-accent); font-weight: 600; }

        /* Mobile nav toggle button */
        #mobile-nav-toggle {
            display: none;
            background: none;
            border: none;
            font-size: 1.5rem;
            cursor: pointer;
            color: var(--docs-text);
            padding: 0.25rem;
            margin-right: 0.5rem;
        }
        @media (max-width: 768px) { #mobile-nav-toggle { display: inline-flex; } }

        /* Theme toggle button */
        #theme-toggle {
            background: none;
            border: none;
            font-size: 1.25rem;
            cursor: pointer;
            color: var(--docs-text-muted);
            padding: 0.25rem;
            border-radius: 0.25rem;
            transition: color 0.1s;
        }
        #theme-toggle:hover { color: var(--docs-text); }
        """;

    private const string LanguagePickerStyles = """
        /* ---- Language picker ---- */
        .language-picker {
            display: inline-flex;
            align-items: center;
        }
        .language-picker select {
            appearance: none;
            -webkit-appearance: none;
            background: var(--docs-bg-subtle);
            color: var(--docs-text);
            border: 1px solid var(--docs-border);
            border-radius: 0.375rem;
            padding: 0.3rem 1.75rem 0.3rem 0.5rem;
            font-size: 0.8125rem;
            font-family: inherit;
            cursor: pointer;
            transition: border-color 0.1s;
            background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%236b7280' d='M2 4l4 4 4-4'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 0.5rem center;
        }
        .language-picker select:hover {
            border-color: var(--docs-primary);
        }
        .language-picker select:focus {
            outline: 2px solid var(--docs-primary);
            outline-offset: 1px;
        }
        """;

    private const string UntranslatedNoticeStyles = """
        /* ---- Untranslated content notice ---- */
        .untranslated-notice {
            background: var(--docs-bg-subtle);
            border: 1px solid var(--docs-border);
            border-radius: 0.375rem;
            padding: 0.75rem 1rem;
            margin-bottom: 1.5rem;
            font-size: 0.875rem;
            color: var(--docs-text-muted);
        }
        """;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context) => Task.CompletedTask;
}
