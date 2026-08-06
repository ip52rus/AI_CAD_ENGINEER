using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDrawingViewLabelVisibilityCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetDrawingViewLabelVisibilityCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_drawing_view_label_visibility";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DrawingViewCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!DrawingViewCommandSupport
                .TryGetRequiredString(
                    root,
                    "viewName",
                    out string viewName,
                    out string viewNameError))
        {
            return DrawingViewCommandSupport
                .CreateError(
                    viewNameError);
        }

        if (!DrawingViewCommandSupport
                .TryGetRequiredBoolean(
                    root,
                    "visible",
                    out bool visible,
                    out string visibleError))
        {
            return DrawingViewCommandSupport
                .CreateError(
                    visibleError);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport
                .FindDrawingView(
                    drawingDocument.ActiveSheet,
                    viewName);

        if (drawingView == null)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    $"Вид \"{viewName}\" не найден.");
        }

        bool previousValue =
            drawingView.ShowLabel;

        try
        {
            drawingView.ShowLabel =
                visible;

            drawingDocument.Update();

            return DrawingViewCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Видимость обозначения вида " +
                            $"\"{drawingView.Name}\" изменена.",

                        view =
                            drawingView.Name,

                        previousVisible =
                            previousValue,

                        visible =
                            drawingView.ShowLabel
                    });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    $"Не удалось изменить видимость " +
                    $"обозначения вида \"{drawingView.Name}\".",
                    exception.Message);
        }
    }
}
