using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class WeldingSymbolReadSupport
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

        DrawingWeldingSymbols? symbols =
            ReadComObject(
                diagnostics,
                "Sheet.WeldingSymbols",
                () => sheet.WeldingSymbols);

        List<object> items =
            new();

        int? rawCount =
            null;

        if (symbols != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "DrawingWeldingSymbols.Count",
                    () => symbols.Count);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    DrawingWeldingSymbol? symbol =
                        ReadComObject(
                            diagnostics,
                            $"DrawingWeldingSymbols.Item[{index}]",
                            () => symbols[index]);

                    if (symbol == null)
                    {
                        continue;
                    }

                    items.Add(
                        ReadWeldingSymbol(
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
                    "welding_symbols",

                rawCount,

                count =
                    items.Count,

                items,

                diagnostics
            });
    }

    private static object ReadWeldingSymbol(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingWeldingSymbol symbol,
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

        WeldingDefinitionsReadResult definitions =
            ReadDefinitions(
                symbol);

        object? retrievedFrom =
            ReadUnknownObjectMetadata(
                ReadProperty(
                    diagnostics,
                    "RetrievedFrom",
                    () => symbol.RetrievedFrom),
                diagnostics,
                "RetrievedFrom");

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

            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => symbol.Layer),

            style =
                ReadWeldSymbolStyle(
                    diagnostics,
                    "Style",
                    () => symbol.Style),

            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => symbol.Leader),

            retrieved =
                ReadNullableBoolean(
                    diagnostics,
                    "Retrieved",
                    () => symbol.Retrieved),

            retrievedFrom,

            definition =
                definitions.FirstDefinition,

            definitionRawCount =
                definitions.RawCount,

            definitionCount =
                definitions.Items.Count,

            definitions =
                definitions.Items,

            definitionDiagnostics =
                definitions.Diagnostics,

            weldSymbolOne =
                definitions.FirstWeldSymbolOne,

            weldSymbolTwo =
                definitions.FirstWeldSymbolTwo,

            referenceKey,

            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "welding_symbol",
                    position,
                    retrievedFrom
                },

            propertyDiagnostics =
                diagnostics
        };
    }

    private static WeldingDefinitionsReadResult ReadDefinitions(
        DrawingWeldingSymbol symbol)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        DrawingWeldingSymbolDefinitions? definitions =
            ReadComObject(
                diagnostics,
                "DrawingWeldingSymbol.Definitions",
                () => symbol.Definitions);

        if (definitions == null)
        {
            return new WeldingDefinitionsReadResult(
                null,
                items,
                diagnostics,
                null,
                null,
                null);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "DrawingWeldingSymbolDefinitions.Count",
                () => definitions.Count);

        object? firstDefinition =
            null;

        object? firstWeldSymbolOne =
            null;

        object? firstWeldSymbolTwo =
            null;

        if (rawCount.HasValue)
        {
            for (int index = 1;
                 index <= rawCount.Value;
                 index++)
            {
                DrawingWeldingSymbolDefinition? definition =
                    ReadComObject(
                        diagnostics,
                        $"DrawingWeldingSymbolDefinitions.Item[{index}]",
                        () => definitions[index]);

                if (definition == null)
                {
                    continue;
                }

                WeldingDefinitionReadResult definitionResult =
                    ReadDefinition(
                        definition,
                        index);

                items.Add(
                    definitionResult.Item);

                if (firstDefinition == null)
                {
                    firstDefinition =
                        definitionResult.Item;

                    firstWeldSymbolOne =
                        definitionResult.WeldSymbolOne;

                    firstWeldSymbolTwo =
                        definitionResult.WeldSymbolTwo;
                }
            }
        }

        return new WeldingDefinitionsReadResult(
            rawCount,
            items,
            diagnostics,
            firstDefinition,
            firstWeldSymbolOne,
            firstWeldSymbolTwo);
    }

    private static WeldingDefinitionReadResult ReadDefinition(
        DrawingWeldingSymbolDefinition definition,
        int index)
    {
        List<object> diagnostics =
            new();

        IdentificationLinePlacementEnum? identificationLinePlacement =
            ReadNullableEnum(
                diagnostics,
                "IdentificationLinePlacement",
                () => definition.IdentificationLinePlacement);

        StaggerTypeEnum? staggerType =
            ReadNullableEnum(
                diagnostics,
                "StaggerType",
                () => definition.StaggerType);

        object? weldSymbolOne =
            ReadWeldSymbol(
                diagnostics,
                "WeldSymbolOne",
                () => definition.WeldSymbolOne);

        object? weldSymbolTwo =
            ReadWeldSymbol(
                diagnostics,
                "WeldSymbolTwo",
                () => definition.WeldSymbolTwo);

        object item =
            new
            {
                index,

                objectTypeRaw =
                    ReadObjectTypeRaw(
                        diagnostics,
                        "Type",
                        () => definition.Type),

                objectType =
                    ReadObjectTypeName(
                        diagnostics,
                        "Type",
                        () => definition.Type),

                allAroundSymbol =
                    ReadNullableBoolean(
                        diagnostics,
                        "AllAroundSymbol",
                        () => definition.AllAroundSymbol),

                closedNoteTail =
                    ReadNullableBoolean(
                        diagnostics,
                        "ClosedNoteTail",
                        () => definition.ClosedNoteTail),

                fieldWeldingSymbol =
                    ReadNullableBoolean(
                        diagnostics,
                        "FieldWeldingSymbol",
                        () => definition.FieldWeldingSymbol),

                identificationLinePlacementRaw =
                    ReadEnumRaw(
                        identificationLinePlacement),

                identificationLinePlacement =
                    ReadEnumName(
                        identificationLinePlacement),

                staggerTypeRaw =
                    ReadEnumRaw(
                        staggerType),

                staggerType =
                    ReadEnumName(
                        staggerType),

                swapArrowAndSymbols =
                    ReadNullableBoolean(
                        diagnostics,
                        "SwapArrowAndSymbols",
                        () => definition.SwapArrowAndSymbols),

                tailNote =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "TailNote",
                            () => definition.TailNote)),

                useSpacer =
                    ReadNullableBoolean(
                        diagnostics,
                        "UseSpacer",
                        () => definition.UseSpacer),

                weldSymbolOne,

                weldSymbolTwo,

                propertyDiagnostics =
                    diagnostics
            };

        return new WeldingDefinitionReadResult(
            item,
            weldSymbolOne,
            weldSymbolTwo);
    }

    private static object? ReadWeldSymbol(
        List<object> diagnostics,
        string propertyName,
        Func<WeldSymbol> reader)
    {
        WeldSymbol? symbol =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (symbol == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        ContourSymbolTypeEnum? contour =
            ReadNullableEnum(
                propertyDiagnostics,
                "Contour",
                () => symbol.Contour);

        object? secondaryWeldSymbol =
            null;

        bool? enableSecondaryFilletWeld =
            ReadNullableBoolean(
                propertyDiagnostics,
                "EnableSecondaryFilletWeld",
                () => symbol.EnableSecondaryFilletWeld);

        if (enableSecondaryFilletWeld == true)
        {
            secondaryWeldSymbol =
                ReadWeldSymbol(
                    propertyDiagnostics,
                    "SecondaryWeldSymbol",
                    () => symbol.SecondaryWeldSymbol);
        }

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    propertyDiagnostics,
                    "Type",
                    () => symbol.Type),

            objectType =
                ReadObjectTypeName(
                    propertyDiagnostics,
                    "Type",
                    () => symbol.Type),

            weldSymbolType =
                ReadNullableInt32(
                    propertyDiagnostics,
                    "WeldSymbolType",
                    () => symbol.WeldSymbolType),

            sizeOrStrength =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "SizeOrStrength",
                        () => symbol.SizeOrStrength)),

            length =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Length",
                        () => symbol.Length)),

            pitch =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Pitch",
                        () => symbol.Pitch)),

            contourRaw =
                ReadEnumRaw(
                    contour),

            contour =
                ReadEnumName(
                    contour),

            root =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Root",
                        () => symbol.Root)),

            gap =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Gap",
                        () => symbol.Gap)),

            depth =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Depth",
                        () => symbol.Depth)),

            leg1 =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Leg1",
                        () => symbol.Leg1)),

            leg2 =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Leg2",
                        () => symbol.Leg2)),

            method =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Method",
                        () => symbol.Method)),

            prefix =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Prefix",
                        () => symbol.Prefix)),

            test =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Test",
                        () => symbol.Test)),

            enableSecondaryFilletWeld,

            secondaryWeldSymbol,

            propertyDiagnostics
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

    private static object? ReadWeldSymbolStyle(
        List<object> diagnostics,
        string propertyName,
        Func<WeldSymbolStyle> reader)
    {
        WeldSymbolStyle? style =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (style == null)
        {
            return null;
        }

        IdentificationLinePlacementEnum? identificationLinePlacement =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.IdentificationLinePlacement",
                () => style.IdentificationLinePlacement);

        LineTypeEnum? identificationLineType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.IdentificationLineType",
                () => style.IdentificationLineType);

        FieldFlagDirectionTypeEnum? fieldFlagDirection =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.FieldFlagDirection",
                () => style.FieldFlagDirection);

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
            identificationLinePlacementRaw =
                ReadEnumRaw(
                    identificationLinePlacement),
            identificationLinePlacement =
                ReadEnumName(
                    identificationLinePlacement),
            identificationLineTypeRaw =
                ReadEnumRaw(
                    identificationLineType),
            identificationLineType =
                ReadEnumName(
                    identificationLineType),
            fieldFlagDirectionRaw =
                ReadEnumRaw(
                    fieldFlagDirection),
            fieldFlagDirection =
                ReadEnumName(
                    fieldFlagDirection),
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

    private static object? ReadUnknownObjectMetadata(
        object? ownerObject,
        List<object> diagnostics,
        string propertyName)
    {
        if (ownerObject == null)
        {
            return null;
        }

        return new
        {
            runtimeType =
                ownerObject
                    .GetType()
                    .FullName,
            objectTypeRaw =
                ReadString(
                    ReadReflectionProperty(
                        ownerObject,
                        diagnostics,
                        $"{propertyName}.Type",
                        "Type")),
            name =
                ReadString(
                    ReadReflectionProperty(
                        ownerObject,
                        diagnostics,
                        $"{propertyName}.Name",
                        "Name")),
            internalName =
                ReadString(
                    ReadReflectionProperty(
                        ownerObject,
                        diagnostics,
                        $"{propertyName}.InternalName",
                        "InternalName"))
        };
    }

    private static object? ReadReflectionProperty(
        object ownerObject,
        List<object> diagnostics,
        string diagnosticName,
        string propertyName)
    {
        try
        {
            return ownerObject
                .GetType()
                .InvokeMember(
                    propertyName,
                    BindingFlags.GetProperty,
                    null,
                    ownerObject,
                    null);
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                diagnosticName,
                exception);

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

internal sealed record WeldingDefinitionsReadResult(
    int? RawCount,
    List<object> Items,
    List<object> Diagnostics,
    object? FirstDefinition,
    object? FirstWeldSymbolOne,
    object? FirstWeldSymbolTwo);

internal sealed record WeldingDefinitionReadResult(
    object Item,
    object? WeldSymbolOne,
    object? WeldSymbolTwo);
