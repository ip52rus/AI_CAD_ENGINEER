using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateAngularDimensionCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateAngularDimensionCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_angular_dimension";

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

        if (!TryGetCommonInputs(
                root,
                diagnostics,
                out string sheetName,
                out string viewName,
                out int firstCurveIndex,
                out PointIntentEnum firstPointIntent,
                out string firstIntentText,
                out int secondCurveIndex,
                out PointIntentEnum secondPointIntent,
                out string secondIntentText,
                out double x,
                out double y) ||
            !TryGetOptionalThirdIntent(
                root,
                diagnostics,
                out int? thirdCurveIndex,
                out PointIntentEnum? thirdPointIntent,
                out string? thirdIntentText) ||
            !TryGetOptionalBoolean(root, "arrowheadsInside", true, diagnostics, out bool arrowheadsInside) ||
            !TryGetOptionalBoolean(root, "useQuadrant", true, diagnostics, out bool useQuadrant) ||
            !TryGetOptionalBoolean(root, "oppositeAngle", false, diagnostics, out bool oppositeAngle))
        {
            return CreateError("Invalid angular dimension input.", diagnostics);
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

        DrawingCurve? firstCurve =
            GetCurve(
                curves,
                firstCurveIndex,
                "firstCurveIndex",
                diagnostics);

        DrawingCurve? secondCurve =
            GetCurve(
                curves,
                secondCurveIndex,
                "secondCurveIndex",
                diagnostics);

        DrawingCurve? thirdCurve =
            thirdCurveIndex.HasValue
                ? GetCurve(
                    curves,
                    thirdCurveIndex.Value,
                    "thirdCurveIndex",
                    diagnostics)
                : null;

        if (firstCurve == null ||
            secondCurve == null ||
            (thirdCurveIndex.HasValue && thirdCurve == null))
        {
            return CreateError("Selected drawing curve was not found.", diagnostics);
        }

        try
        {
            GeometryIntent firstGeometryIntent =
                sheet.CreateGeometryIntent(
                    firstCurve,
                    firstPointIntent);

            GeometryIntent secondGeometryIntent =
                sheet.CreateGeometryIntent(
                    secondCurve,
                    secondPointIntent);

            object thirdGeometryIntent =
                Type.Missing;

            if (thirdCurve != null &&
                thirdPointIntent.HasValue)
            {
                thirdGeometryIntent =
                    sheet.CreateGeometryIntent(
                        thirdCurve,
                        thirdPointIntent.Value);
            }

            Point2d textOrigin =
                _inventor.TransientGeometry.CreatePoint2d(
                    x,
                    y);

            AngularGeneralDimension dimension =
                sheet.DrawingDimensions.GeneralDimensions.AddAngular(
                    textOrigin,
                    firstGeometryIntent,
                    secondGeometryIntent,
                    thirdGeometryIntent,
                    arrowheadsInside,
                    useQuadrant,
                    oppositeAngle,
                    Type.Missing,
                    Type.Missing);

            string? referenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    dimension,
                    diagnostics);

            return CreateSuccess(new
            {
                capability = "create_angular_dimension",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = view.Name,
                geometrySelectors = new
                {
                    first = new { curveIndex = firstCurveIndex, intent = firstIntentText, intentRaw = firstPointIntent.ToString() },
                    second = new { curveIndex = secondCurveIndex, intent = secondIntentText, intentRaw = secondPointIntent.ToString() },
                    third = thirdCurveIndex.HasValue && thirdPointIntent.HasValue
                        ? new { curveIndex = thirdCurveIndex.Value, intent = thirdIntentText, intentRaw = thirdPointIntent.Value.ToString() }
                        : null
                },
                position = ReadPoint(SafeRead(() => dimension.Text.Origin)) ?? new { x, y },
                text = SafeRead(() => dimension.Text.Text),
                dimensionType = SafeRead(() => dimension.DimensionType.ToString()),
                generalDimensionType = SafeRead(() => dimension.GeneralDimensionType.ToString()),
                attached = SafeReadNullableBoolean(() => dimension.Attached),
                arrowheadsInside,
                useQuadrant,
                oppositeAngle,
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "GeneralDimensions.AddAngular",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return CreateError("Failed to create angular dimension.", diagnostics);
        }
    }

    private static bool TryGetCommonInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out string viewName,
        out int firstCurveIndex,
        out PointIntentEnum firstPointIntent,
        out string firstIntentText,
        out int secondCurveIndex,
        out PointIntentEnum secondPointIntent,
        out string secondIntentText,
        out double x,
        out double y)
    {
        sheetName = string.Empty;
        viewName = string.Empty;
        firstCurveIndex = 0;
        firstPointIntent = default;
        firstIntentText = string.Empty;
        secondCurveIndex = 0;
        secondPointIntent = default;
        secondIntentText = string.Empty;
        x = 0.0;
        y = 0.0;

        return TryGetRequiredString(root, "sheetName", diagnostics, out sheetName) &&
               TryGetRequiredString(root, "viewName", diagnostics, out viewName) &&
               TryGetRequiredInt(root, "firstCurveIndex", diagnostics, out firstCurveIndex) &&
               TryGetRequiredIntent(root, "firstIntent", diagnostics, out firstPointIntent, out firstIntentText) &&
               TryGetRequiredInt(root, "secondCurveIndex", diagnostics, out secondCurveIndex) &&
               TryGetRequiredIntent(root, "secondIntent", diagnostics, out secondPointIntent, out secondIntentText) &&
               TryGetRequiredDouble(root, "x", diagnostics, out x) &&
               TryGetRequiredDouble(root, "y", diagnostics, out y);
    }

    private static bool TryGetOptionalThirdIntent(
        JsonElement root,
        List<object> diagnostics,
        out int? thirdCurveIndex,
        out PointIntentEnum? thirdPointIntent,
        out string? thirdIntentText)
    {
        thirdCurveIndex = null;
        thirdPointIntent = null;
        thirdIntentText = null;

        bool hasCurve =
            root.TryGetProperty("thirdCurveIndex", out JsonElement curveElement);

        bool hasIntent =
            root.TryGetProperty("thirdIntent", out JsonElement intentElement);

        if (!hasCurve &&
            !hasIntent)
        {
            return true;
        }

        if (!hasCurve ||
            !hasIntent)
        {
            diagnostics.Add(new { scope = "input.third", message = "thirdCurveIndex and thirdIntent must be supplied together." });
            return false;
        }

        if (!curveElement.TryGetInt32(out int parsedIndex) ||
            parsedIndex < 1)
        {
            diagnostics.Add(new { scope = "input.thirdCurveIndex", message = "thirdCurveIndex must be a positive 1-based integer." });
            return false;
        }

        if (intentElement.ValueKind != JsonValueKind.String)
        {
            diagnostics.Add(new { scope = "input.thirdIntent", message = "thirdIntent must be a string." });
            return false;
        }

        string text =
            intentElement.GetString()?.Trim() ?? string.Empty;

        if (!DimensionCommandSupport.TryParsePointIntent(
                text,
                out PointIntentEnum intent))
        {
            diagnostics.Add(new { scope = "input.thirdIntent", message = "Unsupported point intent.", supported = new[] { "start", "end", "mid", "middle", "center" } });
            return false;
        }

        thirdCurveIndex =
            parsedIndex;

        thirdPointIntent =
            intent;

        thirdIntentText =
            text;

        return true;
    }

    private static DrawingCurve? GetCurve(
        List<DrawingCurve> curves,
        int index,
        string inputName,
        List<object> diagnostics)
    {
        if (index < 1 ||
            index > curves.Count)
        {
            diagnostics.Add(new { scope = $"input.{inputName}", message = "Curve index is outside the drawing view curve collection.", index, count = curves.Count });
            return null;
        }

        return curves[index - 1];
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

    private static bool TryGetRequiredInt(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out int value)
    {
        value = 0;

        if (!DimensionCommandSupport.TryGetRequiredInt32(root, name, out value, out string error))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = error });
            return false;
        }

        if (value < 1)
        {
            diagnostics.Add(new { scope = $"input.{name}", message = $"{name} must be a positive 1-based integer." });
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

    private static bool TryGetRequiredIntent(
        JsonElement root,
        string name,
        List<object> diagnostics,
        out PointIntentEnum intent,
        out string text)
    {
        intent = default;
        text = string.Empty;

        if (!TryGetRequiredString(root, name, diagnostics, out text))
        {
            return false;
        }

        if (!DimensionCommandSupport.TryParsePointIntent(
                text,
                out intent))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Unsupported point intent.", supported = new[] { "start", "end", "mid", "middle", "center" } });
            return false;
        }

        return true;
    }

    private static bool TryGetOptionalBoolean(
        JsonElement root,
        string name,
        bool defaultValue,
        List<object> diagnostics,
        out bool value)
    {
        value = defaultValue;

        if (!root.TryGetProperty(name, out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            diagnostics.Add(new { scope = $"input.{name}", message = "Value must be boolean." });
            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    private static string? TryGetReferenceKey(
        DrawingDocument drawingDocument,
        AngularGeneralDimension dimension,
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
            diagnostics.Add(new { scope = "AngularGeneralDimension.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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
