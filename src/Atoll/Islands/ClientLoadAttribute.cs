using Atoll.Instructions;

namespace Atoll.Islands;

/// <summary>
/// Marks a component for immediate client-side hydration on page load.
/// </summary>
/// <remarks>
/// <para>
/// This is the Atoll equivalent of Astro's <c>client:load</c> directive. The component
/// is server-side rendered and then hydrated as soon as the page loads. Use this for
/// interactive components that must be ready immediately (e.g., navigation menus, above-the-fold
/// interactive content).
/// </para>
/// <para>
/// On the client, this directive uses <c>requestAnimationFrame</c> to schedule hydration
/// after the first paint.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [ClientLoad]
/// public sealed class InteractiveCounter : AtollComponent
/// {
///     [Parameter] public int InitialCount { get; set; }
///
///     protected override Task RenderCoreAsync(RenderContext context)
///     {
///         context.WriteHtml($"&lt;div class=\"counter\"&gt;{InitialCount}&lt;/div&gt;");
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ClientLoadAttribute : ClientDirectiveAttribute
{
    /// <summary>
    /// Initializes a new <see cref="ClientLoadAttribute"/>.
    /// </summary>
    public ClientLoadAttribute() : base(ClientDirectiveType.Load)
    {
    }
}
