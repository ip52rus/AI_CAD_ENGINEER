using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class AutomaticDimensionLayoutSupport
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

        return (DrawingDocument)activeDocument;
    }

    public static Sheet? FindSheet(
        DrawingDocument drawingDocument,
        string sheetName)
    {
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

        if (string.IsNullOrWhiteSpace(value))
        {
            error =
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static double GetOptionalDouble(
        JsonElement root,
        string propertyName,
        double defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.TryGetDouble(
            out double value)
                ? value
                : defaultValue;
    }

    public static bool GetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => defaultValue
        };
    }

    public static ViewBox ReadViewBox(
        DrawingView view)
    {
        double left =
            view.Left;

        double top =
            view.Top;

        double right =
            left + view.Width;

        double bottom =
            top - view.Height;

        return new ViewBox(
            view.Name,
            left,
            right,
            top,
            bottom,
            view.Position.X,
            view.Position.Y);
    }

    public static DimensionInfo ReadDimension(
        dynamic dimension,
        int index)
    {
        Point2d? lineStart = null;
        Point2d? lineEnd = null;
        Point2d? textOrigin = null;
        Point2d? extensionOneStart = null;
        Point2d? extensionTwoStart = null;
        Box2d? rangeBox = null;

        try
        {
            lineStart =
                (Point2d)dimension
                    .DimensionLine
                    .StartPoint;

            lineEnd =
                (Point2d)dimension
                    .DimensionLine
                    .EndPoint;
        }
        catch
        {
        }

        try
        {
            extensionOneStart =
                (Point2d)dimension
                    .ExtensionLineOne
                    .StartPoint;
        }
        catch
        {
        }

        try
        {
            extensionTwoStart =
                (Point2d)dimension
                    .ExtensionLineTwo
                    .StartPoint;
        }
        catch
        {
        }

        try
        {
            textOrigin =
                (Point2d)dimension
                    .Text
                    .Origin;
        }
        catch
        {
        }

        try
        {
            rangeBox =
                (Box2d)dimension
                    .Text
                    .RangeBox;
        }
        catch
        {
        }

        string text = string.Empty;

        try
        {
            text =
                (string)dimension
                    .Text
                    .Text;
        }
        catch
        {
        }

        bool horizontal =
            lineStart != null &&
            lineEnd != null &&
            Math.Abs(
                lineEnd.X - lineStart.X) >=
            Math.Abs(
                lineEnd.Y - lineStart.Y);

        double lineCoordinate =
            horizontal
                ? lineStart?.Y ?? textOrigin?.Y ?? 0.0
                : lineStart?.X ?? textOrigin?.X ?? 0.0;

        return new DimensionInfo(
            index,
            text,
            horizontal,
            lineStart,
            lineEnd,
            extensionOneStart,
            extensionTwoStart,
            textOrigin,
            rangeBox,
            lineCoordinate);
    }

    public static ViewBox? FindOwningView(
        DimensionInfo dimension,
        IReadOnlyList<ViewBox> views)
    {
        if (views.Count == 0)
        {
            return null;
        }

        if (dimension.ExtensionOneStart != null &&
            dimension.ExtensionTwoStart != null)
        {
            double tolerance = 0.25;

            List<(ViewBox View, double Score)> candidates =
                new();

            foreach (ViewBox view in views)
            {
                bool firstInside =
                    IsPointInsideExpandedView(
                        dimension.ExtensionOneStart,
                        view,
                        tolerance);

                bool secondInside =
                    IsPointInsideExpandedView(
                        dimension.ExtensionTwoStart,
                        view,
                        tolerance);

                if (!firstInside ||
                    !secondInside)
                {
                    continue;
                }

                double anchorCoordinate =
                    dimension.Horizontal
                        ? (dimension.ExtensionOneStart.Y +
                           dimension.ExtensionTwoStart.Y) / 2.0
                        : (dimension.ExtensionOneStart.X +
                           dimension.ExtensionTwoStart.X) / 2.0;

                double score =
                    dimension.Horizontal
                        ? Math.Min(
                            Math.Abs(anchorCoordinate - view.Top),
                            Math.Abs(anchorCoordinate - view.Bottom))
                        : Math.Min(
                            Math.Abs(anchorCoordinate - view.Left),
                            Math.Abs(anchorCoordinate - view.Right));

                candidates.Add(
                    (view, score));
            }

            if (candidates.Count > 0)
            {
                return candidates
                    .OrderBy(item => item.Score)
                    .First()
                    .View;
            }
        }

        Point2d? reference =
            dimension.TextOrigin ??
            dimension.LineStart;

        if (reference == null)
        {
            return null;
        }

        ViewBox? nearest = null;
        double nearestDistance = double.MaxValue;

        foreach (ViewBox view in views)
        {
            double dx =
                reference.X - view.CenterX;

            double dy =
                reference.Y - view.CenterY;

            double distance =
                dx * dx + dy * dy;

            if (distance < nearestDistance)
            {
                nearest = view;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    public static string DetermineSide(
        DimensionInfo dimension,
        ViewBox view)
    {
        if (dimension.Horizontal)
        {
            if (dimension.LineCoordinate >= view.Top)
            {
                return "top";
            }

            if (dimension.LineCoordinate <= view.Bottom)
            {
                return "bottom";
            }

            return dimension.LineCoordinate >= view.CenterY
                ? "top"
                : "bottom";
        }

        if (dimension.LineCoordinate >= view.Right)
        {
            return "right";
        }

        if (dimension.LineCoordinate <= view.Left)
        {
            return "left";
        }

        return dimension.LineCoordinate >= view.CenterX
            ? "right"
            : "left";
    }

    private static bool IsPointInsideExpandedView(
        Point2d point,
        ViewBox view,
        double tolerance)
    {
        return
            point.X >= view.Left - tolerance &&
            point.X <= view.Right + tolerance &&
            point.Y >= view.Bottom - tolerance &&
            point.Y <= view.Top + tolerance;
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

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    internal sealed record ViewBox(
        string Name,
        double Left,
        double Right,
        double Top,
        double Bottom,
        double CenterX,
        double CenterY);

    internal sealed record DimensionInfo(
        int Index,
        string Text,
        bool Horizontal,
        Point2d? LineStart,
        Point2d? LineEnd,
        Point2d? ExtensionOneStart,
        Point2d? ExtensionTwoStart,
        Point2d? TextOrigin,
        Box2d? TextRangeBox,
        double LineCoordinate);
}
