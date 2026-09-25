using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSketchGeometryCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetSketchGeometryCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_sketch_geometry";

    public string Execute(
        JsonElement root)
    {
        PartDocument? part =
            ModelConstraintReadSupport
                .ResolvePartDocument(
                    _inventor,
                    root,
                    out DrawingDocument? drawing,
                    out Sheet? sheet,
                    out DrawingView? view,
                    out string? error);

        if (part == null)
        {
            return ModelConstraintReadSupport
                .CreateError(
                    error ??
                    "Не удалось получить модель детали.");
        }

        if (!ModelConstraintReadSupport
                .TryGetRequiredString(
                    root,
                    "sketchName",
                    out string sketchName,
                    out string? sketchError))
        {
            return ModelConstraintReadSupport
                .CreateError(
                    sketchError ??
                    "Некорректное имя эскиза.");
        }

        object? sketch =
            ModelConstraintReadSupport
                .FindSketchByName(
                    part,
                    sketchName);

        if (sketch == null)
        {
            return ModelConstraintReadSupport
                .CreateError(
                    $"Эскиз \"{sketchName}\" не найден.");
        }

        List<object> geometry =
            ModelConstraintReadSupport
                .ReadSketchGeometry(sketch);

        return ModelConstraintReadSupport
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
                        ModelConstraintReadSupport
                            .ReadDocument(part),
                    sketchName,
                    geometryCount = geometry.Count,
                    geometry
                });
    }
}
