using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetPartsListsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetPartsListsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_parts_lists";

    public string Execute(
        JsonElement root)
    {
        return TableReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "parts_lists",
                TableReadSupport
                    .ReadPartsLists);
    }
}
