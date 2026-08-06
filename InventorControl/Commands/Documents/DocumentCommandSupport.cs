using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DocumentCommandSupport
{
    public static Document? FindDocument(
        Inventor.Application inventor,
        string documentNameOrPath)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        foreach (Document document
                 in inventor.Documents)
        {
            if (string.Equals(
                    document.DisplayName,
                    documentNameOrPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }

            if (!string.IsNullOrWhiteSpace(
                    document.FullFileName) &&
                string.Equals(
                    document.FullFileName,
                    documentNameOrPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return document;
            }

            if (!string.IsNullOrWhiteSpace(
                    document.FullFileName) &&
                string.Equals(
                    System.IO.Path.GetFileName(
                    document.FullFileName),
                    documentNameOrPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return document;
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

    public static bool TryGetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue,
        out bool value,
        out string error)
    {
        value =
            defaultValue;

        error =
            string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind !=
            JsonValueKind.True &&
            element.ValueKind !=
            JsonValueKind.False)
        {
            error =
                $"Поле \"{propertyName}\" " +
                "должно содержать true или false.";

            return false;
        }

        value =
            element.GetBoolean();

        return true;
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
