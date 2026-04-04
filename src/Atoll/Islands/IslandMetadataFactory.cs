namespace Atoll.Islands;

/// <summary>
/// Creates <see cref="IslandMetadata"/> from any <see cref="IClientComponent"/> instance
/// by reading the component's <see cref="IClientComponent.ClientModuleUrl"/>,
/// <see cref="IClientComponent.ClientExportName"/>, and the
/// <see cref="ClientDirectiveAttribute"/> applied to the component type.
/// </summary>
/// <remarks>
/// This consolidates the metadata-construction logic that would otherwise be
/// duplicated across every island base class (<c>VanillaJsIsland</c>,
/// <c>WebComponentIsland</c>, <c>AtollIslandSlice</c>, etc.).
/// </remarks>
internal static class IslandMetadataFactory
{
    /// <summary>
    /// Creates <see cref="IslandMetadata"/> for the specified <see cref="IClientComponent"/>
    /// instance.
    /// </summary>
    /// <param name="component">The island component instance.</param>
    /// <returns>
    /// An <see cref="IslandMetadata"/> instance, or <c>null</c> if the component's
    /// concrete type does not have a <see cref="ClientDirectiveAttribute"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="component"/> is <c>null</c>.
    /// </exception>
    public static IslandMetadata? Create(IClientComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        var directiveInfo = DirectiveExtractor.GetDirective(component.GetType());
        if (directiveInfo is null)
        {
            return null;
        }

        return new IslandMetadata(component.ClientModuleUrl, directiveInfo.DirectiveType)
        {
            ComponentExport = component.ClientExportName,
            DirectiveValue = directiveInfo.Value,
            DisplayName = component.GetType().Name,
        };
    }
}
