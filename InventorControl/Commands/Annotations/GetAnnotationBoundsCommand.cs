using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetAnnotationBoundsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetAnnotationBoundsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_annotation_bounds";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            AnnotationCollisionSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return AnnotationCollisionSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!AnnotationCollisionSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return AnnotationCollisionSupport
                .CreateError(
                    sheetNameError);
        }

        double viewPadding =
            Math.Max(
                0.0,
                AnnotationCollisionSupport
                    .GetOptionalDouble(
                        root,
                        "viewPadding",
                        0.2));

        Sheet? sheet =
            AnnotationCollisionSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return AnnotationCollisionSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        List<AnnotationCollisionSupport.AnnotationBox> boxes =
            AnnotationCollisionSupport
                .ReadAllBoxes(
                    sheet,
                    viewPadding);

        return AnnotationCollisionSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    sheetWidth =
                        sheet.Width,

                    sheetHeight =
                        sheet.Height,

                    viewPadding,

                    itemCount =
                        boxes.Count,

                    items =
                        boxes.Select(
                            AnnotationCollisionSupport
                                .ToObject)
                });
    }
}
