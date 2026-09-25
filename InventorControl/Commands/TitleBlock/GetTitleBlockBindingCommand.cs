using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetTitleBlockBindingCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetTitleBlockBindingCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_title_block_binding";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            TitleBlockBindingSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return TitleBlockBindingSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!TitleBlockBindingSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return TitleBlockBindingSupport
                .CreateError(
                    sheetNameError);
        }

        if (!root.TryGetProperty(
                "index",
                out JsonElement indexElement) ||
            !indexElement.TryGetInt32(
                out int index))
        {
            return TitleBlockBindingSupport
                .CreateError(
                    "Поле \"index\" должно содержать целое число.");
        }

        Sheet? sheet =
            TitleBlockBindingSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return TitleBlockBindingSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlock? titleBlock =
            sheet.TitleBlock;

        if (titleBlock == null)
        {
            return TitleBlockBindingSupport
                .CreateError(
                    $"На листе \"{sheet.Name}\" " +
                    "нет основной надписи.");
        }

        TextBoxes textBoxes =
            titleBlock
                .Definition
                .Sketch
                .TextBoxes;

        if (index < 1 ||
            index > textBoxes.Count)
        {
            return TitleBlockBindingSupport
                .CreateError(
                    $"Индекс должен быть от 1 до {textBoxes.Count}.");
        }

        object binding =
            TitleBlockBindingSupport
                .ParseBinding(
                    titleBlock,
                    textBoxes[index],
                    index);

        return TitleBlockBindingSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    titleBlock =
                        titleBlock.Definition.Name,

                    binding
                });
    }
}
