using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class RotateDrawingViewCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public RotateDrawingViewCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "rotate_drawing_view";

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

        if (!DrawingViewCommandSupport.TryGetRequiredDouble(
                root,
                "angleDegrees",
                out double angleDegrees,
                out string angleError))
        {
            return DrawingViewCommandSupport.CreateError(angleError);
        }

        string mode = "relative";
        if (root.TryGetProperty("mode", out JsonElement modeElement) &&
            modeElement.ValueKind == JsonValueKind.String)
        {
            mode = modeElement.GetString()?.Trim().ToLowerInvariant()
                   ?? "relative";
        }

        bool clockwise = true;
        if (root.TryGetProperty("clockwise", out JsonElement clockwiseElement) &&
            (clockwiseElement.ValueKind == JsonValueKind.True ||
             clockwiseElement.ValueKind == JsonValueKind.False))
        {
            clockwise = clockwiseElement.GetBoolean();
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

        double previousRotation = drawingView.Rotation;
        double angleRadians = Math.Abs(angleDegrees) * Math.PI / 180.0;

        try
        {
            if (drawingView.Aligned)
            {
                drawingView.Aligned = false;
            }

            if (mode == "absolute")
            {
                drawingView.Rotation = clockwise
                    ? -angleRadians
                    : angleRadians;
            }
            else if (mode == "relative")
            {
                drawingView.RotateByAngle(angleRadians, clockwise);
            }
            else
            {
                return DrawingViewCommandSupport.CreateError(
                    "Поле \"mode\" должно содержать \"relative\" или \"absolute\".");
            }

            drawingDocument.Update();

            return DrawingViewCommandSupport.CreateSuccess(
                new
                {
                    message = $"Вид \"{drawingView.Name}\" повёрнут.",
                    view = drawingView.Name,
                    mode,
                    clockwise,
                    requestedAngleDegrees = angleDegrees,
                    previousRotationDegrees = previousRotation * 180.0 / Math.PI,
                    newRotationDegrees = drawingView.Rotation * 180.0 / Math.PI,
                    aligned = drawingView.Aligned
                });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport.CreateError(
                $"Не удалось повернуть вид \"{drawingView.Name}\".",
                exception.Message);
        }
    }
}
