namespace Atoll.Cli.Commands.Dev;

/// <summary>
/// Classifies the kind of file change detected by <see cref="DevFileWatcher"/>.
/// </summary>
internal enum FileChangeKind
{
    /// <summary>
    /// Only content files (<c>.md</c>) changed. The project assembly can be reused;
    /// only the <c>CollectionQuery</c> needs to be rebuilt.
    /// </summary>
    ContentOnly,

    /// <summary>
    /// Source code (<c>.cs</c>) or configuration (<c>atoll.json</c>) changed.
    /// A full <c>dotnet build</c> + assembly reload is required.
    /// </summary>
    CodeChange,
}

/// <summary>
/// Watches a project directory for <c>.cs</c>, <c>.md</c>, and <c>atoll.json</c>
/// changes. Debounces rapid-fire filesystem events and classifies each batch into
/// a <see cref="FileChangeKind"/> before raising <see cref="OnChange"/>.
/// </summary>
internal sealed class DevFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _csWatcher;
    private readonly FileSystemWatcher _mdWatcher;
    private readonly FileSystemWatcher _configWatcher;
    private readonly System.Threading.Timer _debounceTimer;
    private readonly int _debounceMs;
    private readonly object _lock = new();

    // Accumulated change kind within the current debounce window.
    private FileChangeKind _pendingKind;
    private bool _pendingChange;

    /// <summary>
    /// Raised once per debounce window when one or more tracked files change.
    /// </summary>
    public event Func<FileChangeKind, Task>? OnChange;

    /// <summary>
    /// Initializes a new <see cref="DevFileWatcher"/> for the given project root
    /// with the default debounce window of 300 ms.
    /// </summary>
    /// <param name="projectRoot">The root directory to watch.</param>
    public DevFileWatcher(string projectRoot)
        : this(projectRoot, 300)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="DevFileWatcher"/> for the given project root.
    /// </summary>
    /// <param name="projectRoot">The root directory to watch.</param>
    /// <param name="debounceMs">
    /// Milliseconds to wait after the last change event before raising <see cref="OnChange"/>.
    /// </param>
    public DevFileWatcher(string projectRoot, int debounceMs)
    {
        ArgumentNullException.ThrowIfNull(projectRoot);

        _debounceMs = debounceMs;
        _debounceTimer = new System.Threading.Timer(OnDebounceElapsed, null,
            Timeout.Infinite, Timeout.Infinite);

        _csWatcher = CreateWatcher(projectRoot, "*.cs");
        _mdWatcher = CreateWatcher(projectRoot, "*.md");
        _configWatcher = CreateWatcher(projectRoot, "atoll.json");
    }

    /// <summary>
    /// Starts watching for file changes.
    /// </summary>
    public void Start()
    {
        _csWatcher.EnableRaisingEvents = true;
        _mdWatcher.EnableRaisingEvents = true;
        _configWatcher.EnableRaisingEvents = true;
        Console.WriteLine("  Watching for changes...");
    }

    /// <summary>
    /// Stops watching for file changes without disposing resources.
    /// </summary>
    public void Stop()
    {
        _csWatcher.EnableRaisingEvents = false;
        _mdWatcher.EnableRaisingEvents = false;
        _configWatcher.EnableRaisingEvents = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _debounceTimer.Dispose();
        _csWatcher.Dispose();
        _mdWatcher.Dispose();
        _configWatcher.Dispose();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private FileSystemWatcher CreateWatcher(string root, string filter)
    {
        var watcher = new FileSystemWatcher(root)
        {
            Filter = filter,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
        };

        var kind = filter is "*.cs" or "atoll.json"
            ? FileChangeKind.CodeChange
            : FileChangeKind.ContentOnly;

        watcher.Changed += (_, e) => HandleEvent(e.FullPath, kind);
        watcher.Created += (_, e) => HandleEvent(e.FullPath, kind);
        watcher.Deleted += (_, e) => HandleEvent(e.FullPath, kind);
        watcher.Renamed += (_, e) => HandleEvent(e.FullPath, kind);

        return watcher;
    }

    private void HandleEvent(string fullPath, FileChangeKind kind)
    {
        // Ignore files under bin/ or obj/ directories.
        if (IsInBuildOutput(fullPath))
        {
            return;
        }

        lock (_lock)
        {
            // Escalate: CodeChange beats ContentOnly.
            if (!_pendingChange || kind == FileChangeKind.CodeChange)
            {
                _pendingKind = kind;
            }
            _pendingChange = true;

            // Reset the debounce timer.
            _debounceTimer.Change(_debounceMs, Timeout.Infinite);
        }
    }

    private void OnDebounceElapsed(object? _)
    {
        FileChangeKind kind;
        lock (_lock)
        {
            if (!_pendingChange)
            {
                return;
            }
            kind = _pendingKind;
            _pendingChange = false;
        }

        Console.WriteLine($"  Change detected ({kind}): rebuilding...");
        var handler = OnChange;
        if (handler is not null)
        {
            // Invoke async handlers. Fire-and-forget from the timer callback;
            // callers are responsible for exception handling within their delegates.
            _ = Task.Run(() => handler(kind));
        }
    }

    private static bool IsInBuildOutput(string fullPath)
    {
        // Normalize separators for reliable substring matching.
        var normalized = fullPath.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }
}
