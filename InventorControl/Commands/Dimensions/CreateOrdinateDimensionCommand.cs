using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateOrdinateDimensionCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateOrdinateDimensionCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_ordinate_dimension";

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
            !TryGetRequiredInt(root, "curveIndex", diagnostics, out int curveIndex) ||
            !TryGetRequiredIntent(root, "intent", diagnostics, out PointIntentEnum pointIntent, out string intentText) ||
            !TryGetRequiredDouble(root, "x", diagnostics, out double x) ||
            !TryGetRequiredDouble(root, "y", diagnostics, out double y) ||
            !TryGetRequiredDimensionType(root, diagnostics, out DimensionTypeEnum dimensionType, out string dimensionTypeText))
        {
            return CreateError("Invalid ordinate dimension input.", diagnostics);
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

        if (curveIndex < 1 ||
            curveIndex > curves.Count)
        {
            diagnostics.Add(new { scope = "input.curveIndex", message = "curveIndex is outside the drawing view curve collection.", curveIndex, count = curves.Count });
            return CreateError("Selected drawing curve was not found.", diagnostics);
        }

        DrawingCurve curve =
            curves[curveIndex - 1];

        try
        {
            GeometryIntent geometryIntent =
                sheet.CreateGeometryIntent(
                    curve,
                    pointIntent);

            Point2d textOrigin =
                _inventor.TransientGeometry.CreatePoint2d(
                    x,
                    y);

            OrdinateDimension dimension =
                sheet.DrawingDimensions.OrdinateDimensions.Add(
                    geometryIntent,
                    textOrigin,
                    dimensionType,
                    Type.Missing,
                    Type.Missing);

            string? referenceKey =
                TryGetReferenceKey(
                    drawingDocument,
                    dimension,
                    diagnostics);

            return CreateSuccess(new
            {
                capability = "create_ordinate_dimension",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = view.Name,
                geometrySelector = new
                {
                    curveIndex,
                    intent = intentText,
                    intentRaw = pointIntent.ToString()
                },
                position = ReadPoint(SafeRead(() => dimension.Text.Origin)) ?? new { x, y },
                text = SafeRead(() => dimension.Text.Text),
                dimensionType = dimensionTypeText,
                dimensionTypeRaw = SafeRead(() => dimension.DimensionType.ToString()),
                attached = SafeReadNullableBoolean(() => dimension.Attached),
                referenceKey,
                dirty = drawingDocument.Dirty,
                diagnostics
            });
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "OrdinateDimensions.Add",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return CreateError("Failed to create ordinate dimension.", diagnostics);
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
        OrdinateDimension dimension,
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
            diagnostics.Add(new { scope = "OrdinateDimension.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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
