using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetFaceEdgesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetFaceEdgesCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_face_edges";

    public string Execute(JsonElement root)
    {
        if (!ModelGeometryReadSupport
                .TryGetRequiredInt32(
                    root,
                    "bodyIndex",
                    out int bodyIndex,
                    out string bodyError))
        {
            return ModelGeometryReadSupport
                .CreateError(bodyError);
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredInt32(
                    root,
                    "faceIndex",
                    out int faceIndex,
                    out string faceError))
        {
            return ModelGeometryReadSupport
                .CreateError(faceError);
        }

        DrawingDocument? drawing =
            ModelGeometryReadSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? error);

        if (drawing == null)
        {
            return ModelGeometryReadSupport
                .CreateError(error!);
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetError))
        {
            return ModelGeometryReadSupport
                .CreateError(
                    sheetError);
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewError))
        {
            return ModelGeometryReadSupport
                .CreateError(
                    viewError);
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

        object? data =
            ModelGeometryReadSupport
                .ReadFaceEdges(
                    part,
                    bodyIndex,
                    faceIndex);

        if (data == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    "Указанное тело или грань не найдены.");
        }

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
                    data
                });
    }
}
