using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class FeatureControlFrameReadSupport
{
    public static string Execute(
        Inventor.Application inventor,
        JsonElement root)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        DrawingDocument? drawingDocument =
            GetActiveDrawingDocument(
                inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return CreateError(
                documentError ??
                "Unable to get active drawing document.");
        }

        Sheet? sheet =
            ResolveSheet(
                drawingDocument,
                root,
                out bool usedActiveSheet,
                out string? sheetError);

        if (sheet == null)
        {
            return CreateError(
                sheetError ??
                "Unable to resolve drawing sheet.");
        }

        List<object> diagnostics =
            new();

        FeatureControlFrames? frames =
            ReadComObject(
                diagnostics,
                "Sheet.FeatureControlFrames",
                () => sheet.FeatureControlFrames);

        List<object> items =
            new();

        int? rawCount =
            null;

        if (frames != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "FeatureControlFrames.Count",
                    () => frames.Count);

            try
            {
                int index =
                    0;

                foreach (FeatureControlFrame frame
                         in frames)
                {
                    index++;

                    items.Add(
                        ReadFeatureControlFrame(
                            drawingDocument,
                            sheet,
                            frame,
                            index));
                }
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    "FeatureControlFrames.Enumeration",
                    exception);
            }
        }

        return CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    sheet.Name,

                usedActiveSheet,

                capability =
                    "feature_control_frames",

                rawCount,

                count =
                    items.Count,

                items,

                diagnostics
            });
    }

    private static object ReadFeatureControlFrame(
        DrawingDocument drawingDocument,
        Sheet sheet,
        FeatureControlFrame frame,
        int index)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => frame.Position));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    frame.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        string datumIdentifier =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "DatumIdentifier",
                    () => frame.DatumIdentifier));

        FeatureControlFrameProfileTypeEnum? profileType =
            ReadNullableEnum(
                diagnostics,
                "ProfileType",
                () => frame.ProfileType);

        FeatureControlFrameRowsReadResult rows =
            ReadRows(
                frame);

        return new
        {
            index,

            indexIsStable =
                false,

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => frame.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => frame.Type),

            parentSheet =
                ReadSheetMetadata(
                    sheet),

            position,

            origin =
                position,

            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => frame.Layer),

            style =
                ReadFeatureControlFrameStyle(
                    diagnostics,
                    "Style",
                    () => frame.Style),

            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => frame.Leader),

            datumIdentifier,

            profileTypeRaw =
                ReadEnumRaw(
                    profileType),

            profileType =
                ReadEnumName(
                    profileType),

            allAroundSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    "AllAroundSymbol",
                    () => frame.AllAroundSymbol),

            overrideMergeSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    "OverrideMergeSymbol",
                    () => frame.OverrideMergeSymbol),

            mergeSymbolOverridden =
                ReadNullableBoolean(
                    diagnostics,
                    "MergeSymbolOverridden",
                    () => frame.MergeSymbolOverridden),

            rowRawCount =
                rows.RawCount,

            rowCount =
                rows.Items.Count,

            rows =
                rows.Items,

            rowDiagnostics =
                rows.Diagnostics,

            referenceKey,

            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "feature_control_frame",
                    position,
                    datumIdentifier
                },

            propertyDiagnostics =
                diagnostics
        };
    }

    private static FeatureControlFrameRowsReadResult ReadRows(
        FeatureControlFrame frame)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        FeatureControlFrameRows? rows =
            ReadComObject(
                diagnostics,
                "FeatureControlFrameRows",
                () => frame.FeatureControlFrameRows);

        if (rows == null)
        {
            return new FeatureControlFrameRowsReadResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "FeatureControlFrameRows.Count",
                () => rows.Count);

        try
        {
            int index =
                0;

            foreach (FeatureControlFrameRow row
                     in rows)
            {
                index++;

                items.Add(
                    ReadRow(
                        row,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "FeatureControlFrameRows.Enumeration",
                exception);
        }

        return new FeatureControlFrameRowsReadResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadRow(
        FeatureControlFrameRow row,
        int index)
    {
        List<object> diagnostics =
            new();

        GeometricCharacteristicEnum? geometricCharacteristic =
            ReadNullableEnum(
                diagnostics,
                "GeometricCharacteristic",
                () => row.GeometricCharacteristic);

        return new
        {
            index,

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => row.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => row.Type),

            geometricCharacteristicRaw =
                ReadEnumRaw(
                    geometricCharacteristic),

            geometricCharacteristic =
                ReadEnumName(
                    geometricCharacteristic),

            tolerance =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "Tolerance",
                        () => row.Tolerance)),

            lowerTolerance =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "LowerTolerance",
                        () => row.LowerTolerance)),

            datumOne =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "DatumOne",
                        () => row.DatumOne)),

            datumTwo =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "DatumTwo",
                        () => row.DatumTwo)),

            datumThree =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "DatumThree",
                        () => row.DatumThree)),

            inlineNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "InlineNote",
                        () => row.InlineNote)),

            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadSheetMetadata(
        Sheet sheet)
    {
        return new
        {
            name =
                sheet.Name,
            width =
                sheet.Width,
            height =
                sheet.Height
        };
    }

    private static object? ReadPoint2d(
        Point2d? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x =
                point.X,
            y =
                point.Y
        };
    }

    private static object? ReadLayer(
        List<object> diagnostics,
        string propertyName,
        Func<Layer> reader)
    {
        Layer? layer =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (layer == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Name",
                        () => layer.Name)),
            internalName =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.InternalName",
                        () => layer.InternalName)),
            visible =
                ReadNullableBoolean(
                    diagnostics,
                    $"{propertyName}.Visible",
                    () => layer.Visible),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => layer.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => layer.Type)
        };
    }

    private static object? ReadFeatureControlFrameStyle(
        List<object> diagnostics,
        string propertyName,
        Func<FeatureControlFrameStyle> reader)
    {
        FeatureControlFrameStyle? style =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (style == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Name",
                        () => style.Name)),
            internalName =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.InternalName",
                        () => style.InternalName)),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => style.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => style.Type)
        };
    }

    private static object? ReadLeaderMetadata(
        List<object> diagnostics,
        string propertyName,
        Func<Leader> reader)
    {
        Leader? leader =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (leader == null)
        {
            return null;
        }

        ArrowheadTypeEnum? arrowheadType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.ArrowheadType",
                () => leader.ArrowheadType);

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => leader.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => leader.Type),
            arrowheadTypeRaw =
                ReadEnumRaw(
                    arrowheadType),
            arrowheadType =
                ReadEnumName(
                    arrowheadType),
            hasRootNode =
                ReadNullableBoolean(
                    diagnostics,
                    $"{propertyName}.HasRootNode",
                    () => leader.HasRootNode)
        };
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

            string referenceKeyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString =
                    referenceKeyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "GetReferenceKey",
                exception);

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
                    AddDiagnostic(
                        diagnostics,
                        "ReleaseKeyContext",
                        exception);
                }
            }
        }
    }

    private static string ReadObjectTypeRaw(
        List<object> diagnostics,
        string propertyName,
        Func<ObjectTypeEnum> reader)
    {
        ObjectTypeEnum? value =
            ReadNullableEnum(
                diagnostics,
                propertyName,
                reader);

        return ReadEnumRaw(
            value);
    }

    private static string ReadObjectTypeName(
        List<object> diagnostics,
        string propertyName,
        Func<ObjectTypeEnum> reader)
    {
        ObjectTypeEnum? value =
            ReadNullableEnum(
                diagnostics,
                propertyName,
                reader);

        return ReadEnumName(
            value);
    }

    private static T? ReadComObject<T>(
        List<object> diagnostics,
        string propertyName,
        Func<T> reader)
        where T : class
    {
        try
        {
            return reader();
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

    private static object? ReadProperty(
        List<object> diagnostics,
        string propertyName,
        Func<object?> reader)
    {
        try
        {
            return reader();
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

    private static int? ReadNullableInt32(
        List<object> diagnostics,
        string propertyName,
        Func<int> reader)
    {
        try
        {
            return reader();
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
        Func<bool> reader)
    {
        try
        {
            return reader();
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

    private static TEnum? ReadNullableEnum<TEnum>(
        List<object> diagnostics,
        string propertyName,
        Func<TEnum> reader)
        where TEnum : struct
    {
        try
        {
            return reader();
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

    private static string ReadEnumRaw<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return Convert
            .ToInt32(
                value.Value)
            .ToString();
    }

    private static string ReadEnumName<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return Enum.GetName(
                   typeof(TEnum),
                   value.Value)
               ?? value.Value.ToString()
               ?? string.Empty;
    }

    private static string ReadString(
        object? value)
    {
        return value?.ToString()
               ?? string.Empty;
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
                    exception.Message
            });
    }

    private static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        error =
            null;

        Document? activeDocument =
            inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "Inventor has no active document.";

            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Active Inventor document is not a drawing.";

            return null;
        }

        return (DrawingDocument)activeDocument;
    }

    private static Sheet? ResolveSheet(
        DrawingDocument drawingDocument,
        JsonElement root,
        out bool usedActiveSheet,
        out string? error)
    {
        usedActiveSheet =
            false;

        error =
            null;

        if (!root.TryGetProperty(
                "sheetName",
                out JsonElement sheetNameElement))
        {
            usedActiveSheet =
                true;

            return drawingDocument.ActiveSheet;
        }

        if (sheetNameElement.ValueKind !=
            JsonValueKind.String)
        {
            error =
                "Field \"sheetName\" must be a string when provided.";

            return null;
        }

        string sheetName =
            sheetNameElement.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                sheetName))
        {
            error =
                "Field \"sheetName\" must not be empty when provided.";

            return null;
        }

        foreach (Sheet sheet
                 in drawingDocument.Sheets)
        {
            if (string.Equals(
                    sheet.Name,
                    sheetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sheet;
            }
        }

        error =
            $"Sheet \"{sheetName}\" was not found.";

        return null;
    }

    private static string CreateSuccess(
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

    private static string CreateError(
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                details
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,

            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}

internal sealed record FeatureControlFrameRowsReadResult(
    int? RawCount,
    List<object> Items,
    List<object> Diagnostics);
