namespace Atoll.Lagoon.OpenGraph;

/// <summary>
/// The result of an OG image generation operation.
/// </summary>
/// <param name="ImageCount">The number of images generated.</param>
/// <param name="OutputDirectory">The directory where images were written.</param>
/// <param name="Elapsed">The time taken to generate and write all images.</param>
public sealed record OgImageGenerationResult(int ImageCount, string OutputDirectory, TimeSpan Elapsed);
