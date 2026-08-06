using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class TitleBlockFieldMapperSupport
{
    private static readonly Regex PropertyRegex =
        new(
            "<Property\\s+" +
            "(?=[^>]*\\bDocument='(?<document>[^']*)')" +
            "(?=[^>]*\\bPropertySet='(?<propertySet>[^']*)')" +
            "(?=[^>]*\\bProperty='(?<property>[^']*)')" +
            "(?=[^>]*\\bFormatID='(?<formatId>[^']*)')" +
            "(?=[^>]*\\bPropertyID='(?<propertyId>[^']*)')" +
            "[^>]*>",
            RegexOptions.IgnoreCase |
            RegexOptions.Compiled);

    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

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

    public static Sheet? FindSheet(
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

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error,
        bool allowEmpty = false)
    {
        value = string.Empty;
        error = string.Empty;

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
            element.GetString()
            ?? string.Empty;

        if (!allowEmpty)
        {
            value =
                value.Trim();

            if (string.IsNullOrWhiteSpace(
                    value))
            {
                error =
                    $"Поле \"{propertyName}\" " +
                    "не должно быть пустым.";

                return false;
            }
        }

        return true;
    }

    public static List<FieldMatch> GetFieldMatches(
        TitleBlock titleBlock,
        string fieldName,
        bool contains)
    {
        List<FieldMatch> matches =
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
            TextBox textBox =
                textBoxes[index];

            string formattedText =
                SafeGetFormattedText(
                    textBox);

            Match propertyMatch =
                PropertyRegex.Match(
                    formattedText);

            if (!propertyMatch.Success)
            {
                continue;
            }

            string propertyName =
                propertyMatch
                    .Groups["property"]
                    .Value;

            bool isMatch =
                contains
                    ? propertyName.Contains(
                        fieldName,
                        StringComparison.OrdinalIgnoreCase)
                    : string.Equals(
                        propertyName,
                        fieldName,
                        StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                continue;
            }

            int? propertyId =
                null;

            if (int.TryParse(
                    propertyMatch
                        .Groups["propertyId"]
                        .Value,
                    out int parsedPropertyId))
            {
                propertyId =
                    parsedPropertyId;
            }

            matches.Add(
                new FieldMatch(
                    index,
                    propertyName,
                    propertyMatch
                        .Groups["propertySet"]
                        .Value,
                    propertyMatch
                        .Groups["document"]
                        .Value,
                    propertyMatch
                        .Groups["formatId"]
                        .Value,
                    propertyId,
                    SafeGetText(
                        textBox),
                    SafeGetResultText(
                        titleBlock,
                        textBox),
                    textBox.Origin.X,
                    textBox.Origin.Y));
        }

        return matches;
    }

    public static List<FieldMatch> GetAllFields(
        TitleBlock titleBlock)
    {
        List<FieldMatch> fields =
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
            TextBox textBox =
                textBoxes[index];

            string formattedText =
                SafeGetFormattedText(
                    textBox);

            Match propertyMatch =
                PropertyRegex.Match(
                    formattedText);

            if (!propertyMatch.Success)
            {
                continue;
            }

            int? propertyId =
                null;

            if (int.TryParse(
                    propertyMatch
                        .Groups["propertyId"]
                        .Value,
                    out int parsedPropertyId))
            {
                propertyId =
                    parsedPropertyId;
            }

            fields.Add(
                new FieldMatch(
                    index,
                    propertyMatch
                        .Groups["property"]
                        .Value,
                    propertyMatch
                        .Groups["propertySet"]
                        .Value,
                    propertyMatch
                        .Groups["document"]
                        .Value,
                    propertyMatch
                        .Groups["formatId"]
                        .Value,
                    propertyId,
                    SafeGetText(
                        textBox),
                    SafeGetResultText(
                        titleBlock,
                        textBox),
                    textBox.Origin.X,
                    textBox.Origin.Y));
        }

        return fields;
    }

    public static string SafeGetText(
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

    public static string SafeGetFormattedText(
        TextBox textBox)
    {
        try
        {
            return textBox.FormattedText;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string SafeGetResultText(
        TitleBlock titleBlock,
        TextBox textBox)
    {
        try
        {
            return titleBlock.GetResultText(
                textBox);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string CreateSuccess(
        object data)
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

    private static JsonSerializerOptions
        CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,

            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }

    internal sealed record FieldMatch(
        int Index,
        string FieldName,
        string PropertySet,
        string SourceDocument,
        string FormatId,
        int? PropertyId,
        string Text,
        string DisplayedText,
        double X,
        double Y);
}
