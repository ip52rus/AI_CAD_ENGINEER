using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetTitleBlockDefinitionTextCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetTitleBlockDefinitionTextCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_title_block_definition_text";

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

        if (!TitleBlockDefinitionTextSupport
                .TryGetRequiredString(
                    root,
                    "value",
                    out string value,
                    out string valueError,
                    allowEmpty: true))
        {
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    valueError);
        }

        string mode =
            "text";

        if (root.TryGetProperty(
                "mode",
                out JsonElement modeElement))
        {
            if (modeElement.ValueKind !=
                JsonValueKind.String)
            {
                return TitleBlockDefinitionTextSupport
                    .CreateError(
                        "Поле \"mode\" должно быть строкой.");
            }

            mode =
                modeElement
                    .GetString()?
                    .Trim()
                    .ToLowerInvariant()
                ?? "text";
        }

        if (mode != "text" &&
            mode != "formatted_text")
        {
            return TitleBlockDefinitionTextSupport
                .CreateError(
                    "Неизвестный режим изменения.",
                    "Допустимые значения: text, formatted_text.");
        }

        string saveAsName =
            string.Empty;

        if (root.TryGetProperty(
                "saveAsName",
                out JsonElement saveAsNameElement))
        {
            if (saveAsNameElement.ValueKind !=
                JsonValueKind.String)
            {
                return TitleBlockDefinitionTextSupport
                    .CreateError(
                        "Поле \"saveAsName\" должно быть строкой.");
            }

            saveAsName =
                saveAsNameElement
                    .GetString()?
                    .Trim()
                ?? string.Empty;
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

        TitleBlockDefinition definition =
            titleBlock.Definition;

        DrawingSketch? editSketch =
            null;

        bool editStarted =
            false;

        try
        {
            definition.Edit(
                out editSketch);

            editStarted =
                true;

            TextBoxes textBoxes =
                editSketch.TextBoxes;

            if (index < 1 ||
                index > textBoxes.Count)
            {
                definition.ExitEdit(
                    false,
                    Type.Missing);

                editStarted =
                    false;

                return TitleBlockDefinitionTextSupport
                    .CreateError(
                        $"Индекс должен быть от 1 до " +
                        $"{textBoxes.Count}.");
            }

            TextBox textBox =
                textBoxes[index];

            string previousText =
                TitleBlockDefinitionTextSupport
                    .GetTextSafely(
                        textBox);

            string previousFormattedText =
                TitleBlockDefinitionTextSupport
                    .GetFormattedTextSafely(
                        textBox);

            if (mode == "formatted_text")
            {
                textBox.FormattedText =
                    value;
            }
            else
            {
                textBox.Text =
                    value;
            }

            object saveAsArgument =
                string.IsNullOrWhiteSpace(
                    saveAsName)
                    ? Type.Missing
                    : saveAsName;

            definition.ExitEdit(
                true,
                saveAsArgument);

            editStarted =
                false;

            drawingDocument.Update();

            string resultingDefinitionName =
                string.IsNullOrWhiteSpace(
                    saveAsName)
                    ? definition.Name
                    : saveAsName;

            return TitleBlockDefinitionTextSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Текст определения основной надписи изменён.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        originalDefinition =
                            definition.Name,

                        resultingDefinition =
                            resultingDefinitionName,

                        index,

                        mode,

                        previousText,

                        previousFormattedText,

                        requestedValue =
                            value,

                        warning =
                            string.IsNullOrWhiteSpace(
                                saveAsName)
                                ? "Исходное определение заменено. " +
                                  "Изменение влияет на все его " +
                                  "экземпляры в документе."
                                : "Создано новое определение. " +
                                  "Оно ещё не установлено на лист.",

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            if (editStarted)
            {
                try
                {
                    definition.ExitEdit(
                        false,
                        Type.Missing);
                }
                catch
                {
                }
            }

            return TitleBlockDefinitionTextSupport
                .CreateError(
                    "Не удалось изменить текст определения " +
                    "основной надписи.",
                    exception.Message);
        }
    }
}
