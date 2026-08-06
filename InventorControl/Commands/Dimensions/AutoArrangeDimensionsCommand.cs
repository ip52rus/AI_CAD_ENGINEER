using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class AutoArrangeDimensionsCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public AutoArrangeDimensionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "auto_arrange_dimensions";

    public string Execute(JsonElement root)
    {
        DrawingDocument? drawingDocument =
            AutomaticDimensionLayoutSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return AutomaticDimensionLayoutSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!AutomaticDimensionLayoutSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return AutomaticDimensionLayoutSupport
                .CreateError(
                    sheetNameError);
        }

        double firstOffset =
            Math.Max(
                0.5,
                AutomaticDimensionLayoutSupport
                    .GetOptionalDouble(
                        root,
                        "firstOffset",
                        1.5));

        double spacing =
            Math.Max(
                0.5,
                AutomaticDimensionLayoutSupport
                    .GetOptionalDouble(
                        root,
                        "spacing",
                        1.0));

        double sheetMargin =
            Math.Max(
                0.5,
                AutomaticDimensionLayoutSupport
                    .GetOptionalDouble(
                        root,
                        "sheetMargin",
                        1.0));

        bool dryRun =
            AutomaticDimensionLayoutSupport
                .GetOptionalBoolean(
                    root,
                    "dryRun",
                    false);

        Sheet? sheet =
            AutomaticDimensionLayoutSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return AutomaticDimensionLayoutSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        List<AutomaticDimensionLayoutSupport.ViewBox> views =
            new();

        foreach (DrawingView view
                 in sheet.DrawingViews)
        {
            views.Add(
                AutomaticDimensionLayoutSupport
                    .ReadViewBox(view));
        }

        GeneralDimensions dimensions =
            sheet.DrawingDimensions.GeneralDimensions;

        Dictionary<string, int> laneCounters =
            new(
                StringComparer.OrdinalIgnoreCase);

        List<object> actions =
            new();

        for (int index = 1;
             index <= dimensions.Count;
             index++)
        {
            dynamic dimension =
                dimensions[index];

            AutomaticDimensionLayoutSupport.DimensionInfo info =
                AutomaticDimensionLayoutSupport
                    .ReadDimension(
                        dimension,
                        index);

            AutomaticDimensionLayoutSupport.ViewBox? view =
                AutomaticDimensionLayoutSupport
                    .FindOwningView(
                        info,
                        views);

            if (view == null ||
                info.TextOrigin == null)
            {
                actions.Add(
                    new
                    {
                        index,
                        status = "skipped",
                        reason =
                            "Не найден связанный вид или позиция текста."
                    });

                continue;
            }

            string side =
                AutomaticDimensionLayoutSupport
                    .DetermineSide(
                        info,
                        view);

            string laneKey =
                $"{view.Name}:{side}";

            laneCounters.TryGetValue(
                laneKey,
                out int laneIndex);

            laneCounters[laneKey] =
                laneIndex + 1;

            double targetX =
                info.TextOrigin.X;

            double targetY =
                info.TextOrigin.Y;

            double offset =
                firstOffset +
                laneIndex * spacing;

            if (info.Horizontal)
            {
                targetX =
                    view.CenterX;

                targetY =
                    side == "top"
                        ? view.Top + offset
                        : view.Bottom - offset;
            }
            else
            {
                targetX =
                    side == "right"
                        ? view.Right + offset
                        : view.Left - offset;

                targetY =
                    view.CenterY;
            }

            targetX =
                Math.Clamp(
                    targetX,
                    sheetMargin,
                    sheet.Width - sheetMargin);

            targetY =
                Math.Clamp(
                    targetY,
                    sheetMargin,
                    sheet.Height - sheetMargin);

            object previous =
                new
                {
                    x = info.TextOrigin.X,
                    y = info.TextOrigin.Y
                };

            if (!dryRun)
            {
                try
                {
                    Point2d target =
                        _inventor
                            .TransientGeometry
                            .CreatePoint2d(
                                targetX,
                                targetY);

                    dimension.Text.Origin =
                        target;

                    dimension.CenterText();
                }
                catch (Exception exception)
                {
                    actions.Add(
                        new
                        {
                            index,
                            text = info.Text,
                            status = "failed",
                            reason = exception.Message
                        });

                    continue;
                }
            }

            actions.Add(
                new
                {
                    index,
                    text = info.Text,
                    owningView = view.Name,

                    orientation =
                        info.Horizontal
                            ? "horizontal"
                            : "vertical",

                    side,
                    laneIndex,
                    previous,

                    target = new
                    {
                        x = targetX,
                        y = targetY
                    },

                    status =
                        dryRun
                            ? "planned"
                            : "moved"
                });
        }

        if (!dryRun)
        {
            drawingDocument.Update();
        }

        return AutomaticDimensionLayoutSupport
            .CreateSuccess(
                new
                {
                    message =
                        dryRun
                            ? "Раскладка размеров рассчитана без изменений."
                            : "Размеры автоматически разложены.",

                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    dryRun,
                    firstOffset,
                    spacing,
                    sheetMargin,

                    actionCount =
                        actions.Count,

                    actions,

                    dirty =
                        drawingDocument.Dirty
                });
    }
}
