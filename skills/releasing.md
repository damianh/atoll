# Releasing

## Overview

This skill handles version bump decisions, triggering releases via GitHub Actions, and writing curated release notes. Use when asked to release, bump a version, or prepare a release.

## How releases work

Releases are triggered via the `release.yml` GitHub Actions workflow dispatch. The workflow:

1. Validates the version format
2. Creates and pushes a git tag (`v{version}`)
3. Builds in Release mode
4. Packs NuGet packages (9 src projects + 1 templates project)
5. Pushes to GitHub Packages (always) and NuGet.org (when `push_to_nuget` is true)
6. Creates a GitHub Release with placeholder auto-generated notes

MinVer derives the assembly/package version from the git tag — no manual version edits needed.

The workflow creates the GitHub Release with `generate_release_notes: true`, which only produces a raw list of PR titles. **You must replace these with curated release notes** (see below).

## Deciding the version bump

Analyse commits since the last release tag to determine the bump. Use `git log <last-tag>..HEAD --oneline` to review changes.

| Bump | When | Example |
|------|------|---------|
| **Major** (`x.0.0`) | Breaking API changes: removed/renamed public types, changed method signatures, removed features | `1.0.0 → 2.0.0` |
| **Minor** (`x.y.0`) | New features, new public API surface, significant enhancements that are backward-compatible | `0.2.0 → 0.3.0` |
| **Patch** (`x.y.z`) | Bug fixes, performance improvements, internal refactors with no public API change | `0.2.0 → 0.2.1` |
| **Prerelease** (`x.y.z-beta.N`) | Testing a release before committing to stable | `0.3.0-beta.1` |

When the current major version is `0`, treat minor bumps as potentially breaking (per SemVer spec for 0.x).

Present the recommendation to the user with reasoning before proceeding.

## Triggering the release

Once the user confirms the version:

1. **Verify CI is green on the target branch** — check with `gh run list --workflow=ci.yml --branch=main --limit=1`.
2. **Check for unreleased changes** — `git log <last-tag>..HEAD --oneline` should show commits.
3. **Trigger the workflow** — use:
   ```
   gh workflow run release.yml -f version=<version> -f push_to_nuget=<true|false>
   ```
   - Default `push_to_nuget` to `false` unless the user explicitly says to publish to NuGet.org.
4. **Monitor the run** — use `gh run list --workflow=release.yml --limit=1` and `gh run watch` to track progress.
5. **Write release notes** — once the workflow completes and the GitHub Release exists, update it with curated release notes (see below).

## Writing release notes

After the release workflow completes, replace the auto-generated notes on the GitHub Release with curated content.

1. **Gather changes** — review commits and merged PRs since the previous release tag:
   ```
   git log <prev-tag>..v<version> --oneline
   ```
2. **Draft the notes** — organise changes into sections. Use only the sections that apply:
   ```markdown
   ## What's New
   - **Feature name** — brief description of what it does and why it matters.

   ## Improvements
   - **Area** — what changed and the user-visible effect.

   ## Bug Fixes
   - **Fix description** — what was broken and how it's resolved.

   ## Breaking Changes
   - **What changed** — migration steps or workaround.
   ```
   Guidelines:
   - Write from the user's perspective, not the developer's. Focus on what changed for consumers of the packages.
   - Merge related commits/PRs into single bullet points — don't list every commit.
   - Skip purely internal changes (CI tweaks, test fixes, refactors) unless they affect users.
   - Keep each bullet to 1–2 sentences.
3. **Present the draft to the user** for approval before publishing.
4. **Update the release** — use a heredoc to preserve formatting:
   ```
   gh release edit v<version> --notes "$(cat <<'EOF'
   <release notes content>
   EOF
   )"
   ```

## Finding the latest release

Use `git tag --list 'v*' --sort=-v:refname | head -1` to find the latest release tag.
