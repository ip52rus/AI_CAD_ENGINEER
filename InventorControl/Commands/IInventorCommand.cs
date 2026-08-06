using System.Text.Json;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public interface IInventorCommand
{
    string Name { get; }

    string Execute(
        JsonElement root);
}