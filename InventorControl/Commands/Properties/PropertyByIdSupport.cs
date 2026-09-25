using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class PropertyByIdSupport
{
    public static Document? GetTargetDocument(
        Inventor.Application inventor,
        JsonElement root,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        error =
            null;

        if (root.TryGetProperty(
                "document",
                out JsonElement documentElement))
        {
            if (documentElement.ValueKind !=
                JsonValueKind.String)
            {
                error =
                    "Поле \"document\" должно быть строкой.";

                return null;
            }

            string documentNameOrPath =
                documentElement
                    .GetString()?
                    .Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                    documentNameOrPath))
            {
                error =
                    "Поле \"document\" не должно быть пустым.";

                return null;
            }

            Document? document =
                DocumentCommandSupport
                    .FindDocument(
                        inventor,
                        documentNameOrPath);

            if (document == null)
            {
                error =
                    $"Открытый документ " +
                    $"\"{documentNameOrPath}\" не найден.";

                return null;
            }

            return document;
        }

        Document? activeDocument =
            inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "В Inventor нет активного документа.";

            return null;
        }

        return activeDocument;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value =
            string.Empty;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле " +
                $"\"{propertyName}\".";

            return false;
        }

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" " +
                "должно быть строкой.";

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            error =
                $"Поле \"{propertyName}\" " +
                "не должно быть пустым.";

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
        value =
            0;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле " +
                $"\"{propertyName}\".";

            return false;
        }

        if (!element.TryGetInt32(
                out value))
        {
            error =
                $"Поле \"{propertyName}\" " +
                "должно содержать целое число.";

            return false;
        }

        return true;
    }

    public static object? ConvertJsonValue(
        JsonElement valueElement)
    {
        return valueElement.ValueKind switch
        {
            JsonValueKind.String =>
                valueElement.GetString(),

            JsonValueKind.Number
                when valueElement.TryGetInt32(
                    out int integerValue) =>
                integerValue,

            JsonValueKind.Number =>
                valueElement.GetDouble(),

            JsonValueKind.True =>
                true,

            JsonValueKind.False =>
                false,

            JsonValueKind.Null =>
                null,

            _ =>
                valueElement.GetRawText()
        };
    }

    public static bool TryResolveProperty(
        Document document,
        string propertySetInternalName,
        int propertyId,
        out Inventor.Property? property,
        out string source,
        out string details)
    {
        property =
            null;

        source =
            string.Empty;

        List<string> errors =
            new();

        try
        {
            dynamic propertySets =
                document.PropertySets;

            dynamic propertySet =
                propertySets.Item(
                    propertySetInternalName);

            dynamic foundProperty =
                propertySet.ItemByPropId(
                    propertyId);

            property =
                (Inventor.Property)foundProperty;

            source =
                "Document.PropertySets";

            details =
                string.Empty;

            return true;
        }
        catch (Exception exception)
        {
            errors.Add(
                "Document.PropertySets: " +
                exception.Message);
        }

        try
        {
            dynamic documentDynamic =
                document;

            dynamic filePropertySets =
                documentDynamic.FilePropertySets;

            dynamic propertySet =
                filePropertySets.Item(
                    propertySetInternalName);

            dynamic foundProperty =
                propertySet.ItemByPropId(
                    propertyId);

            property =
                (Inventor.Property)foundProperty;

            source =
                "Document.FilePropertySets";

            details =
                string.Empty;

            return true;
        }
        catch (Exception exception)
        {
            errors.Add(
                "Document.FilePropertySets: " +
                exception.Message);
        }

        details =
            string.Join(
                System.Environment.NewLine,
                errors);

        return false;
    }

    public static object? NormalizePropertyValue(
        object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(
                "O");
        }

        return value;
    }

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

    private static JsonSerializerOptions
        CreateJsonOptions()
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
