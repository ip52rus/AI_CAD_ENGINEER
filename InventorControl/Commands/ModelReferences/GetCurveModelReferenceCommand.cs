using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetCurveModelReferenceCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetCurveModelReferenceCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_curve_model_reference";

    public string Execute(JsonElement root)
    {
        DrawingDocument? drawing =
            ModelReferenceReadSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawing == null)
        {
            return ModelReferenceReadSupport.Error(
                documentError ??
                "Не удалось получить активный чертёж.");
        }

        if (!ModelReferenceReadSupport.TryGetString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetError))
        {
            return ModelReferenceReadSupport.Error(sheetError);
        }

        if (!ModelReferenceReadSupport.TryGetString(
                root,
                "viewName",
                out string viewName,
                out string viewError))
        {
            return ModelReferenceReadSupport.Error(viewError);
        }

        if (!ModelReferenceReadSupport.TryGetInt(
                root,
                "curveIndex",
                out int curveIndex,
                out string curveError))
        {
            return ModelReferenceReadSupport.Error(curveError);
        }

        Sheet? sheet =
            ModelReferenceReadSupport.FindSheet(
                drawing,
                sheetName);

        if (sheet == null)
        {
            return ModelReferenceReadSupport.Error(
                $"Лист \"{sheetName}\" не найден.");
        }

        DrawingView? view =
            ModelReferenceReadSupport.FindView(
                sheet,
                viewName);

        if (view == null)
        {
            return ModelReferenceReadSupport.Error(
                $"Вид \"{viewName}\" не найден.");
        }

        List<DrawingCurve> curves =
            ModelReferenceReadSupport.GetCurves(view);

        if (curveIndex < 1 ||
            curveIndex > curves.Count)
        {
            return ModelReferenceReadSupport.Error(
                $"curveIndex должен быть от 1 до {curves.Count}.");
        }

        return ModelReferenceReadSupport.Success(
            new
            {
                document = drawing.DisplayName,
                sheet = sheet.Name,
                view = view.Name,
                referencedModel =
                    ModelReferenceReadSupport
                        .ReadViewDescriptor(view),
                curve =
                    ModelReferenceReadSupport
                        .ReadCurve(
                            curves[curveIndex - 1],
                            curveIndex)
            });
    }
}
