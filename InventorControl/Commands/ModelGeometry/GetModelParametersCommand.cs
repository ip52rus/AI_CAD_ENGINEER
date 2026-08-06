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
        DrawingDocument? drawing =
            ModelGeometryReadSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawing == null)
        {
            return ModelGeometryReadSupport
                .CreateError(documentError!);
        }

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
