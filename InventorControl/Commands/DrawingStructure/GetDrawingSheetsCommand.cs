using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingSheetsCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDrawingSheetsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_drawing_sheets";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawing =
            DrawingStructureReadSupport
                .GetActiveDrawing(
                    _inventor,
                    out string? error);

        if (drawing == null)
        {
            return DrawingStructureReadSupport
                .CreateError(
                    error ??
                    "Не удалось получить чертёж.");
        }

        List<object> sheets =
            DrawingStructureReadSupport
                .ReadSheets(
                    drawing);

        return DrawingStructureReadSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawing.DisplayName,
                    sheetCount =
                        sheets.Count,
                    sheets
                });
    }
}
