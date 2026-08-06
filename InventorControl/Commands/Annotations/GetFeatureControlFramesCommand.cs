using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetFeatureControlFramesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetFeatureControlFramesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_feature_control_frames";

    public string Execute(
        JsonElement root)
    {
        return FeatureControlFrameReadSupport
            .Execute(
                _inventor,
                root);
    }
}
