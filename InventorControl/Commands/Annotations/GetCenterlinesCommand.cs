using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetCenterlinesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetCenterlinesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_centerlines";

    public string Execute(
        JsonElement root)
    {
        return AnnotationReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "centerlines",
                AnnotationReadSupport
                    .ReadCenterlines);
    }
}
