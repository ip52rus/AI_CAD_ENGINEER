using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetGeneralDimensionsDetailedCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetGeneralDimensionsDetailedCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_general_dimensions_detailed";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DimensionPositionSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return DimensionPositionSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!DimensionPositionSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return DimensionPositionSupport
                .CreateError(
                    sheetNameError);
        }

        Sheet? sheet =
            DimensionPositionSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return DimensionPositionSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        GeneralDimensions dimensions =
            sheet
                .DrawingDimensions
                .GeneralDimensions;

        List<object> result =
            new();

        for (int index = 1;
             index <= dimensions.Count;
             index++)
        {
            dynamic dimension =
                dimensions[index];

            result.Add(
                new
                {
                    index,

                    objectType =
                        DimensionPositionSupport
                            .GetObjectType(
                                dimension),

                    dimensionType =
                        DimensionPositionSupport
                            .GetDimensionType(
                                dimension),

                    text =
                        DimensionPositionSupport
                            .GetText(
                                dimension),

                    textOrigin =
                        DimensionPositionSupport
                            .GetTextOrigin(
                                dimension),

                    textRangeBox =
                        DimensionPositionSupport
                            .GetTextRangeBox(
                                dimension)
                });
        }

        return DimensionPositionSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    dimensionCount =
                        result.Count,

                    dimensions =
                        result
                });
    }
}
