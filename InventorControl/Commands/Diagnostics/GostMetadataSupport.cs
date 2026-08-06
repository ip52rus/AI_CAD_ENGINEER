using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class GostMetadataSupport
{
    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

        Document? activeDocument = inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error = "В Inventor нет активного документа.";
            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error = "Активный документ не является чертежом.";
            return null;
        }

        return (DrawingDocument)activeDocument;
    }

    public static Sheet? FindSheet(
        DrawingDocument drawingDocument,
        string sheetName)
    {
        ArgumentNullException.ThrowIfNull(drawingDocument);

        foreach (Sheet sheet in drawingDocument.Sheets)
        {
            if (string.Equals(
                    sheet.Name,
                    sheetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sheet;
            }
        }

        return null;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле \"{propertyName}\".";

            return false;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

            return false;
        }

        value = element.GetString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            error =
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static bool TryGetRequiredInt32(
        JsonElement root,
        string propertyName,
        out int value,
        out string error)
    {
        value = 0;
        error = string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле \"{propertyName}\".";

            return false;
        }

        if (!element.TryGetInt32(out value))
        {
            error =
                $"Поле \"{propertyName}\" должно содержать целое число.";

            return false;
        }

        return true;
    }

    public static object ReadAttributeSets(
        object target,
        string scope)
    {
        List<object> sets = new();

        try
        {
            dynamic dynamicTarget = target;
            dynamic attributeSets = dynamicTarget.AttributeSets;

            int setCount = (int)attributeSets.Count;

            for (int setIndex = 1;
                 setIndex <= setCount;
                 setIndex++)
            {
                dynamic attributeSet = attributeSets.Item(setIndex);

                List<object> attributes = new();

                int attributeCount = (int)attributeSet.Count;

                for (int attributeIndex = 1;
                     attributeIndex <= attributeCount;
                     attributeIndex++)
                {
                    dynamic attribute =
                        attributeSet.Item(attributeIndex);

                    object? value = null;
                    string? valueType = null;

                    try
                    {
                        value = NormalizeValue(attribute.Value);
                        valueType = value?.GetType().Name;
                    }
                    catch
                    {
                    }

                    string name = string.Empty;

                    try
                    {
                        name = (string)attribute.Name;
                    }
                    catch
                    {
                    }

                    string type = string.Empty;

                    try
                    {
                        type = attribute.Type.ToString();
                    }
                    catch
                    {
                    }

                    attributes.Add(
                        new
                        {
                            index = attributeIndex,
                            name,
                            type,
                            value,
                            valueType
                        });
                }

                string setName = string.Empty;

                try
                {
                    setName = (string)attributeSet.Name;
                }
                catch
                {
                }

                sets.Add(
                    new
                    {
                        index = setIndex,
                        name = setName,
                        attributeCount,
                        attributes
                    });
            }

            return new
            {
                scope,
                supported = true,
                setCount,
                sets
            };
        }
        catch (Exception exception)
        {
            return new
            {
                scope,
                supported = false,
                setCount = 0,
                sets,
                error = exception.Message
            };
        }
    }

    public static object? NormalizeValue(object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is DateTime dateTime)
        {
            return dateTime.ToString("O");
        }

        if (value is Array array)
        {
            List<object?> values = new();

            foreach (object? item in array)
            {
                values.Add(NormalizeValue(item));
            }

            return values;
        }

        Type valueType = value.GetType();

        if (valueType.IsPrimitive ||
            value is string ||
            value is decimal ||
            value is Guid)
        {
            return value;
        }

        return value.ToString();
    }

    public static string CreateSuccess(object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());
    }

    public static string CreateError(
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                details
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
