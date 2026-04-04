using Atoll.Components;
using Atoll.Routing;
using Atoll.Samples.Blog.Layouts;

namespace Atoll.Samples.Blog.Pages;

// Companion partial class to apply [PageRoute] and [Layout] to the source-generated proxy type.
// @attribute [...] in .cshtml applies to the Razor implementation partial (NOT the proxy).
// These attributes must be on the proxy for RouteDiscovery and LayoutResolver to work correctly.
[PageRoute("/about-razor")]
[Layout(typeof(BlogLayout))]
public partial class AboutRazorPage;
