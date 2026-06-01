using Atoll.Components;
using Atoll.Css;

namespace Atoll.Swell.Styles;

/// <summary>
/// Provides the full CSS theme for <c>Atoll.Swell</c> slide decks. Apply this component
/// once in the deck layout to inject all design tokens and structural styles.
/// </summary>
/// <remarks>
/// Marked with <see cref="GlobalStyleAttribute"/> so CSS is emitted without a scope wrapper —
/// slides must affect the full page. Sections: reset, design tokens, slide container (aspect-ratio,
/// letterboxing), slide layouts (default/cover/center/two-cols/image-right/image-left/section/end),
/// typography, code blocks, slide numbering, transitions, overview grid mode, print styles.
/// </remarks>
[Styles(SkipLink + Reset + Tokens + DeckContainer + SlideBase + LayoutDefault + LayoutCover + LayoutCenter +
        LayoutTwoCols + LayoutImageRight + LayoutImageLeft + LayoutSection + LayoutEnd +
        SlideNumber + Transitions + OverviewGrid + ClickReveal + CodeBlocks + Typography + Navbar + PrintStyles)]
public sealed class SwellTheme : AtollComponent
{
    /// <summary>
    /// Gets the complete CSS text for the Swell theme. Use this to emit styles inline
    /// in a <c>&lt;style&gt;</c> tag within the deck layout template's <c>&lt;head&gt;</c>.
    /// </summary>
    public const string AllCss = SkipLink + Reset + Tokens + DeckContainer + SlideBase + LayoutDefault + LayoutCover + LayoutCenter +
        LayoutTwoCols + LayoutImageRight + LayoutImageLeft + LayoutSection + LayoutEnd +
        SlideNumber + Transitions + OverviewGrid + ClickReveal + CodeBlocks + Typography + Navbar + PrintStyles;

    private const string SkipLink = """
        .swell-skip-link {
            position: absolute;
            top: -100%;
            left: 0;
            background: var(--swell-accent);
            color: #fff;
            padding: 0.5rem 1rem;
            z-index: 10000;
            font-weight: 600;
            text-decoration: none;
        }
        .swell-skip-link:focus { top: 0; }
        """;

    private const string Reset = """
        *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
        html, body { height: 100%; width: 100%; overflow: hidden; }
        body {
            font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
            background: var(--swell-bg-outer);
            color: var(--swell-text);
            line-height: 1.5;
        }
        img, svg { display: block; max-width: 100%; }
        a { color: var(--swell-link); }
        """;

    private const string Tokens = """
        :root {
            /* Slide backgrounds */
            --swell-bg-outer: #1a1a2e;
            --swell-bg-slide: #ffffff;
            --swell-bg-cover: linear-gradient(135deg, #0f3460 0%, #16213e 100%);
            --swell-bg-section: #f0f4ff;
            /* Text */
            --swell-text: #1a1a2e;
            --swell-text-muted: #6b7280;
            --swell-text-on-dark: #ffffff;
            /* Accent */
            --swell-accent: #e94560;
            --swell-accent-2: #0f3460;
            /* Links */
            --swell-link: #0f3460;
            /* Code */
            --swell-code-bg: #f3f4f6;
            --swell-code-text: #111827;
            --swell-code-border: #e5e7eb;
            /* Slide number */
            --swell-slide-number-color: rgba(0,0,0,0.35);
            /* Transition duration */
            --swell-transition-duration: 400ms;
        }
        """;

    private const string DeckContainer = """
        .swell-deck {
            position: relative;
            width: 100%;
            max-height: 100vh;
            overflow: hidden;
            background: var(--swell-bg-outer);
            /* aspect-ratio is set inline from DeckConfig */
        }
        /* Letterbox: centre the deck horizontally when viewport is wider than aspect ratio */
        @media (min-height: 1px) {
            .swell-deck {
                margin: auto;
            }
        }
        """;

    private const string SlideBase = """
        .swell-slide {
            position: absolute;
            inset: 0;
            display: none;
            background: var(--swell-bg-slide);
            overflow: hidden;
        }
        .swell-slide[aria-hidden="false"],
        .swell-slide.swell-active {
            display: block;
        }
        .swell-slide-content {
            position: relative;
            width: 100%;
            height: 100%;
            padding: 2.5rem 3rem;
            overflow: hidden;
        }
        .swell-slide-body {
            height: 100%;
        }
        """;

    private const string LayoutDefault = """
        .swell-layout-default .swell-slide-body {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }
        """;

    private const string LayoutCover = """
        .swell-layout-cover {
            background: var(--swell-bg-cover);
            color: var(--swell-text-on-dark);
            --swell-slide-number-color: rgba(255,255,255,0.45);
        }
        .swell-layout-cover .swell-cover-body {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            height: 100%;
            gap: 1rem;
        }
        .swell-layout-cover h1, .swell-layout-cover h2 {
            color: var(--swell-text-on-dark);
        }
        """;

