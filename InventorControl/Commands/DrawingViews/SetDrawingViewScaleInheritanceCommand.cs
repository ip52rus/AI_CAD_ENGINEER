using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDrawingViewScaleInheritanceCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetDrawingViewScaleInheritanceCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_drawing_view_scale_inheritance";

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
                    "scaleFromBase",
                    out bool scaleFromBase,
                    out string scaleFromBaseError))
        {
            return DrawingViewCommandSupport
                .CreateError(
                    scaleFromBaseError);
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

        DrawingView? parentView =
            drawingView.ParentView;

        if (parentView == null)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    "У базового вида нет родительского вида, " +
                    "поэтому наследование масштаба неприменимо.");
        }

        bool previousValue =
            drawingView.ScaleFromBase;

        try
        {
            drawingView.ScaleFromBase =
                scaleFromBase;

            drawingDocument.Update();

            return DrawingViewCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Наследование масштаба вида " +
                            $"\"{drawingView.Name}\" изменено.",

                        view =
                            drawingView.Name,

                        parentView =
                            parentView.Name,

                        previousScaleFromBase =
                            previousValue,

                        scaleFromBase =
                            drawingView.ScaleFromBase,

                        scale =
                            drawingView.Scale
                    });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    $"Не удалось изменить наследование " +
                    $"масштаба вида \"{drawingView.Name}\".",
                    exception.Message);
        }
    }
}
