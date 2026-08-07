using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSurfaceTextureSymbolsCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetSurfaceTextureSymbolsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_surface_texture_symbols";

    public string Execute(
        JsonElement root)
    {
        return SurfaceTextureSymbolReadSupport
            .Execute(
                _inventor,
                root);
    }
}
