using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetSheetBorderCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetSheetBorderCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_sheet_border";

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
                    "definitionName",
                    out string definitionName,
                    out string definitionNameError))
        {
            return SheetCommandSupport
                .CreateError(
                    definitionNameError);
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

        BorderDefinition? definition =
            FindDefinition(
                drawingDocument,
                definitionName);

        if (definition == null)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Определение рамки \"{definitionName}\" не найдено.");
        }

        string? previousDefinitionName =
            null;

        try
        {
            if (sheet.Border != null)
            {
                previousDefinitionName =
                    sheet.Border.Definition.Name;

                sheet.Border.Delete();
            }

            Border border =
                sheet.AddBorder(
                    definition,
                    Type.Missing);

            drawingDocument.Update();

            return SheetCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Рамка \"{border.Definition.Name}\" установлена.",

                        sheet =
                            sheet.Name,

                        previousDefinitionName,

                        definitionName =
                            border.Definition.Name
                    });
        }
        catch (Exception exception)
        {
            return SheetCommandSupport
                .CreateError(
                    $"Не удалось установить рамку на лист " +
                    $"\"{sheet.Name}\".",
                    exception.Message);
        }
    }

    private static BorderDefinition? FindDefinition(
        DrawingDocument drawingDocument,
        string definitionName)
    {
        foreach (BorderDefinition definition
                 in drawingDocument.BorderDefinitions)
        {
            if (string.Equals(
                    definition.Name,
                    definitionName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return definition;
            }
        }

        return null;
    }
}
