namespace Atoll.Core.Islands;

/// <summary>
/// Detects cyclic references during prop serialization by tracking objects
/// that are currently being serialized.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>WeakSet</c>-based cycle detection
/// in <c>runtime/server/serialize.ts</c>. When serializing complex object graphs
/// for island props, cyclic references would cause infinite recursion. The
/// <see cref="CycleDetector"/> tracks the current serialization path and throws
/// a descriptive error when a cycle is detected.
/// </para>
/// <para>
/// Usage pattern:
/// <code>
/// using var guard = detector.Enter(obj, "ComponentName");
/// // serialize obj's properties...
/// // guard.Dispose() removes obj from tracking
/// </code>
/// </para>
/// </remarks>
internal sealed class CycleDetector
{
    private readonly HashSet<object> _seen = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Enters a new object into the cycle detection tracker. If the object
    /// is already being tracked, a cycle has been detected and an exception is thrown.
    /// </summary>
    /// <param name="value">The object being serialized.</param>
    /// <param name="componentDisplayName">
    /// The display name of the component, used in error messages.
    /// </param>
    /// <returns>
    /// An <see cref="IDisposable"/> guard that removes the object from tracking
    /// when disposed. Use in a <c>using</c> statement.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a cyclic reference is detected.
    /// </exception>
    public CycleGuard Enter(object value, string componentDisplayName)
    {
        if (!_seen.Add(value))
        {
            throw new InvalidOperationException(
                $"Cyclic reference detected while serializing props for <{componentDisplayName}>. " +
                "Cyclic references cannot be safely serialized for client-side usage. " +
                "Please remove the cyclic reference.");
        }

        return new CycleGuard(this, value);
    }

    private void Leave(object value)
    {
        _seen.Remove(value);
    }

    /// <summary>
    /// A disposable guard that removes an object from cycle detection tracking
    /// when disposed.
    /// </summary>
    internal readonly struct CycleGuard : IDisposable
    {
        private readonly CycleDetector _detector;
        private readonly object _value;

        internal CycleGuard(CycleDetector detector, object value)
        {
            _detector = detector;
            _value = value;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _detector.Leave(_value);
        }
    }
}
