using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSketchedSymbolDefinitionsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetSketchedSymbolDefinitionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_sketched_symbol_definitions";

    public string Execute(
        JsonElement root)
    {
        return SketchedSymbolDefinitionReadSupport
            .Execute(
                _inventor,
                root);
    }
}
