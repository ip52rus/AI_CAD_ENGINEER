using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetModelParametersCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetModelParametersCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_model_parameters";

    public string Execute(JsonElement root)
    {
        Document? activeDocument =
            _inventor.ActiveDocument;

        if (activeDocument == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    "В Inventor нет активного документа.");
        }

        bool includeModel =
            ModelGeometryReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeModel",
                    true);

        bool includeUser =
            ModelGeometryReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeUser",
                    true);

        bool includeReference =
            ModelGeometryReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeReference",
                    true);

        if (activeDocument.DocumentType ==
            DocumentTypeEnum.kPartDocumentObject)
        {
            PartDocument activePart =
                (PartDocument)activeDocument;

            List<object> activePartParameters =
                ModelGeometryReadSupport
                    .ReadAllParameters(
                        activePart,
                        includeModel,
                        includeUser,
                        includeReference);

            return ModelGeometryReadSupport
                .CreateSuccess(
                    new
                    {
                        document =
                            ModelGeometryReadSupport
                                .ReadDocument(
                                    activePart),

                        modelDocument =
                            ModelGeometryReadSupport
                                .ReadDocument(
                                    activePart),

                        source =
                            "activePartDocument",

                        includeModel,

                        includeUser,

                        includeReference,

                        parameterCount =
                            activePartParameters.Count,

                        parameters =
                            activePartParameters
                    });
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    "Active document must be a PartDocument or DrawingDocument.",
                    activeDocument.DocumentType.ToString());
        }

        DrawingDocument drawing =
            (DrawingDocument)activeDocument;

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetError))
        {
            return ModelGeometryReadSupport
                .CreateError(sheetError);
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewError))
        {
            return ModelGeometryReadSupport
                .CreateError(viewError);
        }

        Sheet? sheet =
            ModelGeometryReadSupport
                .FindSheet(drawing, sheetName);

        DrawingView? view =
            sheet == null
                ? null
                : ModelGeometryReadSupport
                    .FindView(sheet, viewName);

        if (sheet == null || view == null)
        {
            return ModelGeometryReadSupport
                .CreateError("Лист или вид не найден.");
        }

        PartDocument? part =
            ModelGeometryReadSupport
                .GetReferencedPartDocument(
                    view,
                    out string? referenceError);

        if (part == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    "Не удалось получить модель детали.",
                    referenceError);
        }

        List<object> parameters =
            ModelGeometryReadSupport
                .ReadAllParameters(
                    part,
                    includeModel,
                    includeUser,
                    includeReference);

        return ModelGeometryReadSupport
            .CreateSuccess(
                new
                {
                    drawing = drawing.DisplayName,
                    sheet = sheet.Name,
                    view = view.Name,
                    modelDocument =
                        ModelGeometryReadSupport
                            .ReadDocument(part),
                    includeModel,
                    includeUser,
                    includeReference,
                    parameterCount = parameters.Count,
                    parameters
                });
    }
}
