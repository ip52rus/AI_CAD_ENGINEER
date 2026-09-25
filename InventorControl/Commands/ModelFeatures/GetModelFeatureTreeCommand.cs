using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetModelFeatureTreeCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetModelFeatureTreeCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_model_feature_tree";

    public string Execute(
        JsonElement root)
    {
        bool includeSuppressed =
            ModelFeatureReadSupport
                .GetOptionalBoolean(
                    root,
                    "includeSuppressed",
                    true);

        ModelGeometryReadSupport.PartDocumentResolution? resolution =
            ModelGeometryReadSupport
                .ResolvePartDocument(
                    _inventor,
                    root,
                    out string? error);

        if (resolution == null)
        {
            return ModelFeatureReadSupport
                .CreateError(
                    error ?? "Unable to resolve model document.");
        }

        PartDocument part =
            resolution.PartDocument;

        List<object> features =
            ModelFeatureReadSupport
                .ReadFeatureTree(
                    resolution.ModelDocument,
                    includeSuppressed);

        return ModelFeatureReadSupport
            .CreateSuccess(
                new
                {
                    document =
                        resolution.Source == "activePartDocument"
                            ? ModelFeatureReadSupport
                                .ReadDocument(
                                    resolution.ModelDocument)
                            : null,

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
                        ModelFeatureReadSupport
                            .ReadDocument(
                                resolution.ModelDocument),

                    includeSuppressed,

                    featureCount =
                        features.Count,

                    features
                });
    }
}
