using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CenterGeneralDimensionTextCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CenterGeneralDimensionTextCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "center_general_dimension_text";

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

        try
        {
            dimension.CenterText();

            drawingDocument.Update();

            return DimensionPositionSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Текст размера отцентрирован.",

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
                    "Не удалось отцентрировать текст размера.",
                    exception.Message);
        }
    }
}
