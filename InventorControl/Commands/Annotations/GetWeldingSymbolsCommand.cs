using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetWeldingSymbolsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetWeldingSymbolsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_welding_symbols";

    public string Execute(
        JsonElement root)
    {
        return WeldingSymbolReadSupport
            .Execute(
                _inventor,
                root);
    }
}
