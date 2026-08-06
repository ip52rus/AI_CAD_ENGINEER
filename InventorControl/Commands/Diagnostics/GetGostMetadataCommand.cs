using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetGostMetadataCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetGostMetadataCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_gost_metadata";

    public string Execute(JsonElement root)
    {
        DrawingDocument? drawingDocument =
            GostMetadataSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return GostMetadataSupport.CreateError(
                documentError ??
                "Не удалось получить активный чертёж.");
        }

        if (!GostMetadataSupport.TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetNameError))
        {
            return GostMetadataSupport.CreateError(
                sheetNameError);
        }

        Sheet? sheet =
            GostMetadataSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            return GostMetadataSupport.CreateError(
                $"Лист \"{sheetName}\" не найден.");
        }

        TitleBlock? titleBlock = sheet.TitleBlock;

        if (titleBlock == null)
        {
            return GostMetadataSupport.CreateError(
                $"На листе \"{sheet.Name}\" нет основной надписи.");
        }

        List<object> scopes = new()
        {
            GostMetadataSupport.ReadAttributeSets(
                drawingDocument,
                "drawing_document"),

            GostMetadataSupport.ReadAttributeSets(
                sheet,
                "sheet"),

            GostMetadataSupport.ReadAttributeSets(
                titleBlock,
                "title_block"),

            GostMetadataSupport.ReadAttributeSets(
                titleBlock.Definition,
                "title_block_definition"),

            GostMetadataSupport.ReadAttributeSets(
                titleBlock.Definition.Sketch,
                "title_block_sketch")
        };

        if (root.TryGetProperty(
                "textBoxIndex",
                out JsonElement textBoxIndexElement))
        {
            if (!textBoxIndexElement.TryGetInt32(
                    out int textBoxIndex))
            {
                return GostMetadataSupport.CreateError(
                    "Поле \"textBoxIndex\" должно содержать целое число.");
            }

            TextBoxes textBoxes =
                titleBlock.Definition.Sketch.TextBoxes;

            if (textBoxIndex < 1 ||
                textBoxIndex > textBoxes.Count)
            {
                return GostMetadataSupport.CreateError(
                    $"Индекс текстового поля должен быть " +
                    $"от 1 до {textBoxes.Count}.");
            }

            scopes.Add(
                GostMetadataSupport.ReadAttributeSets(
                    textBoxes[textBoxIndex],
                    $"text_box_{textBoxIndex}"));
        }

        List<object> referencedDocuments = new();

        foreach (DocumentDescriptor descriptor
                 in drawingDocument.ReferencedDocumentDescriptors)
        {
            string displayName = string.Empty;
            string fullFileName = string.Empty;
            string documentType = string.Empty;

            try
            {
                displayName =
                    descriptor.ReferencedDocument.DisplayName;

                fullFileName =
                    descriptor.ReferencedDocument.FullFileName;

                documentType =
                    descriptor.ReferencedDocument.DocumentType
                        .ToString();
            }
            catch
            {
            }

            referencedDocuments.Add(
                new
                {
                    displayName,
                    fullFileName,
                    documentType
                });
        }

        return GostMetadataSupport.CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    sheet.Name,

                titleBlock =
                    titleBlock.Definition.Name,

                referencedDocumentCount =
                    referencedDocuments.Count,

                referencedDocuments,

                scopeCount =
                    scopes.Count,

                scopes
            });
    }
}
