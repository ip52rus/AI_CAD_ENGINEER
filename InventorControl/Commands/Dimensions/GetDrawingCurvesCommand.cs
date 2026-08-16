using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingCurvesCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDrawingCurvesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_drawing_curves";

    public string Execute(JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DimensionCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return DimensionCommandSupport.CreateError(
                documentError ??
                "Не удалось получить активный чертёж.");
        }

        if (!DimensionCommandSupport.TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetNameError))
        {
            return DimensionCommandSupport.CreateError(
                sheetNameError);
        }

        if (!DimensionCommandSupport.TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out string viewNameError))
        {
            return DimensionCommandSupport.CreateError(
                viewNameError);
        }

        Sheet? sheet =
            DimensionCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            return DimensionCommandSupport.CreateError(
                $"Лист \"{sheetName}\" не найден.");
        }

        DrawingView? view =
            DimensionCommandSupport.FindView(
                sheet,
                viewName);

        if (view == null)
        {
            return DimensionCommandSupport.CreateError(
                $"Вид \"{viewName}\" не найден на листе \"{sheet.Name}\".");
        }

        List<DrawingCurve> curves;

        try
        {
            curves =
                DimensionCommandSupport.GetCurves(
                    view);
        }
        catch (Exception exception)
        {
            return DimensionCommandSupport.CreateError(
                $"Не удалось получить кривые вида \"{view.Name}\".",
                exception.Message);
        }

        bool rangeRequested =
            root.TryGetProperty(
                "startIndex",
                out _) ||
            root.TryGetProperty(
                "count",
                out _);

        if (!TryGetOptionalPositiveInteger(
                root,
                "startIndex",
                out int startIndex,
                out string? startIndexError))
        {
            return DimensionCommandSupport.CreateError(
                startIndexError ??
                "startIndex must be a positive 1-based integer.");
        }

        if (!TryGetOptionalPositiveInteger(
                root,
                "count",
                out int requestedCount,
                out string? countError))
        {
            return DimensionCommandSupport.CreateError(
                countError ??
                "count must be a positive integer.");
        }

        int effectiveStartIndex =
            startIndex;

        int effectiveEndIndex =
            requestedCount > 0
                ? (int)Math.Min(
                    curves.Count,
                    (long)effectiveStartIndex + requestedCount - 1)
                : curves.Count;

        List<object> result = new();

        for (int currentIndex = effectiveStartIndex;
             currentIndex <= effectiveEndIndex;
             currentIndex++)
        {
            DrawingCurve curve = curves[currentIndex - 1];

            Point2d? startPoint = null;
            Point2d? endPoint = null;
            Point2d? midPoint = null;
            Point2d? centerPoint = null;
            DrawingEdgeTypeEnum? edgeType = null;
            List<object> propertyDiagnostics = new();

            try
            {
                startPoint = curve.StartPoint;
            }
            catch
            {
            }

            try
            {
                endPoint = curve.EndPoint;
            }
            catch
            {
            }

            try
            {
                midPoint = curve.MidPoint;
            }
            catch
            {
            }

            try
            {
                dynamic dynamicCurve = curve;
                centerPoint = (Point2d?)dynamicCurve.CenterPoint;
            }
            catch
            {
            }

            try
            {
                edgeType = curve.EdgeType;
            }
            catch (Exception exception)
            {
                propertyDiagnostics.Add(new { scope = "DrawingCurve.EdgeType", message = exception.Message, exceptionType = exception.GetType().FullName });
            }

            string modelGeometryType = string.Empty;

            try
            {
                object? modelGeometry =
                    curve.ModelGeometry;

                modelGeometryType =
                    modelGeometry?
                        .GetType()
                        .Name
                    ?? string.Empty;
            }
            catch
            {
            }

            result.Add(
                new
                {
                    index =
                        currentIndex,

                    curveType =
                        curve.CurveType.ToString(),

                    edgeTypeRaw =
                        edgeType.HasValue
                            ? (int?)edgeType.Value
                            : null,

                    edgeType =
                        edgeType?.ToString(),

                    startPoint =
                        DimensionCommandSupport.PointToObject(
                            startPoint),

                    endPoint =
                        DimensionCommandSupport.PointToObject(
                            endPoint),

                    midPoint =
                        DimensionCommandSupport.PointToObject(
                            midPoint),

                    centerPoint =
                        DimensionCommandSupport.PointToObject(
                            centerPoint),

                    modelGeometryType,

                    propertyDiagnostics
                });
        }

        if (rangeRequested)
        {
            return DimensionCommandSupport.CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    view =
                        view.Name,

                    totalRawCount =
                        curves.Count,

                    totalCount =
                        curves.Count,

                    startIndex =
                        effectiveStartIndex,

                    requestedCount =
                        requestedCount > 0
                            ? (int?)requestedCount
                            : null,

                    curveCount =
                        result.Count,

                    curves =
                        result
                });
        }

        return DimensionCommandSupport.CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    sheet.Name,

                view =
                    view.Name,

                curveCount =
                    result.Count,

                curves =
                    result
            });
    }

    private static bool TryGetOptionalPositiveInteger(
        JsonElement root,
        string propertyName,
        out int value,
        out string? error)
    {
        value =
            propertyName == "startIndex"
                ? 1
                : 0;

        error =
            null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return true;
        }

        if (!element.TryGetInt32(
                out value) ||
            value < 1)
        {
            error =
                $"{propertyName} must be a positive integer.";

            return false;
        }

        return true;
    }
}
