using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetSheetBorderCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetSheetBorderCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_sheet_border";

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

        Border? border =
            sheet.Border;

        if (border == null)
        {
            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        hasBorder =
                            false,

                        definitionName =
                            (string?)null
                    });
        }

        return SheetCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    hasBorder =
                        true,

                    definitionName =
                        border.Definition.Name
                });
    }
}
