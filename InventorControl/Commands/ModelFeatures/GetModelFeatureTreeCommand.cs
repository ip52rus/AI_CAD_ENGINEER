using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetModelFeatureTreeCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetModelFeatureTreeCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_model_feature_tree";

    public string Execute(
        JsonElement root)
    {
        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    "В Inventor нет активного документа.");
        }

        bool includeSuppressed =
            ModelFeatureReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeSuppressed",
                    true);

        if (activeDocument.DocumentType ==
            DocumentTypeEnum.kPartDocumentObject)
        {
            List<object> activePartFeatures =
                ModelFeatureReadSupport
                    .ReadFeatureTree(
                        activeDocument,
                        includeSuppressed);

            return ModelFeatureReadSupport
                .CreateSuccess(
                    new
                    {
                        document =
                            ModelFeatureReadSupport
                                .ReadDocument(
                                    activeDocument),

                        modelDocument =
                            ModelFeatureReadSupport
                                .ReadDocument(
                                    activeDocument),

                        source =
                            "activePartDocument",

                        includeSuppressed,

                        featureCount =
                            activePartFeatures.Count,

                        features =
                            activePartFeatures
                    });
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    "Active document must be a PartDocument or DrawingDocument.",
                    activeDocument.DocumentType.ToString());
        }

        DrawingDocument drawing =
            (DrawingDocument)activeDocument;

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

        List<object> features =
            ModelFeatureReadSupport
                .ReadFeatureTree(
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

                    featureCount =
                        features.Count,

                    features
                });
    }
}
