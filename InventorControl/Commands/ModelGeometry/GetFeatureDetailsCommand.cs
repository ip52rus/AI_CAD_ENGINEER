using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetFeatureDetailsCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetFeatureDetailsCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_feature_details";

    public string Execute(JsonElement root)
    {
        if (!TryGetContext(
                root,
                out DrawingDocument? drawing,
                out Sheet? sheet,
                out DrawingView? view,
                out PartDocument? part,
                out string? error))
        {
            return ModelGeometryReadSupport
                .CreateError(error ?? "Ошибка контекста.");
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "featureName",
                    out string featureName,
                    out string featureError))
        {
            return ModelGeometryReadSupport
                .CreateError(featureError);
        }

        object? feature =
            ModelGeometryReadSupport
                .FindFeatureByName(
                    part!,
                    featureName);

        if (feature == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    $"Операция \"{featureName}\" не найдена.");
        }

        return ModelGeometryReadSupport
            .CreateSuccess(
                new
                {
                    drawing = drawing?.DisplayName,
                    sheet = sheet?.Name,
                    view = view?.Name,
                    source =
                        drawing == null
                            ? "activePartDocument"
                            : "drawingViewReferencedPartDocument",
                    modelDocument =
                        ModelGeometryReadSupport
                            .ReadDocument(part!),
                    feature =
                        ModelGeometryReadSupport
                            .ReadFeatureDetails(feature)
                });
    }

    private bool TryGetContext(
        JsonElement root,
        out DrawingDocument? drawing,
        out Sheet? sheet,
        out DrawingView? view,
        out PartDocument? part,
        out string? error)
    {
        drawing = null;
        sheet = null;
        view = null;
        part = null;
        error = null;

        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error = "В Inventor нет активного документа.";
            return false;
        }

        if (activeDocument.DocumentType ==
            DocumentTypeEnum.kPartDocumentObject)
        {
            part =
                (PartDocument)activeDocument;

            return true;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error = "Active document must be a PartDocument or DrawingDocument.";
            return false;
        }

        drawing =
            (DrawingDocument)activeDocument;

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out error))
        {
            return false;
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out error))
        {
            return false;
        }

        sheet =
            ModelGeometryReadSupport
                .FindSheet(drawing, sheetName);

        if (sheet == null)
        {
            error = $"Лист \"{sheetName}\" не найден.";
            return false;
        }

        view =
            ModelGeometryReadSupport
                .FindView(sheet, viewName);

        if (view == null)
        {
            error = $"Вид \"{viewName}\" не найден.";
            return false;
        }

        part =
            ModelGeometryReadSupport
                .GetReferencedPartDocument(
                    view,
                    out error);

        return part != null;
    }
}
