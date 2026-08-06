using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetHoleFeaturesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetHoleFeaturesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_hole_features";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawing =
            ModelFeatureReadSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawing == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!ModelFeatureReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetError))
        {
            return ModelFeatureReadSupport
                .CreateError(
                    sheetError);
        }

        if (!ModelFeatureReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewError))
        {
            return ModelFeatureReadSupport
                .CreateError(
                    viewError);
        }

        bool includeSuppressed =
            ModelFeatureReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeSuppressed",
                    true);

        Sheet? sheet =
            ModelFeatureReadSupport
                .FindSheet(
                    drawing,
                    sheetName);

        if (sheet == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        DrawingView? view =
            ModelFeatureReadSupport
                .FindView(
                    sheet,
                    viewName);

        if (view == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    $"Вид \"{viewName}\" не найден.");
        }

        Document? modelDocument =
            ModelFeatureReadSupport
                .GetReferencedDocument(
                    view,
                    out string? referenceError);

        if (modelDocument == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    "Не удалось получить документ модели.",
                    referenceError);
        }

        List<object> holes =
            ModelFeatureReadSupport
                .ReadHoleFeatures(
                    modelDocument,
                    includeSuppressed);

        return ModelFeatureReadSupport
            .CreateSuccess(
                new
                {
                    drawing =
                        drawing.DisplayName,

                    sheet =
                        sheet.Name,

                    view =
                        view.Name,

                    modelDocument =
                        ModelFeatureReadSupport
                            .ReadDocument(
                                modelDocument),

                    includeSuppressed,

                    holeFeatureCount =
                        holes.Count,

                    holeFeatures =
                        holes
                });
    }
}
