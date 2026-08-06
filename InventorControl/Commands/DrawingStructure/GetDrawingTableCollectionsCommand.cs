using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingTableCollectionsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetDrawingTableCollectionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_drawing_table_collections";

    public string Execute(
        JsonElement root)
    {
        return TableReadSupport
            .ExecuteTableCollectionsRead(
                _inventor,
                root);
    }
}
