using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class MoveLinearDimensionCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveLinearDimensionCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "move_linear_dimension";

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

        if (!DimensionGeometrySupport.TryGetRequiredDouble(
                root,
                "x",
                out double x,
                out string xError))
        {
            return DimensionGeometrySupport.CreateError(
                xError);
        }

        if (!DimensionGeometrySupport.TryGetRequiredDouble(
                root,
                "y",
                out double y,
                out string yError))
        {
            return DimensionGeometrySupport.CreateError(
                yError);
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

        Point2d target =
            _inventor.TransientGeometry.CreatePoint2d(
                x,
                y);

        List<string> errors = new();
        string appliedStrategy = string.Empty;

        try
        {
            dynamic dimensionLine =
                dimension.DimensionLine;

            dimensionLine.Position =
                target;

            appliedStrategy =
                "DimensionLine.Position";
        }
        catch (Exception exception)
        {
            errors.Add(
                "DimensionLine.Position: " +
                exception.Message);
        }

        if (string.IsNullOrWhiteSpace(appliedStrategy))
        {
            try
            {
                dynamic dimensionLine =
                    dimension.DimensionLine;

                dimensionLine.Origin =
                    target;

                appliedStrategy =
                    "DimensionLine.Origin";
            }
            catch (Exception exception)
            {
                errors.Add(
                    "DimensionLine.Origin: " +
                    exception.Message);
            }
        }

        if (string.IsNullOrWhiteSpace(appliedStrategy))
        {
            try
            {
                dimension.Position =
                    target;

                appliedStrategy =
                    "Dimension.Position";
            }
            catch (Exception exception)
            {
                errors.Add(
                    "Dimension.Position: " +
                    exception.Message);
            }
        }

        if (string.IsNullOrWhiteSpace(appliedStrategy))
        {
            try
            {
                dimension.Text.Origin =
                    target;

                dimension.CenterText();

                appliedStrategy =
                    "Text.Origin + CenterText";
            }
            catch (Exception exception)
            {
                errors.Add(
                    "Text.Origin + CenterText: " +
                    exception.Message);
            }
        }

        if (string.IsNullOrWhiteSpace(appliedStrategy))
        {
            return DimensionGeometrySupport.CreateError(
                "Не удалось переместить размерную линию.",
                string.Join(
                    System.Environment.NewLine,
                    errors));
        }

        try
        {
            drawingDocument.Update();
        }
        catch (Exception exception)
        {
            errors.Add(
                "Document.Update: " +
                exception.Message);
        }

        object currentGeometry =
            DimensionGeometrySupport.ReadGeometry(
                dimension,
                dimensionIndex);

        return DimensionGeometrySupport.CreateSuccess(
            new
            {
                message =
                    "Положение линейного размера изменено.",

                document =
                    drawingDocument.DisplayName,

                sheet =
                    sheet.Name,

                dimensionIndex,

                requestedPosition =
                    new
                    {
                        x,
                        y
                    },

                appliedStrategy,

                previousGeometry,

                currentGeometry,

                diagnostics =
                    errors,

                dirty =
                    drawingDocument.Dirty
            });
    }
}
