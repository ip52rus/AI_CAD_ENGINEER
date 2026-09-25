using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class AddDrawingViewBreakCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public AddDrawingViewBreakCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "add_drawing_view_break";

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
        string viewError = string.Empty;
        if (!SheetCommandSupport.TryGetRequiredString(root, "sheetName", out string sheetName, out sheetError) ||
            !SheetCommandSupport.TryGetRequiredString(root, "viewName", out string viewName, out viewError))
        {
            diagnostics.Add(new { scope = "input", message = sheetError.Length > 0 ? sheetError : viewError });
            return CreateError("sheetName and viewName are required.", diagnostics);
        }

        if (!TryGetOrientation(root, out string orientationText, out BreakOrientationEnum orientation, diagnostics) ||
            !TryGetPoint(root, "start", out double startX, out double startY, diagnostics) ||
            !TryGetPoint(root, "end", out double endX, out double endY, diagnostics) ||
            !TryGetBreakStyle(root, out string breakStyleText, out BreakStyleEnum breakStyle, diagnostics) ||
            !TryGetRequiredInt(root, "displayLevel", out int displayLevel, diagnostics) ||
            !TryGetRequiredDouble(root, "gap", out double gap, diagnostics) ||
            !TryGetRequiredInt(root, "numberOfSymbols", out int numberOfSymbols, diagnostics) ||
            !TryGetRequiredBoolean(root, "propagateToParentView", out bool propagateToParentView, diagnostics))
        {
            return CreateError("Invalid drawing view break input.", diagnostics);
        }

        if (displayLevel < 0 || gap < 0.0 || numberOfSymbols < 1)
        {
            diagnostics.Add(new { scope = "input", message = "displayLevel and gap must be non-negative; numberOfSymbols must be positive." });
            return CreateError("Invalid drawing view break values.", diagnostics);
        }

        if (startX == endX && startY == endY)
        {
            diagnostics.Add(new { scope = "input.start/end", message = "start and end points must differ." });
            return CreateError("Break start and end points must differ.", diagnostics);
        }

        Sheet? sheet = SheetCommandSupport.FindSheet(drawingDocument, sheetName);
        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = "Sheet was not found.", sheetName });
            return CreateError("Sheet was not found.", diagnostics);
        }

        DrawingView? view = DrawingViewCommandSupport.FindDrawingView(sheet, viewName);
        if (view == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = "Drawing view was not found on selected sheet.", viewName });
            return CreateError("Drawing view was not found.", diagnostics);
        }

        try
        {
            Point2d start = _inventor.TransientGeometry.CreatePoint2d(startX, startY);
            Point2d end = _inventor.TransientGeometry.CreatePoint2d(endX, endY);

            BreakOperation operation = view.BreakOperations.Add(
                orientation,
                start,
                end,
                breakStyle,
                displayLevel,
                gap,
                numberOfSymbols,
                propagateToParentView);

            string? referenceKey = TryGetReferenceKey(drawingDocument, operation, diagnostics);

            return DocumentCommandSupport.CreateSuccess(new
            {
                capability = "add_drawing_view_break",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = view.Name,
                orientation = orientationText,
                breakStyle = breakStyleText,
                displayLevel = operation.DisplayLevel,
                gap = operation.Gap,
                numberOfSymbols = operation.NumberOfSymbols,
                propagateToParentView,
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.BreakOperations.Add", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to add drawing view break.", diagnostics);
        }
    }

    private static bool TryGetOrientation(JsonElement root, out string text, out BreakOrientationEnum value, List<object> diagnostics)
    {
        text = string.Empty;
        value = BreakOrientationEnum.kHorizontalBreakOrientation;
        if (!SheetCommandSupport.TryGetRequiredString(root, "orientation", out text, out string error))
        {
            diagnostics.Add(new { scope = "input.orientation", message = error });
            return false;
        }
        string normalized = text.Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");
        if (normalized == "horizontal") value = BreakOrientationEnum.kHorizontalBreakOrientation;
        else if (normalized == "vertical") value = BreakOrientationEnum.kVerticalBreakOrientation;
        else
        {
            diagnostics.Add(new { scope = "input.orientation", message = "Unsupported break orientation.", supported = new[] { "horizontal", "vertical" } });
            return false;
        }
        return true;
    }

    private static bool TryGetBreakStyle(JsonElement root, out string text, out BreakStyleEnum value, List<object> diagnostics)
    {
        text = string.Empty;
        value = BreakStyleEnum.kRectangularBreakStyle;
        if (!SheetCommandSupport.TryGetRequiredString(root, "breakStyle", out text, out string error))
        {
            diagnostics.Add(new { scope = "input.breakStyle", message = error });
            return false;
        }
        string normalized = text.Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");
        if (normalized == "rectangular") value = BreakStyleEnum.kRectangularBreakStyle;
        else if (normalized == "structural") value = BreakStyleEnum.kStructuralBreakStyle;
        else
        {
            diagnostics.Add(new { scope = "input.breakStyle", message = "Unsupported break style.", supported = new[] { "rectangular", "structural" } });
            return false;
        }
        return true;
    }

    private static bool TryGetPoint(JsonElement root, string name, out double x, out double y, List<object> diagnostics)
    {
        x = y = 0.0;
        if (!root.TryGetProperty(name, out JsonElement point) || point.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Required point object is missing." });
            return false;
        }
        return TryGetRequiredDouble(point, "x", $"input.{name}.x", diagnostics, out x) &&
               TryGetRequiredDouble(point, "y", $"input.{name}.y", diagnostics, out y);
    }

    private static bool TryGetRequiredDouble(JsonElement root, string name, string scope, List<object> diagnostics, out double value)
    {
        value = 0.0;
        if (!root.TryGetProperty(name, out JsonElement element) || !element.TryGetDouble(out value) || double.IsNaN(value) || double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = "Value must be a finite number." });
            return false;
        }
        return true;
    }

    private static bool TryGetRequiredDouble(JsonElement root, string name, out double value, List<object> diagnostics) =>
        TryGetRequiredDouble(root, name, $"input.{name}", diagnostics, out value);

    private static bool TryGetRequiredInt(JsonElement root, string name, out int value, List<object> diagnostics)
    {
        value = 0;
        if (!root.TryGetProperty(name, out JsonElement element) || !element.TryGetInt32(out value))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be a 32-bit integer." });
            return false;
        }
        return true;
    }

    private static bool TryGetRequiredBoolean(JsonElement root, string name, out bool value, List<object> diagnostics)
    {
        value = false;
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be boolean." });
            return false;
        }
        value = element.GetBoolean();
        return true;
    }

    private static string? TryGetReferenceKey(DrawingDocument document, BreakOperation operation, List<object> diagnostics)
    {
        try
        {
            Array key = Array.CreateInstance(typeof(byte), 0);
            operation.GetReferenceKey(ref key, 0);
            return document.ReferenceKeyManager.KeyToString(ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "BreakOperation.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(new { success = false, error = message, diagnostics }, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
}
