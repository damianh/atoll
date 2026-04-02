namespace Atoll.Core.Components;

/// <summary>
/// Represents a lightweight functional component as a delegate.
/// This is a convenient alternative to implementing <see cref="IAtollComponent"/>
/// for simple components that don't need class-level state or lifecycle.
/// </summary>
/// <param name="context">The rendering context providing access to props, slots, and the render destination.</param>
/// <returns>A <see cref="Task"/> representing the asynchronous render operation.</returns>
/// <example>
/// <code>
/// ComponentDelegate card = async (ctx) =>
/// {
///     var title = ctx.GetProp&lt;string&gt;("title");
///     ctx.WriteHtml($"&lt;div class=\"card\"&gt;&lt;h2&gt;");
///     ctx.WriteText(title);
///     ctx.WriteHtml("&lt;/h2&gt;");
///     await ctx.RenderSlotAsync();
///     ctx.WriteHtml("&lt;/div&gt;");
/// };
/// </code>
/// </example>
public delegate Task ComponentDelegate(RenderContext context);
