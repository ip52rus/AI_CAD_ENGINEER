using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetGeneralNotesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetGeneralNotesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_general_notes";

    public string Execute(
        JsonElement root)
    {
        return AnnotationReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "general_notes",
                AnnotationReadSupport
                    .ReadGeneralNotes);
    }
}
