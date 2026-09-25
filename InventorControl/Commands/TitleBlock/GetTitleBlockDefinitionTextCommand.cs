using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetTitleBlockDefinitionTextCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetTitleBlockDefinitionTextCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_title_block_definition_text";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            TitleBlockDefinitionTextSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!TitleBlockDefinitionTextSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    sheetNameError);
        }

        if (!TitleBlockDefinitionTextSupport
                .TryGetRequiredInt32(
                    root,
                    "index",
                    out int index,
                    out string indexError))
        {
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    indexError);
        }

        Sheet? sheet =
            TitleBlockDefinitionTextSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlock? titleBlock =
            sheet.TitleBlock;

        if (titleBlock == null)
        {
            return TitleBlockDefinitionTextSupport
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
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    $"Индекс должен быть от 1 до " +
                    $"{textBoxes.Count}.");
        }

        TextBox textBox =
            textBoxes[index];

        return TitleBlockDefinitionTextSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    definition =
                        titleBlock.Definition.Name,

                    index,

                    text =
                        TitleBlockDefinitionTextSupport
                            .GetTextSafely(
                                textBox),

                    formattedText =
                        TitleBlockDefinitionTextSupport
                            .GetFormattedTextSafely(
                                textBox),

                    origin =
                        new
                        {
                            x =
                                textBox.Origin.X,

                            y =
                                textBox.Origin.Y
                        },

                    warning =
                        "Изменение определения основной надписи " +
                        "затрагивает все её экземпляры " +
                        "в текущем документе."
                });
    }
}
