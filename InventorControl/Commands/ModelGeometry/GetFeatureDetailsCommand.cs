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
        ModelGeometryReadSupport.PartDocumentResolution? resolution =
            ModelGeometryReadSupport
                .ResolvePartDocument(
                    _inventor,
                    root,
                    out string? error);

        if (resolution == null)
        {
            return ModelGeometryReadSupport
                .CreateError(error ?? "Unable to resolve model document.");
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

        PartDocument part =
            resolution.PartDocument;

        object? feature =
            ModelGeometryReadSupport
                .FindFeatureByName(
                    part,
                    featureName);

        if (feature == null)
        {
            return ModelGeometryReadSupport
                .CreateError(
                    $"Feature \"{featureName}\" was not found.");
        }

        return ModelGeometryReadSupport
            .CreateSuccess(
                new
                {
                    drawing =
                        resolution.Drawing?.DisplayName,

                    sheet =
                        resolution.Sheet?.Name,

                    view =
                        resolution.View?.Name,

                    source =
                        resolution.Source,

                    target =
                        ModelGeometryReadSupport
                            .ReadAssemblyTargetContext(
                                resolution),

                    modelDocument =
                        ModelGeometryReadSupport
                            .ReadDocument(part),

                    feature =
                        ModelGeometryReadSupport
                            .ReadFeatureDetails(feature)
                });
    }
}
