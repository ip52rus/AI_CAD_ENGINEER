using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateDetailViewCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateDetailViewCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "create_detail_view";

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

        if (!SheetCommandSupport.TryGetRequiredString(root, "parentViewName", out string parentViewName, out string parentError))
        {
            diagnostics.Add(new { scope = "input.parentViewName", message = parentError });
            return CreateError("parentViewName is required.", diagnostics);
        }

        if (!TryGetPoint(root, "position", out double positionX, out double positionY, diagnostics) ||
            !TryGetRequiredStyle(root, out string styleText, out DrawingViewStyleEnum viewStyle, diagnostics) ||
            !TryGetCircularFence(root, out double fenceX, out double fenceY, out double fenceRadius, diagnostics))
        {
            return CreateError("Invalid detail view input.", diagnostics);
        }

        if (!TryGetOptionalPositiveDouble(root, "scale", out double? scale, diagnostics) ||
            !TryGetOptionalBoolean(root, "showLabel", true, out bool showLabel, diagnostics) ||
            !TryGetOptionalString(root, "name", out string viewName, diagnostics))
        {
            return CreateError("Invalid detail view option.", diagnostics);
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
            return CreateError("Detail view position is outside sheet bounds.", diagnostics);
        }

        DrawingView? parentView = DrawingViewCommandSupport.FindDrawingView(sheet, parentViewName);
        if (parentView == null)
        {
            diagnostics.Add(new { scope = "input.parentViewName", message = "Parent drawing view was not found on selected sheet.", parentViewName });
            return CreateError("Parent drawing view was not found.", diagnostics);
        }

        try
        {
            Point2d position = _inventor.TransientGeometry.CreatePoint2d(positionX, positionY);
            Point2d fenceCenter = _inventor.TransientGeometry.CreatePoint2d(fenceX, fenceY);
            object scaleArgument = scale.HasValue ? scale.Value : Type.Missing;

            DetailDrawingView detailView = sheet.DrawingViews.AddDetailView(
                parentView,
                position,
                viewStyle,
                true,
                fenceCenter,
                fenceRadius,
                Type.Missing,
                scaleArgument,
                showLabel,
                viewName,
                true);

            drawingDocument.Update();
            string? referenceKey = TryGetReferenceKey(drawingDocument, detailView, diagnostics);

            return DocumentCommandSupport.CreateSuccess(new
            {
                capability = "create_detail_view",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                viewName = detailView.Name,
                requestedName = string.IsNullOrWhiteSpace(viewName) ? null : viewName,
                parentView = parentView.Name,
                position = new { x = detailView.Position.X, y = detailView.Position.Y },
                style = styleText,
                styleRaw = viewStyle.ToString(),
                scale = detailView.Scale,
                scaleString = detailView.ScaleString,
                showLabel,
                viewType = detailView.ViewType.ToString(),
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingViews.AddDetailView", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to create detail view.", diagnostics);
        }
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

    private static bool TryGetCircularFence(JsonElement root, out double x, out double y, out double radius, List<object> diagnostics)
    {
        x = y = radius = 0.0;
        if (!root.TryGetProperty("fence", out JsonElement fence) || fence.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = "input.fence", message = "Required object fence is missing." });
            return false;
        }

        if (!SheetCommandSupport.TryGetRequiredString(fence, "type", out string type, out string typeError) ||
            !string.Equals(type, "circular", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new { scope = "input.fence.type", message = typeError.Length > 0 ? typeError : "Only fence.type=circular is supported." });
            return false;
        }

        if (!TryGetPoint(fence, "center", out x, out y, diagnostics))
            return false;

        if (!TryGetFiniteDouble(fence, "radius", "input.fence.radius", out radius, diagnostics) || radius <= 0.0)
        {
            diagnostics.Add(new { scope = "input.fence.radius", message = "Radius must be a positive finite number." });
            return false;
        }

        return true;
    }

    private static bool TryGetPoint(JsonElement root, string name, out double x, out double y, List<object> diagnostics)
    {
        x = y = 0.0;
        if (!root.TryGetProperty(name, out JsonElement point) || point.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = $"Required point object {name} is missing." });
            return false;
        }

        return TryGetFiniteDouble(point, "x", $"input.{name}.x", out x, diagnostics) &&
               TryGetFiniteDouble(point, "y", $"input.{name}.y", out y, diagnostics);
    }

    private static bool TryGetFiniteDouble(JsonElement root, string name, string scope, out double value, List<object> diagnostics)
    {
        value = 0.0;
        if (!root.TryGetProperty(name, out JsonElement element) || !element.TryGetDouble(out value) || double.IsNaN(value) || double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"Field {name} must be a finite number." });
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

    private static string? TryGetReferenceKey(DrawingDocument document, DetailDrawingView view, List<object> diagnostics)
    {
        try
        {
            Array key = Array.CreateInstance(typeof(byte), 0);
            view.GetReferenceKey(ref key, 0);
            return document.ReferenceKeyManager.KeyToString(ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DetailDrawingView.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(new { success = false, error = message, diagnostics }, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
}
