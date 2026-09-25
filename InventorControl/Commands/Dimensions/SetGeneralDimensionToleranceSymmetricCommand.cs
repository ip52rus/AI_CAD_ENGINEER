using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralDimensionToleranceSymmetricCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralDimensionToleranceSymmetricCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "set_general_dimension_tolerance_symmetric";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        if (!GeneralDimensionToleranceSupport.TryResolveGeneralDimension(
                _inventor,
                root,
                diagnostics,
                out DrawingDocument? drawingDocument,
                out Sheet? sheet,
                out GeneralDimension? dimension,
                out int dimensionIndex) ||
            drawingDocument == null ||
            sheet == null ||
            dimension == null)
        {
            return GeneralDimensionToleranceSupport.CreateError(
                "Selected general dimension was not found.",
                diagnostics);
        }

        if (!GeneralDimensionToleranceSupport.TryGetRequiredDouble(
                root,
                "value",
                diagnostics,
                out double value))
        {
            return GeneralDimensionToleranceSupport.CreateError(
                "Invalid symmetric tolerance input.",
                diagnostics);
        }

        Inventor.Tolerance? tolerance =
            GeneralDimensionToleranceSupport.GetTolerance(
                dimension,
                diagnostics);

        if (tolerance == null)
        {
            return GeneralDimensionToleranceSupport.CreateError(
                "General dimension tolerance was not available.",
                diagnostics);
        }

        object? toleranceBefore =
            GeneralDimensionToleranceSupport.ReadToleranceState(
                tolerance);

        try
        {
            tolerance.SetToSymmetric(
                value);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Tolerance.SetToSymmetric", message = exception.Message, exceptionType = exception.GetType().FullName, value });
            return GeneralDimensionToleranceSupport.CreateError(
                "Failed to set general dimension symmetric tolerance.",
                diagnostics);
        }

        return GeneralDimensionToleranceSupport.CreateSuccess(new
        {
            capability = "set_general_dimension_tolerance_symmetric",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            dimensionIndex,
            dimensionObjectType =
                GeneralDimensionToleranceSupport.ReadDimensionObjectType(
                    dimension,
                    diagnostics),
            requestedValue = value,
            numericValueSemantics = "caller value passed directly to Inventor Tolerance.SetToSymmetric without unit conversion",
            toleranceBefore,
            toleranceAfter =
                GeneralDimensionToleranceSupport.ReadToleranceState(
                    tolerance),
            dimensionReferenceKey =
                GeneralDimensionToleranceSupport.GetDimensionReferenceKey(
                    drawingDocument,
                    dimension,
                    diagnostics),
            dirty = drawingDocument.Dirty,
            diagnostics
        });
    }
}
