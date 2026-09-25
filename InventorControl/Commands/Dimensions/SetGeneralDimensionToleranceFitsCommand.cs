using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralDimensionToleranceFitsCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralDimensionToleranceFitsCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "set_general_dimension_tolerance_fits";

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

        if (!GeneralDimensionToleranceSupport.TryGetRequiredFitsToleranceType(
                root,
                diagnostics,
                out ToleranceTypeEnum toleranceType,
                out string toleranceTypeText) ||
            !GeneralDimensionToleranceSupport.TryGetRequiredInputString(
                root,
                "holeTolerance",
                diagnostics,
                out string holeTolerance) ||
            !GeneralDimensionToleranceSupport.TryGetRequiredInputString(
                root,
                "shaftTolerance",
                diagnostics,
                out string shaftTolerance))
        {
            return GeneralDimensionToleranceSupport.CreateError(
                "Invalid fits tolerance input.",
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
            tolerance.SetToFits(
                toleranceType,
                holeTolerance,
                shaftTolerance);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Tolerance.SetToFits", message = exception.Message, exceptionType = exception.GetType().FullName, toleranceType = toleranceTypeText, toleranceTypeRaw = toleranceType.ToString(), holeTolerance, shaftTolerance });
            return GeneralDimensionToleranceSupport.CreateError(
                "Failed to set general dimension fits tolerance.",
                diagnostics);
        }

        return GeneralDimensionToleranceSupport.CreateSuccess(new
        {
            capability = "set_general_dimension_tolerance_fits",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            dimensionIndex,
            dimensionObjectType =
                GeneralDimensionToleranceSupport.ReadDimensionObjectType(
                    dimension,
                    diagnostics),
            requestedToleranceType = toleranceTypeText,
            requestedToleranceTypeRaw = toleranceType.ToString(),
            requestedToleranceTypeValue = Convert.ToInt32(toleranceType),
            requestedHoleTolerance = holeTolerance,
            requestedShaftTolerance = shaftTolerance,
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
