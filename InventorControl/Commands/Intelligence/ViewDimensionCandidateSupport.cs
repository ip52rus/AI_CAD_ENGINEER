using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class ViewDimensionCandidateSupport
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

    public static List<DrawingCurve> GetCurves(
        DrawingView view)
    {
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

    public static Point2d? TryGetCenter(
        DrawingCurve curve)
    {
        try
        {
            dynamic dynamicCurve =
                curve;

            return
                (Point2d?)dynamicCurve.CenterPoint;
        }
        catch
        {
            return null;
        }
    }

    public static double? TryGetRadius(
        DrawingCurve curve)
    {
        try
        {
            dynamic geometry =
                curve.Segments[1].Geometry;

            return
                (double)geometry.Radius;
        }
        catch
        {
        }

        try
        {
            dynamic dynamicCurve =
                curve;

            dynamic geometry =
                dynamicCurve.Evaluator2D;

            double minParam =
                0.0;

            double maxParam =
                0.0;

            geometry.GetParamExtents(
                ref minParam,
                ref maxParam);

            return null;
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

    internal sealed record CurvePoint(
        int CurveIndex,
        string Intent,
        double X,
        double Y);

    internal sealed record CircleCandidate(
        int CurveIndex,
        string CurveType,
        double CenterX,
        double CenterY,
        double? Radius);
}
