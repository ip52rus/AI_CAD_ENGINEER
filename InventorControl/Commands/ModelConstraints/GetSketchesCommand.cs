using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSketchesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetSketchesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_sketches";

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

        List<object> sketches =
            ModelConstraintReadSupport
                .ReadSketches(part);

        return ModelConstraintReadSupport
            .CreateSuccess(
                new
                {
                    drawing = drawing!.DisplayName,
                    sheet = sheet!.Name,
                    view = view!.Name,
                    modelDocument =
                        ModelConstraintReadSupport
                            .ReadDocument(part),
                    sketchCount = sketches.Count,
                    sketches
                });
    }
}
