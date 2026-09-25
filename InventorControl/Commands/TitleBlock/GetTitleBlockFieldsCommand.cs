using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetTitleBlockFieldsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetTitleBlockFieldsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_title_block_fields";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            TitleBlockFieldSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return TitleBlockFieldSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!TitleBlockFieldSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return TitleBlockFieldSupport
                .CreateError(
                    sheetNameError);
        }

        Sheet? sheet =
            TitleBlockFieldSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return TitleBlockFieldSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlock? titleBlock =
            sheet.TitleBlock;

        if (titleBlock == null)
        {
            return TitleBlockFieldSupport
                .CreateError(
                    $"На листе \"{sheet.Name}\" " +
                    "нет основной надписи.");
        }

        TextBoxes textBoxes =
            titleBlock
                .Definition
                .Sketch
                .TextBoxes;

        List<object> fields =
            new();

        for (int index = 1;
             index <= textBoxes.Count;
             index++)
        {
            TextBox textBox =
                textBoxes[index];

            string text =
                TitleBlockFieldSupport
                    .SafeGetText(
                        textBox);

            string formattedText =
                TitleBlockFieldSupport
                    .SafeGetFormattedText(
                        textBox);

            string resultText =
                TitleBlockFieldSupport
                    .SafeGetResultText(
                        titleBlock,
                        textBox);

            bool looksPrompted =
                formattedText.Contains(
                    "<Prompt",
                    StringComparison.OrdinalIgnoreCase) ||
                formattedText.Contains(
                    "Prompted",
                    StringComparison.OrdinalIgnoreCase);

            fields.Add(
                new
                {
                    index,

                    text,

                    formattedText,

                    displayedText =
                        resultText,

                    looksPrompted,

                    origin =
                        new
                        {
                            x =
                                textBox.Origin.X,

                            y =
                                textBox.Origin.Y
                        },

                    width =
                        textBox.Width,

                    height =
                        textBox.Height
                });
        }

        return TitleBlockFieldSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    titleBlock =
                        titleBlock.Definition.Name,

                    fieldCount =
                        fields.Count,

                    fields
                });
    }
}
