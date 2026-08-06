using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetSheetOrientationCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetSheetOrientationCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_sheet_orientation";

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
                    "orientation",
                    out string orientationText,
                    out string orientationError))
        {
            return SheetCommandSupport
                .CreateError(
                    orientationError);
        }

        if (!TryParseOrientation(
                orientationText,
                out PageOrientationTypeEnum orientation))
        {
            return SheetCommandSupport
                .CreateError(
                    "Неизвестная ориентация листа.",
                    "Допустимые значения: landscape, portrait.");
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

        PageOrientationTypeEnum previousOrientation =
            sheet.Orientation;

        double previousWidth =
            sheet.Width;

        double previousHeight =
            sheet.Height;

        try
        {
            sheet.Orientation =
                orientation;

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Ориентация листа \"{sheet.Name}\" изменена.",

                        sheet =
                            sheet.Name,

                        previousOrientation =
                            previousOrientation.ToString(),

                        newOrientation =
                            sheet.Orientation.ToString(),

                        previousWidth,

                        previousHeight,

                        width =
                            sheet.Width,

                        height =
                            sheet.Height
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось изменить ориентацию листа " +
                    $"\"{sheet.Name}\".",
                    exception.Message);
        }
    }

    private static bool TryParseOrientation(
        string value,
        out PageOrientationTypeEnum orientation)
    {
        switch (value
                    .Trim()
                    .ToLowerInvariant())
        {
            case "landscape":
                orientation =
                    PageOrientationTypeEnum
                        .kLandscapePageOrientation;

                return true;

            case "portrait":
                orientation =
                    PageOrientationTypeEnum
                        .kPortraitPageOrientation;

                return true;

            default:
                orientation =
                    default;

                return false;
        }
    }
}
