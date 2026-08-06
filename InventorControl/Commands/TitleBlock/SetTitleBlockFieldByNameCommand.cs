using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SetTitleBlockFieldByNameCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetTitleBlockFieldByNameCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_title_block_field_by_name";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            TitleBlockFieldMapperSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!TitleBlockFieldMapperSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    sheetNameError);
        }

        if (!TitleBlockFieldMapperSupport
                .TryGetRequiredString(
                    root,
                    "fieldName",
                    out string fieldName,
                    out string fieldNameError))
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    fieldNameError);
        }

        if (!TitleBlockFieldMapperSupport
                .TryGetRequiredString(
                    root,
                    "value",
                    out string value,
                    out string valueError,
                    allowEmpty: true))
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    valueError);
        }

        bool contains =
            false;

        if (root.TryGetProperty(
                "contains",
                out JsonElement containsElement))
        {
            if (containsElement.ValueKind !=
                JsonValueKind.True &&
                containsElement.ValueKind !=
                JsonValueKind.False)
            {
                return TitleBlockFieldMapperSupport
                    .CreateError(
                        "Поле \"contains\" должно содержать " +
                        "true или false.");
            }

            contains =
                containsElement.GetBoolean();
        }

        Sheet? sheet =
            TitleBlockFieldMapperSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlock? titleBlock =
            sheet.TitleBlock;

        if (titleBlock == null)
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    $"На листе \"{sheet.Name}\" " +
                    "нет основной надписи.");
        }

        List<TitleBlockFieldMapperSupport.FieldMatch> matches =
            TitleBlockFieldMapperSupport
                .GetFieldMatches(
                    titleBlock,
                    fieldName,
                    contains);

        if (matches.Count == 0)
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    $"Поле основной надписи " +
                    $"\"{fieldName}\" не найдено.");
        }

        if (matches.Count > 1)
        {
            return TitleBlockFieldMapperSupport
                .CreateError(
                    $"Найдено несколько полей для " +
                    $"\"{fieldName}\".",
                    string.Join(
                        ", ",
                        matches.Select(
                            match =>
                                $"{match.Index}: " +
                                $"{match.FieldName}")));
        }

        TitleBlockFieldMapperSupport.FieldMatch match =
            matches[0];

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

            if (match.Index < 1 ||
                match.Index > textBoxes.Count)
            {
                definition.ExitEdit(
                    false,
                    Type.Missing);

                editStarted =
                    false;

                return TitleBlockFieldMapperSupport
                    .CreateError(
                        "Индекс найденного поля вышел " +
                        "за пределы коллекции TextBoxes.");
            }

            TextBox textBox =
                textBoxes[match.Index];

            string previousText =
                TitleBlockFieldMapperSupport
                    .SafeGetText(
                        textBox);

            textBox.Text =
                value;

            definition.ExitEdit(
                true,
                Type.Missing);

            editStarted =
                false;

            drawingDocument.Update();

            return TitleBlockFieldMapperSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Поле основной надписи изменено по имени.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        definition =
                            definition.Name,

                        fieldName =
                            match.FieldName,

                        index =
                            match.Index,

                        previousText,

                        value,

                        warning =
                            "Изменено определение основной надписи. " +
                            "Изменение влияет на все его экземпляры " +
                            "в текущем документе.",

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

            return TitleBlockFieldMapperSupport
                .CreateError(
                    "Не удалось изменить поле основной " +
                    "надписи по имени.",
                    exception.Message);
        }
    }
}
