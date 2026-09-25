using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateBaseViewCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateBaseViewCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_base_view";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            BaseViewCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return BaseViewCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!BaseViewCommandSupport
                .TryGetRequiredString(
                    root,
                    "modelDocument",
                    out string modelDocumentName,
                    out string modelError))
        {
            return BaseViewCommandSupport
                .CreateError(
                    modelError);
        }

        if (!BaseViewCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "x",
                    out double x,
                    out string xError))
        {
            return BaseViewCommandSupport
                .CreateError(
                    xError);
        }

        if (!BaseViewCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "y",
                    out double y,
                    out string yError))
        {
            return BaseViewCommandSupport
                .CreateError(
                    yError);
        }

        if (!BaseViewCommandSupport
                .TryGetRequiredDouble(
                    root,
                    "scale",
                    out double scale,
                    out string scaleError))
        {
            return BaseViewCommandSupport
                .CreateError(
                    scaleError);
        }

        if (scale <= 0.0)
        {
            return BaseViewCommandSupport
                .CreateError(
                    "Масштаб вида должен быть больше нуля.");
        }

        string orientationText =
            BaseViewCommandSupport
                .GetOptionalString(
                    root,
                    "orientation",
                    "front");

        if (!BaseViewCommandSupport
                .TryParseOrientation(
                    orientationText,
                    out ViewOrientationTypeEnum orientation,
                    out string orientationError))
        {
            return BaseViewCommandSupport
                .CreateError(
                    orientationError);
        }

        string styleText =
            BaseViewCommandSupport
                .GetOptionalString(
                    root,
                    "style",
                    "hidden_line_removed");

        DrawingViewStyleEnum style =
            BaseViewCommandSupport
                .ParseViewStyle(
                    styleText);

        string modelViewName =
            BaseViewCommandSupport
                .GetOptionalString(
                    root,
                    "modelViewName");

        Document? modelDocument =
            BaseViewCommandSupport
                .FindOpenModelDocument(
                    _inventor,
                    modelDocumentName);

        if (modelDocument == null)
        {
            return BaseViewCommandSupport
                .CreateError(
                    "Указанный документ модели не найден среди открытых документов.",
                    "Сначала открой модель командой open_document, " +
                    "затем передай её DisplayName, имя файла или полный путь.");
        }

        Sheet sheet =
            drawingDocument.ActiveSheet;

        if (!BaseViewCommandSupport
                .IsPointInsideSheet(
                    sheet,
                    x,
                    y))
        {
            return BaseViewCommandSupport
                .CreateError(
                    "Позиция базового вида находится вне листа.",
                    $"Размер листа: {sheet.Width:F3} × {sheet.Height:F3}. " +
                    $"Получено: X={x:F3}; Y={y:F3}.");
        }

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            Inventor._Document modelForView =
    (Inventor._Document)modelDocument;

            DrawingView drawingView =
                sheet.DrawingViews
                    .AddBaseView(
                        modelForView,
                        position,
                        scale,
                        orientation,
                        style,
                        modelViewName);

            drawingDocument.Update();

            return BaseViewCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Базовый вид создан.",

                        drawing =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        modelDocument =
                            modelDocument.DisplayName,

                        name =
                            drawingView.Name,

                        viewType =
                            drawingView.ViewType
                                .ToString(),

                        orientation =
                            drawingView.Camera
                                .ViewOrientationType
                                .ToString(),

                        position =
                            new
                            {
                                x =
                                    drawingView.Position.X,

                                y =
                                    drawingView.Position.Y
                            },

                        width =
                            drawingView.Width,

                        height =
                            drawingView.Height,

                        scale =
                            drawingView.Scale,

                        scaleString =
                            drawingView.ScaleString,

                        style =
                            style.ToString(),

                        modelViewName,

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return BaseViewCommandSupport
                .CreateError(
                    "Не удалось создать базовый вид.",
                    exception.Message);
        }
    }
}
