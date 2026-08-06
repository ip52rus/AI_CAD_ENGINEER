using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingViewsDetailedCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDrawingViewsDetailedCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_drawing_views_detailed";

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

        if (!DrawingStructureReadSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string? sheetError))
        {
            return DrawingStructureReadSupport
                .CreateError(
                    sheetError ??
                    "Некорректное имя листа.");
        }

        Sheet? sheet =
            DrawingStructureReadSupport
                .FindSheet(
                    drawing,
                    sheetName);

        if (sheet == null)
        {
            return DrawingStructureReadSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        List<object> views =
            DrawingStructureReadSupport
                .ReadViews(
                    sheet);

        return DrawingStructureReadSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawing.DisplayName,
                    sheet =
                        sheet.Name,
                    viewCount =
                        views.Count,
                    views
                });
    }
}