    private const string LayoutCenter = """
        .swell-layout-center .swell-center-body {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            gap: 1rem;
        }
        """;

    private const string LayoutTwoCols = """
        .swell-layout-two-cols .swell-two-cols-body {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 2rem;
            align-items: start;
            height: 100%;
        }
        .swell-layout-two-cols .swell-col-left,
        .swell-layout-two-cols .swell-col-right {
            overflow: auto;
            height: 100%;
        }
        """;

    private const string LayoutImageRight = """
        .swell-layout-image-right .swell-image-right-body {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 2rem;
            align-items: center;
        }
        .swell-layout-image-right .swell-image-right-body img {
            width: 100%;
            height: 100%;
            object-fit: contain;
        }
        """;

    private const string LayoutImageLeft = """
        .swell-layout-image-left .swell-image-left-body {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 2rem;
            align-items: center;
        }
        .swell-layout-image-left .swell-image-left-body > :first-child { order: 2; }
        .swell-layout-image-left .swell-image-left-body > :last-child  { order: 1; }
        .swell-layout-image-left .swell-image-left-body img {
            width: 100%;
            height: 100%;
            object-fit: contain;
        }
        """;

    private const string LayoutSection = """
        .swell-layout-section {
            background: var(--swell-bg-section);
        }
        .swell-layout-section .swell-section-body {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            gap: 0.5rem;
        }
        .swell-layout-section h2 {
            font-size: clamp(2rem, 5vw, 3.5rem);
            color: var(--swell-accent-2);
            border-bottom: 4px solid var(--swell-accent);
            padding-bottom: 0.5rem;
        }
        """;

    private const string LayoutEnd = """
        .swell-layout-end .swell-end-body {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            gap: 1rem;
        }
        """;

    private const string SlideNumber = """
        .swell-slide-number {
            position: absolute;
            bottom: 0.75rem;
            right: 1rem;
            font-size: 0.75rem;
            color: var(--swell-slide-number-color);
            user-select: none;
        }
        """;

    private const string Transitions = """
        /* Fade */
        .swell-deck[data-transitioning] .swell-slide.swell-leaving {
            animation: swell-fade-out var(--swell-transition-duration) ease forwards;
        }
        .swell-deck[data-transitioning] .swell-slide.swell-entering {
            animation: swell-fade-in var(--swell-transition-duration) ease forwards;
        }
        @keyframes swell-fade-out { from { opacity: 1; } to { opacity: 0; } }
        @keyframes swell-fade-in  { from { opacity: 0; } to { opacity: 1; } }

        /* Slide left */
        .swell-deck[data-transition="slideleft"] .swell-slide.swell-leaving {
            animation: swell-slide-out-left var(--swell-transition-duration) ease forwards;
        }
        .swell-deck[data-transition="slideleft"] .swell-slide.swell-entering {
            animation: swell-slide-in-right var(--swell-transition-duration) ease forwards;
        }
        @keyframes swell-slide-out-left  { from { transform: translateX(0); } to { transform: translateX(-100%); } }
        @keyframes swell-slide-in-right  { from { transform: translateX(100%); } to { transform: translateX(0); } }

        /* Slide right (backwards) */
        .swell-deck[data-transition="slideright"] .swell-slide.swell-leaving {
            animation: swell-slide-out-right var(--swell-transition-duration) ease forwards;
        }
        .swell-deck[data-transition="slideright"] .swell-slide.swell-entering {
            animation: swell-slide-in-left var(--swell-transition-duration) ease forwards;
        }
        @keyframes swell-slide-out-right { from { transform: translateX(0); } to { transform: translateX(100%); } }
        @keyframes swell-slide-in-left   { from { transform: translateX(-100%); } to { transform: translateX(0); } }

        /* Slide up */
        .swell-deck[data-transition="slideup"] .swell-slide.swell-leaving {
            animation: swell-slide-out-up var(--swell-transition-duration) ease forwards;
        }
        .swell-deck[data-transition="slideup"] .swell-slide.swell-entering {
            animation: swell-slide-in-up var(--swell-transition-duration) ease forwards;
        }
        @keyframes swell-slide-out-up { from { transform: translateY(0); } to { transform: translateY(-100%); } }
        @keyframes swell-slide-in-up  { from { transform: translateY(100%); } to { transform: translateY(0); } }
        """;

    private const string OverviewGrid = """
        .swell-deck.swell-overview {
            overflow-y: auto;
        }
        .swell-deck.swell-overview .swell-slide {
            position: static;
            display: block;
            width: 300px;
            height: 170px;
            flex-shrink: 0;
            cursor: pointer;
            border: 3px solid transparent;
            border-radius: 4px;
            overflow: hidden;
            transform-origin: top left;
        }
        .swell-deck.swell-overview .swell-slide.swell-active {
            border-color: var(--swell-accent);
        }
        .swell-deck.swell-overview {
            display: flex;
            flex-wrap: wrap;
            gap: 1rem;
            padding: 2rem;
            align-items: flex-start;
            overflow-y: auto;
            overflow-x: hidden;
            max-height: 100vh;
        }
        .swell-deck.swell-overview .swell-slide-content {
            transform: scale(0.4);
            transform-origin: top left;
            width: 250%;
            height: 250%;
            pointer-events: none;
        }
        """;

