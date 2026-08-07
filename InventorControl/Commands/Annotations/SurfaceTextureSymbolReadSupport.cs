using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class SurfaceTextureSymbolReadSupport
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

        SurfaceTextureSymbols? symbols =
            ReadComObject(
                diagnostics,
                "Sheet.SurfaceTextureSymbols",
                () => sheet.SurfaceTextureSymbols);

        List<object> items =
            new();

        int? rawCount =
            null;

        if (symbols != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "SurfaceTextureSymbols.Count",
                    () => symbols.Count);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    SurfaceTextureSymbol? symbol =
                        ReadComObject(
                            diagnostics,
                            $"SurfaceTextureSymbols.Item[{index}]",
                            () => symbols[index]);

                    if (symbol == null)
                    {
                        continue;
                    }

                    items.Add(
                        ReadSurfaceTextureSymbol(
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
                    "surface_texture_symbols",

                rawCount,

                count =
                    items.Count,

                items,

                diagnostics
            });
    }

    private static object ReadSurfaceTextureSymbol(
        DrawingDocument drawingDocument,
        Sheet sheet,
        SurfaceTextureSymbol symbol,
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

        SurfaceTextureTypeEnum? surfaceTextureType =
            ReadNullableEnum(
                diagnostics,
                "SurfaceTextureType",
                () => symbol.SurfaceTextureType);

        LayDirectionTypeEnum? layDirection =
            ReadNullableEnum(
                diagnostics,
                "LayDirection",
                () => symbol.LayDirection);

        string productionMethod =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "ProductionMethod",
                    () => symbol.ProductionMethod));

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
                ReadSurfaceTextureStyle(
                    diagnostics,
                    "Style",
                    () => symbol.Style),

            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => symbol.Leader),

            surfaceTextureTypeRaw =
                ReadEnumRaw(
                    surfaceTextureType),

            surfaceTextureType =
                ReadEnumName(
                    surfaceTextureType),

            surfaceWaviness =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "SurfaceWaviness",
                        () => symbol.SurfaceWaviness)),

            productionMethod,

            samplingLength =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "SamplingLength",
                        () => symbol.SamplingLength)),

            roughness =
                new
                {
                    minimum =
                        ReadString(
                            ReadProperty(
                                diagnostics,
                                "MinimumRoughness",
                                () => symbol.MinimumRoughness)),

                    maximum =
                        ReadString(
                            ReadProperty(
                                diagnostics,
                                "MaximumRoughness",
                                () => symbol.MaximumRoughness)),

                    additional =
                        ReadString(
                            ReadProperty(
                                diagnostics,
                                "AdditionalRoughness",
                                () => symbol.AdditionalRoughness))
                },

            additionalProductionMethod =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "AdditionalProductionMethod",
                        () => symbol.AdditionalProductionMethod)),

            additionalRoughness =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "AdditionalRoughness",
                        () => symbol.AdditionalRoughness)),

            additionalSamplingLength =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "AdditionalSamplingLength",
                        () => symbol.AdditionalSamplingLength)),

            allAroundSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    "AllAroundSymbol",
                    () => symbol.AllAroundSymbol),

            forceTail =
                ReadNullableBoolean(
                    diagnostics,
                    "ForceTail",
                    () => symbol.ForceTail),

            majority =
                ReadNullableBoolean(
                    diagnostics,
                    "Majority",
                    () => symbol.Majority),

            layDirectionRaw =
                ReadEnumRaw(
                    layDirection),

            layDirection =
                ReadEnumName(
                    layDirection),

            definition =
                ReadDefinition(
                    diagnostics,
                    "Definition",
                    () => symbol.Definition),

            referenceKey,

            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "surface_texture_symbol",
                    position,
                    surfaceTextureType =
                        ReadEnumName(
                            surfaceTextureType),
                    productionMethod
                },

            propertyDiagnostics =
                diagnostics
        };
    }

    private static object? ReadDefinition(
        List<object> diagnostics,
        string propertyName,
        Func<SurfaceTextureSymbolDefinition> reader)
    {
        SurfaceTextureSymbolDefinition? definition =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (definition == null)
        {
            return null;
        }

        SurfaceTextureTypeEnum? surfaceTextureType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.SurfaceTextureType",
                () => definition.SurfaceTextureType);

        SurfaceTextureStandardReferenceTypeEnum? standardReference =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.StandardReference",
                () => definition.StandardReference);

        DraftingStandardEnum? standardReferenceType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.StandardReferenceType",
                () => definition.StandardReferenceType);

        List<object> properties =
            new();

        AddKnownDefinitionProperties(
            properties,
            diagnostics,
            propertyName,
            definition);

        return new
        {
            apiType =
                definition
                    .GetType()
                    .FullName,

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => definition.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => definition.Type),

            surfaceTextureTypeRaw =
                ReadEnumRaw(
                    surfaceTextureType),

            surfaceTextureType =
                ReadEnumName(
                    surfaceTextureType),

            standardReferenceRaw =
                ReadEnumRaw(
                    standardReference),

            standardReference =
                ReadEnumName(
                    standardReference),

            standardReferenceTypeRaw =
                ReadEnumRaw(
                    standardReferenceType),

            standardReferenceType =
                ReadEnumName(
                    standardReferenceType),

            allAroundSymbol =
                ReadNullableBoolean(
                    diagnostics,
                    $"{propertyName}.AllAroundSymbol",
                    () => definition.AllAroundSymbol),

            isForceTailShown =
                ReadNullableBoolean(
                    diagnostics,
                    $"{propertyName}.IsForceTailShown",
                    () => definition.IsForceTailShown),

            isMajority =
                ReadNullableBoolean(
                    diagnostics,
                    $"{propertyName}.IsMajority",
                    () => definition.IsMajority),

            properties
        };
    }

    private static void AddKnownDefinitionProperties(
        List<object> properties,
        List<object> diagnostics,
        string propertyName,
        SurfaceTextureSymbolDefinition definition)
    {
        if (definition is SurfaceTextureGOSTDefinition gostDefinition)
        {
            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.FirstRequirement",
                "FirstRequirement",
                () => gostDefinition.FirstRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.SecondRequirement",
                "SecondRequirement",
                () => gostDefinition.SecondRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ThirdRequirement",
                "ThirdRequirement",
                () => gostDefinition.ThirdRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.FourthRequirement",
                "FourthRequirement",
                () => gostDefinition.FourthRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ProductionMethod",
                "ProductionMethod",
                () => gostDefinition.ProductionMethod);

            AddEnumDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.TextureDirection",
                "TextureDirection",
                () => gostDefinition.TextureDirection);

            return;
        }

        if (definition is SurfaceTextureISODefinition isoDefinition)
        {
            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.FirstRequirement",
                "FirstRequirement",
                () => isoDefinition.FirstRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.SecondRequirement",
                "SecondRequirement",
                () => isoDefinition.SecondRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ThirdRequirement",
                "ThirdRequirement",
                () => isoDefinition.ThirdRequirement);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.MachiningAllowance",
                "MachiningAllowance",
                () => isoDefinition.MachiningAllowance);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ManufacturingProcess",
                "ManufacturingProcess",
                () => isoDefinition.ManufacturingProcess);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.NumberOfFeatureElements",
                "NumberOfFeatureElements",
                () => isoDefinition.NumberOfFeatureElements);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ProcessNoteOne",
                "ProcessNoteOne",
                () => isoDefinition.ProcessNoteOne);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ProcessNoteTwo",
                "ProcessNoteTwo",
                () => isoDefinition.ProcessNoteTwo);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.SurfaceProfileLowerTolerance",
                "SurfaceProfileLowerTolerance",
                () => isoDefinition.SurfaceProfileLowerTolerance);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.SurfaceProfileUpperTolerance",
                "SurfaceProfileUpperTolerance",
                () => isoDefinition.SurfaceProfileUpperTolerance);

            AddEnumDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.LayDirection",
                "LayDirection",
                () => isoDefinition.LayDirection);

            AddEnumDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ProfileDirection",
                "ProfileDirection",
                () => isoDefinition.ProfileDirection);

            return;
        }

        if (definition is SurfaceTextureANSIDefinition ansiDefinition)
        {
            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.AdditionalMaxRoughness",
                "AdditionalMaxRoughness",
                () => ansiDefinition.AdditionalMaxRoughness);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.AdditionalMinRoughness",
                "AdditionalMinRoughness",
                () => ansiDefinition.AdditionalMinRoughness);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.Cutoff",
                "Cutoff",
                () => ansiDefinition.Cutoff);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.MachiningAllowance",
                "MachiningAllowance",
                () => ansiDefinition.MachiningAllowance);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.MaximumRoughness",
                "MaximumRoughness",
                () => ansiDefinition.MaximumRoughness);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.MinimumRoughness",
                "MinimumRoughness",
                () => ansiDefinition.MinimumRoughness);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.ProcessNote",
                "ProcessNote",
                () => ansiDefinition.ProcessNote);

            AddStringDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.SamplingLength",
                "SamplingLength",
                () => ansiDefinition.SamplingLength);

            AddEnumDefinitionProperty(
                properties,
                diagnostics,
                $"{propertyName}.LayDirection",
                "LayDirection",
                () => ansiDefinition.LayDirection);
        }
    }

    private static void AddStringDefinitionProperty(
        List<object> properties,
        List<object> diagnostics,
        string diagnosticName,
        string name,
        Func<string> reader)
    {
        object? value =
            ReadProperty(
                diagnostics,
                diagnosticName,
                () => reader());

        if (value == null)
        {
            return;
        }

        properties.Add(
            new
            {
                name,
                value =
                    ReadString(
                        value)
            });
    }

    private static void AddEnumDefinitionProperty<TEnum>(
        List<object> properties,
        List<object> diagnostics,
        string diagnosticName,
        string name,
        Func<TEnum> reader)
        where TEnum : struct
    {
        TEnum? value =
            ReadNullableEnum(
                diagnostics,
                diagnosticName,
                reader);

        if (!value.HasValue)
        {
            return;
        }

        properties.Add(
            new
            {
                name,
                raw =
                    ReadEnumRaw(
                        value),
                value =
                    ReadEnumName(
                        value)
            });
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

    private static object? ReadSurfaceTextureStyle(
        List<object> diagnostics,
        string propertyName,
        Func<SurfaceTextureStyle> reader)
    {
        SurfaceTextureStyle? style =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (style == null)
        {
            return null;
        }

        DraftingStandardEnum? standardReference =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.StandardReference",
                () => style.StandardReference);

        SurfaceTextureStandardReferenceTypeEnum? standardReferenceType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.StandardReferenceType",
                () => style.StandardReferenceType);

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
            standardReferenceRaw =
                ReadEnumRaw(
                    standardReference),
            standardReference =
                ReadEnumName(
                    standardReference),
            standardReferenceTypeRaw =
                ReadEnumRaw(
                    standardReferenceType),
            standardReferenceType =
                ReadEnumName(
                    standardReferenceType),
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
