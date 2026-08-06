using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetLeaderNotesCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetLeaderNotesCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_leader_notes";

    public string Execute(
        JsonElement root)
    {
        return AnnotationReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "leader_notes",
                AnnotationReadSupport
                    .ReadLeaderNotes);
    }
}
