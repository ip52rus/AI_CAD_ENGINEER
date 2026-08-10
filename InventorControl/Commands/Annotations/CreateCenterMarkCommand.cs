using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateCenterMarkCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateCenterMarkCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_center_mark";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        DrawingDocument? drawingDocument =
            SheetCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError });
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(root, "sheetName", out string sheetName, out string sheetError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetError });
            return CreateError("sheetName is required.", diagnostics);
        }

        if (!SheetCommandSupport.TryGetRequiredString(root, "viewName", out string viewName, out string viewError))
        {
            diagnostics.Add(new { scope = "input.viewName", message = viewError });
            return CreateError("viewName is required.", diagnostics);
        }

        if (!TryGetRequiredIndex(root, "curveIndex", diagnostics, out int curveIndex) ||
            !TryGetOptionalIntent(root, diagnostics, out PointIntentEnum? pointIntent, out string? requestedIntent) ||
            !TryGetOptionalBoolean(root, "extensionLinesVisible", diagnostics, out bool? extensionLinesVisible))
        {
            return CreateError("Invalid center mark input.", diagnostics);
        }

        Sheet? sheet =
            SheetCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return CreateError("Sheet was not found.", diagnostics);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = "Drawing view was not found on selected sheet.", viewName });
            return CreateError("Drawing view was not found.", diagnostics);
        }

        DrawingCurve? drawingCurve =
            TryGetDrawingCurve(
                drawingView,
                curveIndex,
                "input.curveIndex",
                diagnostics);

        if (drawingCurve == null)
        {
            return CreateError("Drawing curve was not found.", diagnostics);
        }

        try
        {
            GeometryIntent geometryIntent =
                pointIntent.HasValue
                    ? sheet.CreateGeometryIntent(
                        drawingCurve,
                        pointIntent.Value)
                    : sheet.CreateGeometryIntent(
                        drawingCurve);

            Centermark centerMark =
                extensionLinesVisible.HasValue
                    ? sheet.Centermarks.Add(
                        geometryIntent,
                        extensionLinesVisible.Value,
                        false,
                        Type.Missing,
                        Type.Missing)
                    : sheet.Centermarks.Add(
                        geometryIntent);

            return CreateSuccess(new
            {
                capability = "create_center_mark",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = drawingView.Name,
                curveIndex,
                requestedIntent,
                extensionLinesVisibleRequested = extensionLinesVisible,
                position = ReadPoint(SafeRead(() => centerMark.Position)),
                centerPoint = ReadPoint(SafeRead(() => centerMark.Position)),
                visible = SafeReadNullableBoolean(() => centerMark.Visible),
                attached = SafeReadNullableBoolean(() => centerMark.Attached),
                extensionLinesVisible = SafeReadNullableBoolean(() => centerMark.ExtensionLinesVisible),
                style = ReadNamedObject(SafeRead(() => centerMark.Style)),
                layer = ReadNamedObject(SafeRead(() => centerMark.Layer)),
                referenceKey =
                    TryGetReferenceKey(
                        drawingDocument,
                        centerMark,
                        diagnostics),
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Centermarks.Add", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to create center mark.", diagnostics);
        }
    }

    private static bool TryGetRequiredIndex(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out int value)
    {
        value =
            0;

        if (!root.TryGetProperty(propertyName, out JsonElement element) ||
            !element.TryGetInt32(out value) ||
            value < 1)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a positive 1-based integer." });
            return false;
        }

        return true;
    }

    private static bool TryGetOptionalIntent(
        JsonElement root,
        List<object> diagnostics,
        out PointIntentEnum? pointIntent,
        out string? requestedIntent)
    {
        pointIntent =
            null;

        requestedIntent =
            null;

        if (!root.TryGetProperty("intent", out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            diagnostics.Add(new { scope = "input.intent", message = "intent must be a string when supplied." });
            return false;
        }

        requestedIntent =
            element.GetString()?.Trim();

        if (string.Equals(
                requestedIntent,
                "center",
                StringComparison.OrdinalIgnoreCase))
        {
            pointIntent =
                PointIntentEnum.kCenterPointIntent;
            return true;
        }

        diagnostics.Add(new { scope = "input.intent", message = "Unsupported center mark intent.", supported = new[] { "center" } });
        return false;
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out bool? value)
    {
        value =
            null;

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.True &&
            element.ValueKind != JsonValueKind.False)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a boolean when supplied." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static DrawingCurve? TryGetDrawingCurve(
        DrawingView drawingView,
        int curveIndex,
        string scope,
        List<object> diagnostics)
    {
        try
        {
            DrawingCurvesEnumerator curves =
                drawingView.DrawingCurves[Type.Missing];

            int count =
                curves.Count;

            if (curveIndex < 1 ||
                curveIndex > count)
            {
                diagnostics.Add(new { scope, message = "curveIndex is outside the drawing view curve collection.", curveIndex, count });
                return null;
            }

            return curves[curveIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        Centermark centerMark,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            centerMark.GetReferenceKey(
                ref key,
                0);

            return drawingDocument.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Centermark.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadPoint(Point2d? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x = point.X,
            y = point.Y
        };
    }

    private static object? ReadNamedObject(dynamic? value)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            return new
            {
                name = (string?)value.Name,
                internalName = TryReadInternalName(value)
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadInternalName(dynamic value)
    {
        try
        {
            return value.InternalName;
        }
        catch
        {
            return null;
        }
    }

    private static T? SafeRead<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return default;
        }
    }

    private static bool? SafeReadNullableBoolean(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return null;
        }
    }

    private static string CreateSuccess(object data) =>
        JsonSerializer.Serialize(
            new { success = true, data },
            CreateJsonOptions());

    private static string CreateError(
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

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
