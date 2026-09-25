using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetHoleTablesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetHoleTablesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_hole_tables";

    public string Execute(
        JsonElement root)
    {
        return TableReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "hole_tables",
                TableReadSupport
                    .ReadHoleTablesDetailed);
    }
}
