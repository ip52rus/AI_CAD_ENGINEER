using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateBaselineDimensionCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateBaselineDimensionCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_baseline_dimension";

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
            !TryGetRequiredString(root, "viewName", diagnostics, out string viewName) ||
            !TryGetGeometrySelectors(root, diagnostics, out List<GeometrySelectorInput> selectors) ||
            !TryGetRequiredDouble(root, "x", diagnostics, out double x) ||
            !TryGetRequiredDouble(root, "y", diagnostics, out double y) ||
            !TryGetRequiredDimensionType(root, diagnostics, out DimensionTypeEnum dimensionType, out string dimensionTypeText))
        {
            return CreateError("Invalid baseline dimension input.", diagnostics);
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

        DrawingView? view =
            DimensionCommandSupport.FindView(
                sheet,
                viewName);

        if (view == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = "Drawing view was not found on selected sheet.", viewName });
            return CreateError("Drawing view was not found.", diagnostics);
        }

        List<DrawingCurve> curves;

        try
        {
            curves =
                DimensionCommandSupport.GetCurves(
                    view);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.DrawingCurves", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to read drawing curves.", diagnostics);
        }

        ObjectCollection geometryIntents =
            _inventor.TransientObjects.CreateObjectCollection();

        foreach (GeometrySelectorInput selector in selectors)
        {
            DrawingCurve? curve =
                GetCurve(
                    curves,
                    selector.CurveIndex,
                    selector.Index,
                    diagnostics);

            if (curve == null)
            {
                return CreateError("Selected drawing curve was not found.", diagnostics);
            }

            try
            {
                GeometryIntent geometryIntent =
                    sheet.CreateGeometryIntent(
                        curve,
                        selector.PointIntent);

                geometryIntents.Add(
                    geometryIntent);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = $"geometryIntents[{selector.Index}].CreateGeometryIntent", message = exception.Message, exceptionType = exception.GetType().FullName, selector.CurveIndex, selector.Intent });
                return CreateError("Failed to create geometry intent.", diagnostics);
            }
        }

        try
        {
            Point2d placementPoint =
                _inventor.TransientGeometry.CreatePoint2d(
                    x,
                    y);

            BaselineDimensionSet dimensionSet =
                sheet.DrawingDimensions.BaselineDimensionSets.Add(
                    geometryIntents,
                    placementPoint,
                    dimensionType,
                    Type.Missing,
                    Type.Missing);

            string? referenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    dimensionSet,
                    diagnostics);

            return CreateSuccess(new
            {
                capability = "create_baseline_dimension",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = view.Name,
                geometrySelectors = selectors.Select(selector => selector.ToResponse()).ToArray(),
                placementPoint = new { x, y },
                dimensionType = dimensionTypeText,
                dimensionTypeRaw = dimensionType.ToString(),
                createdDimensionCount = SafeReadNullableInt32(() => dimensionSet.Members.Count),
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "BaselineDimensionSets.Add",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return CreateError("Failed to create baseline dimension.", diagnostics);
        }
    }

    private static bool TryGetGeometrySelectors(
        JsonElement root,
        List<object> diagnostics,
        out List<GeometrySelectorInput> selectors)
    {
        selectors =
            new List<GeometrySelectorInput>();

        if (!root.TryGetProperty("geometryIntents", out JsonElement element))
        {
            diagnostics.Add(new { scope = "input.geometryIntents", message = "geometryIntents is required." });
            return false;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.geometryIntents", message = "geometryIntents must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement selectorElement in element.EnumerateArray())
        {
            index++;

            if (selectorElement.ValueKind != JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.geometryIntents[{index}]", message = "Geometry intent selector must be an object." });
                return false;
            }

            if (!selectorElement.TryGetProperty("curveIndex", out JsonElement curveIndexElement) ||
                !curveIndexElement.TryGetInt32(out int curveIndex) ||
                curveIndex < 1)
            {
                diagnostics.Add(new { scope = $"input.geometryIntents[{index}].curveIndex", message = "curveIndex must be a positive 1-based integer." });
                return false;
            }

            if (!selectorElement.TryGetProperty("intent", out JsonElement intentElement) ||
                intentElement.ValueKind != JsonValueKind.String)
            {
                diagnostics.Add(new { scope = $"input.geometryIntents[{index}].intent", message = "intent must be a string." });
                return false;
            }

            string intentText =
                intentElement.GetString()?.Trim() ?? string.Empty;

            if (!DimensionCommandSupport.TryParsePointIntent(
                    intentText,
                    out PointIntentEnum pointIntent))
            {
                diagnostics.Add(new { scope = $"input.geometryIntents[{index}].intent", message = "Unsupported point intent.", supported = new[] { "start", "end", "mid", "middle", "center" } });
                return false;
            }

            selectors.Add(
                new GeometrySelectorInput(
                    index,
                    curveIndex,
                    intentText,
                    pointIntent));
        }

        if (selectors.Count < 2)
        {
            diagnostics.Add(new { scope = "input.geometryIntents", message = "At least two geometry intents are required." });
            return false;
        }

        return true;
    }

    private static DrawingCurve? GetCurve(
        List<DrawingCurve> curves,
        int curveIndex,
        int selectorIndex,
        List<object> diagnostics)
    {
        if (curveIndex < 1 ||
            curveIndex > curves.Count)
        {
            diagnostics.Add(new { scope = $"input.geometryIntents[{selectorIndex}].curveIndex", message = "curveIndex is outside the drawing view curve collection.", curveIndex, count = curves.Count });
            return null;
        }

        return curves[curveIndex - 1];
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

    private static bool TryGetRequiredDouble(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out double value)
    {
        value = 0.0;

        if (!DimensionCommandSupport.TryGetRequiredDouble(root, name, out value, out string error))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = error });
            return false;
        }

        return true;
    }

    private static bool TryGetRequiredDimensionType(
        JsonElement root,
        List<object> diagnostics,
        out DimensionTypeEnum dimensionType,
        out string dimensionTypeText)
    {
        dimensionType =
            default;

        dimensionTypeText =
            string.Empty;

        if (!TryGetRequiredString(root, "dimensionType", diagnostics, out dimensionTypeText))
        {
            return false;
        }

        if (!DimensionCommandSupport.TryParseDimensionType(
                dimensionTypeText,
                out dimensionType))
        {
            diagnostics.Add(new { scope = "input.dimensionType", message = "Unsupported dimension type.", supported = new[] { "horizontal", "vertical", "aligned" } });
            return false;
        }

        return true;
    }

    private static string? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        BaselineDimensionSet dimensionSet,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            dimensionSet.GetReferenceKey(
                ref key,
                0);

            return drawingDocument.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "BaselineDimensionSet.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static int? SafeReadNullableInt32(Func<int> read)
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

    private sealed record GeometrySelectorInput(
        int Index,
        int CurveIndex,
        string Intent,
        PointIntentEnum PointIntent)
    {
        public object ToResponse() =>
            new
            {
                index = Index,
                curveIndex = CurveIndex,
                intent = Intent,
                intentRaw = PointIntent.ToString()
            };
    }
}
