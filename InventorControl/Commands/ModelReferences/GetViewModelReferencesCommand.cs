using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetViewModelReferencesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetViewModelReferencesCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_view_model_references";

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

        int maxCurves =
            ModelReferenceReadSupport.GetOptionalInt(
                root,
                "maxCurves",
                0);

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

        int count =
            maxCurves > 0
                ? Math.Min(maxCurves, curves.Count)
                : curves.Count;

        List<object> result = new();
        int availableModelReferences = 0;

        for (int index = 1;
             index <= count;
             index++)
        {
            object item =
                ModelReferenceReadSupport.ReadCurve(
                    curves[index - 1],
                    index);

            result.Add(item);

            try
            {
                if (curves[index - 1].ModelGeometry != null)
                {
                    availableModelReferences++;
                }
            }
            catch
            {
            }
        }

        return ModelReferenceReadSupport.Success(
            new
            {
                document = drawing.DisplayName,
                sheet = sheet.Name,
                view = new
                {
                    name = view.Name,
                    viewType = view.ViewType.ToString(),
                    scale = view.Scale,
                    scaleString = view.ScaleString,
                    rotation = view.Rotation,
                    isFlatPatternView = view.IsFlatPatternView,
                    referencedModel =
                        ModelReferenceReadSupport
                            .ReadViewDescriptor(view)
                },
                totalCurveCount = curves.Count,
                returnedCurveCount = result.Count,
                truncated = result.Count < curves.Count,
                availableModelReferences,
                curves = result
            });
    }
}
