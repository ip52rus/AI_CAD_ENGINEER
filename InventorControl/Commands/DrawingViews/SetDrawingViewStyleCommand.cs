using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDrawingViewStyleCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetDrawingViewStyleCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "set_drawing_view_style";

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
                "style",
                out string styleText,
                out string styleError))
        {
            return DrawingViewCommandSupport.CreateError(styleError);
        }

        if (!TryParseStyle(styleText, out DrawingViewStyleEnum style))
        {
            return DrawingViewCommandSupport.CreateError(
                "Неизвестный стиль вида.",
                "Допустимые значения: hidden_line_removed, hidden_line, shaded, shaded_hidden_line.");
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

        DrawingViewStyleEnum previousStyle = drawingView.ViewStyle;

        try
        {
            drawingView.ViewStyle = style;
            drawingDocument.Update();

            return DrawingViewCommandSupport.CreateSuccess(
                new
                {
                    message = $"Стиль вида \"{drawingView.Name}\" изменён.",
                    view = drawingView.Name,
                    previousStyle = previousStyle.ToString(),
                    newStyle = drawingView.ViewStyle.ToString()
                });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Не удалось изменить стиль вида \"{drawingView.Name}\".",
                exception.Message);
        }
    }

    private static bool TryParseStyle(
        string value,
        out DrawingViewStyleEnum style)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "hidden_line_removed":
                style = DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle;
                return true;

            case "hidden_line":
                style = DrawingViewStyleEnum.kHiddenLineDrawingViewStyle;
                return true;

            case "shaded":
                style = DrawingViewStyleEnum.kShadedDrawingViewStyle;
                return true;

            case "shaded_hidden_line":
                style = DrawingViewStyleEnum.kShadedHiddenLineDrawingViewStyle;
                return true;

            default:
                style = default;
                return false;
        }
    }
}
