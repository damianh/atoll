using Aspire.Hosting.ApplicationModel;

namespace Atoll.Hosting.Aspire;

/// <summary>
/// Represents an Atoll static site resource managed by Aspire.
/// Runs the <c>atoll dev</c> command as a managed executable resource.
/// </summary>
public sealed class AtollSiteResource(string name, string command, string workingDirectory, string siteDirectory)
    : ExecutableResource(name, command, workingDirectory)
{
    /// <summary>
    /// Gets the absolute path to the Atoll site directory.
    /// </summary>
    public string SiteDirectory { get; } = siteDirectory;

    /// <summary>
    /// Gets or sets a value indicating whether the site should write rendered
    /// output to the <c>dist/</c> directory after each rebuild cycle.
    /// </summary>
    public bool WriteDist { get; set; }
}
