using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateRadiusDimensionCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateRadiusDimensionCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_radius_dimension";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DimensionCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return DimensionCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!DimensionCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return DimensionCommandSupport
                .CreateError(
                    sheetNameError);
        }

        if (!DimensionCommandSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewNameError))
        {
            return DimensionCommandSupport
                .CreateError(
                    viewNameError);
        }

        if (!DimensionCommandSupport
                .TryGetRequiredInt32(
                    root,
                    "curveIndex",
                    out int curveIndex,
                    out string curveIndexError))
        {
            return DimensionCommandSupport
                .CreateError(
                    curveIndexError);
        }

        if (!DimensionCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "x",
                    out double x,
                    out string xError))
        {
            return DimensionCommandSupport
                .CreateError(
                    xError);
        }

        if (!DimensionCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "y",
                    out double y,
                    out string yError))
        {
            return DimensionCommandSupport
                .CreateError(
                    yError);
        }

        bool arrowheadsInside =
            false;

        if (root.TryGetProperty(
                "arrowheadsInside",
                out JsonElement arrowheadsElement))
        {
            if (arrowheadsElement.ValueKind !=
                    JsonValueKind.True &&
                arrowheadsElement.ValueKind !=
                    JsonValueKind.False)
            {
                return DimensionCommandSupport
                    .CreateError(
                        "Поле \"arrowheadsInside\" должно " +
                        "содержать true или false.");
            }

            arrowheadsInside =
                arrowheadsElement.GetBoolean();
        }

        Sheet? sheet =
            DimensionCommandSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return DimensionCommandSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        DrawingView? view =
            DimensionCommandSupport
                .FindView(
                    sheet,
                    viewName);

        if (view == null)
        {
            return DimensionCommandSupport
                .CreateError(
                    $"Вид \"{viewName}\" не найден " +
                    $"на листе \"{sheet.Name}\".");
        }

        List<DrawingCurve> curves;

        try
        {
            curves =
                DimensionCommandSupport
                    .GetCurves(
                        view);
        }
        catch (Exception exception)
        {
            return DimensionCommandSupport
                .CreateError(
                    $"Не удалось получить кривые вида " +
                    $"\"{view.Name}\".",
                    exception.Message);
        }

        if (curveIndex < 1 ||
            curveIndex > curves.Count)
        {
            return DimensionCommandSupport
                .CreateError(
                    $"curveIndex должен быть от 1 до " +
                    $"{curves.Count}.");
        }

        DrawingCurve curve =
            curves[curveIndex - 1];

        string curveType =
            curve.CurveType.ToString();

        bool isCircular =
            string.Equals(
                curveType,
                "kCircleCurve",
                StringComparison.Ordinal) ||
            string.Equals(
                curveType,
                "kCircularArcCurve",
                StringComparison.Ordinal);

        if (!isCircular)
        {
            return DimensionCommandSupport
                .CreateError(
                    "Радиальный размер можно создать " +
                    "только для окружности или круговой дуги.",
                    $"Тип выбранной кривой: {curveType}");
        }

        try
        {
            GeometryIntent geometryIntent =
                sheet.CreateGeometryIntent(
                    curve,
                    Type.Missing);

            Point2d textOrigin =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            GeneralDimensions generalDimensions =
                sheet
                    .DrawingDimensions
                    .GeneralDimensions;

            dynamic dimensionsDynamic =
                generalDimensions;

            object createdDimension;

            try
            {
                createdDimension =
                    dimensionsDynamic.AddRadius(
                        textOrigin,
                        geometryIntent,
                        arrowheadsInside);
            }
            catch
            {
                createdDimension =
                    dimensionsDynamic.AddRadius(
                        textOrigin,
                        geometryIntent);
            }

            drawingDocument.Update();

            dynamic dimensionDynamic =
                createdDimension;

            string text =
                string.Empty;

            try
            {
                text =
                    (string)dimensionDynamic.Text.Text;
            }
            catch
            {
            }

            return DimensionCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Радиальный размер создан.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        view =
                            view.Name,

                        curveIndex,

                        curveType,

                        position =
                            new
                            {
                                x,
                                y
                            },

                        arrowheadsInside,

                        dimensionObjectType =
                            createdDimension
                                .GetType()
                                .Name,

                        text,

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return DimensionCommandSupport
                .CreateError(
                    "Не удалось создать радиальный размер.",
                    exception.Message);
        }
    }
}
