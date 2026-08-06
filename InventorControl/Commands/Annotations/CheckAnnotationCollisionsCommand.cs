using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CheckAnnotationCollisionsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CheckAnnotationCollisionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "check_annotation_collisions";

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

        double clearance =
            Math.Max(
                0.0,
                AnnotationCollisionSupport
                    .GetOptionalDouble(
                        root,
                        "clearance",
                        0.15));

        double sheetMargin =
            Math.Max(
                0.0,
                AnnotationCollisionSupport
                    .GetOptionalDouble(
                        root,
                        "sheetMargin",
                        0.5));

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

        List<object> collisions =
            new();

        for (int firstIndex = 0;
             firstIndex < boxes.Count;
             firstIndex++)
        {
            AnnotationCollisionSupport.AnnotationBox first =
                boxes[firstIndex];

            if (first.Kind ==
                "view")
            {
                continue;
            }

            if (AnnotationCollisionSupport
                    .IsMovableAnnotationKind(
                        first.Kind) &&
                !AnnotationCollisionSupport
                    .IsInsideSheet(
                        first,
                        sheet,
                        sheetMargin))
            {
                collisions.Add(
                    new
                    {
                        type =
                            "outside_sheet",

                        first =
                            AnnotationCollisionSupport
                                .ToObject(
                                    first),

                        second =
                            (object?)null
                    });
            }

            for (int secondIndex =
                     firstIndex + 1;
                 secondIndex < boxes.Count;
                 secondIndex++)
            {
                AnnotationCollisionSupport.AnnotationBox second =
                    boxes[secondIndex];

                if (first.Kind ==
                        "view" &&
                    second.Kind ==
                        "view")
                {
                    continue;
                }

                if (!AnnotationCollisionSupport
                        .Intersects(
                            first,
                            second,
                            clearance))
                {
                    continue;
                }

                collisions.Add(
                    new
                    {
                        type =
                            $"{first.Kind}_vs_{second.Kind}",

                        first =
                            AnnotationCollisionSupport
                                .ToObject(
                                    first),

                        second =
                            AnnotationCollisionSupport
                                .ToObject(
                                    second)
                    });
            }
        }

        return AnnotationCollisionSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    clearance,

                    sheetMargin,

                    viewPadding,

                    collisionCount =
                        collisions.Count,

                    hasCollisions =
                        collisions.Count > 0,

                    collisions
                });
    }
}
