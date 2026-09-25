using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetTitleBlockFieldMapCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public GetTitleBlockFieldMapCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "get_title_block_field_map";

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

        List<TitleBlockFieldMapperSupport.FieldMatch> fields =
            TitleBlockFieldMapperSupport
                .GetAllFields(
                    titleBlock);

        return TitleBlockFieldMapperSupport
            .CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    definition =
                        titleBlock.Definition.Name,

                    fieldCount =
                        fields.Count,

                    fields =
                        fields.Select(
                            field =>
                                new
                                {
                                    index =
                                        field.Index,

                                    fieldName =
                                        field.FieldName,

                                    propertySet =
                                        field.PropertySet,

                                    sourceDocument =
                                        field.SourceDocument,

                                    formatId =
                                        field.FormatId,

                                    propertyId =
                                        field.PropertyId,

                                    text =
                                        field.Text,

                                    displayedText =
                                        field.DisplayedText,

                                    origin =
                                        new
                                        {
                                            x =
                                                field.X,

                                            y =
                                                field.Y
                                        }
                                })
                });
    }
}
