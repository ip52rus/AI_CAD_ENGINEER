using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralDimensionModelValueOverrideCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralDimensionModelValueOverrideCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "set_general_dimension_model_value_override";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        DrawingDocument? drawingDocument =
            DimensionCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError });
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        if (!TryGetRequiredString(root, "sheetName", diagnostics, out string sheetName) ||
            !TryGetRequiredDimensionIndex(root, diagnostics, out int dimensionIndex) ||
            !TryGetRequiredDouble(root, "value", diagnostics, out double value))
        {
            return CreateError("Invalid general dimension model value override input.", diagnostics);
        }

        GeneralDimension? dimension =
            ResolveGeneralDimension(
                drawingDocument,
                sheetName,
                dimensionIndex,
                diagnostics,
                out Sheet? sheet);

        if (dimension == null ||
            sheet == null)
        {
            return CreateError("Selected general dimension was not found.", diagnostics);
        }

        double? modelValue =
            SafeReadNullableDouble(
                () => dimension.ModelValue,
                "GeneralDimension.ModelValue",
                diagnostics);

        bool? modelValueOverriddenBefore =
            SafeReadNullableBoolean(
                () => dimension.ModelValueOverridden,
                "GeneralDimension.ModelValueOverridden.Before",
                diagnostics);

        double? overrideModelValueBefore =
            SafeReadNullableDouble(
                () => dimension.OverrideModelValue,
                "GeneralDimension.OverrideModelValue.Before",
                diagnostics);

        try
        {
            dimension.OverrideModelValue =
                value;

            dimension.ModelValueOverridden =
                true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimension.OverrideModelValue.Set", message = exception.Message, exceptionType = exception.GetType().FullName, dimensionIndex, value });
            return CreateError("Failed to set general dimension model value override.", diagnostics);
        }

        return CreateSuccess(new
        {
            capability = "set_general_dimension_model_value_override",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            dimensionIndex,
            modelValue,
            modelValueOverriddenBefore,
            modelValueOverriddenAfter = SafeReadNullableBoolean(
                () => dimension.ModelValueOverridden,
                "GeneralDimension.ModelValueOverridden.After",
                diagnostics),
            overrideModelValueBefore,
            overrideModelValueAfter = SafeReadNullableDouble(
                () => dimension.OverrideModelValue,
                "GeneralDimension.OverrideModelValue.After",
                diagnostics),
            text = SafeReadString(
                () => dimension.Text.Text,
                "DimensionText.Text",
                diagnostics),
            referenceKey = TryGetReferenceKey(
                drawingDocument,
                dimension,
                diagnostics),
            dirty = drawingDocument.Dirty,
            diagnostics
        });
    }

    private static GeneralDimension? ResolveGeneralDimension(
        DrawingDocument drawingDocument,
        string sheetName,
        int dimensionIndex,
        List<object> diagnostics,
        out Sheet? sheet)
    {
        sheet =
            DimensionCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return null;
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
            return null;
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
            return null;
        }

        if (dimensionIndex > count)
        {
            diagnostics.Add(new { scope = "input.dimensionIndex", message = "dimensionIndex is outside the general dimensions collection.", dimensionIndex, count });
            return null;
        }

        try
        {
            return dimensions[dimensionIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimensions.Item", message = exception.Message, exceptionType = exception.GetType().FullName, dimensionIndex });
            return null;
        }
    }

    private static bool TryGetRequiredString(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out string value)
    {
        value = string.Empty;

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

    private static bool TryGetRequiredDouble(
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

    private static string? TryGetReferenceKey(
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

    private static bool? SafeReadNullableBoolean(
        Func<bool> read,
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

    private static string? SafeReadString(
        Func<string> read,
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

    private static string CreateSuccess(object data) =>
        JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                diagnostics
            },
            CreateJsonOptions());

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
