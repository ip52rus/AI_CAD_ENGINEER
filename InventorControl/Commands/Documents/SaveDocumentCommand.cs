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

        if (!DocumentCommandSupport
                .TryGetOptionalBoolean(
                    root,
                    "silent",
                    true,
                    out bool silent,
                    out string silentError))
        {
            return DocumentCommandSupport
                .CreateError(
                    silentError);
        }

        List<object> openDocumentsBefore =
            DocumentCommandSupport
                .SnapshotOpenDocuments(
                    _inventor);

        bool wasDirty =
            document.Dirty;

        bool silentOperationBefore;
        bool silentOperationDuring;
        bool silentOperationAfter;

        try
        {
            using (DocumentCommandSupport.SilentOperationScope silentScope =
                   DocumentCommandSupport
                       .BeginSilentOperation(
                           _inventor,
                           silent))
            {
                silentOperationBefore =
                    silentScope.PreviousSilentOperation;

                silentOperationDuring =
                    silentScope.AppliedSilentOperation;

                document.Save();
            }

            silentOperationAfter =
                _inventor.SilentOperation;

            List<object> openDocumentsAfter =
                DocumentCommandSupport
                    .SnapshotOpenDocuments(
                        _inventor);

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

                        silent,

                        silentOperationBefore,

                        silentOperationDuring,

                        silentOperationAfter,

                        silentOperationRestored =
                            silentOperationAfter ==
                            silentOperationBefore,

                        wasDirty,

                        dirty =
                            document.Dirty,

                        openDocumentsBefore,

                        openDocumentsAfter
                    });
        }
        catch (Exception exception)
        {
            silentOperationAfter =
                _inventor.SilentOperation;

            return DocumentCommandSupport
                .CreateError(
                    $"Не удалось сохранить документ " +
                    $"\"{document.DisplayName}\".",
                    $"{exception.Message} SilentOperationAfter={silentOperationAfter}.");
        }
    }
}
