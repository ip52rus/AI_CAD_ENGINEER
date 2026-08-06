using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetBodyFacesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetBodyFacesCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_body_faces";

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

        PartDocument? part =
            ResolvePart(
                root,
                out DrawingDocument? drawing,
                out Sheet? sheet,
                out DrawingView? view,
                out string? error);

        if (part == null)
        {
            return ModelGeometryReadSupport
                .CreateError(error ?? "Не удалось получить модель.");
        }

        object? data =
            ModelGeometryReadSupport
                .ReadBodyFaces(
                    part,
                    bodyIndex);

        if (data == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    $"Тело с индексом {bodyIndex} не найдено.");
        }

        return ModelGeometryReadSupport
            .CreateSuccess(
                new
                {
                    drawing = drawing!.DisplayName,
                    sheet = sheet!.Name,
                    view = view!.Name,
                    modelDocument =
                        ModelGeometryReadSupport
                            .ReadDocument(part),
                    data
                });
    }

    private PartDocument? ResolvePart(
        JsonElement root,
        out DrawingDocument? drawing,
        out Sheet? sheet,
        out DrawingView? view,
        out string? error)
    {
        drawing =
            ModelGeometryReadSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out error);

        sheet = null;
        view = null;

        if (drawing == null)
        {
            return null;
        }

        if (!ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out error) ||
            !ModelGeometryReadSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out error))
        {
            return null;
        }

        sheet =
            ModelGeometryReadSupport
                .FindSheet(drawing, sheetName);

        view =
            sheet == null
                ? null
                : ModelGeometryReadSupport
                    .FindView(sheet, viewName);

        if (sheet == null || view == null)
        {
            error = "Лист или вид не найден.";
            return null;
        }

        return ModelGeometryReadSupport
            .GetReferencedPartDocument(
                view,
                out error);
    }
}
