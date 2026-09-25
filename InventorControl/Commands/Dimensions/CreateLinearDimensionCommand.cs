using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateLinearDimensionCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateLinearDimensionCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "create_linear_dimension";

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

        if (!DimensionCommandSupport.TryGetRequiredInt32(
                root,
                "firstCurveIndex",
                out int firstCurveIndex,
                out string firstCurveError))
        {
            return DimensionCommandSupport.CreateError(
                firstCurveError);
        }

        if (!DimensionCommandSupport.TryGetRequiredString(
                root,
                "firstIntent",
                out string firstIntentText,
                out string firstIntentError))
        {
            return DimensionCommandSupport.CreateError(
                firstIntentError);
        }

        if (!DimensionCommandSupport.TryGetRequiredInt32(
                root,
                "secondCurveIndex",
                out int secondCurveIndex,
                out string secondCurveError))
        {
            return DimensionCommandSupport.CreateError(
                secondCurveError);
        }

        if (!DimensionCommandSupport.TryGetRequiredString(
                root,
                "secondIntent",
                out string secondIntentText,
                out string secondIntentError))
        {
            return DimensionCommandSupport.CreateError(
                secondIntentError);
        }

        if (!DimensionCommandSupport.TryGetRequiredString(
                root,
                "dimensionType",
                out string dimensionTypeText,
                out string dimensionTypeError))
        {
            return DimensionCommandSupport.CreateError(
                dimensionTypeError);
        }

        if (!DimensionCommandSupport.TryGetRequiredDouble(
                root,
                "x",
                out double x,
                out string xError))
        {
            return DimensionCommandSupport.CreateError(
                xError);
        }

        if (!DimensionCommandSupport.TryGetRequiredDouble(
                root,
                "y",
                out double y,
                out string yError))
        {
            return DimensionCommandSupport.CreateError(
                yError);
        }

        if (!DimensionCommandSupport.TryParsePointIntent(
                firstIntentText,
                out PointIntentEnum firstPointIntent))
        {
            return DimensionCommandSupport.CreateError(
                "Неизвестный firstIntent.",
                "Допустимые значения: start, end, mid, center.");
        }

        if (!DimensionCommandSupport.TryParsePointIntent(
                secondIntentText,
                out PointIntentEnum secondPointIntent))
        {
            return DimensionCommandSupport.CreateError(
                "Неизвестный secondIntent.",
                "Допустимые значения: start, end, mid, center.");
        }

        if (!DimensionCommandSupport.TryParseDimensionType(
                dimensionTypeText,
                out DimensionTypeEnum dimensionType))
        {
            return DimensionCommandSupport.CreateError(
                "Неизвестный dimensionType.",
                "Допустимые значения: horizontal, vertical, aligned.");
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

        if (firstCurveIndex < 1 ||
            firstCurveIndex > curves.Count)
        {
            return DimensionCommandSupport.CreateError(
                $"firstCurveIndex должен быть от 1 до {curves.Count}.");
        }

        if (secondCurveIndex < 1 ||
            secondCurveIndex > curves.Count)
        {
            return DimensionCommandSupport.CreateError(
                $"secondCurveIndex должен быть от 1 до {curves.Count}.");
        }

        DrawingCurve firstCurve =
            curves[firstCurveIndex - 1];

        DrawingCurve secondCurve =
            curves[secondCurveIndex - 1];

        try
        {
            GeometryIntent firstGeometryIntent =
                sheet.CreateGeometryIntent(
                    firstCurve,
                    firstPointIntent);

            GeometryIntent secondGeometryIntent =
                sheet.CreateGeometryIntent(
                    secondCurve,
                    secondPointIntent);

            Point2d textOrigin =
                _inventor.TransientGeometry.CreatePoint2d(
                    x,
                    y);

            GeneralDimensions generalDimensions =
                sheet.DrawingDimensions.GeneralDimensions;

            dynamic dimensionsDynamic =
                generalDimensions;

            object createdDimension =
                dimensionsDynamic.AddLinear(
                    textOrigin,
                    firstGeometryIntent,
                    secondGeometryIntent,
                    dimensionType);

            drawingDocument.Update();

            dynamic dimensionDynamic =
                createdDimension;

            string dimensionObjectType =
                createdDimension
                    .GetType()
                    .Name;

            string text = string.Empty;

            try
            {
                text =
                    (string)dimensionDynamic.Text.Text;
            }
            catch
            {
            }

            return DimensionCommandSupport.CreateSuccess(
                new
                {
                    message =
                        "Линейный размер создан.",

                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    view =
                        view.Name,

                    firstCurveIndex,

                    firstIntent =
                        firstPointIntent.ToString(),

                    secondCurveIndex,

                    secondIntent =
                        secondPointIntent.ToString(),

                    dimensionType =
                        dimensionType.ToString(),

                    position =
                        new
                        {
                            x,
                            y
                        },

                    dimensionObjectType,

                    text,

                    dirty =
                        drawingDocument.Dirty
                });
        }
        catch (Exception exception)
        {
            return DimensionCommandSupport.CreateError(
                "Не удалось создать линейный размер.",
                exception.Message);
        }
    }
}
