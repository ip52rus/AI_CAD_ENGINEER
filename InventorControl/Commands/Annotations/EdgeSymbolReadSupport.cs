using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class EdgeSymbolReadSupport
{
    internal static object ReadEdgeSymbolSnapshot(
        DrawingDocument drawingDocument,
        Sheet sheet,
        EdgeSymbol symbol,
        int index)
    {
        return ReadEdgeSymbol(
            drawingDocument,
            sheet,
            symbol,
            index);
    }

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

        EdgeSymbols? symbols =
            ReadComObject(
                diagnostics,
                "Sheet.EdgeSymbols",
                () => sheet.EdgeSymbols);

        List<object> items =
            new();

        int? rawCount =
            null;

        if (symbols != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "EdgeSymbols.Count",
                    () => symbols.Count);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    EdgeSymbol? symbol =
                        ReadComObject(
                            diagnostics,
                            $"EdgeSymbols.Item[{index}]",
                            () => symbols[index]);

                    if (symbol == null)
                    {
                        continue;
                    }

                    items.Add(
                        ReadEdgeSymbol(
                            drawingDocument,
                            sheet,
                            symbol,
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
                    "edge_symbols",

                rawCount,

                count =
                    items.Count,

                items,

                diagnostics
            });
    }

    private static object ReadEdgeSymbol(
        DrawingDocument drawingDocument,
        Sheet sheet,
        EdgeSymbol symbol,
        int index)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => symbol.Position));

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

                    symbol.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        EdgeSymbolDefinitionReadResult definition =
            ReadDefinition(
                symbol);

        return new
        {
            index,

            indexIsStable =
                false,

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => symbol.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => symbol.Type),

            parentSheet =
                ReadSheetMetadata(
                    sheet),

            position,

            origin =
                position,

            definition =
                definition.Item,

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
                        "edge_symbol",
                    position,
                    definition =
                        definition.SelectorSnapshot
                },

            propertyDiagnostics =
                diagnostics
        };
    }

    private static EdgeSymbolDefinitionReadResult ReadDefinition(
        EdgeSymbol symbol)
    {
        List<object> diagnostics =
            new();

        EdgeSymbolDefinition? definition =
            ReadComObject(
                diagnostics,
                "EdgeSymbol.Definition",
                () => symbol.Definition);

        if (definition == null)
        {
            return new EdgeSymbolDefinitionReadResult(
                null,
                null,
                diagnostics);
        }

        EdgeSymbolIndicationTypeEnum? indicationType =
            ReadNullableEnum(
                diagnostics,
                "Definition.IndicationType",
                () => definition.IndicationType);

        EdgeSymbolValuePositionTypeEnum? valuePositionType =
            ReadNullableEnum(
                diagnostics,
                "Definition.ValuePositionType",
                () => definition.ValuePositionType);

        string edges =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Definition.Edges",
                    () => definition.Edges));

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

                edges,

                indicationTypeRaw =
                    ReadEnumRaw(
                        indicationType),

                indicationType =
                    ReadEnumName(
                        indicationType),

                horizontalValue =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Definition.HorizontalValue",
                            () => definition.HorizontalValue)),

                horizontalValueLower =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Definition.HorizontalValueLower",
                            () => definition.HorizontalValueLower)),

                verticalValue =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Definition.VerticalValue",
                            () => definition.VerticalValue)),

                verticalValueLower =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Definition.VerticalValueLower",
                            () => definition.VerticalValueLower)),

                undefinedValue =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Definition.UndefinedValue",
                            () => definition.UndefinedValue)),

                undefinedValueLower =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Definition.UndefinedValueLower",
                            () => definition.UndefinedValueLower)),

                rangeOfValues =
                    ReadNullableBoolean(
                        diagnostics,
                        "Definition.RangeOfValues",
                        () => definition.RangeOfValues),

                referenceToISO =
                    ReadNullableBoolean(
                        diagnostics,
                        "Definition.ReferenceToISO",
                        () => definition.ReferenceToISO),

                sidesDefined =
                    ReadNullableBoolean(
                        diagnostics,
                        "Definition.SidesDefined",
                        () => definition.SidesDefined),

                statesOfAllEdgesAroundProfile =
                    ReadNullableBoolean(
                        diagnostics,
                        "Definition.StatesOfAllEdgesAroundProfile",
                        () => definition.StatesOfAllEdgesAroundProfile),

                valuePositionTypeRaw =
                    ReadEnumRaw(
                        valuePositionType),

                valuePositionType =
                    ReadEnumName(
                        valuePositionType),

                layer =
                    ReadLayer(
                        diagnostics,
                        "Definition.Layer",
                        () => definition.Layer),

                style =
                    ReadEdgeSymbolStyle(
                        diagnostics,
                        "Definition.Style",
                        () => definition.Style),

                propertyDiagnostics =
                    diagnostics
            };

        return new EdgeSymbolDefinitionReadResult(
            item,
            new
            {
                edges,
                indicationType =
                    ReadEnumName(
                        indicationType),
                valuePositionType =
                    ReadEnumName(
                        valuePositionType)
            },
            diagnostics);
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

    private static object? ReadEdgeSymbolStyle(
        List<object> diagnostics,
        string propertyName,
        Func<EdgeSymbolStyle> reader)
    {
        EdgeSymbolStyle? style =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (style == null)
        {
            return null;
        }

        object? referenceKey =
            null;

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
                    () => style.Type),
            referenceKey
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

internal sealed record EdgeSymbolDefinitionReadResult(
    object? Item,
    object? SelectorSnapshot,
    List<object> Diagnostics);
