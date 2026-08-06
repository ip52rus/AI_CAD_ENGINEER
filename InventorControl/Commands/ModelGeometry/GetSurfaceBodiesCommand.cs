using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSurfaceBodiesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetSurfaceBodiesCommand(
        Inventor.Application inventor)
    {
        _inventor = inventor;
    }

    public string Name =>
        "get_surface_bodies";

    public string Execute(JsonElement root)
    {
        PartDocument? part =
            GetPart(
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

        List<object> bodies =
            ModelGeometryReadSupport
                .ReadSurfaceBodies(part);

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
                    bodyCount = bodies.Count,
                    bodies
                });
    }

    private PartDocument? GetPart(
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
