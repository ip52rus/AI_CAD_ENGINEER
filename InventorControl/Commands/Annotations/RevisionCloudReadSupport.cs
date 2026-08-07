using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class RevisionCloudReadSupport
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

        RevisionClouds? clouds =
            ReadComObject(
                diagnostics,
                "Sheet.RevisionClouds",
                () => sheet.RevisionClouds);

        List<object> items =
            new();

        int? rawCount =
            null;

        if (clouds != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "RevisionClouds.Count",
                    () => clouds.Count);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    RevisionCloud? cloud =
                        ReadComObject(
                            diagnostics,
                            $"RevisionClouds.Item[{index}]",
                            () => clouds[index]);

                    if (cloud == null)
                    {
                        continue;
                    }

                    items.Add(
                        ReadRevisionCloud(
                            drawingDocument,
                            sheet,
                            cloud,
                            index));
                }
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
                    "revision_clouds",

                rawCount,

                count =
                    items.Count,

                items,

                diagnostics
            });
    }

    private static object ReadRevisionCloud(
        DrawingDocument drawingDocument,
        Sheet sheet,
        RevisionCloud cloud,
        int index)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => cloud.Position));

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

                    cloud.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        RevisionCloudDefinitionReadResult definition =
            ReadDefinition(
                cloud);

        return new
        {
            index,

            indexIsStable =
                false,

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => cloud.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => cloud.Type),

            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "Name",
                        () => cloud.Name)),

            parentSheet =
                ReadSheetMetadata(
                    sheet),

            position,

            origin =
                position,

            rangeBox =
                ReadBox2d(
                    ReadComObject(
                        diagnostics,
                        "RangeBox",
                        () => cloud.RangeBox)),

            sketch =
                ReadSketchMetadata(
                    drawingDocument,
                    diagnostics,
                    "Sketch",
                    () => cloud.Sketch),

            definition =
                definition.Item,

            controlPointRawCount =
                definition.ControlPointRawCount,

            controlPointCount =
                definition.ControlPoints.Count,

            controlPoints =
                definition.ControlPoints,

            definitionDiagnostics =
                definition.Diagnostics,

            referenceKey,

            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "revision_cloud",
                    name =
                        ReadString(
                            ReadProperty(
                                diagnostics,
                                "Name",
                                () => cloud.Name)),
                    position
                },

            propertyDiagnostics =
                diagnostics
        };
    }

    private static RevisionCloudDefinitionReadResult ReadDefinition(
        RevisionCloud cloud)
    {
        List<object> diagnostics =
            new();

        RevisionCloudDefinition? definition =
            ReadComObject(
                diagnostics,
                "RevisionCloud.Definition",
                () => cloud.Definition);

        List<object> controlPoints =
            new();

        int? controlPointRawCount =
            null;

        if (definition == null)
        {
            return new RevisionCloudDefinitionReadResult(
                null,
                controlPointRawCount,
                controlPoints,
                diagnostics);
        }

        RevisionCloudControlPoints? points =
            ReadComObject(
                diagnostics,
                "RevisionCloudDefinition.ControlPoints",
                () => definition.ControlPoints);

        if (points != null)
        {
            controlPointRawCount =
                ReadNullableInt32(
                    diagnostics,
                    "RevisionCloudControlPoints.Count",
                    () => points.Count);

            if (controlPointRawCount.HasValue)
            {
                for (int index = 1;
                     index <= controlPointRawCount.Value;
                     index++)
                {
                    RevisionCloudControlPoint? point =
                        ReadComObject(
                            diagnostics,
                            $"RevisionCloudControlPoints.Item[{index}]",
                            () => points[index]);

                    if (point == null)
                    {
                        continue;
                    }

                    controlPoints.Add(
                        ReadControlPoint(
                            point,
                            index));
                }
            }
        }

        object item =
            new
            {
                objectTypeRaw =
                    ReadObjectTypeRaw(
                        diagnostics,
                        "Definition.Type",
                        () => definition.Type),

                objectType =
                    ReadObjectTypeName(
                        diagnostics,
                        "Definition.Type",
                        () => definition.Type),

                inverted =
                    ReadNullableBoolean(
                        diagnostics,
                        "Definition.Inverted",
                        () => definition.Inverted),

                layer =
                    ReadLayer(
                        diagnostics,
                        "Definition.Layer",
                        () => definition.Layer),

                minimumArcRadius =
                    ReadNullableDouble(
                        diagnostics,
                        "Definition.MinimumArcRadius",
                        () => definition.MinimumArcRadius),

                maximumArcRadius =
                    ReadNullableDouble(
                        diagnostics,
                        "Definition.MaximumArcRadius",
                        () => definition.MaximumArcRadius),

                setValuesAsDefault =
                    ReadNullableBoolean(
                        diagnostics,
                        "Definition.SetValuesAsDefault",
                        () => definition.SetValuesAsDefault),

                controlPointRawCount,

                controlPointCount =
                    controlPoints.Count,

                controlPoints,

                propertyDiagnostics =
                    diagnostics
            };

        return new RevisionCloudDefinitionReadResult(
            item,
            controlPointRawCount,
            controlPoints,
            diagnostics);
    }

    private static object ReadControlPoint(
        RevisionCloudControlPoint point,
        int fallbackIndex)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index =
                ReadNullableInt32(
                    diagnostics,
                    "Index",
                    () => point.Index)
                ?? fallbackIndex,

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => point.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => point.Type),

            position =
                ReadPoint2d(
                    ReadComObject(
                        diagnostics,
                        "Position",
                        () => point.Position)),

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

    private static object? ReadSketchMetadata(
        DrawingDocument drawingDocument,
        List<object> diagnostics,
        string propertyName,
        Func<DrawingSketch> reader)
    {
        DrawingSketch? sketch =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (sketch == null)
        {
            return null;
        }

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

                    sketch.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Name",
                        () => sketch.Name)),
            visible =
                ReadNullableBoolean(
                    diagnostics,
                    $"{propertyName}.Visible",
                    () => sketch.Visible),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => sketch.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => sketch.Type),
            referenceKey
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

    private static object? ReadBox2d(
        Box2d? box)
    {
        if (box == null)
        {
            return null;
        }

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

    private static double? ReadNullableDouble(
        List<object> diagnostics,
        string propertyName,
        Func<double> reader)
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

internal sealed record RevisionCloudDefinitionReadResult(
    object? Item,
    int? ControlPointRawCount,
    List<object> ControlPoints,
    List<object> Diagnostics);
