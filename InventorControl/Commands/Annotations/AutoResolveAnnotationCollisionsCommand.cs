using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class AutoResolveAnnotationCollisionsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public AutoResolveAnnotationCollisionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "auto_resolve_annotation_collisions";

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

        double step =
            Math.Max(
                0.2,
                AnnotationCollisionSupport
                    .GetOptionalDouble(
                        root,
                        "step",
                        0.75));

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

        int maxAttempts =
            Math.Max(
                1,
                AnnotationCollisionSupport
                    .GetOptionalInt32(
                        root,
                        "maxAttempts",
                        12));

        bool dryRun =
            AnnotationCollisionSupport
                .GetOptionalBoolean(
                    root,
                    "dryRun",
                    true);

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

        List<object> actions =
            new();

        GeneralDimensions dimensions =
            sheet
                .DrawingDimensions
                .GeneralDimensions;

        for (int index = 1;
             index <= dimensions.Count;
             index++)
        {
            dynamic dimension =
                dimensions[index];

            Box2d currentBox;

            try
            {
                currentBox =
                    (Box2d)dimension
                        .Text
                        .RangeBox;
            }
            catch
            {
                actions.Add(
                    new
                    {
                        kind =
                            "dimension",

                        index,

                        status =
                            "skipped",

                        reason =
                            "Не удалось получить RangeBox."
                    });

                continue;
            }

            Point2d currentOrigin =
                (Point2d)dimension
                    .Text
                    .Origin;

            bool moved =
                false;

            object? plannedTarget =
                null;

            for (int attempt = 1;
                 attempt <= maxAttempts;
                 attempt++)
            {
                double direction =
                    attempt % 2 == 1
                        ? 1.0
                        : -1.0;

                double distance =
                    ((attempt + 1) / 2) *
                    step *
                    direction;

                Point2d target =
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            currentOrigin.X,
                            currentOrigin.Y + distance);

                AnnotationCollisionSupport.AnnotationBox candidate =
                    new(
                        $"dimension:{index}",
                        "dimension",
                        AnnotationCollisionSupport
                            .ReadDimensionText(
                                dimension),
                        currentBox.MinPoint.X,
                        currentBox.MinPoint.Y + distance,
                        currentBox.MaxPoint.X,
                        currentBox.MaxPoint.Y + distance);

                List<AnnotationCollisionSupport.AnnotationBox> boxes =
                    AnnotationCollisionSupport
                        .ReadAllBoxes(
                            sheet,
                            viewPadding);

                bool collision =
                    false;

                foreach (
                    AnnotationCollisionSupport.AnnotationBox box
                    in boxes)
                {
                    if (box.Id == candidate.Id)
                    {
                        continue;
                    }

                    if (AnnotationCollisionSupport
                            .Intersects(
                                candidate,
                                box,
                                clearance))
                    {
                        collision =
                            true;

                        break;
                    }
                }

                bool inside =
                    AnnotationCollisionSupport
                        .IsInsideSheet(
                            candidate,
                            sheet,
                            sheetMargin);

                if (collision ||
                    !inside)
                {
                    continue;
                }

                plannedTarget =
                    new
                    {
                        x =
                            target.X,

                        y =
                            target.Y
                    };

                if (!dryRun)
                {
                    dimension.Text.Origin =
                        target;

                    dimension.CenterText();
                }

                moved =
                    true;

                break;
            }

            actions.Add(
                new
                {
                    kind =
                        "dimension",

                    index,

                    text =
                        AnnotationCollisionSupport
                            .ReadDimensionText(
                                dimension),

                    previous =
                        new
                        {
                            x =
                                currentOrigin.X,

                            y =
                                currentOrigin.Y
                        },

                    target =
                        plannedTarget,

                    status =
                        moved
                            ? dryRun
                                ? "planned"
                                : "moved"
                            : "unresolved"
                });
        }

        HoleThreadNotes notes =
            sheet
                .DrawingNotes
                .HoleThreadNotes;

        for (int index = 1;
             index <= notes.Count;
             index++)
        {
            HoleThreadNote note =
                notes[index];

            Box2d currentBox;
            Point2d currentOrigin;

            try
            {
                currentBox =
                    note.Text.RangeBox;

                currentOrigin =
                    note.Text.Origin;
            }
            catch
            {
                actions.Add(
                    new
                    {
                        kind =
                            "hole_thread_note",

                        index,

                        status =
                            "skipped",

                        reason =
                            "Не удалось получить положение."
                    });

                continue;
            }

            bool moved =
                false;

            object? plannedTarget =
                null;

            for (int attempt = 1;
                 attempt <= maxAttempts;
                 attempt++)
            {
                double direction =
                    attempt % 2 == 1
                        ? 1.0
                        : -1.0;

                double distance =
                    ((attempt + 1) / 2) *
                    step *
                    direction;

                Point2d target =
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            currentOrigin.X,
                            currentOrigin.Y + distance);

                AnnotationCollisionSupport.AnnotationBox candidate =
                    new(
                        $"hole_thread_note:{index}",
                        "hole_thread_note",
                        AnnotationCollisionSupport
                            .ReadHoleThreadNoteText(
                                note),
                        currentBox.MinPoint.X,
                        currentBox.MinPoint.Y + distance,
                        currentBox.MaxPoint.X,
                        currentBox.MaxPoint.Y + distance);

                List<AnnotationCollisionSupport.AnnotationBox> boxes =
                    AnnotationCollisionSupport
                        .ReadAllBoxes(
                            sheet,
                            viewPadding);

                bool collision =
                    false;

                foreach (
                    AnnotationCollisionSupport.AnnotationBox box
                    in boxes)
                {
                    if (box.Id == candidate.Id)
                    {
                        continue;
                    }

                    if (AnnotationCollisionSupport
                            .Intersects(
                                candidate,
                                box,
                                clearance))
                    {
                        collision =
                            true;

                        break;
                    }
                }

                bool inside =
                    AnnotationCollisionSupport
                        .IsInsideSheet(
                            candidate,
                            sheet,
                            sheetMargin);

                if (collision ||
                    !inside)
                {
                    continue;
                }

                plannedTarget =
                    new
                    {
                        x =
                            target.X,

                        y =
                            target.Y
                    };

                if (!dryRun)
                {
                    dynamic textDynamic =
                        note.Text;

                    textDynamic.Origin =
                        target;
                }

                moved =
                    true;

                break;
            }

            actions.Add(
                new
                {
                    kind =
                        "hole_thread_note",

                    index,

                    text =
                        AnnotationCollisionSupport
                            .ReadHoleThreadNoteText(
                                note),

                    previous =
                        new
                        {
                            x =
                                currentOrigin.X,

                            y =
                                currentOrigin.Y
                        },

                    target =
                        plannedTarget,

                    status =
                        moved
                            ? dryRun
                                ? "planned"
                                : "moved"
                            : "unresolved"
                });
        }

        if (!dryRun)
        {
            drawingDocument.Update();
        }

        return AnnotationCollisionSupport
            .CreateSuccess(
                new
                {
                    message =
                        dryRun
                            ? "План устранения коллизий рассчитан."
                            : "Попытка устранения коллизий выполнена.",

                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    dryRun,

                    clearance,

                    step,

                    sheetMargin,

                    viewPadding,

                    maxAttempts,

                    actionCount =
                        actions.Count,

                    actions,

                    dirty =
                        drawingDocument.Dirty
                });
    }
}
