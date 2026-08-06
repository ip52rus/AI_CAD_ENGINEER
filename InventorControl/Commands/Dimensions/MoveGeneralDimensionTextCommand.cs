using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class MoveGeneralDimensionTextCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public MoveGeneralDimensionTextCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_general_dimension_text";

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

        if (!DimensionPositionSupport
                .TryGetRequiredInt32(
                    root,
                    "dimensionIndex",
                    out int dimensionIndex,
                    out string dimensionIndexError))
        {
            return DimensionPositionSupport
                .CreateError(
                    dimensionIndexError);
        }

        if (!DimensionPositionSupport
                .TryGetRequiredDouble(
                    root,
                    "x",
                    out double x,
                    out string xError))
        {
            return DimensionPositionSupport
                .CreateError(
                    xError);
        }

        if (!DimensionPositionSupport
                .TryGetRequiredDouble(
                    root,
                    "y",
                    out double y,
                    out string yError))
        {
            return DimensionPositionSupport
                .CreateError(
                    yError);
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

        if (dimensionIndex < 1 ||
            dimensionIndex > dimensions.Count)
        {
            return DimensionPositionSupport
                .CreateError(
                    $"dimensionIndex должен быть от 1 до " +
                    $"{dimensions.Count}.");
        }

        dynamic dimension =
            dimensions[dimensionIndex];

        object? previousTextOrigin =
            DimensionPositionSupport
                .GetTextOrigin(
                    dimension);

        object? previousTextRangeBox =
            DimensionPositionSupport
                .GetTextRangeBox(
                    dimension);

        try
        {
            Point2d newOrigin =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            dimension.Text.Origin =
                newOrigin;

            drawingDocument.Update();

            return DimensionPositionSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Текст размера перемещён.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        dimensionIndex,

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

                        previousTextOrigin,

                        textOrigin =
                            DimensionPositionSupport
                                .GetTextOrigin(
                                    dimension),

                        previousTextRangeBox,

                        textRangeBox =
                            DimensionPositionSupport
                                .GetTextRangeBox(
                                    dimension),

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return DimensionPositionSupport
                .CreateError(
                    "Не удалось переместить текст размера.",
                    exception.Message);
        }
    }
}
