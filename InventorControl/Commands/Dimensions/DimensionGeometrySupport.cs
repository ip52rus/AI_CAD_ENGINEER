using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DimensionGeometrySupport
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

    public static Sheet? FindSheet(
        DrawingDocument drawingDocument,
        string sheetName)
    {
        foreach (Sheet sheet in drawingDocument.Sheets)
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

        if (element.ValueKind != JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

            return false;
        }

        value = element.GetString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
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
        value = 0;
        error = string.Empty;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            error =
                $"Не найдено обязательное поле \"{propertyName}\".";

            return false;
        }

        if (!element.TryGetInt32(out value))
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

        if (!element.TryGetDouble(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            error =
                $"Поле \"{propertyName}\" должно содержать корректное число.";

            return false;
        }

        return true;
    }

    public static object? ReadPoint(
        Func<object?> getter)
    {
        try
        {
            object? value = getter();

            if (value == null)
            {
                return null;
            }

            dynamic point = value;

            return new
            {
                x = (double)point.X,
                y = (double)point.Y
            };
        }
        catch
        {
            return null;
        }
    }

    public static object? ReadVector(
        Func<object?> getter)
    {
        try
        {
            object? value = getter();

            if (value == null)
            {
                return null;
            }

            dynamic vector = value;

            return new
            {
                x = (double)vector.X,
                y = (double)vector.Y
            };
        }
        catch
        {
            return null;
        }
    }

    public static object? ReadRangeBox(
        dynamic dimension)
    {
        try
        {
            Box2d box = (Box2d)dimension.Text.RangeBox;

            return new
            {
                minPoint = new
                {
                    x = box.MinPoint.X,
                    y = box.MinPoint.Y
                },
                maxPoint = new
                {
                    x = box.MaxPoint.X,
                    y = box.MaxPoint.Y
                }
            };
        }
        catch
        {
            return null;
        }
    }

    public static string ReadText(
        dynamic dimension)
    {
        try
        {
            return (string)dimension.Text.Text;
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ReadDimensionType(
        dynamic dimension)
    {
        try
        {
            return dimension.DimensionType.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string ReadObjectType(
        dynamic dimension)
    {
        try
        {
            return dimension.Type.ToString();
        }
        catch
        {
            return dimension.GetType().Name;
        }
    }

    public static object ReadGeometry(
        dynamic dimension,
        int index)
    {
        object? dimensionLine = null;
        object? extensionLineOne = null;
        object? extensionLineTwo = null;

        try
        {
            dimensionLine = dimension.DimensionLine;
        }
        catch
        {
        }

        try
        {
            extensionLineOne = dimension.ExtensionLineOne;
        }
        catch
        {
        }

        try
        {
            extensionLineTwo = dimension.ExtensionLineTwo;
        }
        catch
        {
        }

        return new
        {
            index,
            objectType = ReadObjectType(dimension),
            dimensionType = ReadDimensionType(dimension),
            text = ReadText(dimension),

            textOrigin = ReadPoint(
                () => dimension.Text.Origin),

            textRangeBox = ReadRangeBox(dimension),

            dimensionLine = dimensionLine == null
                ? null
                : new
                {
                    startPoint = ReadPoint(
                        () => ((dynamic)dimensionLine).StartPoint),

                    endPoint = ReadPoint(
                        () => ((dynamic)dimensionLine).EndPoint),

                    midPoint = ReadPoint(
                        () => ((dynamic)dimensionLine).MidPoint),

                    origin = ReadPoint(
                        () => ((dynamic)dimensionLine).Origin),

                    position = ReadPoint(
                        () => ((dynamic)dimensionLine).Position),

                    direction = ReadVector(
                        () => ((dynamic)dimensionLine).Direction)
                },

            extensionLineOne = extensionLineOne == null
                ? null
                : new
                {
                    startPoint = ReadPoint(
                        () => ((dynamic)extensionLineOne).StartPoint),

                    endPoint = ReadPoint(
                        () => ((dynamic)extensionLineOne).EndPoint)
                },

            extensionLineTwo = extensionLineTwo == null
                ? null
                : new
                {
                    startPoint = ReadPoint(
                        () => ((dynamic)extensionLineTwo).StartPoint),

                    endPoint = ReadPoint(
                        () => ((dynamic)extensionLineTwo).EndPoint)
                }
        };
    }

    public static string CreateSuccess(object data)
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

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
