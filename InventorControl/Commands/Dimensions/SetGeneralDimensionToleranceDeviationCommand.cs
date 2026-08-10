using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralDimensionToleranceDeviationCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralDimensionToleranceDeviationCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "set_general_dimension_tolerance_deviation";

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
                "upper",
                diagnostics,
                out double upper) ||
            !GeneralDimensionToleranceSupport.TryGetRequiredDouble(
                root,
                "lower",
                diagnostics,
                out double lower))
        {
            return GeneralDimensionToleranceSupport.CreateError(
                "Invalid deviation tolerance input.",
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
            tolerance.SetToDeviation(
                upper,
                lower);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Tolerance.SetToDeviation", message = exception.Message, exceptionType = exception.GetType().FullName, upper, lower });
            return GeneralDimensionToleranceSupport.CreateError(
                "Failed to set general dimension deviation tolerance.",
                diagnostics);
        }

        return GeneralDimensionToleranceSupport.CreateSuccess(new
        {
            capability = "set_general_dimension_tolerance_deviation",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            dimensionIndex,
            dimensionObjectType =
                GeneralDimensionToleranceSupport.ReadDimensionObjectType(
                    dimension,
                    diagnostics),
            requestedUpper = upper,
            requestedLower = lower,
            numericValueSemantics = "caller values passed directly to Inventor Tolerance.SetToDeviation without unit conversion, reordering, or sign normalization",
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