    private const string ClickReveal = """
        .swell-click { visibility: hidden; }
        .swell-click.swell-click-visible { visibility: visible; }
        """;

    private const string CodeBlocks = """
        .swell-slide pre {
            background: var(--swell-code-bg);
            border: 1px solid var(--swell-code-border);
            border-radius: 6px;
            padding: 1rem 1.25rem;
            overflow-x: auto;
            max-height: 55%;
            font-size: 0.85em;
            line-height: 1.6;
        }
        .swell-slide code {
            font-family: "Cascadia Code", "Fira Code", "Consolas", monospace;
            font-size: inherit;
            color: var(--swell-code-text);
        }
        .swell-slide :not(pre) > code {
            background: var(--swell-code-bg);
            padding: 0.15em 0.35em;
            border-radius: 3px;
            font-size: 0.85em;
        }
        """;

    private const string Typography = """
        .swell-slide h1 { font-size: clamp(1.8rem, 4vw, 3rem); font-weight: 700; line-height: 1.1; }
        .swell-slide h2 { font-size: clamp(1.4rem, 3vw, 2.25rem); font-weight: 600; line-height: 1.2; }
        .swell-slide h3 { font-size: clamp(1.1rem, 2.5vw, 1.75rem); font-weight: 600; }
        .swell-slide p  { font-size: clamp(0.95rem, 1.8vw, 1.25rem); }
        .swell-slide ul, .swell-slide ol { padding-left: 1.5rem; font-size: clamp(0.95rem, 1.8vw, 1.2rem); }
        .swell-slide li { margin-bottom: 0.4em; }
        .swell-slide blockquote {
            border-left: 4px solid var(--swell-accent);
            padding-left: 1rem;
            margin: 1rem 0;
            color: var(--swell-text-muted);
            font-style: italic;
        }
        .swell-slide table { border-collapse: collapse; width: 100%; font-size: 0.9em; }
        .swell-slide th, .swell-slide td { border: 1px solid var(--swell-code-border); padding: 0.5rem 0.75rem; }
        .swell-slide th { background: var(--swell-code-bg); font-weight: 600; }
        """;

    private const string Navbar = """
        .swell-navbar {
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            display: flex;
            align-items: center;
            gap: 0.125rem;
            padding: 0.375rem 0.75rem;
            background: rgba(0, 0, 0, 0.75);
            backdrop-filter: blur(8px);
            -webkit-backdrop-filter: blur(8px);
            z-index: 9000;
            opacity: 0;
            transform: translateY(100%);
            transition: opacity 0.25s, transform 0.25s;
            pointer-events: none;
        }
        body:hover .swell-navbar,
        .swell-navbar:focus-within,
        .swell-navbar.swell-navbar-visible {
            opacity: 1;
            transform: translateY(0);
            pointer-events: auto;
        }
        .swell-navbar button {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 2rem;
            height: 2rem;
            border: none;
            border-radius: 0.375rem;
            background: transparent;
            color: rgba(255, 255, 255, 0.7);
            cursor: pointer;
            transition: background 0.15s, color 0.15s;
            padding: 0;
        }
        .swell-navbar button:hover {
            background: rgba(255, 255, 255, 0.15);
            color: #fff;
        }
        .swell-navbar button:active {
            background: rgba(255, 255, 255, 0.25);
        }
        .swell-navbar button.swell-navbar-active {
            color: var(--swell-accent);
        }
        .swell-navbar-counter {
            color: rgba(255, 255, 255, 0.7);
            font-size: 0.8rem;
            font-variant-numeric: tabular-nums;
            min-width: 3.5rem;
            text-align: center;
            user-select: none;
        }
        .swell-navbar-sep {
            width: 1px;
            height: 1.25rem;
            background: rgba(255, 255, 255, 0.2);
            margin: 0 0.25rem;
        }
        .swell-overview .swell-navbar { display: none; }
        """;

    private const string PrintStyles = """
        @media print {
            html, body { overflow: visible; height: auto; }
            .swell-deck { overflow: visible; max-height: none; background: none; }
            .swell-slide {
                position: static;
                display: block !important;
                width: 100%;
                height: auto;
                min-height: 100vh;
                page-break-after: always;
                break-after: page;
            }
            .swell-slide-number { display: none; }
            .swell-navbar { display: none; }
        }
        """;

    /// <inheritdoc />
    protected override Task RenderCoreAsync(RenderContext context) => Task.CompletedTask;
}
