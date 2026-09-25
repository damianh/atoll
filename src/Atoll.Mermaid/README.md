# Atoll.Mermaid

Mermaid diagram support for Atoll sites.

Fenced ` ```mermaid ` blocks are rendered as `<pre class="mermaid">` at build time and turned into SVG in the browser by a pinned copy of Mermaid that ships in this package. It is served from your site under `/scripts/atoll-mermaid/<version>/`, so nothing is loaded from a CDN, and Mermaid is only downloaded on pages that contain a diagram.

The bundled files are in `Islands/Assets/vendor/mermaid/`. `manifest.json` records the npm tarball integrity and a SHA-256 hash for every file, and the tests check the bundled files against it. To upgrade, run `eng/update-mermaid.ps1 -Version <x.y.z>` from the repository root. See the Mermaid docs page for details.
