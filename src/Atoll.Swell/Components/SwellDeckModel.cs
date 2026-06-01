namespace Atoll.Swell.Components;

/// <summary>
/// Model for the <c>SwellDeckTemplate.cshtml</c> Razor template.
/// </summary>
/// <param name="Src">URL of the slide deck page to embed.</param>
/// <param name="AspectRatioCss">CSS <c>aspect-ratio</c> value (e.g. <c>"16/9"</c>).</param>
/// <param name="Title">Accessible title for the iframe.</param>
public sealed record SwellDeckModel(string Src, string AspectRatioCss, string Title);
