using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingTextObjectsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDrawingTextObjectsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_drawing_text_objects";

    public string Execute(
        JsonElement root)
    {
        return DrawingTextReadSupport
            .Execute(
                _inventor,
                root);
    }
}
