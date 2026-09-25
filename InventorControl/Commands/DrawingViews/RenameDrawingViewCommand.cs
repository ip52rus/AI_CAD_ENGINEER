using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class RenameDrawingViewCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public RenameDrawingViewCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "rename_drawing_view";

    public string Execute(JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DrawingViewCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return DrawingViewCommandSupport.CreateError(
                documentError ?? "Не удалось получить активный чертёж.");
        }

        if (!DrawingViewCommandSupport.TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out string viewNameError))
        {
            return DrawingViewCommandSupport.CreateError(viewNameError);
        }

        if (!DrawingViewCommandSupport.TryGetRequiredString(
                root,
                "newName",
                out string newName,
                out string newNameError))
        {
            return DrawingViewCommandSupport.CreateError(newNameError);
        }

        Sheet sheet = drawingDocument.ActiveSheet;

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(sheet, viewName);

        if (drawingView == null)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Вид \"{viewName}\" не найден.");
        }

        DrawingView? duplicate =
            DrawingViewCommandSupport.FindDrawingView(sheet, newName);

        if (duplicate != null &&
            !string.Equals(
                duplicate.Name,
                drawingView.Name,
                StringComparison.OrdinalIgnoreCase))
        {
            return DrawingViewCommandSupport.CreateError(
                $"Вид с именем \"{newName}\" уже существует.");
        }

        string previousName = drawingView.Name;

        try
        {
            drawingView.Name = newName;
            drawingDocument.Update();

            return DrawingViewCommandSupport.CreateSuccess(
                new
                {
                    message = $"Вид \"{previousName}\" переименован.",
                    previousName,
                    newName = drawingView.Name
                });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Не удалось переименовать вид \"{previousName}\".",
                exception.Message);
        }
    }
}
