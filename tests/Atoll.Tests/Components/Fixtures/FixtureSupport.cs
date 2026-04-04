using Atoll.Components;
using Atoll.Rendering;

namespace Atoll.Tests.Components.Fixtures;

/// <summary>Model for the GreetingSlice fixture.</summary>
public sealed record GreetingModel(string Name);

/// <summary>Simple C# component used from within Razor slice fixtures.</summary>
public sealed class SimpleInlineComponent : AtollComponent
{
    protected override Task RenderCoreAsync(RenderContext context)
    {
        WriteHtml("<span>inline</span>");
        return Task.CompletedTask;
    }
}
