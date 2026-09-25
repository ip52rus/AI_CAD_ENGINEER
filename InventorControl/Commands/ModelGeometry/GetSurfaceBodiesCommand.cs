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

        List<object> bodies =
            ModelGeometryReadSupport
                .ReadSurfaceBodies(part);

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

                    bodyCount =
                        bodies.Count,

                    bodies
                });
    }
}
