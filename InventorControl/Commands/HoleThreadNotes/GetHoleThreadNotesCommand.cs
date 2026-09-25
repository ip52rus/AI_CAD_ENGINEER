using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetHoleThreadNotesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetHoleThreadNotesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_hole_thread_notes";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            HoleThreadNoteCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!HoleThreadNoteCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    sheetNameError);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        HoleThreadNotes notes;

        try
        {
            notes =
                sheet
                    .DrawingNotes
                    .HoleThreadNotes;
        }
        catch (Exception exception)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    "Unable to read Sheet.DrawingNotes.HoleThreadNotes.",
                    exception.Message);
        }

        List<object> result =
            new();

        List<object> diagnostics =
            new();

        int rawCount;

        try
        {
            rawCount =
                notes.Count;
        }
        catch (Exception exception)
        {
            return HoleThreadNoteCommandSupport
                .CreateError(
                    "Unable to read HoleThreadNotes.Count.",
                    exception.Message);
        }

        for (int index = 1;
             index <= rawCount;
             index++)
        {
            try
            {
                HoleThreadNote note =
                    notes[index];

                result.Add(
                    ReadHoleThreadNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
            catch (Exception exception)
            {
                diagnostics.Add(
                    new
                    {
                        scope =
                            "HoleThreadNotes.Item",
                        index,
                        available =
                            false,
                        error =
                            exception.Message,
                        exceptionType =
                            exception
                                .GetType()
                                .FullName
                    });
            }
        }

        return HoleThreadNoteCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    rawCount,

                    noteCount =
                        result.Count,

                    notes =
                        result,

                    diagnostics
                });
    }

    private static object ReadHoleThreadNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        HoleThreadNote note,
        int index)
    {
        List<object> propertyDiagnostics =
            new();

        DimensionText? textObject =
            ReadComObject(
                propertyDiagnostics,
                "Text",
                () => note.Text);

        string? text =
            textObject == null
                ? null
                : ReadString(
                    propertyDiagnostics,
                    "Text.Text",
                    () => textObject.Text);

        string? formattedText =
            textObject == null
                ? null
                : ReadString(
                    propertyDiagnostics,
                    "Text.FormattedText",
                    () => textObject.FormattedText);

        object? origin =
            textObject == null
                ? null
                : ReadPoint2d(
                    ReadComObject(
                        propertyDiagnostics,
                        "Text.Origin",
                        () => textObject.Origin));

        object? rangeBox =
            textObject == null
                ? null
                : ReadBox2d(
                    ReadComObject(
                        propertyDiagnostics,
                        "Text.RangeBox",
                        () => textObject.RangeBox));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                note,
                propertyDiagnostics);

        object? style =
            ReadNamedObject(
                ReadComObject(
                    propertyDiagnostics,
                    "Style",
                    () => note.Style),
                propertyDiagnostics);

        object? layer =
            ReadNamedObject(
                ReadComObject(
                    propertyDiagnostics,
                    "Layer",
                    () => note.Layer),
                propertyDiagnostics);

        object? edge =
            ReadEntityMetadata(
                ReadComObject(
                    propertyDiagnostics,
                    "Edge",
                    () => note.Edge),
                propertyDiagnostics);

        object? intent =
            ReadEntityMetadata(
                ReadComObject(
                    propertyDiagnostics,
                    "Intent",
                    () => note.Intent),
                propertyDiagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadString(
                    propertyDiagnostics,
                    "Type",
                    () => note.Type.ToString()),
            objectType =
                ReadObjectTypeName(
                    ReadComObject(
                        propertyDiagnostics,
                        "Type",
                        () => note.Type),
                    propertyDiagnostics),
            parentSheet =
                new
                {
                    name =
                        sheet.Name,
                    width =
                        sheet.Width,
                    height =
                        sheet.Height
                },
            text,
            formattedText,
            formattedHoleThreadNote =
                ReadString(
                    propertyDiagnostics,
                    "FormattedHoleThreadNote",
                    () => note.FormattedHoleThreadNote),
            formattedQuantityNote =
                ReadString(
                    propertyDiagnostics,
                    "FormattedQuantityNote",
                    () => note.FormattedQuantityNote),
            position =
                origin,
            textOrigin =
                origin,
            origin,
            rangeBox,
            isHoleNote =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "IsHoleNote",
                    () => note.IsHoleNote),
            attached =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "Attached",
                    () => note.Attached),
            rightHandedThread =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "RightHandedThread",
                    () => note.RightHandedThread),
            useCustomThreadDesignation =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "UseCustomThreadDesignation",
                    () => note.UseCustomThreadDesignation),
            arrowheadsInside =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ArrowheadsInside",
                    () => note.ArrowheadsInside),
            leaderFromCenter =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "LeaderFromCenter",
                    () => note.LeaderFromCenter),
            singleDimensionLine =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "SingleDimensionLine",
                    () => note.SingleDimensionLine),
            useDefaultFormat =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "UseDefaultFormat",
                    () => note.UseDefaultFormat),
            usePartUnits =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "UsePartUnits",
                    () => note.UsePartUnits),
            tapDrill =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "TapDrill",
                    () => note.TapDrill),
            layer,
            style,
            dimensionStyle =
                style,
            edge,
            intent,
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "hole_thread_note",
                    position =
                        origin,
                    text
                },
            propertyDiagnostics
        };
    }

    private static T? ReadComObject<T>(
        List<object> diagnostics,
        string propertyName,
        Func<T> read)
        where T : class
    {
        try
        {
            return read();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static object? ReadComObject(
        List<object> diagnostics,
        string propertyName,
        Func<object> read)
    {
        try
        {
            return read();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static string? ReadString(
        List<object> diagnostics,
        string propertyName,
        Func<object?> read)
    {
        try
        {
            return read()?.ToString();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static bool? ReadNullableBoolean(
        List<object> diagnostics,
        string propertyName,
        Func<bool> read)
    {
        try
        {
            return read();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static object? ReadReferenceKey(
        DrawingDocument drawingDocument,
        HoleThreadNote note,
        List<object> diagnostics)
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
                Array.CreateInstance(
                    typeof(byte),
                    0);

            note.GetReferenceKey(
                ref referenceKey,
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
            AddDiagnostic(
                diagnostics,
                "HoleThreadNote.GetReferenceKey",
                exception);

            return null;
        }
        finally
        {
            if (keyContext != 0)
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
                    AddDiagnostic(
                        diagnostics,
                        "ReferenceKeyManager.ReleaseKeyContext",
                        exception);
                }
            }
        }
    }

    private static object? ReadNamedObject(
        object? namedObject,
        List<object> diagnostics)
    {
        if (namedObject == null)
        {
            return null;
        }

        dynamic value =
            namedObject;

        return new
        {
            name =
                ReadString(
                    diagnostics,
                    "Name",
                    () => value.Name),
            objectTypeRaw =
                ReadString(
                    diagnostics,
                    "Type",
                    () => value.Type),
            objectType =
                ReadObjectTypeName(
                    ReadComObject(
                        diagnostics,
                        "Type",
                        () => value.Type),
                    diagnostics)
        };
    }

    private static object? ReadEntityMetadata(
        object? entityObject,
        List<object> diagnostics)
    {
        if (entityObject == null)
        {
            return null;
        }

        dynamic value =
            entityObject;

        object? modelGeometry =
            ReadComObject(
                diagnostics,
                "ModelGeometry",
                () => value.ModelGeometry);

        return new
        {
            runtimeType =
                entityObject
                    .GetType()
                    .Name,
            objectTypeRaw =
                ReadString(
                    diagnostics,
                    "Type",
                    () => value.Type),
            objectType =
                ReadObjectTypeName(
                    ReadComObject(
                        diagnostics,
                        "Type",
                        () => value.Type),
                    diagnostics),
            name =
                ReadString(
                    diagnostics,
                    "Name",
                    () => value.Name),
            curveType =
                ReadString(
                    diagnostics,
                    "CurveType",
                    () => value.CurveType),
            edgeType =
                ReadString(
                    diagnostics,
                    "EdgeType",
                    () => value.EdgeType),
            projectedCurveType =
                ReadString(
                    diagnostics,
                    "ProjectedCurveType",
                    () => value.ProjectedCurveType),
            modelGeometryType =
                modelGeometry?
                    .GetType()
                    .Name
        };
    }

    private static object? ReadObjectTypeName(
        object? value,
        List<object> diagnostics)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            return Enum.GetName(
                       typeof(ObjectTypeEnum),
                       Convert.ToInt32(value))
                   ?? value.ToString();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "ObjectTypeEnum",
                exception);

            return value.ToString();
        }
    }

    private static object? ReadPoint2d(
        object? pointObject)
    {
        if (pointObject == null)
        {
            return null;
        }

        dynamic point =
            pointObject;

        try
        {
            return new
            {
                x =
                    (double)point.X,
                y =
                    (double)point.Y
            };
        }
        catch (Exception exception)
        {
            return new
            {
                unavailable =
                    true,
                error =
                    exception.Message
            };
        }
    }

    private static object? ReadBox2d(
        object? boxObject)
    {
        if (boxObject == null)
        {
            return null;
        }

        dynamic box =
            boxObject;

        try
        {
            return new
            {
                minPoint =
                    ReadPoint2d(
                        box.MinPoint),
                maxPoint =
                    ReadPoint2d(
                        box.MaxPoint)
            };
        }
        catch (Exception exception)
        {
            return new
            {
                unavailable =
                    true,
                error =
                    exception.Message
            };
        }
    }

    private static void AddDiagnostic(
        List<object> diagnostics,
        string propertyName,
        Exception exception)
    {
        diagnostics.Add(
            new
            {
                property =
                    propertyName,
                available =
                    false,
                error =
                    exception.Message,
                exceptionType =
                    exception
                        .GetType()
                        .FullName
            });
    }
}
