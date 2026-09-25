using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class GeneralDimensionToleranceSupport
{
    public static bool TryResolveGeneralDimension(
        Inventor.Application inventor,
        JsonElement root,
        List<object> diagnostics,
        out DrawingDocument? drawingDocument,
        out Sheet? sheet,
        out GeneralDimension? dimension,
        out int dimensionIndex)
    {
        drawingDocument =
            null;

        sheet =
            null;

        dimension =
            null;

        dimensionIndex =
            0;

        drawingDocument =
            DimensionCommandSupport.GetActiveDrawingDocument(
                inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError });
            return false;
        }

        if (!TryGetRequiredString(root, "sheetName", diagnostics, out string sheetName) ||
            !TryGetRequiredDimensionIndex(root, diagnostics, out dimensionIndex))
        {
            return false;
        }

        sheet =
            DimensionCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return false;
        }

        GeneralDimensions dimensions;

        try
        {
            dimensions =
                sheet.DrawingDimensions.GeneralDimensions;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingDimensions.GeneralDimensions", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }

        int count;

        try
        {
            count =
                dimensions.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimensions.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }

        if (dimensionIndex > count)
        {
            diagnostics.Add(new { scope = "input.dimensionIndex", message = "dimensionIndex is outside the general dimensions collection.", dimensionIndex, count });
            return false;
        }

        try
        {
            dimension =
                dimensions[dimensionIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimensions.Item", message = exception.Message, exceptionType = exception.GetType().FullName, dimensionIndex });
            return false;
        }

        return true;
    }

    public static Inventor.Tolerance? GetTolerance(
        GeneralDimension dimension,
        List<object> diagnostics)
    {
        try
        {
            return dimension.Tolerance;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimension.Tolerance", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static object? ReadToleranceState(
        Inventor.Tolerance? tolerance)
    {
        if (tolerance == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        ToleranceTypeEnum? toleranceType =
            SafeRead(
                () => tolerance.ToleranceType,
                "Tolerance.ToleranceType",
                propertyDiagnostics);

        return new
        {
            toleranceType =
                toleranceType?.ToString(),

            toleranceTypeRaw =
                toleranceType.HasValue
                    ? Convert.ToInt32(toleranceType.Value)
                    : (int?)null,

            upper =
                SafeReadNullableDouble(
                    () => tolerance.Upper,
                    "Tolerance.Upper",
                    propertyDiagnostics),

            lower =
                SafeReadNullableDouble(
                    () => tolerance.Lower,
                    "Tolerance.Lower",
                    propertyDiagnostics),

            holeTolerance =
                SafeRead(
                    () => tolerance.HoleTolerance,
                    "Tolerance.HoleTolerance",
                    propertyDiagnostics),

            shaftTolerance =
                SafeRead(
                    () => tolerance.ShaftTolerance,
                    "Tolerance.ShaftTolerance",
                    propertyDiagnostics),

            propertyDiagnostics
        };
    }

    public static string? GetDimensionReferenceKey(
        DrawingDocument drawingDocument,
        GeneralDimension dimension,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            dimension.GetReferenceKey(
                ref key,
                0);

            return drawingDocument.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimension.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    public static string? ReadDimensionObjectType(
        GeneralDimension dimension,
        List<object> diagnostics)
    {
        return SafeRead(
            () => dimension.Type.ToString(),
            "GeneralDimension.Type",
            diagnostics);
    }

    public static bool TryGetRequiredDouble(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out double value)
    {
        value =
            0.0;

        if (!DimensionCommandSupport.TryGetRequiredDouble(root, name, out value, out string error))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = error });
            return false;
        }

        return true;
    }

    public static bool TryGetRequiredLimitsToleranceType(
        JsonElement root,
        List<object> diagnostics,
        out ToleranceTypeEnum toleranceType,
        out string toleranceTypeText)
    {
        toleranceType =
            default;

        toleranceTypeText =
            string.Empty;

        if (!TryGetRequiredString(root, "toleranceType", diagnostics, out toleranceTypeText))
        {
            return false;
        }

        switch (toleranceTypeText.Trim().ToLowerInvariant())
        {
            case "limits_stacked":
                toleranceType =
                    ToleranceTypeEnum.kLimitsStackedTolerance;
                return true;

            case "limit_linear":
                toleranceType =
                    ToleranceTypeEnum.kLimitLinearTolerance;
                return true;

            default:
                diagnostics.Add(new
                {
                    scope = "input.toleranceType",
                    message = "Unsupported limits tolerance type.",
                    supported = new[] { "limits_stacked", "limit_linear" }
                });
                return false;
        }
    }

    public static bool TryGetRequiredFitsToleranceType(
        JsonElement root,
        List<object> diagnostics,
        out ToleranceTypeEnum toleranceType,
        out string toleranceTypeText)
    {
        toleranceType =
            default;

        toleranceTypeText =
            string.Empty;

        if (!TryGetRequiredString(root, "toleranceType", diagnostics, out toleranceTypeText))
        {
            return false;
        }

        switch (toleranceTypeText.Trim().ToLowerInvariant())
        {
            case "limits_fits_stacked":
                toleranceType =
                    ToleranceTypeEnum.kLimitsFitsStackedTolerance;
                return true;

            case "limits_fits_linear":
                toleranceType =
                    ToleranceTypeEnum.kLimitsFitsLinearTolerance;
                return true;

            case "limits_fits_show_size":
                toleranceType =
                    ToleranceTypeEnum.kLimitsFitsShowSizeTolerance;
                return true;

            case "limits_fits_show_tolerance":
                toleranceType =
                    ToleranceTypeEnum.kLimitsFitsShowTolerance;
                return true;

            default:
                diagnostics.Add(new
                {
                    scope = "input.toleranceType",
                    message = "Unsupported fits tolerance type.",
                    supported = new[] { "limits_fits_stacked", "limits_fits_linear", "limits_fits_show_size", "limits_fits_show_tolerance" }
                });
                return false;
        }
    }

    public static bool TryGetRequiredInputString(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out string value) =>
        TryGetRequiredString(
            root,
            name,
            diagnostics,
            out value);

    public static string CreateSuccess(object data) =>
        JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());

    public static string CreateError(
        string message,
        List<object> diagnostics) =>
        JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                diagnostics
            },
            CreateJsonOptions());

    private static bool TryGetRequiredString(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out string value)
    {
        value =
            string.Empty;

        if (!DimensionCommandSupport.TryGetRequiredString(root, name, out value, out string error))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = error });
            return false;
        }

        return true;
    }

    private static bool TryGetRequiredDimensionIndex(
        JsonElement root,
        List<object> diagnostics,
        out int dimensionIndex)
    {
        dimensionIndex =
            0;

        if (!DimensionCommandSupport.TryGetRequiredInt32(root, "dimensionIndex", out dimensionIndex, out string error))
        {
            diagnostics.Add(new { scope = "input.dimensionIndex", message = error });
            return false;
        }

        if (dimensionIndex < 1)
        {
            diagnostics.Add(new { scope = "input.dimensionIndex", message = "dimensionIndex must be a positive 1-based integer." });
            return false;
        }

        return true;
    }

    private static T? SafeRead<T>(
        Func<T> read,
        string scope,
        List<object> diagnostics)
    {
        try
        {
            return read();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return default;
        }
    }

    private static double? SafeReadNullableDouble(
        Func<double> read,
        string scope,
        List<object> diagnostics)
    {
        try
        {
            return read();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
