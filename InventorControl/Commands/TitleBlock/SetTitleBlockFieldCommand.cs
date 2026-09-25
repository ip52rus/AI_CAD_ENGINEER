using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetTitleBlockFieldCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetTitleBlockFieldCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_title_block_field";

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

        if (!TitleBlockFieldSupport
                .TryGetRequiredString(
                    root,
                    "value",
                    out string newValue,
                    out string valueError))
        {
            return TitleBlockFieldSupport
                .CreateError(
                    valueError);
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

        TextBox? textBox =
            TitleBlockFieldSupport
                .FindTextBox(
                    titleBlock,
                    root,
                    out string? fieldError);

        if (textBox == null)
        {
            return TitleBlockFieldSupport
                .CreateError(
                    fieldError ??
                    "Текстовое поле не найдено.");
        }

        TextBoxes textBoxes =
            titleBlock
                .Definition
                .Sketch
                .TextBoxes;

        int resolvedIndex =
            0;

        for (int index = 1;
             index <= textBoxes.Count;
             index++)
        {
            if (ReferenceEquals(
                    textBoxes[index],
                    textBox))
            {
                resolvedIndex =
                    index;

                break;
            }
        }

        string previousValue =
            TitleBlockFieldSupport
                .SafeGetResultText(
                    titleBlock,
                    textBox);

        string sourceText =
            TitleBlockFieldSupport
                .SafeGetText(
                    textBox);

        string sourceFormattedText =
            TitleBlockFieldSupport
                .SafeGetFormattedText(
                    textBox);

        try
        {
            titleBlock.SetPromptResultText(
                textBox,
                newValue);

            drawingDocument.Update();

            string actualValue =
                TitleBlockFieldSupport
                    .SafeGetResultText(
                        titleBlock,
                        textBox);

            return TitleBlockFieldSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Значение поля основной надписи изменено.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        titleBlock =
                            titleBlock.Definition.Name,

                        index =
                            resolvedIndex,

                        text =
                            sourceText,

                        formattedText =
                            sourceFormattedText,

                        previousValue,

                        requestedValue =
                            newValue,

                        actualValue,

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            return TitleBlockFieldSupport
                .CreateError(
                    "Не удалось изменить поле основной надписи.",
                    "Вероятно, выбранное поле не является " +
                    "Prompted Entry. " +
                    exception.Message);
        }
    }
}
