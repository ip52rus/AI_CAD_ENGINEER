using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class SketchedSymbolDefinitionReadSupport
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

        List<object> diagnostics =
            new();

        SketchedSymbolDefinitions? definitions =
            ReadComObject(
                diagnostics,
                "DrawingDocument.SketchedSymbolDefinitions",
                () => drawingDocument.SketchedSymbolDefinitions);

        List<object> items =
            new();

        int? rawCount =
            null;

        if (definitions != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "SketchedSymbolDefinitions.Count",
                    () => definitions.Count);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    SketchedSymbolDefinition? definition =
                        ReadComObject(
                            diagnostics,
                            $"SketchedSymbolDefinitions.Item[{index}]",
                            () => definitions[index]);

                    if (definition == null)
                    {
                        continue;
                    }

                    items.Add(
                        ReadSketchedSymbolDefinition(
                            drawingDocument,
                            definition,
                            index));
                }
            }
        }

        return CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                capability =
                    "sketched_symbol_definitions",

                rawCount,

                count =
                    items.Count,

                items,

                diagnostics
            });
    }

    private static object ReadSketchedSymbolDefinition(
        DrawingDocument drawingDocument,
        SketchedSymbolDefinition definition,
        int index)
    {
        List<object> diagnostics =
            new();

        string name =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Name",
                    () => definition.Name));

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

                    definition.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,

            name,

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

            isReferenced =
                ReadNullableBoolean(
                    diagnostics,
                    "IsReferenced",
                    () => definition.IsReferenced),

            promptedTextBoxes =
                ReadPromptedTextBoxes(
                    definition,
                    diagnostics),

            referenceKey,

            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadPromptedTextBoxes(
        SketchedSymbolDefinition definition,
        List<object> definitionDiagnostics)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        DrawingSketch? sketch =
            ReadComObject(
                diagnostics,
                "Sketch",
                () => definition.Sketch);

        if (sketch == null)
        {
            AddNestedDiagnostics(
                definitionDiagnostics,
                "SketchedSymbolDefinition.PromptedTextBoxes",
                diagnostics);

            return CreateTextBoxCollectionResult(
                null,
                items,
                diagnostics);
        }

        TextBoxes? textBoxes =
            ReadComObject(
                diagnostics,
                "Sketch.TextBoxes",
                () => sketch.TextBoxes);

        if (textBoxes == null)
        {
            AddNestedDiagnostics(
                definitionDiagnostics,
                "SketchedSymbolDefinition.PromptedTextBoxes",
                diagnostics);

            return CreateTextBoxCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "Sketch.TextBoxes.Count",
                () => textBoxes.Count);

        if (rawCount.HasValue)
        {
            for (int index = 1;
                 index <= rawCount.Value;
                 index++)
            {
                TextBox? textBox =
                    ReadComObject(
                        diagnostics,
                        $"Sketch.TextBoxes.Item[{index}]",
                        () => textBoxes[index]);

                if (textBox == null)
                {
                    continue;
                }

                items.Add(
                    ReadTextBox(
                        textBox,
                        index));
            }
        }

        AddNestedDiagnostics(
            definitionDiagnostics,
            "SketchedSymbolDefinition.PromptedTextBoxes",
            diagnostics);

        return CreateTextBoxCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadTextBox(
        TextBox textBox,
        int index)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index,

            text =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "Text",
                        () => textBox.Text)),

            formattedText =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedText",
                        () => textBox.FormattedText)),

            isPrompted =
                (bool?)null,

            promptStatusKnown =
                false,

            promptStatusSource =
                "No dedicated prompted-entry flag/property was exposed by local Inventor 2027 TextBox interop.",

            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => textBox.Type),

            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => textBox.Type),

            propertyDiagnostics =
                diagnostics
        };
    }

    private static object CreateTextBoxCollectionResult(
        int? rawCount,
        List<object> items,
        List<object> diagnostics)
    {
        return new
        {
            rawCount,

            count =
                items.Count,

            items,

            promptStatusKnown =
                false,

            promptStatusSource =
                "TextBoxes are returned in Inventor order. Local Inventor 2027 TextBox interop exposes Text and FormattedText, but no dedicated prompted-entry status property.",

            diagnostics
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

    private static void AddNestedDiagnostics(
        List<object> targetDiagnostics,
        string propertyName,
        List<object> nestedDiagnostics)
    {
        if (nestedDiagnostics.Count == 0)
        {
            return;
        }

        targetDiagnostics.Add(
            new
            {
                property =
                    propertyName,

                diagnostics =
                    nestedDiagnostics
            });
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
