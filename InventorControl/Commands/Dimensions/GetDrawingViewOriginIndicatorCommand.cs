using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class GetDrawingViewOriginIndicatorCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDrawingViewOriginIndicatorCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_drawing_view_origin_indicator";

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

        bool hasOriginIndicator;

        try
        {
            hasOriginIndicator =
                drawingView.HasOriginIndicator;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.HasOriginIndicator", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CreateError("Failed to read drawing view origin indicator state.", diagnostics);
        }

        return CreateSuccess(new
        {
            capability = "drawing_view_origin_indicator",
            document = drawingDocument.DisplayName,
            sheet = sheet.Name,
            view = drawingView.Name,
            hasOriginIndicator,
            originIndicator = hasOriginIndicator
                ? ReadOriginIndicator(drawingView, diagnostics)
                : null,
            viewReferenceKey = TryGetViewReferenceKey(drawingDocument, drawingView, diagnostics),
            diagnostics
        });
    }

    private static object? ReadOriginIndicator(
        DrawingView drawingView,
        List<object> diagnostics)
    {
        List<object> propertyDiagnostics = new();

        OriginIndicator? originIndicator;

        try
        {
            originIndicator =
                drawingView.OriginIndicator;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.OriginIndicator", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (originIndicator == null)
        {
            diagnostics.Add(new { scope = "DrawingView.OriginIndicator", message = "HasOriginIndicator was true, but OriginIndicator returned null." });
            return null;
        }

        ObjectTypeEnum? objectTypeRaw =
            SafeRead(
                () => originIndicator.Type,
                "OriginIndicator.Type",
                propertyDiagnostics);

        return new
        {
            objectTypeRaw =
                objectTypeRaw?.ToString(),
            objectType =
                objectTypeRaw.HasValue
                    ? Enum.GetName(typeof(ObjectTypeEnum), Convert.ToInt32(objectTypeRaw.Value)) ?? objectTypeRaw.Value.ToString()
                    : null,
            attached =
                SafeReadNullableBoolean(
                    () => originIndicator.Attached,
                    "OriginIndicator.Attached",
                    propertyDiagnostics),
            visible =
                SafeReadNullableBoolean(
                    () => originIndicator.Visible,
                    "OriginIndicator.Visible",
                    propertyDiagnostics),
            intent =
                ReadGeometryIntent(
                    SafeRead(
                        () => originIndicator.Intent,
                        "OriginIndicator.Intent",
                        propertyDiagnostics),
                    propertyDiagnostics),
            layer =
                ReadNamedObject(
                    SafeRead(
                        () => originIndicator.Layer,
                        "OriginIndicator.Layer",
                        propertyDiagnostics),
                    propertyDiagnostics),
            leaderStyle =
                ReadNamedObject(
                    SafeRead(
                        () => originIndicator.LeaderStyle,
                        "OriginIndicator.LeaderStyle",
                        propertyDiagnostics),
                    propertyDiagnostics),
            referenceKey =
                (string?)null,
            propertyDiagnostics
        };
    }

    private static object? ReadGeometryIntent(
        GeometryIntent? intent,
        List<object> diagnostics)
    {
        if (intent == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                SafeRead(
                    () => intent.Type.ToString(),
                    "GeometryIntent.Type",
                    diagnostics),
            intentTypeRaw =
                SafeRead(
                    () => intent.IntentType.ToString(),
                    "GeometryIntent.IntentType",
                    diagnostics),
            geometry =
                ReadObjectMetadata(
                    SafeRead(
                        () => intent.Geometry,
                        "GeometryIntent.Geometry",
                        diagnostics),
                    diagnostics),
            intent =
                ReadObjectMetadata(
                    SafeRead(
                        () => intent.Intent,
                        "GeometryIntent.Intent",
                        diagnostics),
                    diagnostics)
        };
    }

    private static object? ReadNamedObject(
        object? value,
        List<object> diagnostics)
    {
        if (value == null)
        {
            return null;
        }

        return new
        {
            objectType =
                value.GetType().Name,
            name =
                SafeReadString(
                    value,
                    "Name",
                    diagnostics),
            internalName =
                SafeReadString(
                    value,
                    "InternalName",
                    diagnostics)
        };
    }

    private static object? ReadObjectMetadata(
        object? value,
        List<object> diagnostics)
    {
        if (value == null)
        {
            return null;
        }

        return new
        {
            objectType =
                value.GetType().Name,
            name =
                SafeReadString(
                    value,
                    "Name",
                    diagnostics),
            type =
                SafeReadString(
                    value,
                    "Type",
                    diagnostics)
        };
    }

    private static string? SafeReadString(
        object owner,
        string propertyName,
        List<object> diagnostics)
    {
        try
        {
            object? value =
                owner
                    .GetType()
                    .InvokeMember(
                        propertyName,
                        System.Reflection.BindingFlags.GetProperty,
                        null,
                        owner,
                        null);

            return value?.ToString();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = $"{owner.GetType().Name}.{propertyName}", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static T? SafeRead<T>(
        Func<T> read,
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
            return default;
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

    private static string? TryGetViewReferenceKey(
        DrawingDocument drawingDocument,
        DrawingView drawingView,
        List<object> diagnostics)
    {
        try
        {
            Array key =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            drawingView.GetReferenceKey(
                ref key,
                0);

            return drawingDocument.ReferenceKeyManager.KeyToString(
                ref key);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "DrawingView.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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
