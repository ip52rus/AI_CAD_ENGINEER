using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetEdgeSymbolsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetEdgeSymbolsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_edge_symbols";

    public string Execute(
        JsonElement root)
    {
        return EdgeSymbolReadSupport
            .Execute(
                _inventor,
                root);
    }
}
