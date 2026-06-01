using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;

namespace Atoll.Swell.Export;

/// <summary>
/// Exports a Swell slide deck to PPTX format using Playwright screenshots + DocumentFormat.OpenXml.
/// Each slide is screenshotted as a PNG and inserted as a full-bleed image in a PPTX slide.
/// Presenter notes are added as PPTX speaker notes. Fidelity loss is acceptable.
/// </summary>
public sealed class PptxExporter
{
    private const long EmuPerPixel = 914400L / 96L; // 96 DPI → EMU

    /// <summary>
    /// Minimum valid slide ID in OOXML. Slide IDs must be >= 256 per the spec.
    /// </summary>
    private const uint MinSlideId = 256;

    /// <summary>
    /// Exports the deck to a <c>.pptx</c> file.
    /// </summary>
    /// <param name="options">Export configuration.</param>
    /// <param name="notes">Optional per-slide presenter notes (index → note text).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path of the generated PPTX file.</returns>
    public async Task<string> ExportAsync(
        ExportOptions options,
        IReadOnlyList<string>? notes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ExportHelper.ValidateSlideCount(options);

        var outputFile = options.OutputPath.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)
            ? options.OutputPath
            : options.OutputPath + ".pptx";

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputFile) ?? ".");

        var height = ExportHelper.ResolveHeight(options.AspectRatio);
        var slideScreenshots = await ExportHelper.CaptureScreenshotsAsync(options, height, cancellationToken);

        BuildPptx(outputFile, slideScreenshots, notes ?? [], ExportHelper.BaseWidth, height);

        return outputFile;
    }

    private static void BuildPptx(
        string outputFile,
        List<byte[]> screenshots,
        IReadOnlyList<string> notes,
        int widthPx,
        int heightPx)
    {
        using var presentation = PresentationDocument.Create(outputFile, PresentationDocumentType.Presentation);

        var presentationPart = presentation.AddPresentationPart();
        presentationPart.Presentation = new P.Presentation();

        var slideSizeEmuW = (long)widthPx * EmuPerPixel;
        var slideSizeEmuH = (long)heightPx * EmuPerPixel;

        // Use custom slide size with explicit EMU dimensions — works for any aspect ratio.
        presentationPart.Presentation.AppendChild(new P.SlideSize
        {
            Cx = (Int32Value)(int)(slideSizeEmuW),
            Cy = (Int32Value)(int)(slideSizeEmuH),
        });

        var slideIdList = new P.SlideIdList();
        presentationPart.Presentation.AppendChild(slideIdList);

        for (var i = 0; i < screenshots.Count; i++)
        {
            var note = i < notes.Count ? notes[i] : "";
            AddSlide(presentationPart, slideIdList, screenshots[i], note, MinSlideId + (uint)i, slideSizeEmuW, slideSizeEmuH);
        }

        presentationPart.Presentation.Save();
    }

    private static void AddSlide(
        PresentationPart presentationPart,
        P.SlideIdList slideIdList,
        byte[] imageBytes,
        string notes,
        uint slideId,
        long widthEmu,
        long heightEmu)
    {
        var slidePart = presentationPart.AddNewPart<SlidePart>();
        var slide = new P.Slide();
        slidePart.Slide = slide;

        var cSld = new P.CommonSlideData();
        var spTree = new P.ShapeTree(
            new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()),
            new P.GroupShapeProperties(new A.TransformGroup()));
        cSld.AppendChild(spTree);
        slide.AppendChild(cSld);

        // Add the screenshot as a full-bleed image.
        var imagePart = slidePart.AddImagePart(ImagePartType.Png);
        using var ms = new MemoryStream(imageBytes);
        imagePart.FeedData(ms);
        var imageRid = slidePart.GetIdOfPart(imagePart);

        var pic = new A.Pictures.Picture(
            new A.Pictures.NonVisualPictureProperties(
                new A.Pictures.NonVisualDrawingProperties { Id = 2U, Name = "Screenshot" },
                new A.Pictures.NonVisualPictureDrawingProperties(new A.PictureLocks { NoChangeAspect = true }),
                new P.ApplicationNonVisualDrawingProperties()),
            new A.Pictures.BlipFill(
                new A.Blip { Embed = imageRid },
                new A.Stretch(new A.FillRectangle())),
            new A.Pictures.ShapeProperties(
                new A.Transform2D(
                    new A.Offset { X = 0L, Y = 0L },
                    new A.Extents { Cx = widthEmu, Cy = heightEmu }),
                new A.PresetGeometry { Preset = A.ShapeTypeValues.Rectangle }
            ));

        spTree.AppendChild(pic);

        // Add speaker notes if present.
        if (!string.IsNullOrWhiteSpace(notes))
        {
            var notesSlidePart = slidePart.AddNewPart<NotesSlidePart>();
            notesSlidePart.NotesSlide = new P.NotesSlide(
                new P.CommonSlideData(
                    new P.ShapeTree(
                        new P.NonVisualGroupShapeProperties(
                            new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                            new P.NonVisualGroupShapeDrawingProperties(),
                            new P.ApplicationNonVisualDrawingProperties()),
                        new P.GroupShapeProperties(new A.TransformGroup()),
                        new P.Shape(
                            new P.NonVisualShapeProperties(
                                new P.NonVisualDrawingProperties { Id = 2U, Name = "Notes Placeholder" },
                                new P.NonVisualShapeDrawingProperties(new A.ShapeLocks { NoChangeArrowheads = true }),
                                new P.ApplicationNonVisualDrawingProperties(new P.PlaceholderShape { Type = P.PlaceholderValues.Body, Index = 1U })),
                            new P.ShapeProperties(),
                            new P.TextBody(
                                new A.BodyProperties(),
                                new A.ListStyle(),
                                new A.Paragraph(new A.Run(new A.Text(notes))))))));

            notesSlidePart.NotesSlide.Save();
        }

        slidePart.Slide.Save();

        // Register slide in the presentation.
        slideIdList.AppendChild(new P.SlideId
        {
            Id = slideId,
            RelationshipId = presentationPart.GetIdOfPart(slidePart),
        });
    }
}
