using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class DeleteGeneralDimensionCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteGeneralDimensionCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "delete_general_dimension";

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

        object previousGeometry =
            DimensionGeometrySupport.ReadGeometry(
                dimension,
                dimensionIndex);

        try
        {
            dimension.Delete();

            drawingDocument.Update();

            return DimensionGeometrySupport.CreateSuccess(
                new
                {
                    message =
                        "Размер удалён.",

                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    deletedIndex =
                        dimensionIndex,

                    previousGeometry,

                    remainingDimensionCount =
                        dimensions.Count,

                    dirty =
                        drawingDocument.Dirty
                });
        }
        catch (Exception exception)
        {
            return DimensionGeometrySupport.CreateError(
                "Не удалось удалить размер.",
                exception.Message);
        }
    }
}
