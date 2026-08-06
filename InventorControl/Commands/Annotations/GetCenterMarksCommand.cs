using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetCenterMarksCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetCenterMarksCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_center_marks";

    public string Execute(
        JsonElement root)
    {
        return AnnotationReadSupport
            .ExecuteSheetRead(
                _inventor,
                root,
                "center_marks",
                AnnotationReadSupport
                    .ReadCenterMarks);
    }
}
