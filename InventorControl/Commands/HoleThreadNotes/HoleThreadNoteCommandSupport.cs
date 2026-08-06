using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class HoleThreadNoteCommandSupport
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

    public static DrawingView? FindView(
        Sheet sheet,
        string viewName)
    {
        ArgumentNullException.ThrowIfNull(
            sheet);

        foreach (DrawingView view
                 in sheet.DrawingViews)
        {
            if (string.Equals(
                    view.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return view;
            }
        }

        return null;
    }

    public static List<DrawingCurve> GetCurves(
        DrawingView view)
    {
        ArgumentNullException.ThrowIfNull(
            view);

        List<DrawingCurve> curves =
            new();

        DrawingCurvesEnumerator enumerator =
            view.DrawingCurves[
                Type.Missing];

        foreach (DrawingCurve curve
                 in enumerator)
        {
            curves.Add(
                curve);
        }

        return curves;
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
                $"Поле \"{propertyName}\" " +
                "должно содержать корректное число.";

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
                $"Поле \"{propertyName}\" должно " +
                "содержать true или false.";

            return false;
        }

        value =
            element.GetBoolean();

        return true;
    }

    public static string SafeGetText(
        HoleThreadNote note)
    {
        try
        {
            return note.Text.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static object? SafeGetTextOrigin(
        HoleThreadNote note)
    {
        try
        {
            Point2d origin =
                note.Text.Origin;

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

    public static object? SafeGetRangeBox(
        HoleThreadNote note)
    {
        try
        {
            Box2d box =
                note.Text.RangeBox;

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
