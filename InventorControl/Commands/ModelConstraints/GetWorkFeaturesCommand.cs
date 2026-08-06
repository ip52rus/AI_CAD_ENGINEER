using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetWorkFeaturesCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetWorkFeaturesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_work_features";

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

        object workFeatures =
            ModelConstraintReadSupport
                .ReadWorkFeatures(part);

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
                    workFeatures
                });
    }
}
