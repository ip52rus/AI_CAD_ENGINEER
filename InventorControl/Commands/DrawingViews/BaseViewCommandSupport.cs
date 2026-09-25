using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class BaseViewCommandSupport
{
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

    public static Document? FindOpenModelDocument(
        Inventor.Application inventor,
        string documentNameOrPath)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        foreach (Document document
                 in inventor.Documents)
        {
            if (document.DocumentType !=
                    DocumentTypeEnum.kPartDocumentObject &&
                document.DocumentType !=
                    DocumentTypeEnum.kAssemblyDocumentObject &&
                document.DocumentType !=
                    DocumentTypeEnum.kPresentationDocumentObject)
            {
                continue;
            }

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

        if (element.ValueKind !=
            JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

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
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static bool TryGetRequiredDouble(
        JsonElement root,
        string propertyName,
        out double value,
        out string error)
    {
        value = 0.0;
        error = string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле \"{propertyName}\".";

            return false;
        }

        if (!element.TryGetDouble(
                out value) ||
            double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            error =
                $"Поле \"{propertyName}\" должно содержать допустимое число.";

            return false;
        }

        return true;
    }

    public static string GetOptionalString(
        JsonElement root,
        string propertyName,
        string defaultValue = "")
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            element.ValueKind !=
                JsonValueKind.String)
        {
            return defaultValue;
        }

        return
            element.GetString()?
                .Trim()
            ?? defaultValue;
    }

    public static bool TryParseOrientation(
        string value,
        out ViewOrientationTypeEnum orientation,
        out string error)
    {
        error = string.Empty;

        string normalized =
            value.Trim()
                .ToLowerInvariant()
                .Replace("-", "_")
                .Replace(" ", "_");

        orientation =
            normalized switch
            {
                "front" or
                "спереди" =>
                    ViewOrientationTypeEnum
                        .kFrontViewOrientation,

                "back" or
                "сзади" =>
                    ViewOrientationTypeEnum
                        .kBackViewOrientation,

                "top" or
                "сверху" =>
                    ViewOrientationTypeEnum
                        .kTopViewOrientation,

                "bottom" or
                "снизу" =>
                    ViewOrientationTypeEnum
                        .kBottomViewOrientation,

                "left" or
                "слева" =>
                    ViewOrientationTypeEnum
                        .kLeftViewOrientation,

                "right" or
                "справа" =>
                    ViewOrientationTypeEnum
                        .kRightViewOrientation,

                "iso_top_right" or
                "iso_ne" or
                "изометрия_сверху_справа" =>
                    ViewOrientationTypeEnum
                        .kIsoTopRightViewOrientation,

                "iso_top_left" or
                "iso_nw" or
                "изометрия_сверху_слева" =>
                    ViewOrientationTypeEnum
                        .kIsoTopLeftViewOrientation,

                "iso_bottom_right" or
                "iso_se" or
                "изометрия_снизу_справа" =>
                    ViewOrientationTypeEnum
                        .kIsoBottomRightViewOrientation,

                "iso_bottom_left" or
                "iso_sw" or
                "изометрия_снизу_слева" =>
                    ViewOrientationTypeEnum
                        .kIsoBottomLeftViewOrientation,

                "current" or
                "текущий" =>
                    ViewOrientationTypeEnum
                        .kCurrentViewOrientation,

                _ =>
                    (ViewOrientationTypeEnum)(-1)
            };

        if ((int)orientation == -1)
        {
            error =
                "Неизвестная ориентация вида. " +
                "Допустимые значения: front, back, top, bottom, " +
                "left, right, iso_top_right, iso_top_left, " +
                "iso_bottom_right, iso_bottom_left, current.";

            return false;
        }

        return true;
    }

    public static DrawingViewStyleEnum ParseViewStyle(
        string value)
    {
        string normalized =
            value.Trim()
                .ToLowerInvariant()
                .Replace("-", "_")
                .Replace(" ", "_");

        return normalized switch
        {
            "hidden_line" or
            "с_невидимыми_линиями" =>
                DrawingViewStyleEnum
                    .kHiddenLineDrawingViewStyle,

            "shaded" or
            "тонированный" =>
                DrawingViewStyleEnum
                    .kShadedDrawingViewStyle,

            "shaded_hidden_line" or
            "тонированный_с_невидимыми_линиями" =>
                DrawingViewStyleEnum
                    .kShadedHiddenLineDrawingViewStyle,

            _ =>
                DrawingViewStyleEnum
                    .kHiddenLineRemovedDrawingViewStyle
        };
    }

    public static bool IsPointInsideSheet(
        Sheet sheet,
        double x,
        double y)
    {
        return
            x >= 0.0 &&
            y >= 0.0 &&
            x <= sheet.Width &&
            y <= sheet.Height;
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
}
