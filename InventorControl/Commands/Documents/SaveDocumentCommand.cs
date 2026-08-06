using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SaveDocumentCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SaveDocumentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "save_document";

    public string Execute(
        JsonElement root)
    {
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

        if (string.IsNullOrWhiteSpace(
                document.FullFileName))
        {
            return DocumentCommandSupport
                .CreateError(
                    "Документ ещё не имеет пути к файлу. " +
                    "Используй команду save_document_as.");
        }

        try
        {
            bool wasDirty =
                document.Dirty;

            document.Save();

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            $"Документ \"{document.DisplayName}\" сохранён.",

                        name =
                            document.DisplayName,

                        fullFileName =
                            document.FullFileName,

                        wasDirty,

                        dirty =
                            document.Dirty
                    });
        }
        catch (Exception exception)
        {
            return DocumentCommandSupport
                .CreateError(
                    $"Не удалось сохранить документ " +
                    $"\"{document.DisplayName}\".",
                    exception.Message);
        }
    }
}
