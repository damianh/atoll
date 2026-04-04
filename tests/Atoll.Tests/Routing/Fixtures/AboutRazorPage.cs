using Atoll.Routing;

namespace Atoll.Tests.Routing.Fixtures;

// Companion partial class to apply [PageRoute] to the source-generated proxy type.
// @attribute [PageRoute(...)] in .cshtml applies to the Razor implementation partial,
// NOT to the source-generated proxy. This companion file ensures the proxy has the
// [PageRoute] attribute so RouteDiscovery.DiscoverRoutesFromAttributes() can find it.
[PageRoute("/razor-about")]
public partial class AboutRazorPage;
