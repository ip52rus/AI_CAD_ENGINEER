using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class TitleBlockBindingSupport
{
    private static readonly Regex
        PropertyRegex =
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

    private static readonly Regex
        PromptRegex =
            new(
                "<Prompt[^>]*>(?<prompt>.*?)</Prompt>",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline |
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

    public static object ParseBinding(
        TitleBlock titleBlock,
        TextBox textBox,
        int index)
    {
        string text =
            SafeGetText(
                textBox);

        string formattedText =
            SafeGetFormattedText(
                textBox);

        string displayedText =
            SafeGetResultText(
                titleBlock,
                textBox);

        Match propertyMatch =
            PropertyRegex.Match(
                formattedText);

        if (propertyMatch.Success)
        {
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

            return new
            {
                index,

                bindingType =
                    "property",

                text,

                displayedText,

                sourceDocument =
                    propertyMatch
                        .Groups["document"]
                        .Value,

                propertySet =
                    propertyMatch
                        .Groups["propertySet"]
                        .Value,

                property =
                    propertyMatch
                        .Groups["property"]
                        .Value,

                formatId =
                    propertyMatch
                        .Groups["formatId"]
                        .Value,

                propertyId,

                origin =
                    new
                    {
                        x =
                            textBox.Origin.X,

                        y =
                            textBox.Origin.Y
                    }
            };
        }

        Match promptMatch =
            PromptRegex.Match(
                formattedText);

        if (promptMatch.Success)
        {
            return new
            {
                index,

                bindingType =
                    "prompt",

                text,

                displayedText,

                prompt =
                    promptMatch
                        .Groups["prompt"]
                        .Value,

                origin =
                    new
                    {
                        x =
                            textBox.Origin.X,

                        y =
                            textBox.Origin.Y
                    }
            };
        }

        return new
        {
            index,

            bindingType =
                "static",

            text,

            displayedText,

            origin =
                new
                {
                    x =
                        textBox.Origin.X,

                    y =
                        textBox.Origin.Y
                }
        };
    }

    private static string
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

    private static string
        SafeGetFormattedText(
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

    private static string
        SafeGetResultText(
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
