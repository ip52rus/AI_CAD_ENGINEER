using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class AnalyzeDimensionLayoutCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public AnalyzeDimensionLayoutCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "analyze_dimension_layout";

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

        List<object> result =
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

            AutomaticDimensionLayoutSupport.ViewBox? owningView =
                AutomaticDimensionLayoutSupport
                    .FindOwningView(
                        info,
                        views);

            string currentSide =
                owningView == null
                    ? "unknown"
                    : AutomaticDimensionLayoutSupport
                        .DetermineSide(
                            info,
                            owningView);

            result.Add(
                new
                {
                    index =
                        info.Index,

                    text =
                        info.Text,

                    orientation =
                        info.Horizontal
                            ? "horizontal"
                            : "vertical",

                    lineCoordinate =
                        info.LineCoordinate,

                    owningView =
                        owningView?.Name,

                    currentSide,

                    extensionOneStart =
                        info.ExtensionOneStart == null
                            ? null
                            : new
                            {
                                x = info.ExtensionOneStart.X,
                                y = info.ExtensionOneStart.Y
                            },

                    extensionTwoStart =
                        info.ExtensionTwoStart == null
                            ? null
                            : new
                            {
                                x = info.ExtensionTwoStart.X,
                                y = info.ExtensionTwoStart.Y
                            },

                    textOrigin =
                        info.TextOrigin == null
                            ? null
                            : new
                            {
                                x = info.TextOrigin.X,
                                y = info.TextOrigin.Y
                            },

                    textRangeBox =
                        info.TextRangeBox == null
                            ? null
                            : new
                            {
                                minPoint = new
                                {
                                    x = info.TextRangeBox.MinPoint.X,
                                    y = info.TextRangeBox.MinPoint.Y
                                },

                                maxPoint = new
                                {
                                    x = info.TextRangeBox.MaxPoint.X,
                                    y = info.TextRangeBox.MaxPoint.Y
                                }
                            }
                });
        }

        return AutomaticDimensionLayoutSupport
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

                    viewCount =
                        views.Count,

                    dimensionCount =
                        result.Count,

                    views,

                    dimensions =
                        result
                });
    }
}
