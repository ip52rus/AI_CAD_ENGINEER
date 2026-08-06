using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingViewCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDrawingViewCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "get_drawing_view";

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

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                drawingDocument.ActiveSheet,
                viewName);

        if (drawingView == null)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Вид \"{viewName}\" не найден.");
        }

        string? parentViewName = null;

        try
        {
            parentViewName = drawingView.ParentView?.Name;
        }
        catch
        {
        }

        return DrawingViewCommandSupport.CreateSuccess(
            new
            {
                name = drawingView.Name,
                viewType = drawingView.ViewType.ToString(),
                orientation = drawingView.Camera.ViewOrientationType.ToString(),
                position = new
                {
                    x = drawingView.Position.X,
                    y = drawingView.Position.Y
                },
                center = new
                {
                    x = drawingView.Center.X,
                    y = drawingView.Center.Y
                },
                width = drawingView.Width,
                height = drawingView.Height,
                rotationRadians = drawingView.Rotation,
                rotationDegrees = drawingView.Rotation * 180.0 / Math.PI,
                scale = drawingView.Scale,
                scaleString = drawingView.ScaleString,
                scaleFromBase = drawingView.ScaleFromBase,
                aligned = drawingView.Aligned,
                suppressed = drawingView.Suppressed,
                viewStyle = drawingView.ViewStyle.ToString(),
                showLabel = drawingView.ShowLabel,
                parentView = parentViewName,
                left = drawingView.Left,
                top = drawingView.Top,
                upToDate = drawingView.UpToDate
            });
    }
}
