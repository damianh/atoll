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
[Styles(Reset + LightTokens + DarkTokens + Layout + ScrollbarStyles + Typography + Prose + CodeBlocks + SyntaxHighlightTokens + CodeCopyButtonStyles + ExpressiveCodeStyles +
        SidebarNav + TocNav + PaginationStyles + BreadcrumbStyles + HeroStyles + SplashStyles + SearchStyles +
        LanguagePickerStyles + UntranslatedNoticeStyles + AsideStyles + ContentFooterStyles + FooterLinkStyles)]
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
            --docs-code-bg: #f5f5f5;
            --docs-code-text: #1f2328;
            --docs-code-inline-bg: #f3f4f6;
            --docs-code-inline-text: #be123c;
            /* Aside variants */
            --aside-note-border: #3b82f6;
            --aside-note-bg: #eff6ff;
            --aside-note-text: #1e40af;
            --aside-tip-border: #10b981;
            --aside-tip-bg: #ecfdf5;
            --aside-tip-text: #065f46;
            --aside-caution-border: #f59e0b;
            --aside-caution-bg: #fffbeb;
            --aside-caution-text: #92400e;
            --aside-danger-border: #ef4444;
            --aside-danger-bg: #fef2f2;
            --aside-danger-text: #991b1b;
            /* Scrollbar */
            --docs-scrollbar-thumb: #c1c1c1;
            --docs-scrollbar-thumb-hover: #a8a8a8;
            --docs-scrollbar-track: transparent;
            /* Dimensions */
            --docs-sidebar-width: 16rem;
            --docs-toc-width: 14rem;
            --docs-header-height: 3.5rem;
        }
        """;

    private const string DarkTokens = """
        [data-theme="dark"] {
            /* Backgrounds — GitHub Dark */
            --docs-bg: #0d1117;
            --docs-bg-raised: #161b22;
            --docs-bg-subtle: #21262d;
            /* Text */
            --docs-text: #e6edf3;
            --docs-text-muted: #8d96a0;
            --docs-text-faint: #6e7681;
            /* Accent */
            --docs-primary: #58a6ff;
            --docs-primary-hover: #79c0ff;
            --docs-accent: #f78166;
            /* Links */
            --docs-link: #58a6ff;
            --docs-link-hover: #79c0ff;
            /* Borders */
            --docs-border: #30363d;
            /* Sidebar */
            --docs-sidebar-bg: #161b22;
            --docs-sidebar-link-active-bg: #1f2428;
            --docs-sidebar-link-active-text: #58a6ff;
            /* Code */
            --docs-code-bg: #161b22;
            --docs-code-text: #e6edf3;
            --docs-code-inline-bg: #343942;
            --docs-code-inline-text: #f78166;
            /* Scrollbar */
            --docs-scrollbar-thumb: #484f58;
            --docs-scrollbar-thumb-hover: #6e7681;
            --docs-scrollbar-track: transparent;
            /* Aside variants */
            --aside-note-border: #58a6ff;
            --aside-note-bg: #161b22;
            --aside-note-text: #79c0ff;
            --aside-tip-border: #3fb950;
            --aside-tip-bg: #161b22;
            --aside-tip-text: #56d364;
            --aside-caution-border: #d29922;
            --aside-caution-bg: #161b22;
            --aside-caution-text: #e3b341;
            --aside-danger-border: #f85149;
            --aside-danger-bg: #161b22;
            --aside-danger-text: #ff7b72;
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
            z-index: 10;
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

    private const string ScrollbarStyles = """
        /* ---- Scrollbar theming ---- */

        /* Modern browsers (Firefox, Chrome 121+) */
        .docs-sidebar,
        .docs-toc,
        #search-results,
        .prose pre {
            scrollbar-width: thin;
            scrollbar-color: var(--docs-scrollbar-thumb) var(--docs-scrollbar-track);
        }

        /* WebKit / Blink (Chrome < 121, Safari, Edge) */
        .docs-sidebar::-webkit-scrollbar,
        .docs-toc::-webkit-scrollbar,
        #search-results::-webkit-scrollbar,
        .prose pre::-webkit-scrollbar {
            width: 8px;
            height: 8px;
        }
        .docs-sidebar::-webkit-scrollbar-track,
        .docs-toc::-webkit-scrollbar-track,
        #search-results::-webkit-scrollbar-track,
        .prose pre::-webkit-scrollbar-track {
            background: var(--docs-scrollbar-track);
        }
        .docs-sidebar::-webkit-scrollbar-thumb,
        .docs-toc::-webkit-scrollbar-thumb,
        #search-results::-webkit-scrollbar-thumb,
        .prose pre::-webkit-scrollbar-thumb {
            background-color: var(--docs-scrollbar-thumb);
            border-radius: 4px;
        }
        .docs-sidebar::-webkit-scrollbar-thumb:hover,
        .docs-toc::-webkit-scrollbar-thumb:hover,
        #search-results::-webkit-scrollbar-thumb:hover,
        .prose pre::-webkit-scrollbar-thumb:hover {
            background-color: var(--docs-scrollbar-thumb-hover);
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
        .prose h1, .prose h2, .prose h3, .prose h4, .prose h5, .prose h6 {
            scroll-margin-top: calc(var(--docs-header-height) + 1rem);
        }
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
            line-height: 1.4;
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

    private const string SyntaxHighlightTokens = """
        /* ---- Syntax Highlighting Tokens ---- */
        /* Light theme — VS Code Light+ inspired */
        .highlight code .tm-keyword { color: #0000ff; }
        .highlight code .tm-string { color: #a31515; }
        .highlight code .tm-comment { color: #008000; font-style: italic; }
        .highlight code .tm-type { color: #267f99; }
        .highlight code .tm-number { color: #098658; }
        .highlight code .tm-function { color: #795e26; }
        .highlight code .tm-variable { color: #001080; }
        .highlight code .tm-constant { color: #0070c1; }
        .highlight code .tm-punctuation { color: #383a42; }
        .highlight code .tm-namespace { color: #267f99; }
        .highlight code .tm-preprocessor { color: #808080; }
        /* Dark theme — Material Palenight inspired */
        [data-theme="dark"] .highlight code .tm-keyword { color: #c792ea; }
        [data-theme="dark"] .highlight code .tm-string { color: #c3e88d; }
        [data-theme="dark"] .highlight code .tm-comment { color: #546e7a; font-style: italic; }
        [data-theme="dark"] .highlight code .tm-type { color: #ffcb6b; }
        [data-theme="dark"] .highlight code .tm-number { color: #f78c6c; }
        [data-theme="dark"] .highlight code .tm-function { color: #82aaff; }
        [data-theme="dark"] .highlight code .tm-variable { color: #f07178; }
        [data-theme="dark"] .highlight code .tm-constant { color: #f78c6c; }
        [data-theme="dark"] .highlight code .tm-punctuation { color: #89ddff; }
        [data-theme="dark"] .highlight code .tm-namespace { color: #ffcb6b; }
        [data-theme="dark"] .highlight code .tm-preprocessor { color: #546e7a; }
        """;

    private const string CodeCopyButtonStyles = """
        /* ---- Code copy button ---- */
        .code-block-wrapper {
            position: relative;
        }
        .code-copy-btn {
            position: absolute;
            top: 0.5rem;
            right: 0.5rem;
            display: flex;
            align-items: center;
            justify-content: center;
            width: 2rem;
            height: 2rem;
            padding: 0;
            border: 1px solid var(--docs-border);
            border-radius: 0.375rem;
            background: var(--docs-bg-subtle);
            color: var(--docs-text-muted);
            cursor: pointer;
            opacity: 0;
            transition: opacity 0.15s ease, background 0.15s ease, color 0.15s ease;
        }
        .code-block-wrapper:hover .code-copy-btn,
        .code-copy-btn:focus-visible {
            opacity: 1;
        }
        .code-copy-btn:hover {
            background: var(--docs-bg-raised);
            color: var(--docs-text);
        }
        .code-copy-btn svg {
            width: 1rem;
            height: 1rem;
            flex-shrink: 0;
        }
        .code-copy-btn .check-icon { display: none; }
        .code-copy-btn.copied .copy-icon { display: none; }
        .code-copy-btn.copied .check-icon { display: block; }
        .code-copy-btn.copied {
            color: var(--aside-tip-text);
            opacity: 1;
        }
        """;

    private const string ExpressiveCodeStyles = """
        /* ---- Expressive Code: line-level structure ---- */
        .ec-line {
            display: block;
        }
        .ec-line-content {
            display: inline;
        }

        /* ---- Expressive Code: frames ---- */
        .ec-frame {
            margin-bottom: 1.25rem;
            border-radius: 0.5rem;
            overflow: hidden;
            border: 1px solid var(--docs-border);
            background: var(--docs-code-bg);
        }
        .ec-frame .highlight {
            margin-bottom: 0;
            border-radius: 0;
            border: none;
        }
        .ec-header {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.4rem 0.75rem;
            background: var(--docs-bg-subtle);
            border-bottom: 1px solid var(--docs-border);
            min-height: 2.25rem;
        }
        /* Editor frame tab */
        .ec-tab {
            display: flex;
            align-items: center;
            padding: 0.1rem 0.75rem;
            background: var(--docs-code-bg);
            border-radius: 0.25rem 0.25rem 0 0;
            border: 1px solid var(--docs-border);
            border-bottom: none;
            font-size: 0.8rem;
            color: var(--docs-text-muted);
            font-family: ui-monospace, "Cascadia Code", "Fira Code", monospace;
        }
        .ec-title {
            color: var(--docs-text);
            font-size: 0.8rem;
            font-family: ui-monospace, "Cascadia Code", "Fira Code", monospace;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
            max-width: 40ch;
        }
        /* Terminal dots (three circles using box-shadow trick) */
        .ec-terminal-dots {
            display: inline-block;
            width: 0.75rem;
            height: 0.75rem;
            border-radius: 50%;
            background: #ff5f57;
            box-shadow: 1.375rem 0 0 #febc2e, 2.75rem 0 0 #28c840;
            flex-shrink: 0;
            margin-right: 2rem;
        }
        /* Copy button in frame header: static positioning inside flex row */
        .ec-frame .code-copy-btn {
            position: static;
            margin-left: auto;
            opacity: 0;
            flex-shrink: 0;
        }
        .ec-header:hover .code-copy-btn,
        .ec-frame .code-copy-btn:focus-visible {
            opacity: 1;
        }
        /* Dark theme overrides */
        [data-theme="dark"] .ec-frame {
            border-color: var(--docs-border);
        }
        [data-theme="dark"] .ec-header {
            background: var(--docs-bg-raised);
            border-bottom-color: var(--docs-border);
        }
        [data-theme="dark"] .ec-tab {
            background: var(--docs-code-bg);
            border-color: var(--docs-border);
        }

        /* ---- Expressive Code: line markers ---- */
        :root {
            --ec-mark-rgb: 59, 130, 246;
            --ec-ins-rgb: 34, 197, 94;
            --ec-del-rgb: 239, 68, 68;
        }
        [data-theme="dark"] {
            --ec-mark-rgb: 56, 132, 244;
            --ec-ins-rgb: 63, 185, 80;
            --ec-del-rgb: 248, 81, 73;
        }
        .ec-mark {
            background: rgba(var(--ec-mark-rgb), 0.15);
            box-shadow: inset 3px 0 0 rgba(var(--ec-mark-rgb), 0.5);
        }
        .ec-ins {
            background: rgba(var(--ec-ins-rgb), 0.15);
            box-shadow: inset 3px 0 0 rgba(var(--ec-ins-rgb), 0.5);
        }
        .ec-del {
            background: rgba(var(--ec-del-rgb), 0.15);
            box-shadow: inset 3px 0 0 rgba(var(--ec-del-rgb), 0.5);
        }

        /* ---- Expressive Code: inline text markers ---- */
        mark.ec-text-marker {
            background: #fef08a;
            border-bottom: 2px solid #facc15;
            border-radius: 0.2em;
            padding: 0 0.1em;
            color: inherit;
        }
        [data-theme="dark"] mark.ec-text-marker {
            background: rgba(250, 204, 21, 0.25);
            border-bottom: 2px solid rgba(250, 204, 21, 0.5);
        }

        /* ---- Expressive Code: collapsible sections ---- */
        .ec-collapse-group {
            display: block;
        }
        .ec-collapse-summary {
            display: block;
            list-style: none;
            cursor: pointer;
            padding: 0.2rem 1rem;
            font-size: 0.8rem;
            color: var(--docs-text-muted);
            background: var(--docs-bg-subtle);
            border-top: 1px solid var(--docs-border);
            border-bottom: 1px solid var(--docs-border);
            user-select: none;
        }
        .ec-collapse-summary::-webkit-details-marker { display: none; }
        .ec-collapse-summary::before {
            content: "▶ ";
            font-size: 0.65em;
            vertical-align: middle;
        }
        .ec-collapse-group[open] > .ec-collapse-summary::before {
            content: "▼ ";
        }
        .ec-collapse-summary:hover {
            color: var(--docs-text);
            background: var(--docs-bg-raised);
        }
        [data-theme="dark"] .ec-collapse-summary {
            background: var(--docs-bg-raised);
        }
        [data-theme="dark"] .ec-collapse-summary:hover {
            background: var(--docs-bg-subtle);
        }

        /* ---- Expressive Code: word wrap ---- */
        [data-wrap] pre,
        [data-wrap].highlight {
            white-space: pre-wrap;
            word-break: break-all;
            overflow-x: visible;
        }
        [data-wrap] .ec-line-content {
            display: inline-block;
            padding-left: var(--ec-indent, 0);
            text-indent: calc(-1 * var(--ec-indent, 0));
        }

        /* ---- Expressive Code: line numbers ---- */
        [data-line-numbers] {
            counter-reset: ec-line-num;
        }
        [data-line-numbers] .ec-line {
            counter-increment: ec-line-num;
            display: flex;
            align-items: baseline;
        }
        [data-line-numbers] .ec-line::before {
            content: counter(ec-line-num);
            display: inline-block;
            min-width: 3ch;
            text-align: right;
            margin-right: 1.5ch;
            color: var(--docs-text-faint);
            user-select: none;
            flex-shrink: 0;
        }

        /* ---- Expressive Code: responsive & print polish ---- */
        /* Ensure frames don't overflow on small screens */
        .ec-frame {
            max-width: 100%;
            min-width: 0;
        }
        /* Horizontal scrolling inside framed pre blocks */
        .ec-frame pre.highlight {
            overflow-x: auto;
        }
        /* Long titles: allow truncation on narrow headers */
        .ec-header {
            overflow: hidden;
        }
        /* Print: expand all collapsed sections */
        @media print {
            .ec-collapse-group[open] > .ec-collapse-content,
            .ec-collapse-group > .ec-collapse-content {
                display: block !important;
            }
            .ec-collapse-summary {
                display: none;
            }
        }
        """;

    private const string SidebarNav = """
        .docs-sidebar nav ul { list-style: none; }
        .docs-sidebar nav li { margin: 0.1rem 0; }
        .docs-sidebar nav a {
            display: block;
            padding: 0.3rem 0.75rem;
            border-radius: 0.25rem;
            font-size: 0.875rem;
            color: var(--docs-text-muted);
            transition: background 0.1s, color 0.1s;
        }
        .docs-sidebar nav a:hover {
            color: var(--docs-text);
            text-decoration: none;
        }
        .docs-sidebar nav a[aria-current="page"] {
            background: var(--docs-sidebar-link-active-bg);
            color: var(--docs-sidebar-link-active-text);
            font-weight: 600;
            border-inline-start: 3px solid var(--docs-primary);
            padding-inline-start: calc(0.75rem - 3px);
        }
        .sidebar-group-heading {
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--docs-text);
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
        .sidebar-badge-note {
            background: var(--aside-note-bg);
            color: var(--aside-note-text);
        }
        .sidebar-badge-tip {
            background: var(--aside-tip-bg);
            color: var(--aside-tip-text);
        }
        .sidebar-badge-success {
            background: var(--aside-tip-bg);
            color: var(--aside-tip-text);
        }
        .sidebar-badge-caution {
            background: var(--aside-caution-bg);
            color: var(--aside-caution-text);
        }
        .sidebar-badge-danger {
            background: var(--aside-danger-bg);
            color: var(--aside-danger-text);
        }
        /* Collapsible group chevron */
        .docs-sidebar details > summary {
            list-style: none;
            cursor: pointer;
            display: flex;
            align-items: center;
            font-size: 0.875rem;
            font-weight: 600;
            color: var(--docs-text);
            padding: 0.5rem 0.75rem 0.25rem;
            gap: 0.35rem;
        }
        .docs-sidebar details > summary::-webkit-details-marker { display: none; }
        .docs-sidebar details > summary .sidebar-chevron {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 1.25rem;
            height: 1.25rem;
            flex-shrink: 0;
            transition: transform 0.2s ease-in-out;
        }
        .docs-sidebar details[open] > summary .sidebar-chevron {
            transform: rotate(90deg);
        }
        /* Chevron at end (right in LTR) — default Starlight style */
        .docs-sidebar .sidebar-chevron-end > summary {
            justify-content: space-between;
        }
        /* Chevron at start (left in LTR) — Duende style */
        .docs-sidebar .sidebar-chevron-start > summary {
            flex-direction: row-reverse;
            justify-content: flex-end;
        }
        /* Add top border between sidebar groups for visual separation */
        .docs-sidebar nav > ul > .sidebar-group-item + .sidebar-group-item {
            margin-top: 0.5rem;
            padding-top: 0.5rem;
            border-top: 1px solid var(--docs-border);
        }
        """;

    private const string TocNav = """
        /* ---- Table of contents ---- */
        .docs-toc nav ul { list-style: none; }
        .docs-toc nav > ul > li { margin-bottom: 0.35rem; }
        .docs-toc nav a {
            font-size: 0.8125rem;
            color: var(--docs-text-muted);
            transition: color 0.15s, border-color 0.15s;
            display: block;
            padding: 0.15rem 0 0.15rem 0.75rem;
            border-left: 2px solid transparent;
        }
        .docs-toc nav a:hover { color: var(--docs-link-hover); text-decoration: none; }
        .docs-toc nav a[aria-current="true"] {
            color: var(--docs-link);
            font-weight: 600;
            border-left-color: var(--docs-link);
        }
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
            border: none;
            border-radius: 0.75rem;
            padding: 0;
            max-width: 36rem;
            width: calc(100% - 2rem);
            margin: 10vh auto 0;
            background: var(--docs-bg);
            box-shadow: 0 25px 50px rgba(0,0,0,0.25);
            overflow: hidden;
        }
        #search-dialog::backdrop {
            background: rgba(0,0,0,0.4);
            -webkit-backdrop-filter: blur(0.25rem);
            backdrop-filter: blur(0.25rem);
        }
        .search-dialog-inner {
            width: 100%;
        }
        .search-dialog-header {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.75rem 1rem;
            border-bottom: 1px solid var(--docs-border);
        }
        .search-dialog-header svg {
            flex-shrink: 0;
            width: 1.25rem;
            height: 1.25rem;
            color: var(--docs-text-muted);
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
            padding: 0.5rem 0.75rem;
        }
        .search-result-count {
            font-size: 0.8125rem;
            color: var(--docs-text-muted);
            margin: 0 0 0.5rem 0.25rem;
        }
        .search-no-results {
            padding: 1rem;
            text-align: center;
            color: var(--docs-text-muted);
            font-size: 0.875rem;
        }

        /* ---- Tree-view result groups ---- */
        .search-result-group {
            background: var(--docs-bg-subtle);
            border-radius: 0.5rem;
            margin-bottom: 0.75rem;
            display: flex;
            flex-direction: column;
            gap: 1px;
            overflow: hidden;
        }

        /* Page-level parent row */
        .search-result-page {
            position: relative;
            padding: 0.75rem 1rem 0.75rem 3rem;
            cursor: pointer;
            color: var(--docs-text);
            transition: outline-color 0.1s;
            outline: 1px solid transparent;
        }
        .search-result-page:hover,
        .search-result-page:focus-within {
            outline-color: var(--docs-accent);
        }
        .search-result-page:focus {
            background: var(--docs-sidebar-link-active-bg);
            outline-color: var(--docs-accent);
            outline-style: solid;
        }
        /* Document icon (page-level) */
        .search-result-page::before {
            content: '';
            position: absolute;
            inset-block: 0;
            inset-inline-start: 0.625rem;
            width: 1.5rem;
            background: var(--docs-text-muted);
            -webkit-mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='currentColor' viewBox='0 0 24 24'%3E%3Cpath d='M9 10h1a1 1 0 1 0 0-2H9a1 1 0 0 0 0 2Zm0 2a1 1 0 0 0 0 2h6a1 1 0 0 0 0-2H9Zm11-3V8l-6-6a1 1 0 0 0-1 0H7a3 3 0 0 0-3 3v14a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3V9Zm-6-4 3 3h-2a1 1 0 0 1-1-1V5Zm4 14a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1h5v3a3 3 0 0 0 3 3h3v9Zm-3-3H9a1 1 0 0 0 0 2h6a1 1 0 0 0 0-2Z'/%3E%3C/svg%3E") center no-repeat;
            mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='currentColor' viewBox='0 0 24 24'%3E%3Cpath d='M9 10h1a1 1 0 1 0 0-2H9a1 1 0 0 0 0 2Zm0 2a1 1 0 0 0 0 2h6a1 1 0 0 0 0-2H9Zm11-3V8l-6-6a1 1 0 0 0-1 0H7a3 3 0 0 0-3 3v14a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3V9Zm-6-4 3 3h-2a1 1 0 0 1-1-1V5Zm4 14a1 1 0 0 1-1 1H7a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1h5v3a3 3 0 0 0 3 3h3v9Zm-3-3H9a1 1 0 0 0 0 2h6a1 1 0 0 0 0-2Z'/%3E%3C/svg%3E") center no-repeat;
        }

        /* Nested heading child rows (tree children) */
        .search-result-nested {
            position: relative;
            padding: 0.5rem 1rem 0.5rem 3rem;
            cursor: pointer;
            color: var(--docs-text);
            background: var(--docs-bg-subtle);
            transition: outline-color 0.1s;
            outline: 1px solid transparent;
        }
        .search-result-nested:hover,
        .search-result-nested:focus-within {
            outline-color: var(--docs-accent);
        }
        .search-result-nested:focus {
            background: var(--docs-sidebar-link-active-bg);
            outline-color: var(--docs-accent);
            outline-style: solid;
        }
        /* Tree connector line (├─ for middle items) */
        .search-result-nested::before {
            content: '';
            position: absolute;
            inset-block: 0;
            inset-inline-start: 0.625rem;
            width: 1.5rem;
            background: var(--docs-text-faint);
            -webkit-mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' stroke='currentColor' stroke-linecap='round' viewBox='0 0 16 1000' preserveAspectRatio='xMinYMin slice'%3E%3Cpath d='M8 0v1000m6-988H8'/%3E%3C/svg%3E") 0% 0% / 100% no-repeat;
            mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' stroke='currentColor' stroke-linecap='round' viewBox='0 0 16 1000' preserveAspectRatio='xMinYMin slice'%3E%3Cpath d='M8 0v1000m6-988H8'/%3E%3C/svg%3E") 0% 0% / 100% no-repeat;
        }
        /* Tree connector line (└─ for last item) */
        .search-result-nested-last::before {
            -webkit-mask-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' viewBox='0 0 16 16'%3E%3Cpath d='M8 0v12m6 0H8'/%3E%3C/svg%3E");
            mask-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' stroke='currentColor' stroke-linecap='round' stroke-linejoin='round' viewBox='0 0 16 16'%3E%3Cpath d='M8 0v12m6 0H8'/%3E%3C/svg%3E");
        }

        /* Section/topic badge */
        .search-result-badge {
            font-size: 0.6875rem;
            color: var(--docs-text-muted);
            padding: 0.35rem 0.75rem;
            background: var(--docs-bg);
            border-top: 1px solid var(--docs-border);
        }
        .search-result-badge::before {
            content: '';
            display: inline-block;
            width: 0.5rem;
            height: 0.5rem;
            border: 1px solid var(--docs-border);
            border-radius: 0.125rem;
            margin-right: 0.35rem;
            vertical-align: middle;
        }

        /* Shared link + text styles */
        .search-result-link {
            text-decoration: none;
            color: inherit;
            display: block;
        }
        .search-result-title {
            font-weight: 600;
            font-size: 0.9375rem;
            display: block;
            line-height: 1.4;
        }
        .search-result-desc {
            font-size: 0.8125rem;
            color: var(--docs-text-muted);
            display: block;
            line-height: 1.45;
            margin-top: 0.1rem;
        }
        .search-result-desc mark,
        .search-result-title mark {
            background: transparent;
            color: var(--docs-accent);
            font-weight: 600;
        }
        .search-dialog-footer {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-top: 1px solid var(--docs-border);
            font-size: 0.75rem;
            color: var(--docs-text-faint);
        }
        .search-dialog-footer kbd {
            display: inline-block;
            padding: 0.1rem 0.35rem;
            border: 1px solid var(--docs-border);
            border-radius: 0.2rem;
            font-size: 0.7rem;
            font-family: inherit;
            background: var(--docs-bg-subtle);
        }

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

    private const string AsideStyles = """
        /* ---- Aside / callout boxes ---- */
        .aside {
            border-left: 3px solid var(--aside-note-border);
            background: var(--aside-note-bg);
            border-radius: 0.375rem;
            padding: 0.875rem 1rem;
            margin-bottom: 1.25rem;
        }
        .aside-note  { border-left-color: var(--aside-note-border);  background: var(--aside-note-bg); }
        .aside-tip   { border-left-color: var(--aside-tip-border);   background: var(--aside-tip-bg); }
        .aside-caution { border-left-color: var(--aside-caution-border); background: var(--aside-caution-bg); }
        .aside-danger  { border-left-color: var(--aside-danger-border);  background: var(--aside-danger-bg); }
        .aside-title {
            display: flex;
            align-items: center;
            gap: 0.4rem;
            font-weight: 600;
            font-size: 0.9375rem;
            margin-bottom: 0.375rem;
        }
        .aside-note  .aside-title { color: var(--aside-note-text); }
        .aside-tip   .aside-title { color: var(--aside-tip-text); }
        .aside-caution .aside-title { color: var(--aside-caution-text); }
        .aside-danger  .aside-title { color: var(--aside-danger-text); }
        .aside-title svg {
            width: 1.125rem;
            height: 1.125rem;
            flex-shrink: 0;
        }
        .aside-content {
            font-size: 0.9rem;
            color: var(--docs-text);
        }
        .aside-content p:last-child { margin-bottom: 0; }
        """;

    private const string ContentFooterStyles = """
        /* ---- Content footer (edit link + last updated) ---- */
        .docs-content-footer {
            display: flex;
            flex-wrap: wrap;
            align-items: center;
            justify-content: space-between;
            gap: 0.75rem;
            margin-top: 2rem;
            padding-top: 1.25rem;
            border-top: 1px solid var(--docs-border);
            font-size: 0.875rem;
            color: var(--docs-text-muted);
        }
        .docs-edit-link {
            color: var(--docs-text-muted);
            text-decoration: underline;
            transition: color 0.1s;
        }
        .docs-edit-link:hover { color: var(--docs-link-hover); }
        .docs-last-updated {
            margin: 0;
            font-size: 0.875rem;
            color: var(--docs-text-muted);
        }
        .docs-last-updated span { font-weight: 500; }
        """;

    private const string FooterLinkStyles = """
        /* ---- Custom footer links ---- */
        .docs-footer-links {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 0 1.25rem;
            list-style: none;
            margin: 0.25rem 0 0;
            padding: 0;
        }
        .docs-footer a {
            color: var(--docs-text-muted);
            transition: color 0.1s;
        }
        .docs-footer a:hover { color: var(--docs-link-hover); text-decoration: none; }
        """;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context) => Task.CompletedTask;
}
