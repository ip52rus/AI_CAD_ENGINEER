using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class ActivateDocumentCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public ActivateDocumentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "activate_document";

    public string Execute(
        JsonElement root)
    {
        if (!DocumentCommandSupport
                .TryGetRequiredString(
                    root,
                    "document",
                    out string documentNameOrPath,
                    out string documentError))
        {
            return DocumentCommandSupport
                .CreateError(
                    documentError);
        }

        Document? document =
            DocumentCommandSupport
                .FindDocument(
                    _inventor,
                    documentNameOrPath);

        if (document == null)
        {
            return DocumentCommandSupport
                .CreateError(
                    $"Открытый документ " +
                    $"\"{documentNameOrPath}\" не найден.");
        }

        try
        {
            document.Activate();

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Документ \"{document.DisplayName}\" активирован.",

                        name =
                            document.DisplayName,

                        fullFileName =
                            document.FullFileName,

                        documentType =
                            document.DocumentType
                                .ToString()
                    });
        }
        catch (Exception exception)
        {
            return DocumentCommandSupport
                .CreateError(
                    $"Не удалось активировать документ " +
                    $"\"{document.DisplayName}\".",
                    exception.Message);
        }
    }
}
