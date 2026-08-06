using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetDrawingViewAlignmentCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetDrawingViewAlignmentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_drawing_view_alignment";

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
                    "aligned",
                    out bool aligned,
                    out string alignedError))
        {
            return DrawingViewCommandSupport
                .CreateError(
                    alignedError);
        }

        Sheet sheet =
            drawingDocument.ActiveSheet;

        DrawingView? drawingView =
            DrawingViewCommandSupport
                .FindDrawingView(
                    sheet,
                    viewName);

        if (drawingView == null)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    $"Вид \"{viewName}\" не найден.");
        }

        bool previousAligned =
            drawingView.Aligned;

        try
        {
            if (!aligned)
            {
                drawingView.Aligned =
                    false;

                drawingDocument.Update();

                return DrawingViewCommandSupport
                    .CreateSuccess(
                        new
                        {
                            message =
                                $"Выравнивание вида " +
                                $"\"{drawingView.Name}\" отключено.",

                            view =
                                drawingView.Name,

                            previousAligned,

                            aligned =
                                drawingView.Aligned
                        });
            }

            if (!DrawingViewCommandSupport
                    .TryGetRequiredString(
                        root,
                        "targetViewName",
                        out string targetViewName,
                        out string targetViewNameError))
            {
                return DrawingViewCommandSupport
                    .CreateError(
                        targetViewNameError);
            }

            if (!DrawingViewCommandSupport
                    .TryGetRequiredString(
                        root,
                        "alignmentType",
                        out string alignmentTypeText,
                        out string alignmentTypeError))
            {
                return DrawingViewCommandSupport
                    .CreateError(
                        alignmentTypeError);
            }

            DrawingView? targetView =
                DrawingViewCommandSupport
                    .FindDrawingView(
                        sheet,
                        targetViewName);

            if (targetView == null)
            {
                return DrawingViewCommandSupport
                    .CreateError(
                        $"Целевой вид \"{targetViewName}\" не найден.");
            }

            if (!TryParseAlignmentType(
                    alignmentTypeText,
                    out DrawingViewAlignmentEnum alignmentType))
            {
                return DrawingViewCommandSupport
                    .CreateError(
                        "Неизвестный тип выравнивания.",
                        "Допустимые значения: horizontal, " +
                        "vertical, in_position.");
            }

            if (drawingView.Aligned)
            {
                drawingView.Aligned =
                    false;
            }

            drawingView.Align(
                targetView,
                alignmentType);

            drawingDocument.Update();

            return DrawingViewCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Вид \"{drawingView.Name}\" выровнен.",

                        view =
                            drawingView.Name,

                        targetView =
                            targetView.Name,

                        alignmentType =
                            alignmentType.ToString(),

                        previousAligned,

                        aligned =
                            drawingView.Aligned
                    });
        }
        catch (Exception exception)
        {
            return DrawingViewCommandSupport
                .CreateError(
                    $"Не удалось изменить выравнивание вида " +
                    $"\"{drawingView.Name}\".",
                    exception.Message);
        }
    }

    private static bool TryParseAlignmentType(
        string value,
        out DrawingViewAlignmentEnum alignmentType)
    {
        switch (value
                    .Trim()
                    .ToLowerInvariant())
        {
            case "horizontal":
                alignmentType =
                    DrawingViewAlignmentEnum
                        .kHorizontalViewAlignment;

                return true;

            case "vertical":
                alignmentType =
                    DrawingViewAlignmentEnum
                        .kVerticalViewAlignment;

                return true;

            case "in_position":
                alignmentType =
                    DrawingViewAlignmentEnum
                        .kInPositionViewAlignment;

                return true;

            default:
                alignmentType =
                    default;

                return false;
        }
    }
}
