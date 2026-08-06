using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateSheetCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateSheetCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_sheet";

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

        string sizeText =
            "a3";

        if (root.TryGetProperty(
                "size",
                out JsonElement sizeElement))
        {
            if (sizeElement.ValueKind !=
                JsonValueKind.String)
            {
                return SheetCommandSupport
                    .CreateError(
                        "Поле \"size\" должно быть строкой.");
            }

            sizeText =
                sizeElement
                    .GetString()?
                    .Trim()
                    .ToLowerInvariant()
                ?? "a3";
        }

        if (!TryParseSheetSize(
                sizeText,
                out DrawingSheetSizeEnum sheetSize))
        {
            return SheetCommandSupport
                .CreateError(
                    "Неизвестный размер листа.",
                    "Допустимые значения: " +
                    "a0, a1, a2, a3, a4, custom.");
        }

        string orientationText =
            "landscape";

        if (root.TryGetProperty(
                "orientation",
                out JsonElement orientationElement))
        {
            if (orientationElement.ValueKind !=
                JsonValueKind.String)
            {
                return SheetCommandSupport
                    .CreateError(
                        "Поле \"orientation\" должно быть строкой.");
            }

            orientationText =
                orientationElement
                    .GetString()?
                    .Trim()
                    .ToLowerInvariant()
                ?? "landscape";
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

        string sheetName =
            string.Empty;

        if (root.TryGetProperty(
                "sheetName",
                out JsonElement sheetNameElement))
        {
            if (sheetNameElement.ValueKind !=
                JsonValueKind.String)
            {
                return SheetCommandSupport
                    .CreateError(
                        "Поле \"sheetName\" должно быть строкой.");
            }

            sheetName =
                sheetNameElement
                    .GetString()?
                    .Trim()
                ?? string.Empty;
        }

        object? width =
            Type.Missing;

        object? height =
            Type.Missing;

        double? requestedWidth =
            null;

        double? requestedHeight =
            null;

        if (sheetSize ==
            DrawingSheetSizeEnum.kCustomDrawingSheetSize)
        {
            if (!SheetCommandSupport
                    .TryGetRequiredDouble(
                        root,
                        "width",
                        out double customWidth,
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
                        out double customHeight,
                        out string heightError))
            {
                return SheetCommandSupport
                    .CreateError(
                        heightError);
            }

            if (customWidth <= 0.0 ||
                customHeight <= 0.0)
            {
                return SheetCommandSupport
                    .CreateError(
                        "Ширина и высота листа должны быть больше нуля.");
            }

            requestedWidth =
                customWidth;

            requestedHeight =
                customHeight;

            width =
                customWidth;

            height =
                customHeight;
        }

        try
        {
            Sheet sheet =
                drawingDocument.Sheets.Add(
                    sheetSize,
                    orientation,
                    sheetName,
                    width,
                    height);

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Лист \"{sheet.Name}\" создан.",

                        name =
                            sheet.Name,

                        size =
                            sheet.Size.ToString(),

                        orientation =
                            orientation.ToString(),

                        width =
                            sheet.Width,

                        height =
                            sheet.Height,

                        requestedWidth,

                        requestedHeight,

                        isActive =
                            string.Equals(
                                drawingDocument.ActiveSheet.Name,
                                sheet.Name,
                                StringComparison.OrdinalIgnoreCase),

                        sheetCount =
                            drawingDocument.Sheets.Count
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    "Не удалось создать лист.",
                    exception.Message);
        }
    }

    private static bool TryParseSheetSize(
        string value,
        out DrawingSheetSizeEnum sheetSize)
    {
        switch (value)
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

    private static bool TryParseOrientation(
        string value,
        out PageOrientationTypeEnum orientation)
    {
        switch (value)
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
