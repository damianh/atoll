using Atoll.Css;

namespace Atoll.Tests.Components.Fixtures;

// Companion partial class to apply [Styles] attribute to the source-generated proxy type.
// This is necessary because @attribute in .cshtml applies to the Razor-generated partial class
// (the ExecuteAsync implementation), NOT to the source-generated proxy class that external code
// references. Applying the attribute here ensures StyleScoper and CssAggregator can discover it.
[Styles(".styled-slice { color: blue; }")]
public partial class StyledSlice;
