using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class FillTitleBlockSupport
{
    private static readonly Regex PropertyRegex =
        new(
            "<Property\\s+" +
            "(?=[^>]*\\bProperty='(?<property>[^']*)')" +
            "[^>]*>",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled);

    public static DrawingDocument?
        GetActiveDrawingDocument(
            Inventor.Application inventor,
            out string? error)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        error =
            null;

        Document? activeDocument =
            inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "В Inventor нет активного документа.";

            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Активный документ не является чертежом.";

            return null;
        }

        return
            (DrawingDocument)activeDocument;
    }

    public static Sheet?
        FindSheet(
            DrawingDocument drawingDocument,
            string sheetName)
    {
        ArgumentNullException.ThrowIfNull(
            drawingDocument);

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

        return null;
    }

    public static bool
        TryGetRequiredString(
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

    public static Dictionary<string, int>
        BuildFieldIndex(
            TitleBlock titleBlock,
            out List<string> duplicateFieldNames)
    {
        Dictionary<string, int> result =
            new(
                StringComparer.OrdinalIgnoreCase);

        duplicateFieldNames =
            new();

        TextBoxes textBoxes =
            titleBlock
                .Definition
                .Sketch
                .TextBoxes;

        for (int index = 1;
             index <= textBoxes.Count;
             index++)
        {
            string formattedText;

            try
            {
                formattedText =
                    textBoxes[index]
                        .FormattedText;
            }
            catch
            {
                continue;
            }

            Match match =
                PropertyRegex.Match(
                    formattedText);

            if (!match.Success)
            {
                continue;
            }

            string fieldName =
                match
                    .Groups["property"]
                    .Value
                    .Trim();

            if (string.IsNullOrWhiteSpace(
                    fieldName))
            {
                continue;
            }

            if (!result.TryAdd(
                    fieldName,
                    index))
            {
                duplicateFieldNames.Add(
                    fieldName);
            }
        }

        return result;
    }

    public static string
        SafeGetText(
            TextBox textBox)
    {
        try
        {
            return textBox.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string
        CreateSuccess(
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

    public static string
        CreateError(
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
