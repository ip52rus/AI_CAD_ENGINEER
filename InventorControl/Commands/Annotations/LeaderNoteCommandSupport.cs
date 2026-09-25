using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class LeaderNoteCommandSupport
{
    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    true,
                data
            },
            CreateJsonOptions());
    }

    public static string CreateError(
        string error,
        List<object> diagnostics)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,
                error,
                diagnostics
            },
            CreateJsonOptions());
    }

    public static bool TryGetRequiredStringAllowEmpty(
        JsonElement root,
        string propertyName,
        List<object> diagnostics,
        out string value)
    {
        value =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"Required field \"{propertyName}\" is missing." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            diagnostics.Add(new { scope = $"input.{propertyName}", message = $"Field \"{propertyName}\" must be a string." });
            return false;
        }

        value =
            element.GetString()
            ?? string.Empty;

        return true;
    }

    public static bool TryGetLeaderPoints(
        JsonElement root,
        List<object> diagnostics,
        out List<(double X, double Y)> points)
    {
        points =
            new List<(double X, double Y)>();

        if (!root.TryGetProperty(
                "leaderPoints",
                out JsonElement element))
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints is required." });
            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.Array)
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints must be an array." });
            return false;
        }

        int index =
            0;

        foreach (JsonElement pointElement in element.EnumerateArray())
        {
            if (pointElement.ValueKind !=
                JsonValueKind.Object)
            {
                diagnostics.Add(new { scope = $"input.leaderPoints[{index}]", message = "Leader point must be an object." });
                return false;
            }

            if (!TryGetFiniteDouble(pointElement, "x", $"input.leaderPoints[{index}].x", diagnostics, out double x) ||
                !TryGetFiniteDouble(pointElement, "y", $"input.leaderPoints[{index}].y", diagnostics, out double y))
            {
                return false;
            }

            points.Add(
                (x, y));

            index++;
        }

        if (points.Count == 0)
        {
            diagnostics.Add(new { scope = "input.leaderPoints", message = "leaderPoints must contain at least one point." });
            return false;
        }

        return true;
    }

    public static bool TryResolveOptionalAttachmentInput(
        JsonElement root,
        List<object> diagnostics,
        out bool hasAttachment,
        out string viewName,
        out int curveIndex,
        out string intentText,
        out PointIntentEnum pointIntent)
    {
        hasAttachment =
            false;
        viewName =
            string.Empty;
        curveIndex =
            0;
        intentText =
            string.Empty;
        pointIntent =
            default;

        bool hasViewName =
            root.TryGetProperty(
                "viewName",
                out JsonElement viewNameElement);

        bool hasCurveIndex =
            root.TryGetProperty(
                "curveIndex",
                out JsonElement curveIndexElement);

        bool hasIntent =
            root.TryGetProperty(
                "intent",
                out JsonElement intentElement);

        if (!hasViewName &&
            !hasCurveIndex &&
            !hasIntent)
        {
            return true;
        }

        hasAttachment =
            true;

        bool valid =
            true;

        if (!hasViewName ||
            viewNameElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(viewNameElement.GetString()))
        {
            diagnostics.Add(new { scope = "input.viewName", message = "viewName is required as a non-empty string when attachment is supplied." });
            valid =
                false;
        }
        else
        {
            viewName =
                viewNameElement.GetString()!.Trim();
        }

        if (!hasCurveIndex ||
            !curveIndexElement.TryGetInt32(out curveIndex) ||
            curveIndex < 1)
        {
            diagnostics.Add(new { scope = "input.curveIndex", message = "curveIndex is required as a positive 1-based integer when attachment is supplied." });
            valid =
                false;
        }

        if (!hasIntent ||
            intentElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(intentElement.GetString()))
        {
            diagnostics.Add(new { scope = "input.intent", message = "intent is required as a non-empty string when attachment is supplied." });
            valid =
                false;
        }
        else
        {
            intentText =
                intentElement.GetString()!.Trim();

            if (!DimensionCommandSupport.TryParsePointIntent(
                    intentText,
                    out pointIntent))
            {
                diagnostics.Add(new { scope = "input.intent", message = "Unsupported point intent.", supported = new[] { "start", "end", "mid", "middle", "center" } });
                valid =
                    false;
            }
        }

        return valid;
    }

    public static LeaderNote? ResolveLeaderNote(
        Sheet sheet,
        int noteIndex,
        List<object> diagnostics)
    {
        LeaderNotes notes;

        try
        {
            notes =
                sheet
                    .DrawingNotes
                    .LeaderNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.LeaderNotes", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        int count;

        try
        {
            count =
                notes.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNotes.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (noteIndex < 1 ||
            noteIndex > count)
        {
            diagnostics.Add(new { scope = "input.noteIndex", message = "noteIndex is outside the LeaderNotes collection.", noteIndex, count });
            return null;
        }

        try
        {
            return notes[noteIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNotes.Item", message = exception.Message, exceptionType = exception.GetType().FullName, noteIndex });
            return null;
        }
    }

    public static DrawingCurve? ResolveDrawingCurve(
        DrawingView drawingView,
        int curveIndex,
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
                diagnostics.Add(new { scope = "input.curveIndex", message = "curveIndex is outside the drawing view curve collection.", curveIndex, count });
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

    public static object ReadLeaderNoteFacts(
        DrawingDocument drawingDocument,
        Sheet sheet,
        LeaderNote note,
        int noteIndex)
    {
        List<object> propertyDiagnostics =
            new();

        return new
        {
            noteIndex,
            text =
                ReadString(
                    propertyDiagnostics,
                    "LeaderNote.Text",
                    () => note.Text),
            formattedText =
                ReadString(
                    propertyDiagnostics,
                    "LeaderNote.FormattedText",
                    () => note.FormattedText),
            position =
                ReadPoint2d(
                    propertyDiagnostics,
                    "LeaderNote.Position",
                    () => note.Position),
            rangeBox =
                ReadBox2d(
                    propertyDiagnostics,
                    "LeaderNote.RangeBox",
                    () => note.RangeBox),
            layer =
                ReadNamedObject(
                    propertyDiagnostics,
                    "LeaderNote.Layer",
                    () => note.Layer),
            dimensionStyle =
                ReadNamedObject(
                    propertyDiagnostics,
                    "LeaderNote.DimensionStyle",
                    () => note.DimensionStyle),
            attachedEntity =
                ReadGeometryIntent(
                    propertyDiagnostics,
                    "LeaderNote._AttachedEntity",
                    () => note._AttachedEntity),
            leader =
                ReadLeader(
                    propertyDiagnostics,
                    "LeaderNote.Leader",
                    () => note.Leader),
            referenceKey =
                ReadReferenceKey(
                    drawingDocument,
                    propertyDiagnostics,
                    keyContext =>
                    {
                        Array referenceKey =
                            Array.CreateInstance(
                                typeof(byte),
                                0);

                        note.GetReferenceKey(
                            ref referenceKey,
                            keyContext);

                        return referenceKey;
                    }),
            parentSheet =
                new
                {
                    name =
                        sheet.Name
                },
            propertyDiagnostics
        };
    }

    public static object FormatLeaderPoints(
        List<(double X, double Y)> points)
    {
        return points
            .Select(
                point => new
                {
                    x =
                        point.X,
                    y =
                        point.Y
                })
            .ToList();
    }

    public static object? ReadPoint2d(
        List<object> diagnostics,
        string scope,
        Func<Point2d> reader)
    {
        try
        {
            Point2d point =
                reader();

            return new
            {
                x =
                    point.X,
                y =
                    point.Y
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static bool TryGetFiniteDouble(
        JsonElement element,
        string propertyName,
        string scope,
        List<object> diagnostics,
        out double value)
    {
        value =
            0.0;

        if (!element.TryGetProperty(propertyName, out JsonElement valueElement) ||
            !valueElement.TryGetDouble(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            diagnostics.Add(new { scope, message = $"{propertyName} must be a finite number." });
            return false;
        }

        return true;
    }

    private static string? ReadString(
        List<object> diagnostics,
        string scope,
        Func<string> reader)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadBox2d(
        List<object> diagnostics,
        string scope,
        Func<Box2d> reader)
    {
        try
        {
            Box2d box =
                reader();

            return new
            {
                minPoint =
                    new
                    {
                        x =
                            box.MinPoint.X,
                        y =
                            box.MinPoint.Y
                    },
                maxPoint =
                    new
                    {
                        x =
                            box.MaxPoint.X,
                        y =
                            box.MaxPoint.Y
                    }
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadNamedObject<T>(
        List<object> diagnostics,
        string scope,
        Func<T> reader)
    {
        try
        {
            dynamic value =
                reader()!;

            return new
            {
                name =
                    SafeReadDynamicString(
                        value,
                        "Name"),
                internalName =
                    SafeReadDynamicString(
                        value,
                        "InternalName"),
                objectType =
                    value.GetType().FullName
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadGeometryIntent(
        List<object> diagnostics,
        string scope,
        Func<GeometryIntent?> reader)
    {
        try
        {
            GeometryIntent? intent =
                reader();

            if (intent == null)
            {
                return null;
            }

            return new
            {
                objectType =
                    SafeReadDynamicString(
                        intent,
                        "Type"),
                intentType =
                    SafeReadDynamicString(
                        intent,
                        "IntentType"),
                pointOnSheet =
                    ReadPoint2d(
                        diagnostics,
                        $"{scope}.PointOnSheet",
                        () => intent.PointOnSheet),
                geometry =
                    ReadRuntimeType(
                        diagnostics,
                        $"{scope}.Geometry",
                        () => intent.Geometry),
                intent =
                    ReadRuntimeValue(
                        diagnostics,
                        $"{scope}.Intent",
                        () => intent.Intent)
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? ReadRuntimeType(
        List<object> diagnostics,
        string scope,
        Func<object?> reader)
    {
        try
        {
            object? value =
                reader();

            return value?.GetType().FullName;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadRuntimeValue(
        List<object> diagnostics,
        string scope,
        Func<object?> reader)
    {
        try
        {
            object? value =
                reader();

            if (value == null)
            {
                return null;
            }

            return new
            {
                value =
                    value.ToString(),
                runtimeType =
                    value.GetType().FullName
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static object? ReadLeader(
        List<object> diagnostics,
        string scope,
        Func<Leader> reader)
    {
        try
        {
            Leader leader =
                reader();

            return new
            {
                objectType =
                    SafeReadDynamicString(
                        leader,
                        "Type"),
                hasRootNode =
                    SafeReadDynamicBoolean(
                        leader,
                        "HasRootNode"),
                arrowheadType =
                    SafeReadDynamicString(
                        leader,
                        "ArrowheadType")
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope, message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
    }

    private static string? SafeReadDynamicString(
        dynamic value,
        string propertyName)
    {
        try
        {
            object? property =
                propertyName switch
                {
                    "Name" =>
                        value.Name,
                    "InternalName" =>
                        value.InternalName,
                    "Type" =>
                        value.Type,
                    "IntentType" =>
                        value.IntentType,
                    "ArrowheadType" =>
                        value.ArrowheadType,
                    _ =>
                        null
                };

            return property?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool? SafeReadDynamicBoolean(
        dynamic value,
        string propertyName)
    {
        try
        {
            return propertyName switch
            {
                "HasRootNode" =>
                    value.HasRootNode,
                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? ReadReferenceKey(
        DrawingDocument drawingDocument,
        List<object> diagnostics,
        Func<int, Array> getReferenceKey)
    {
        int keyContext =
            0;

        try
        {
            ReferenceKeyManager manager =
                drawingDocument.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            Array referenceKey =
                getReferenceKey(
                    keyContext);

            string keyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNote.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }
        finally
        {
            if (keyContext !=
                0)
            {
                try
                {
                    drawingDocument
                        .ReferenceKeyManager
                        .ReleaseKeyContext(
                            keyContext);
                }
                catch (Exception exception)
                {
                    diagnostics.Add(new { scope = "ReferenceKeyManager.ReleaseKeyContext", message = exception.Message, exceptionType = exception.GetType().FullName });
                }
            }
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
