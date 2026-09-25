using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateCenterlineBisectorCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateCenterlineBisectorCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_centerline_bisector";

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

        if (!TryGetRequiredIndex(root, "firstCurveIndex", diagnostics, out int firstCurveIndex) ||
            !TryGetRequiredIndex(root, "secondCurveIndex", diagnostics, out int secondCurveIndex))
        {
            return CreateError("Invalid centerline bisector input.", diagnostics);
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

        DrawingCurve? firstCurve =
            TryGetDrawingCurve(
                drawingView,
                firstCurveIndex,
                "input.firstCurveIndex",
                diagnostics);

        DrawingCurve? secondCurve =
            TryGetDrawingCurve(
                drawingView,
                secondCurveIndex,
                "input.secondCurveIndex",
                diagnostics);

        if (firstCurve == null ||
            secondCurve == null)
        {
            return CreateError("One or more drawing curves were not found.", diagnostics);
        }

        try
        {
            GeometryIntent firstIntent =
                sheet.CreateGeometryIntent(
                    firstCurve);

            GeometryIntent secondIntent =
                sheet.CreateGeometryIntent(
                    secondCurve);

            Centerline centerline =
                sheet.Centerlines.AddBisector(
                    firstIntent,
                    secondIntent,
                    Type.Missing,
                    Type.Missing);

            return CreateSuccess(new
            {
                capability = "create_centerline_bisector",
                document = drawingDocument.DisplayName,
                sheet = sheet.Name,
                view = drawingView.Name,
                firstCurveIndex,
                secondCurveIndex,
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
            diagnostics.Add(new { scope = "Centerlines.AddBisector", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to create centerline bisector.", diagnostics);
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
}
