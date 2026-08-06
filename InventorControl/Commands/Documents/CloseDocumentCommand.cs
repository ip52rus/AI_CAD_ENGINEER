using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CloseDocumentCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CloseDocumentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "close_document";

    public string Execute(
        JsonElement root)
    {
        if (!DocumentCommandSupport
                .TryGetOptionalBoolean(
                    root,
                    "skipSave",
                    false,
                    out bool skipSave,
                    out string skipSaveError))
        {
            return DocumentCommandSupport
                .CreateError(
                    skipSaveError);
        }

        Document? document;

        if (root.TryGetProperty(
                "document",
                out JsonElement documentElement) &&
            documentElement.ValueKind ==
            JsonValueKind.String &&
            !string.IsNullOrWhiteSpace(
                documentElement.GetString()))
        {
            string documentNameOrPath =
                documentElement
                    .GetString()!
                    .Trim();

            document =
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
        }
        else
        {
            document =
                _inventor.ActiveDocument;
        }

        if (document == null)
        {
            return DocumentCommandSupport
                .CreateError(
                    "В Inventor нет активного документа.");
        }

        string name =
            document.DisplayName;

        string fullFileName =
            document.FullFileName;

        bool wasDirty =
            document.Dirty;

        try
        {
            document.Close(
                skipSave);

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Документ \"{name}\" закрыт.",

                        name,

                        fullFileName,

                        wasDirty,

                        skipSave,

                        remainingDocumentCount =
                            _inventor.Documents.Count
                    });
        }
        catch (Exception exception)
        {
            return DocumentCommandSupport
                .CreateError(
                    $"Не удалось закрыть документ " +
                    $"\"{name}\".",
                    exception.Message);
        }
    }
}
