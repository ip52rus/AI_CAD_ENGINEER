using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDrawingViewSuppressedCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetDrawingViewSuppressedCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "set_drawing_view_suppressed";

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

        if (!DrawingViewCommandSupport.TryGetRequiredBoolean(
                root,
                "suppressed",
                out bool suppressed,
                out string suppressedError))
        {
            return DrawingViewCommandSupport.CreateError(suppressedError);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                drawingDocument.ActiveSheet,
                viewName);

        if (drawingView == null)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Вид \"{viewName}\" не найден.");
        }

        bool previousValue = drawingView.Suppressed;

        try
        {
            drawingView.Suppressed = suppressed;
            drawingDocument.Update();

            return DrawingViewCommandSupport.CreateSuccess(
                new
                {
                    message = $"Состояние подавления вида \"{drawingView.Name}\" изменено.",
                    view = drawingView.Name,
                    previousSuppressed = previousValue,
                    suppressed = drawingView.Suppressed
                });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Не удалось изменить состояние вида \"{drawingView.Name}\".",
                exception.Message);
        }
    }
}
