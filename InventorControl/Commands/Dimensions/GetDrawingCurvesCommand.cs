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

        List<object> result = new();

        for (int index = 0;
             index < curves.Count;
             index++)
        {
            DrawingCurve curve = curves[index];

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
                    index = index + 1,

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
}
