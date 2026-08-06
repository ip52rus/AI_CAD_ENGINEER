using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetTitleBlockBindingsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetTitleBlockBindingsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_title_block_bindings";

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

        List<object> bindings =
            new();

        int propertyBindingCount =
            0;

        int promptBindingCount =
            0;

        int staticTextCount =
            0;

        for (int index = 1;
             index <= textBoxes.Count;
             index++)
        {
            object binding =
                TitleBlockBindingSupport
                    .ParseBinding(
                        titleBlock,
                        textBoxes[index],
                        index);

            string bindingJson =
                JsonSerializer.Serialize(
                    binding);

            if (bindingJson.Contains(
                    "\"bindingType\":\"property\"",
                    StringComparison.Ordinal))
            {
                propertyBindingCount++;
            }
            else if (bindingJson.Contains(
                         "\"bindingType\":\"prompt\"",
                         StringComparison.Ordinal))
            {
                promptBindingCount++;
            }
            else
            {
                staticTextCount++;
            }

            bindings.Add(
                binding);
        }

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

                    textBoxCount =
                        textBoxes.Count,

                    propertyBindingCount,

                    promptBindingCount,

                    staticTextCount,

                    bindings
                });
    }
}
