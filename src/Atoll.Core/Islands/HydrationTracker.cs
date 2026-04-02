using Atoll.Core.Instructions;

namespace Atoll.Core.Islands;

/// <summary>
/// Tracks which hydration scripts have been emitted during page rendering to prevent
/// duplicate script injection. Each island type and directive handler script is tracked
/// by a unique key.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's hydration script deduplication in
/// <c>runtime/server/render/head.ts</c>. A page with multiple islands should only
/// include:
/// </para>
/// <list type="bullet">
/// <item>One copy of the <c>atoll-island.js</c> bootstrap script</item>
/// <item>One copy of each directive handler script (e.g., load, idle, visible, media)</item>
/// </list>
/// <para>
/// The tracker works with the existing <see cref="InstructionProcessor"/> for instruction-based
/// deduplication, but also provides standalone tracking for scenarios where instructions
/// are not used (e.g., direct script injection into the HTML output).
/// </para>
/// </remarks>
public sealed class HydrationTracker
{
    private readonly HashSet<string> _emittedScripts = [];

    /// <summary>
    /// The key prefix for directive handler scripts.
    /// </summary>
    private const string DirectiveKeyPrefix = "atoll:directive:";

    /// <summary>
    /// Attempts to mark the bootstrap script as emitted.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this is the first time the bootstrap script is being emitted;
    /// <c>false</c> if it was already emitted.
    /// </returns>
    public bool TryEmitBootstrap()
    {
        return _emittedScripts.Add(HydrationScriptGenerator.BootstrapScriptKey);
    }

    /// <summary>
    /// Attempts to mark a directive handler script as emitted.
    /// </summary>
    /// <param name="directiveType">The directive type.</param>
    /// <returns>
    /// <c>true</c> if this is the first time the directive script is being emitted;
    /// <c>false</c> if it was already emitted.
    /// </returns>
    public bool TryEmitDirective(ClientDirectiveType directiveType)
    {
        return _emittedScripts.Add(GetDirectiveKey(directiveType));
    }

    /// <summary>
    /// Gets a value indicating whether the bootstrap script has been emitted.
    /// </summary>
    public bool HasBootstrap => _emittedScripts.Contains(HydrationScriptGenerator.BootstrapScriptKey);

    /// <summary>
    /// Gets a value indicating whether the specified directive handler script has been emitted.
    /// </summary>
    /// <param name="directiveType">The directive type to check.</param>
    /// <returns><c>true</c> if the directive script has been emitted; otherwise, <c>false</c>.</returns>
    public bool HasDirective(ClientDirectiveType directiveType)
    {
        return _emittedScripts.Contains(GetDirectiveKey(directiveType));
    }

    /// <summary>
    /// Gets the total number of scripts that have been emitted.
    /// </summary>
    public int EmittedCount => _emittedScripts.Count;

    /// <summary>
    /// Gets the <see cref="ScriptInstruction"/> instances required for the specified
    /// directive type, skipping any that have already been emitted. This method both
    /// checks and marks scripts as emitted atomically.
    /// </summary>
    /// <param name="directiveType">The directive type for the island.</param>
    /// <param name="islandScriptUrl">The URL of the <c>atoll-island.js</c> script.</param>
    /// <param name="directiveScriptUrl">The URL of the directive handler script.</param>
    /// <returns>A list of script instructions that should be emitted (may be empty if all were already emitted).</returns>
    public IReadOnlyList<ScriptInstruction> GetRequiredScripts(
        ClientDirectiveType directiveType,
        string islandScriptUrl,
        string directiveScriptUrl)
    {
        ArgumentNullException.ThrowIfNull(islandScriptUrl);
        ArgumentNullException.ThrowIfNull(directiveScriptUrl);

        var scripts = new List<ScriptInstruction>();

        if (TryEmitBootstrap())
        {
            scripts.Add(ScriptInstruction.Module(islandScriptUrl));
        }

        if (TryEmitDirective(directiveType))
        {
            scripts.Add(ScriptInstruction.Module(directiveScriptUrl));
        }

        return scripts;
    }

    /// <summary>
    /// Gets the <see cref="ScriptInstruction"/> instances required for the specified
    /// directive type using inline scripts, skipping any that have already been emitted.
    /// </summary>
    /// <param name="directiveType">The directive type for the island.</param>
    /// <returns>A list of script instructions that should be emitted (may be empty if all were already emitted).</returns>
    public IReadOnlyList<ScriptInstruction> GetRequiredInlineScripts(ClientDirectiveType directiveType)
    {
        var scripts = new List<ScriptInstruction>();

        if (TryEmitBootstrap())
        {
            var bootstrapHtml = HydrationScriptGenerator.GenerateBootstrapScript(null);
            scripts.Add(new ScriptInstruction(
                HydrationScriptGenerator.BootstrapScriptKey,
                Rendering.RenderFragment.FromHtml(bootstrapHtml))
            {
                IsInline = true,
            });
        }

        if (TryEmitDirective(directiveType))
        {
            // For inline mode, the directives script is included in the bootstrap
            // so we don't need a separate script instruction. However, if the
            // directive needs to be loaded separately, add it here.
            var directiveKey = GetDirectiveKey(directiveType);
            var directiveName = GetDirectiveName(directiveType);
            scripts.Add(new ScriptInstruction(
                directiveKey,
                Rendering.RenderFragment.FromHtml(
                    $"<script type=\"module\">/* atoll:{directiveName} directive loaded */</script>"))
            {
                IsInline = true,
            });
        }

        return scripts;
    }

    /// <summary>
    /// Adds all required scripts for the specified directive type to the
    /// <see cref="InstructionProcessor"/>. Scripts already added to the processor
    /// (or previously emitted via this tracker) are skipped.
    /// </summary>
    /// <param name="processor">The instruction processor to add scripts to.</param>
    /// <param name="directiveType">The directive type for the island.</param>
    /// <param name="islandScriptUrl">The URL of the <c>atoll-island.js</c> script.</param>
    /// <param name="directiveScriptUrl">The URL of the directive handler script.</param>
    public void AddToProcessor(
        InstructionProcessor processor,
        ClientDirectiveType directiveType,
        string islandScriptUrl,
        string directiveScriptUrl)
    {
        ArgumentNullException.ThrowIfNull(processor);
        ArgumentNullException.ThrowIfNull(islandScriptUrl);
        ArgumentNullException.ThrowIfNull(directiveScriptUrl);

        var scripts = GetRequiredScripts(directiveType, islandScriptUrl, directiveScriptUrl);
        foreach (var script in scripts)
        {
            processor.Add(script);
        }
    }

    /// <summary>
    /// Resets the tracker, clearing all emitted script tracking.
    /// </summary>
    public void Reset()
    {
        _emittedScripts.Clear();
    }

    private static string GetDirectiveKey(ClientDirectiveType directiveType)
    {
        return DirectiveKeyPrefix + GetDirectiveName(directiveType);
    }

    private static string GetDirectiveName(ClientDirectiveType directiveType)
    {
        return directiveType switch
        {
            ClientDirectiveType.Load => "load",
            ClientDirectiveType.Idle => "idle",
            ClientDirectiveType.Visible => "visible",
            ClientDirectiveType.Media => "media",
            _ => directiveType.ToString().ToLowerInvariant(),
        };
    }
}
