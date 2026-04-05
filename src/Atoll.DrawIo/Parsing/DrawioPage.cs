using Atoll.DrawIo.Model;

namespace Atoll.DrawIo.Parsing;

/// <summary>
/// Represents a single diagram page within a draw.io file.
/// Corresponds to a <c>&lt;diagram&gt;</c> element in the <c>&lt;mxfile&gt;</c> envelope.
/// </summary>
public sealed class DrawioPage
{
    /// <summary>
    /// Initializes a new instance of <see cref="DrawioPage"/>.
    /// </summary>
    /// <param name="name">The display name of the page.</param>
    /// <param name="id">The unique identifier of the page.</param>
    /// <param name="model">The parsed mxGraph model for this page.</param>
    public DrawioPage(string name, string id, MxGraphModel model)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(model);
        Name = name;
        Id = id;
        Model = model;
    }

    /// <summary>Gets the display name of this page.</summary>
    public string Name { get; }

    /// <summary>Gets the unique identifier of this page.</summary>
    public string Id { get; }

    /// <summary>Gets the parsed mxGraph model for this page.</summary>
    public MxGraphModel Model { get; }
}
