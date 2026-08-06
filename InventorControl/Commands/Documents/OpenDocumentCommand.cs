using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class OpenDocumentCommand : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public OpenDocumentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "open_document";

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
                    "visible",
                    true,
                    out bool visible,
                    out string visibleError))
        {
            return DocumentCommandSupport
                .CreateError(
                    visibleError);
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

        if (!System.IO.File.Exists(
                filePath))
        {
            return DocumentCommandSupport
                .CreateError(
                    "Указанный файл не найден.",
                    filePath);
        }

        Document? existingDocument =
            DocumentCommandSupport
                .FindDocument(
                    _inventor,
                    filePath);

        if (existingDocument != null)
        {
            existingDocument.Activate();

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Документ уже был открыт и активирован.",

                        name =
                            existingDocument.DisplayName,

                        fullFileName =
                            existingDocument.FullFileName,

                        documentType =
                            existingDocument.DocumentType
                                .ToString(),

                        alreadyOpen =
                            true,

                        silent
                    });
        }

        bool previousSilentOperation =
            _inventor.SilentOperation;

        try
        {
            _inventor.SilentOperation =
                silent;

            Document document =
                _inventor.Documents.Open(
                    filePath,
                    visible);

            if (visible)
            {
                document.Activate();
            }

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        message =
                            "Документ открыт.",

                        name =
                            document.DisplayName,

                        fullFileName =
                            document.FullFileName,

                        documentType =
                            document.DocumentType
                                .ToString(),

                        visible,

                        silent,

                        alreadyOpen =
                            false
                    });
        }
        catch (Exception exception)
        {
            return DocumentCommandSupport
                .CreateError(
                    "Не удалось открыть документ.",
                    exception.Message);
        }
        finally
        {
            _inventor.SilentOperation =
                previousSilentOperation;
        }
    }
}
