using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class GeneralNoteCommandSupport
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

    public static GeneralNote? ResolveGeneralNote(
        Sheet sheet,
        int noteIndex,
        List<object> diagnostics)
    {
        GeneralNotes notes;

        try
        {
            notes =
                sheet
                    .DrawingNotes
                    .GeneralNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.GeneralNotes", message = exception.Message, exceptionType = exception.GetType().FullName });
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
            diagnostics.Add(new { scope = "GeneralNotes.Count", message = exception.Message, exceptionType = exception.GetType().FullName });
            return null;
        }

        if (noteIndex < 1 ||
            noteIndex > count)
        {
            diagnostics.Add(new { scope = "input.noteIndex", message = "noteIndex is outside the GeneralNotes collection.", noteIndex, count });
            return null;
        }

        try
        {
            return notes[noteIndex];
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralNotes.Item", message = exception.Message, exceptionType = exception.GetType().FullName, noteIndex });
            return null;
        }
    }

    public static object ReadGeneralNoteFacts(
        DrawingDocument drawingDocument,
        Sheet sheet,
        GeneralNote note,
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
                    "GeneralNote.Text",
                    () => note.Text),
            formattedText =
                ReadString(
                    propertyDiagnostics,
                    "GeneralNote.FormattedText",
                    () => note.FormattedText),
            position =
                ReadPoint2d(
                    propertyDiagnostics,
                    "GeneralNote.Position",
                    () => note.Position),
            rangeBox =
                ReadBox2d(
                    propertyDiagnostics,
                    "GeneralNote.RangeBox",
                    () => note.RangeBox),
            layer =
                ReadNamedObject(
                    propertyDiagnostics,
                    "GeneralNote.Layer",
                    () => note.Layer),
            textStyle =
                ReadNamedObject(
                    propertyDiagnostics,
                    "GeneralNote.TextStyle",
                    () => note.TextStyle),
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

    private static string? SafeReadDynamicString(
        dynamic value,
        string propertyName)
    {
        try
        {
            return propertyName switch
            {
                "Name" =>
                    value.Name,
                "InternalName" =>
                    value.InternalName,
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
            diagnostics.Add(new { scope = "GeneralNote.GetReferenceKey", message = exception.Message, exceptionType = exception.GetType().FullName });
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
