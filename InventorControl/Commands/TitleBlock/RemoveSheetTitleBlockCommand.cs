using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class RemoveSheetTitleBlockCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public RemoveSheetTitleBlockCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "remove_sheet_title_block";

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

        if (sheet.TitleBlock == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"На листе \"{sheet.Name}\" нет основной надписи.");
        }

        string previousDefinitionName =
            sheet.TitleBlock.Definition.Name;

        try
        {
            sheet.TitleBlock.Delete();

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Основная надпись удалена с листа " +
                            $"\"{sheet.Name}\".",

                        sheet =
                            sheet.Name,

                        previousDefinitionName,

                        hasTitleBlock =
                            sheet.TitleBlock != null
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось удалить основную надпись " +
                    $"с листа \"{sheet.Name}\".",
                    exception.Message);
        }
    }
}
