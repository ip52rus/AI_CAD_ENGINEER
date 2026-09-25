using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetTitleBlockDefinitionsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetTitleBlockDefinitionsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_title_block_definitions";

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

        List<object> definitions =
            new();

        int index =
            1;

        foreach (TitleBlockDefinition definition
                 in drawingDocument.TitleBlockDefinitions)
        {
            definitions.Add(
                new
                {
                    index,

                    name =
                        definition.Name,

                    isReferenced =
                        definition.IsReferenced
                });

            index++;
        }

        return SheetCommandSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    definitionCount =
                        definitions.Count,

                    definitions
                });
    }
}
