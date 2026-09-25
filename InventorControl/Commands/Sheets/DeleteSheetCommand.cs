using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class DeleteSheetCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public DeleteSheetCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_sheet";

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
                .TryGetOptionalBoolean(
                    root,
                    "retainDependentViews",
                    false,
                    out bool retainDependentViews,
                    out string retainDependentViewsError))
        {
            return SheetCommandSupport
                .CreateError(
                    retainDependentViewsError);
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

        if (drawingDocument.Sheets.Count <= 1)
        {
            return SheetCommandSupport
                .CreateError(
                    "Нельзя удалить единственный лист чертежа.");
        }

        string deletedSheetName =
            sheet.Name;

        bool wasActive =
            string.Equals(
                drawingDocument.ActiveSheet.Name,
                sheet.Name,
                StringComparison.OrdinalIgnoreCase);

        try
        {
            sheet.Delete(
                retainDependentViews);

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Лист \"{deletedSheetName}\" удалён.",

                        deletedSheetName,

                        wasActive,

                        retainDependentViews,

                        activeSheet =
                            drawingDocument.ActiveSheet.Name,

                        remainingSheetCount =
                            drawingDocument.Sheets.Count
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось удалить лист " +
                    $"\"{deletedSheetName}\".",
                    exception.Message);
        }
    }
}
