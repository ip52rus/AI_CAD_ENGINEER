using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class FillTitleBlockCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public FillTitleBlockCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "fill_title_block";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            FillTitleBlockSupport
                .GetActiveDrawingDocument(
                    _inventor,
                    out string? documentError);

        if (drawingDocument == null)
        {
            return FillTitleBlockSupport
                .CreateError(
                    documentError ??
                    "Не удалось получить активный чертёж.");
        }

        if (!FillTitleBlockSupport
                .TryGetRequiredString(
                    root,
                    "sheetName",
                    out string sheetName,
                    out string sheetNameError))
        {
            return FillTitleBlockSupport
                .CreateError(
                    sheetNameError);
        }

        if (!root.TryGetProperty(
                "fields",
                out JsonElement fieldsElement))
        {
            return FillTitleBlockSupport
                .CreateError(
                    "Не найдено обязательное поле \"fields\".");
        }

        if (fieldsElement.ValueKind !=
            JsonValueKind.Object)
        {
            return FillTitleBlockSupport
                .CreateError(
                    "Поле \"fields\" должно быть JSON-объектом.");
        }

        Dictionary<string, string> requestedFields =
            new(
                StringComparer.OrdinalIgnoreCase);

        foreach (JsonProperty property
                 in fieldsElement.EnumerateObject())
        {
            if (property.Value.ValueKind !=
                JsonValueKind.String)
            {
                return FillTitleBlockSupport
                    .CreateError(
                        $"Значение поля \"{property.Name}\" " +
                        "должно быть строкой.");
            }

            requestedFields[property.Name] =
                property.Value.GetString()
                ?? string.Empty;
        }

        if (requestedFields.Count == 0)
        {
            return FillTitleBlockSupport
                .CreateError(
                    "Объект \"fields\" не должен быть пустым.");
        }

        Sheet? sheet =
            FillTitleBlockSupport
                .FindSheet(
                    drawingDocument,
                    sheetName);

        if (sheet == null)
        {
            return FillTitleBlockSupport
                .CreateError(
                    $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlock? titleBlock =
            sheet.TitleBlock;

        if (titleBlock == null)
        {
            return FillTitleBlockSupport
                .CreateError(
                    $"На листе \"{sheet.Name}\" " +
                    "нет основной надписи.");
        }

        Dictionary<string, int> fieldIndex =
            FillTitleBlockSupport
                .BuildFieldIndex(
                    titleBlock,
                    out List<string> duplicateFieldNames);

        if (duplicateFieldNames.Count > 0)
        {
            return FillTitleBlockSupport
                .CreateError(
                    "В определении основной надписи есть " +
                    "дублирующиеся имена полей.",
                    string.Join(
                        ", ",
                        duplicateFieldNames
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase)));
        }

        List<string> missingFields =
            requestedFields.Keys
                .Where(
                    fieldName =>
                        !fieldIndex.ContainsKey(
                            fieldName))
                .ToList();

        if (missingFields.Count > 0)
        {
            return FillTitleBlockSupport
                .CreateError(
                    "Не все запрошенные поля найдены.",
                    string.Join(
                        ", ",
                        missingFields));
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

            List<object> changedFields =
                new();

            foreach (KeyValuePair<string, string> pair
                     in requestedFields)
            {
                int index =
                    fieldIndex[pair.Key];

                TextBox textBox =
                    textBoxes[index];

                string previousText =
                    FillTitleBlockSupport
                        .SafeGetText(
                            textBox);

                textBox.Text =
                    pair.Value;

                changedFields.Add(
                    new
                    {
                        fieldName =
                            pair.Key,

                        index,

                        previousText,

                        value =
                            pair.Value
                    });
            }

            definition.ExitEdit(
                true,
                Type.Missing);

            editStarted =
                false;

            drawingDocument.Update();

            return FillTitleBlockSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Основная надпись заполнена.",

                        document =
                            drawingDocument.DisplayName,

                        sheet =
                            sheet.Name,

                        definition =
                            definition.Name,

                        changedFieldCount =
                            changedFields.Count,

                        changedFields,

                        warning =
                            "Изменено определение основной надписи. " +
                            "Изменения влияют на все его экземпляры " +
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

            return FillTitleBlockSupport
                .CreateError(
                    "Не удалось заполнить основную надпись.",
                    exception.Message);
        }
    }
}
