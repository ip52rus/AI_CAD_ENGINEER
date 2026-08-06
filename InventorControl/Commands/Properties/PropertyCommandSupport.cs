using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class PropertyCommandSupport
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

    public static PropertySet? FindPropertySet(
        Document document,
        string propertySetName)
    {
        ArgumentNullException.ThrowIfNull(
            document);

        foreach (PropertySet propertySet
                 in document.PropertySets)
        {
            if (string.Equals(
                    propertySet.DisplayName,
                    propertySetName,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    propertySet.Name,
                    propertySetName,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    propertySet.InternalName,
                    propertySetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return propertySet;
            }
        }

        return null;
    }

    public static Inventor.Property? FindProperty(
        PropertySet propertySet,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(
            propertySet);

        foreach (Inventor.Property property
                 in propertySet)
        {
            if (string.Equals(
                    property.DisplayName,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    property.Name,
                    propertyName,
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    property.PropId.ToString(),
                    propertyName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return property;
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

    public static object? NormalizePropertyValue(
        object? value)
    {
        if (value == null)
        {
            return null;
        }

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
