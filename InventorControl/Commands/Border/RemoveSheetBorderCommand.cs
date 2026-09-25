using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class RemoveSheetBorderCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public RemoveSheetBorderCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "remove_sheet_border";

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

        if (sheet.Border == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"На листе \"{sheet.Name}\" нет рамки.");
        }

        string previousDefinitionName =
            sheet.Border.Definition.Name;

        try
        {
            sheet.Border.Delete();

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Рамка удалена с листа \"{sheet.Name}\".",

                        sheet =
                            sheet.Name,

                        previousDefinitionName,

                        hasBorder =
                            sheet.Border != null
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось удалить рамку с листа " +
                    $"\"{sheet.Name}\".",
                    exception.Message);
        }
    }
}
