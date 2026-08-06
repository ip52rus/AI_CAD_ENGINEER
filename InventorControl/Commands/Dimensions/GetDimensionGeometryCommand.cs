using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDimensionGeometryCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDimensionGeometryCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_dimension_geometry";

    public string Execute(JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DimensionGeometrySupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return DimensionGeometrySupport.CreateError(
                documentError ??
                "Не удалось получить активный чертёж.");
        }

        if (!DimensionGeometrySupport.TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetNameError))
        {
            return DimensionGeometrySupport.CreateError(
                sheetNameError);
        }

        if (!DimensionGeometrySupport.TryGetRequiredInt32(
                root,
                "dimensionIndex",
                out int dimensionIndex,
                out string dimensionIndexError))
        {
            return DimensionGeometrySupport.CreateError(
                dimensionIndexError);
        }

        Sheet? sheet =
            DimensionGeometrySupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            return DimensionGeometrySupport.CreateError(
                $"Лист \"{sheetName}\" не найден.");
        }

        GeneralDimensions dimensions =
            sheet.DrawingDimensions.GeneralDimensions;

        if (dimensionIndex < 1 ||
            dimensionIndex > dimensions.Count)
        {
            return DimensionGeometrySupport.CreateError(
                $"dimensionIndex должен быть от 1 до " +
                $"{dimensions.Count}.");
        }

        dynamic dimension =
            dimensions[dimensionIndex];

        return DimensionGeometrySupport.CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    sheet.Name,

                geometry =
                    DimensionGeometrySupport.ReadGeometry(
                        dimension,
                        dimensionIndex)
            });
    }
}
