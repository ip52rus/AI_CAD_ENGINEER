using System.Text.Encodings.Web;
using System.Text.Json;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class PingCommand : IInventorCommand
{
    public string Name =>
        "ping";

    public string Execute(
        JsonElement root)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    true,

                data =
                    new
                    {
                        message =
                            "Связь с InventorControl работает."
                    }
            },
            new JsonSerializerOptions
            {
                WriteIndented =
                    true,

                Encoder =
                    JavaScriptEncoder
                        .UnsafeRelaxedJsonEscaping
            });
    }
}