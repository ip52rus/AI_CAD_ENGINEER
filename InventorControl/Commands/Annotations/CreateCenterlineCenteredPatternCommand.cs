using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateCenterlineCenteredPatternCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateCenterlineCenteredPatternCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_centerline_centered_pattern";

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

        if (!TryGetRequiredBoolean(root, "closed", diagnostics, out bool closed) ||
            !TryGetRequiredSelector(root, "patternCenter", diagnostics, out CurveIntentSelector patternCenterSelector) ||
            !TryGetRequiredSelectorArray(root, "centerEntities", diagnostics, out List<CurveIntentSelector> centerEntitySelectors))
        {
            return CreateError("Invalid centered pattern centerline input.", diagnostics);
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

        try
        {
            DrawingCurve? patternCenterCurve =
                TryGetDrawingCurve(
                    drawingView,
                    patternCenterSelector.CurveIndex,
                    "input.patternCenter.curveIndex",
                    diagnostics);

            if (patternCenterCurve == null)
            {
                return CreateError("Pattern center drawing curve was not found.", diagnostics);
            }

            GeometryIntent patternCenterIntent =
                sheet.CreateGeometryIntent(
                    patternCenterCurve,
                    patternCenterSelector.PointIntent);

            ObjectCollection centerEntities =
                _inventor.TransientObjects.CreateObjectCollection();

            List<object> returnedCenterEntities =
                new();

            for (int index = 0;
                 index < centerEntitySelectors.Count;
                 index++)
            {
                CurveIntentSelector selector =
                    centerEntitySelectors[index];

                DrawingCurve? curve =
                    TryGetDrawingCurve(
                        drawingView,
                        selector.CurveIndex,
                        $"input.centerEntities[{index + 1}].curveIndex",
                        diagnostics);

                if (curve == null)
                {
                    return CreateError("One or more centered pattern entity curves were not found.", diagnostics);
                }

                GeometryIntent entityIntent =
                    sheet.CreateGeometryIntent(
                        curve,
                        selector.PointIntent);

                centerEntities.Add(
                    entityIntent);

                returnedCenterEntities.Add(new
                {
                    index = index + 1,
                    curveIndex = selector.CurveIndex,
                    intent = selector.Intent,
                    intentRaw = selector.PointIntent.ToString()
                });
            }

            Centerline centerline =
                sheet.Centerlines.AddCenteredPattern(
                    patternCenterIntent,
                    centerEntities,
                    Type.Missing,
                    Type.Missing,
                    closed);

            return CreateSuccess(new
            {
                capability = "create_centerline_centered_pattern",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = drawingView.Name,
                patternCenter = new
                {
                    curveIndex = patternCenterSelector.CurveIndex,
                    intent = patternCenterSelector.Intent,
                    intentRaw = patternCenterSelector.PointIntent.ToString()
                },
                centerEntities = returnedCenterEntities,
                closed,
                centerlineType = SafeRead(() => centerline.CenterlineType.ToString()),
                geometryType = SafeRead(() => centerline.GeometryType.ToString()),
                startPoint = ReadPoint(SafeRead(() => centerline.StartPoint)),
                endPoint = ReadPoint(SafeRead(() => centerline.EndPoint)),
                visible = SafeReadNullableBoolean(() => centerline.Visible),
                attached = SafeReadNullableBoolean(() => centerline.Attached),
                style = ReadNamedObject(SafeRead(() => centerline.Style)),
                layer = ReadNamedObject(SafeRead(() => centerline.Layer)),
                referenceKey =
                    TryGetReferenceKey(
                        drawingDocument,
                        centerline,
                        diagnostics),
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Centerlines.AddCenteredPattern", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to create centered pattern centerline.", diagnostics);
        }
    }

    private static bool TryGetRequiredBoolean(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out bool value)
    {
        value =
            false;

        if (!root.TryGetProperty(propertyName, out JsonElement element) ||
            (element.ValueKind != JsonValueKind.True &&
             element.ValueKind != JsonValueKind.False))
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a required boolean." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static bool TryGetRequiredSelector(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out CurveIntentSelector selector)
    {
        selector =
            default;

        if (!root.TryGetProperty(propertyName, out JsonElement element) ||
            element.ValueKind != JsonValueKind.Object)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be an object." });
            return false;
        }

        return TryParseSelector(
            element,
            $"input.{propertyName}",
            diagnostics,
            out selector);
    }

    private static bool TryGetRequiredSelectorArray(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out List<CurveIntentSelector> selectors)
    {
        selectors =
            new List<CurveIntentSelector>();

        if (!root.TryGetProperty(propertyName, out JsonElement arrayElement) ||
            arrayElement.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must be a non-empty array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement item in arrayElement.EnumerateArray())
        {
            index++;

            if (item.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.{propertyName}[{index}]", message = "centerEntities item must be an object." });
                return false;
            }

            if (!TryParseSelector(
                    item,
                    $"input.{propertyName}[{index}]",
                    diagnostics,
                    out CurveIntentSelector selector))
            {
                return false;
            }

            selectors.Add(
                selector);
        }

        if (selectors.Count == 0)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"{propertyName} must contain at least one selector." });
            return false;
        }

        return true;
    }

    private static bool TryParseSelector(
        JsonElement element,
        string scope,
        List<object> diagnostics,
        out CurveIntentSelector selector)
    {
        selector =
            default;

        if (!element.TryGetProperty("curveIndex", out JsonElement curveIndexElement) ||
            !curveIndexElement.TryGetInt32(out int curveIndex) ||
            curveIndex < 1)
        {
            diagnostics.Add(new { scope = $"{scope}.curveIndex", message = "curveIndex must be a positive 1-based integer." });
            return false;
        }

        if (!element.TryGetProperty("intent", out JsonElement intentElement) ||
            intentElement.ValueKind != JsonValueKind.String)
        {
            diagnostics.Add(new { scope = $"{scope}.intent", message = "intent must be a required string." });
            return false;
        }

        string intent =
            intentElement.GetString()?.Trim()
            ?? string.Empty;

        if (!string.Equals(
                intent,
                "center",
                StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new { scope = $"{scope}.intent", message = "Unsupported centered pattern intent.", supported = new[] { "center" } });
            return false;
        }

        selector =
            new CurveIntentSelector(
                curveIndex,
                intent,
                PointIntentEnum.kCenterPointIntent);

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
        Centerline centerline,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            centerline.GetReferenceKey(
                ref key,
                0);

            return drawingDocument.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Centerline.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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

    private readonly record struct CurveIntentSelector(
        int CurveIndex,
        string Intent,
        PointIntentEnum PointIntent);
}
