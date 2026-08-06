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

        try
        {
            string previousFileName =
                document.FullFileName;

            document.SaveAs(
                filePath,
                saveCopyAs);

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

                        saveCopyAs
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
