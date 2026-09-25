using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetRevisionCloudsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetRevisionCloudsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_revision_clouds";

    public string Execute(
        JsonElement root)
    {
        return RevisionCloudReadSupport
            .Execute(
                _inventor,
                root);
    }
}
