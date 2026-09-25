using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class ActivateSheetCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public ActivateSheetCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "activate_sheet";

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

        string previousActiveSheet =
            drawingDocument.ActiveSheet.Name;

        try
        {
            sheet.Activate();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Лист \"{sheet.Name}\" активирован.",

                        previousActiveSheet,

                        activeSheet =
                            drawingDocument.ActiveSheet.Name
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось активировать лист " +
                    $"\"{sheet.Name}\".",
                    exception.Message);
        }
    }
}
