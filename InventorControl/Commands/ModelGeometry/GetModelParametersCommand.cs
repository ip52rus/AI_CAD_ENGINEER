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

        PartDocument part =
            resolution.PartDocument;

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
                    document =
                        resolution.Source == "activePartDocument"
                            ? ModelGeometryReadSupport
                                .ReadDocument(part)
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
                        ModelGeometryReadSupport
                            .ReadDocument(part),

                    includeModel,

                    includeUser,

                    includeReference,

                    parameterCount =
                        parameters.Count,

                    parameters
                });
    }
}
