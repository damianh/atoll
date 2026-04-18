using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Atoll.Hosting.Aspire;

namespace Atoll.Hosting.Aspire.Tests;

public sealed class AtollSiteResourceTests
{
    [Fact]
    public void AddAtollSite_CreatesResourceWithCorrectName()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddAtollSite("my-site", ".");

        var resource = builder.Resources.OfType<AtollSiteResource>().Single();

        resource.Name.ShouldBe("my-site");
    }

    [Fact]
    public void AddAtollSite_SiteDirectoryIsAbsolutePath()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddAtollSite("my-site", ".");

        var resource = builder.Resources.OfType<AtollSiteResource>().Single();

        Path.IsPathFullyQualified(resource.SiteDirectory).ShouldBeTrue();
    }

    [Fact]
    public void AddAtollSite_HasHttpEndpointOnDefaultPort()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddAtollSite("my-site", ".");

        var resource = builder.Resources.OfType<AtollSiteResource>().Single();
        var endpoint = resource.Annotations
            .OfType<EndpointAnnotation>()
            .Single(e => e.Name == "http");

        endpoint.TargetPort.ShouldBe(4321);
        endpoint.UriScheme.ShouldBe("http");
    }

    [Fact]
    public void AddAtollSite_HasHealthCheckAnnotation()
    {
        var builder = DistributedApplication.CreateBuilder();
        builder.AddAtollSite("my-site", ".");

        var resource = builder.Resources.OfType<AtollSiteResource>().Single();

        resource.Annotations.OfType<HealthCheckAnnotation>().ShouldNotBeEmpty();
    }

    [Fact]
    public void WithWriteDist_SetsFlag()
    {
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddAtollSite("my-site", ".")
            .WithWriteDist();

        resourceBuilder.Resource.WriteDist.ShouldBeTrue();
    }

    [Fact]
    public void WithWriteDist_NotSetByDefault()
    {
        var builder = DistributedApplication.CreateBuilder();
        var resourceBuilder = builder.AddAtollSite("my-site", ".");

        resourceBuilder.Resource.WriteDist.ShouldBeFalse();
    }
}
