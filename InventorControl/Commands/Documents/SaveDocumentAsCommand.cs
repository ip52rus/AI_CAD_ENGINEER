using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class SaveDocumentAsCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SaveDocumentAsCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "save_document_as";

    public string Execute(
        JsonElement root)
    {
        if (!DocumentCommandSupport
                .TryGetRequiredString(
                    root,
                    "filePath",
                    out string filePath,
                    out string filePathError))
        {
            return DocumentCommandSupport
                .CreateError(
                    filePathError);
        }

        if (!DocumentCommandSupport
                .TryGetOptionalBoolean(
                    root,
                    "saveCopyAs",
                    false,
                    out bool saveCopyAs,
                    out string saveCopyAsError))
        {
            return DocumentCommandSupport
                .CreateError(
                    saveCopyAsError);
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

        string? directory =
            System.IO.Path.GetDirectoryName(
                filePath);

        if (!string.IsNullOrWhiteSpace(
                directory) &&
            !Directory.Exists(
                directory))
        {
            return DocumentCommandSupport
                .CreateError(
                    "Папка назначения не существует.",
                    directory);
        }

        List<object> openDocumentsBefore =
            DocumentCommandSupport
                .SnapshotOpenDocuments(
                    _inventor);

        string previousFileName =
            document.FullFileName;

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

                document.SaveAs(
                    filePath,
                    saveCopyAs);
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
                            saveCopyAs
                                ? "Копия документа сохранена."
                                : "Документ сохранён под новым именем.",

                        name =
                            document.DisplayName,

                        previousFileName,

                        requestedFileName =
                            filePath,

                        currentFileName =
                            document.FullFileName,

                        saveCopyAs,

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
