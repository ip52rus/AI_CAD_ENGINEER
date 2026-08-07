using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateAuxiliaryViewCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateAuxiliaryViewCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "create_auxiliary_view";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();
        DrawingDocument? drawingDocument = SheetCommandSupport.GetActiveDrawingDocument(_inventor, out string? documentError);
        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError });
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        string sheetError = string.Empty;
        string parentError = string.Empty;
        if (!SheetCommandSupport.TryGetRequiredString(root, "sheetName", out string sheetName, out sheetError) ||
            !SheetCommandSupport.TryGetRequiredString(root, "parentViewName", out string parentViewName, out parentError))
        {
            diagnostics.Add(new { scope = "input", message = sheetError.Length > 0 ? sheetError : parentError });
            return CreateError("sheetName and parentViewName are required.", diagnostics);
        }

        if (!TryGetRequiredIndex(root, out int curveIndex, diagnostics) ||
            !TryGetPoint(root, "position", out double positionX, out double positionY, diagnostics) ||
            !TryGetRequiredStyle(root, out string styleText, out DrawingViewStyleEnum viewStyle, diagnostics) ||
            !TryGetOptionalPositiveDouble(root, "scale", out double? scale, diagnostics) ||
            !TryGetOptionalBoolean(root, "showLabel", true, out bool showLabel, diagnostics) ||
            !TryGetOptionalString(root, "name", out string viewName, diagnostics))
        {
            return CreateError("Invalid auxiliary view input.", diagnostics);
        }

        Sheet? sheet = SheetCommandSupport.FindSheet(drawingDocument, sheetName);
        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return CreateError("Sheet was not found.", diagnostics);
        }

        if (!BaseViewCommandSupport.IsPointInsideSheet(sheet, positionX, positionY))
        {
            diagnostics.Add(new { scope = "input.position", message = "Position is outside sheet bounds.", x = positionX, y = positionY });
            return CreateError("Auxiliary view position is outside sheet bounds.", diagnostics);
        }

        DrawingView? parentView = DrawingViewCommandSupport.FindDrawingView(sheet, parentViewName);
        if (parentView == null)
        {
            diagnostics.Add(new { scope = "input.parentViewName", message = "Parent drawing view was not found on selected sheet.", parentViewName });
            return CreateError("Parent drawing view was not found.", diagnostics);
        }

        DrawingCurve? orientationCurve = TryGetCurve(parentView, curveIndex, diagnostics);
        if (orientationCurve == null)
            return CreateError("Orientation curve was not found.", diagnostics);

        if (root.TryGetProperty("orientationCurveSnapshot", out JsonElement snapshot))
        {
            if (!ValidateSnapshot(snapshot, orientationCurve, curveIndex, diagnostics))
                return CreateError("Orientation curve snapshot does not match the selected curve.", diagnostics);
        }

        try
        {
            Point2d position = _inventor.TransientGeometry.CreatePoint2d(positionX, positionY);
            object scaleArgument = scale.HasValue ? scale.Value : Type.Missing;
            DrawingView auxiliaryView = sheet.DrawingViews.AddAuxiliaryView(
                parentView,
                orientationCurve,
                position,
                viewStyle,
                scaleArgument,
                showLabel,
                viewName);

            drawingDocument.Update();
            string? referenceKey = TryGetReferenceKey(drawingDocument, auxiliaryView, diagnostics);

            return DocumentCommandSupport.CreateSuccess(new
            {
                capability = "create_auxiliary_view",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                viewName = auxiliaryView.Name,
                requestedName = string.IsNullOrWhiteSpace(viewName) ? null : viewName,
                parentView = parentView.Name,
                orientationCurveIndex = curveIndex,
                position = new { x = auxiliaryView.Position.X, y = auxiliaryView.Position.Y },
                style = styleText,
                styleRaw = viewStyle.ToString(),
                scale = auxiliaryView.Scale,
                scaleString = auxiliaryView.ScaleString,
                showLabel,
                viewType = auxiliaryView.ViewType.ToString(),
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingViews.AddAuxiliaryView", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to create auxiliary view.", diagnostics);
        }
    }

    private static DrawingCurve? TryGetCurve(DrawingView parentView, int index, List<object> diagnostics)
    {
        try
        {
            int count = parentView.DrawingCurves.Count;
            if (index < 1 || index > count)
            {
                diagnostics.Add(new { scope = "input.orientationCurveIndex", message = "Curve index is outside the parent view curve collection.", index, count });
                return null;
            }

            DrawingCurvesEnumerator curves = parentView.DrawingCurves[Type.Missing];
            return curves[index];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "parentView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool ValidateSnapshot(JsonElement snapshot, DrawingCurve curve, int index, List<object> diagnostics)
    {
        if (snapshot.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = "input.orientationCurveSnapshot", message = "Snapshot must be an object." });
            return false;
        }

        bool matches = true;
        if (snapshot.TryGetProperty("curveType", out JsonElement curveType) && curveType.ValueKind == JsonValueKind.String)
        {
            string actual;
            try { actual = curve.CurveType.ToString(); }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = "orientationCurve.CurveType", message = exception.Message, exceptionType = exception.GetType().FullName });
                return false;
            }
            if (!string.Equals(curveType.GetString(), actual, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new { scope = "input.orientationCurveSnapshot.curveType", message = "Curve type does not match.", expected = curveType.GetString(), actual, index });
                matches = false;
            }
        }

        try
        {
            if (snapshot.TryGetProperty("startPoint", out JsonElement start) && !ComparePoint(start, curve.StartPoint, "startPoint", diagnostics)) matches = false;
            if (snapshot.TryGetProperty("endPoint", out JsonElement end) && !ComparePoint(end, curve.EndPoint, "endPoint", diagnostics)) matches = false;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "orientationCurve.points", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
        if (snapshot.TryGetProperty("modelGeometryType", out JsonElement modelType) && modelType.ValueKind == JsonValueKind.String)
        {
            string actual;
            try { actual = curve.ModelGeometry?.GetType().Name ?? string.Empty; }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = "orientationCurve.ModelGeometry", message = exception.Message, exceptionType = exception.GetType().FullName });
                return false;
            }
            if (!string.Equals(modelType.GetString(), actual, StringComparison.OrdinalIgnoreCase))
            {
                diagnostics.Add(new { scope = "input.orientationCurveSnapshot.modelGeometryType", message = "Model geometry type does not match.", expected = modelType.GetString(), actual, index });
                matches = false;
            }
        }

        return matches;
    }

    private static bool ComparePoint(JsonElement element, Point2d actual, string name, List<object> diagnostics)
    {
        if (element.ValueKind != JsonValueKind.Object || !TryGetSnapshotNumber(element, "x", out double x) || !TryGetSnapshotNumber(element, "y", out double y))
        {
            diagnostics.Add(new { scope = $"input.orientationCurveSnapshot.{name}", message = "Snapshot point must contain finite x and y." });
            return false;
        }

        if (Math.Abs(x - actual.X) > 1e-9 || Math.Abs(y - actual.Y) > 1e-9)
        {
            diagnostics.Add(new { scope = $"input.orientationCurveSnapshot.{name}", message = "Point does not match.", expected = new { x, y }, actual = new { x = actual.X, y = actual.Y } });
            return false;
        }
        return true;
    }

    private static bool TryGetSnapshotNumber(JsonElement root, string name, out double value)
    {
        value = 0.0;
        return root.TryGetProperty(name, out JsonElement element) &&
               element.TryGetDouble(out value) &&
               !double.IsNaN(value) &&
               !double.IsInfinity(value);
    }

    private static bool TryGetRequiredIndex(JsonElement root, out int index, List<object> diagnostics)
    {
        index = 0;
        if (!root.TryGetProperty("orientationCurveIndex", out JsonElement element) || !element.TryGetInt32(out index) || index < 1)
        {
            diagnostics.Add(new { scope = "input.orientationCurveIndex", message = "orientationCurveIndex must be a positive 1-based integer." });
            return false;
        }
        return true;
    }

    private static bool TryGetRequiredStyle(JsonElement root, out string styleText, out DrawingViewStyleEnum style, List<object> diagnostics)
    {
        styleText = string.Empty;
        style = DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle;
        if (!SheetCommandSupport.TryGetRequiredString(root, "style", out styleText, out string error))
        {
            diagnostics.Add(new { scope = "input.style", message = error });
            return false;
        }
        string normalized = styleText.ToLowerInvariant().Replace("-", "_").Replace(" ", "_");
        if (normalized is not ("hidden_line_removed" or "hidden_line" or "shaded" or "shaded_hidden_line"))
        {
            diagnostics.Add(new { scope = "input.style", message = "Unsupported drawing view style.", supported = new[] { "hidden_line_removed", "hidden_line", "shaded", "shaded_hidden_line" } });
            return false;
        }
        style = BaseViewCommandSupport.ParseViewStyle(normalized);
        return true;
    }

    private static bool TryGetPoint(JsonElement root, string name, out double x, out double y, List<object> diagnostics)
    {
        x = y = 0.0;
        if (!root.TryGetProperty(name, out JsonElement point) || point.ValueKind != JsonValueKind.Object ||
            !TryGetFiniteDouble(point, "x", $"input.{name}.x", out x, diagnostics) ||
            !TryGetFiniteDouble(point, "y", $"input.{name}.y", out y, diagnostics))
        {
            if (!root.TryGetProperty(name, out _)) diagnostics.Add(new { scope = $"input.{name}", message = "Required point object is missing." });
            return false;
        }
        return true;
    }

    private static bool TryGetFiniteDouble(JsonElement root, string name, string scope, out double value, List<object> diagnostics)
    {
        value = 0.0;
        if (!root.TryGetProperty(name, out JsonElement element) || !element.TryGetDouble(out value) || double.IsNaN(value) || double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = "Value must be a finite number." });
            return false;
        }
        return true;
    }

    private static bool TryGetOptionalPositiveDouble(JsonElement root, string name, out double? value, List<object> diagnostics)
    {
        value = null;
        if (!root.TryGetProperty(name, out JsonElement element)) return true;
        if (!element.TryGetDouble(out double parsed) || double.IsNaN(parsed) || double.IsInfinity(parsed) || parsed <= 0.0)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be a positive finite number." });
            return false;
        }
        value = parsed;
        return true;
    }

    private static bool TryGetOptionalBoolean(JsonElement root, string name, bool defaultValue, out bool value, List<object> diagnostics)
    {
        value = defaultValue;
        if (!root.TryGetProperty(name, out JsonElement element)) return true;
        if (element.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be boolean." });
            return false;
        }
        value = element.GetBoolean();
        return true;
    }

    private static bool TryGetOptionalString(JsonElement root, string name, out string value, List<object> diagnostics)
    {
        value = string.Empty;
        if (!root.TryGetProperty(name, out JsonElement element)) return true;
        if (element.ValueKind != JsonValueKind.String)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be a string." });
            return false;
        }
        value = element.GetString()?.Trim() ?? string.Empty;
        if (value.Length == 0)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must not be empty." });
            return false;
        }
        return true;
    }

    private static string? TryGetReferenceKey(DrawingDocument document, DrawingView view, List<object> diagnostics)
    {
        try
        {
            Array key = Array.CreateInstance(typeof(byte), 0);
            view.GetReferenceKey(ref key, 0);
            return document.ReferenceKeyManager.KeyToString(ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(new { success = false, error = message, diagnostics }, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
}
