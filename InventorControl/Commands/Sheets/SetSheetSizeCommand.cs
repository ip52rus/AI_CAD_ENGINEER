using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetSheetSizeCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetSheetSizeCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_sheet_size";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            SheetCommandSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return SheetCommandSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return SheetCommandSupport
                .CreateError(
                    sheetNameError);
        }

        if (!SheetCommandSupport
                .TryGetRequiredString(
                    root,
                    "size",
                    out string sizeText,
                    out string sizeError))
        {
            return SheetCommandSupport
                .CreateError(
                    sizeError);
        }

        if (!TryParseSheetSize(
                sizeText,
                out DrawingSheetSizeEnum sheetSize))
        {
            return SheetCommandSupport
                .CreateError(
                    "Неизвестный размер листа.",
                    "Допустимые значения: a0, a1, a2, a3, a4, custom.");
        }

        Sheet? sheet =
            SheetCommandSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        double previousWidth =
            sheet.Width;

        double previousHeight =
            sheet.Height;

        DrawingSheetSizeEnum previousSize =
            sheet.Size;

        double? requestedWidth =
            null;

        double? requestedHeight =
            null;

        try
        {
            sheet.Size =
                sheetSize;

            if (sheetSize ==
                DrawingSheetSizeEnum.kCustomDrawingSheetSize)
            {
                if (!SheetCommandSupport
                        .TryGetRequiredDouble(
                            root,
                            "width",
                            out double width,
                            out string widthError))
                {
                    return SheetCommandSupport
                        .CreateError(
                            widthError);
                }

                if (!SheetCommandSupport
                        .TryGetRequiredDouble(
                            root,
                            "height",
                            out double height,
                            out string heightError))
                {
                    return SheetCommandSupport
                        .CreateError(
                            heightError);
                }

                if (width <= 0.0 ||
                    height <= 0.0)
                {
                    return SheetCommandSupport
                        .CreateError(
                            "Ширина и высота листа должны быть больше нуля.");
                }

                requestedWidth =
                    width;

                requestedHeight =
                    height;

                sheet.Width =
                    width;

                sheet.Height =
                    height;
            }

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Размер листа \"{sheet.Name}\" изменён.",

                        sheet =
                            sheet.Name,

                        previousSize =
                            previousSize.ToString(),

                        newSize =
                            sheet.Size.ToString(),

                        previousWidth,

                        previousHeight,

                        width =
                            sheet.Width,

                        height =
                            sheet.Height,

                        requestedWidth,

                        requestedHeight
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось изменить размер листа " +
                    $"\"{sheet.Name}\".",
                    exception.Message);
        }
    }

    private static bool TryParseSheetSize(
        string value,
        out DrawingSheetSizeEnum sheetSize)
    {
        switch (value
                    .Trim()
                    .ToLowerInvariant())
        {
            case "a0":
                sheetSize =
                    DrawingSheetSizeEnum
                        .kA0DrawingSheetSize;

                return true;

            case "a1":
                sheetSize =
                    DrawingSheetSizeEnum
                        .kA1DrawingSheetSize;

                return true;

            case "a2":
                sheetSize =
                    DrawingSheetSizeEnum
                        .kA2DrawingSheetSize;

                return true;

            case "a3":
                sheetSize =
                    DrawingSheetSizeEnum
                        .kA3DrawingSheetSize;

                return true;

            case "a4":
                sheetSize =
                    DrawingSheetSizeEnum
                        .kA4DrawingSheetSize;

                return true;

            case "custom":
                sheetSize =
                    DrawingSheetSizeEnum
                        .kCustomDrawingSheetSize;

                return true;

            default:
                sheetSize =
                    default;

                return false;
        }
    }
}
