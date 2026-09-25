using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetBalloonsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetBalloonsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_balloons";

    public string Execute(
        JsonElement root)
    {
        return AnnotationReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "balloons",
                AnnotationReadSupport
                    .ReadBalloons);
    }
}
