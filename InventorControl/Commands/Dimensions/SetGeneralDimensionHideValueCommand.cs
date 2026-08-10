using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralDimensionHideValueCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralDimensionHideValueCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "set_general_dimension_hide_value";

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
            !TryGetRequiredBoolean(root, "hideValue", diagnostics, out bool hideValue))
        {
            return CreateError("Invalid general dimension hide value input.", diagnostics);
        }

        Sheet? sheet =
            DimensionCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return CreateError("Sheet was not found.", diagnostics);
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
            return CreateError("Failed to read general dimensions.", diagnostics);
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
            return CreateError("Failed to read general dimension count.", diagnostics);
        }

        if (dimensionIndex > count)
        {
            diagnostics.Add(new { scope = "input.dimensionIndex", message = "dimensionIndex is outside the general dimensions collection.", dimensionIndex, count });
            return CreateError("Selected general dimension was not found.", diagnostics);
        }

        GeneralDimension dimension;

        try
        {
            dimension =
                dimensions[dimensionIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimensions.Item", message = exception.Message, exceptionType = exception.GetType().FullName, dimensionIndex });
            return CreateError("Failed to read selected general dimension.", diagnostics);
        }

        bool? hideValueBefore =
            SafeReadNullableBoolean(
                () => dimension.HideValue,
                "GeneralDimension.HideValue.Before",
                diagnostics);

        try
        {
            dimension.HideValue =
                hideValue;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralDimension.HideValue.Set", message = exception.Message, exceptionType = exception.GetType().FullName, dimensionIndex });
            return CreateError("Failed to set general dimension hide value.", diagnostics);
        }

        string? referenceKey =
            TryGetReferenceKey(
                drawingDocument,
                dimension,
                diagnostics);

        return CreateSuccess(new
        {
            capability = "set_general_dimension_hide_value",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            dimensionIndex,
            hideValueBefore,
            hideValueAfter = SafeReadNullableBoolean(
                () => dimension.HideValue,
                "GeneralDimension.HideValue.After",
                diagnostics),
            referenceKey,
            dirty = drawingDocument.Dirty,
            diagnostics
        });
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

    private static bool TryGetRequiredBoolean(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out bool value)
    {
        value =
            false;

        if (!root.TryGetProperty(name, out JsonElement element))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = $"{name} is required." });
            return false;
        }

        if (element.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = $"{name} must be boolean." });
            return false;
        }

        value =
            element.GetBoolean();

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
