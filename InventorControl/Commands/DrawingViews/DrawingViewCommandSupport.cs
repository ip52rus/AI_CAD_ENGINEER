using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DrawingViewCommandSupport
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

    public static DrawingView? FindDrawingView(
        Sheet sheet,
        string viewName)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        foreach (DrawingView drawingView in sheet.DrawingViews)
        {
            if (string.Equals(
                    drawingView.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return drawingView;
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

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            error = $"Не найдено обязательное поле \"{propertyName}\".";
            return false;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            error = $"Поле \"{propertyName}\" должно быть строкой.";
            return false;
        }

        value = element.GetString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = $"Поле \"{propertyName}\" не должно быть пустым.";
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

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            error = $"Не найдено обязательное поле \"{propertyName}\".";
            return false;
        }

        if (!element.TryGetDouble(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            error = $"Поле \"{propertyName}\" должно содержать допустимое число.";
            return false;
        }

        return true;
    }

    public static bool TryGetRequiredBoolean(
        JsonElement root,
        string propertyName,
        out bool value,
        out string error)
    {
        value = false;
        error = string.Empty;

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            error = $"Не найдено обязательное поле \"{propertyName}\".";
            return false;
        }

        if (element.ValueKind != JsonValueKind.True &&
            element.ValueKind != JsonValueKind.False)
        {
            error = $"Поле \"{propertyName}\" должно содержать true или false.";
            return false;
        }

        value = element.GetBoolean();
        return true;
    }

    public static string CreateSuccess(object data)
    {
        return JsonSerializer.Serialize(
            new { success = true, data },
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
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
