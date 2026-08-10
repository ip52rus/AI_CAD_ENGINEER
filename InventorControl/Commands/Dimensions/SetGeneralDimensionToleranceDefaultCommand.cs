using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralDimensionToleranceDefaultCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralDimensionToleranceDefaultCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "set_general_dimension_tolerance_default";

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
            tolerance.SetToDefault();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Tolerance.SetToDefault", message = exception.Message, exceptionType = exception.GetType().FullName });
            return GeneralDimensionToleranceSupport.CreateError(
                "Failed to set general dimension tolerance to default.",
                diagnostics);
        }

        return GeneralDimensionToleranceSupport.CreateSuccess(new
        {
            capability = "set_general_dimension_tolerance_default",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            dimensionIndex,
            dimensionObjectType =
                GeneralDimensionToleranceSupport.ReadDimensionObjectType(
                    dimension,
                    diagnostics),
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
