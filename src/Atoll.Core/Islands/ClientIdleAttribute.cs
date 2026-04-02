using Atoll.Core.Instructions;

namespace Atoll.Core.Islands;

/// <summary>
/// Marks a component for client-side hydration when the browser is idle.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>client:idle</c> directive. The component
/// is server-side rendered and hydrated when the browser's main thread is idle, using
/// <c>requestIdleCallback</c> (with a <c>setTimeout</c> fallback).
/// </para>
/// <para>
/// Use this for lower-priority interactive components that don't need to be ready
/// immediately (e.g., sidebar widgets, below-the-fold interactive content).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ClientIdle]
/// public sealed class SidebarWidget : AtollComponent
/// {
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml("&lt;aside&gt;Interactive sidebar content&lt;/aside&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ClientIdleAttribute : ClientDirectiveAttribute
{
    /// <summary>
    /// Initializes a new <see cref="ClientIdleAttribute"/>.
    /// </summary>
    public ClientIdleAttribute() : base(ClientDirectiveType.Idle)
    {
    }
}
