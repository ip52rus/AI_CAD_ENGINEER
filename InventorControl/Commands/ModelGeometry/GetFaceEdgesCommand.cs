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
                    "The specified body or face was not found.");
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

                    data
                });
    }
}
