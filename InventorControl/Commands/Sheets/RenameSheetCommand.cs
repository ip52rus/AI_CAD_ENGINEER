using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class RenameSheetCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public RenameSheetCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "rename_sheet";

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
                    "newName",
                    out string newName,
                    out string newNameError))
        {
            return SheetCommandSupport
                .CreateError(
                    newNameError);
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

        Sheet? duplicateSheet =
            SheetCommandSupport
                .FindSheet(
                    drawingDocument,
                    newName);

        if (duplicateSheet != null &&
            !string.Equals(
                duplicateSheet.Name,
                sheet.Name,
                StringComparison.OrdinalIgnoreCase))
        {
            return SheetCommandSupport
                .CreateError(
                    $"Лист с именем \"{newName}\" уже существует.");
        }

        string previousName =
            sheet.Name;

        try
        {
            sheet.Name =
                newName;

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Лист \"{previousName}\" переименован.",

                        previousName,

                        newName =
                            sheet.Name,

                        isActive =
                            string.Equals(
                                drawingDocument.ActiveSheet.Name,
                                sheet.Name,
                                StringComparison.OrdinalIgnoreCase)
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось переименовать лист " +
                    $"\"{previousName}\".",
                    exception.Message);
        }
    }
}
