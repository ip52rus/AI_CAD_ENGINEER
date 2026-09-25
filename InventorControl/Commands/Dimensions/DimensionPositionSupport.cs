using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DimensionPositionSupport
{
    public static DrawingDocument? GetActiveDrawingDocument(
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
                $"Поле \"{propertyName}\" должно содержать целое число.";

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
        value =
            0.0;

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

        if (!element.TryGetDouble(
                out value) ||
            double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            error =
                $"Поле \"{propertyName}\" должно содержать корректное число.";

            return false;
        }

        return true;
    }

    public static object? GetTextOrigin(
        dynamic dimension)
    {
        try
        {
            Point2d origin =
                (Point2d)dimension.Text.Origin;

            return new
            {
                x =
                    origin.X,

                y =
                    origin.Y
            };
        }
        catch
        {
            return null;
        }
    }

    public static object? GetTextRangeBox(
        dynamic dimension)
    {
        try
        {
            Box2d box =
                (Box2d)dimension.Text.RangeBox;

            return new
            {
                minPoint =
                    new
                    {
                        x =
                            box.MinPoint.X,

                        y =
                            box.MinPoint.Y
                    },

                maxPoint =
                    new
                    {
                        x =
                            box.MaxPoint.X,

                        y =
                            box.MaxPoint.Y
                    }
            };
        }
        catch
        {
            return null;
        }
    }

    public static string GetText(
        dynamic dimension)
    {
        try
        {
            return
                (string)dimension.Text.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetDimensionType(
        dynamic dimension)
    {
        try
        {
            return
                dimension.DimensionType.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string GetObjectType(
        dynamic dimension)
    {
        try
        {
            return
                dimension.Type.ToString();
        }
        catch
        {
            return
                dimension.GetType().Name;
        }
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
