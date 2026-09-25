using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class CreateDrawingDocumentCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public CreateDrawingDocumentCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_drawing_document";

    public string Execute(
        JsonElement root)
    {
        List<object> diagnostics =
            new();

        if (!DocumentCommandSupport
                .TryGetRequiredString(
                    root,
                    "templatePath",
                    out string templatePath,
                    out string templatePathError))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.templatePath",

                    message =
                        templatePathError
                });

            return CreateError(
                "Не указан путь к шаблону чертежа.",
                diagnostics);
        }

        if (!DocumentCommandSupport
                .TryGetOptionalBoolean(
                    root,
                    "visible",
                    true,
                    out bool visible,
                    out string visibleError))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.visible",

                    message =
                        visibleError
                });

            return CreateError(
                visibleError,
                diagnostics);
        }

        if (!System.IO.File.Exists(
                templatePath))
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "input.templatePath",

                    message =
                        "Template file does not exist.",

                    templatePath
                });

            return CreateError(
                "Файл шаблона чертежа не найден.",
                diagnostics);
        }

        try
        {
            Inventor._Document document =
                _inventor.Documents.Add(
                    DocumentTypeEnum.kDrawingDocumentObject,
                    templatePath,
                    visible);

            DrawingDocument drawingDocument =
                (DrawingDocument)document;

            int sheetCount =
                drawingDocument.Sheets.Count;

            return DocumentCommandSupport
                .CreateSuccess(
                    new
                    {
                        capability =
                            "create_drawing_document",

                        document =
                            drawingDocument.DisplayName,

                        fullFileName =
                            drawingDocument.FullFileName,

                        documentType =
                            "Drawing",

                        documentTypeRaw =
                            drawingDocument.DocumentType
                                .ToString(),

                        templatePath,

                        visible,

                        sheetCount,

                        dirty =
                            drawingDocument.Dirty
                    });
        }
        catch (Exception exception)
        {
            diagnostics.Add(
                new
                {
                    scope =
                        "Application.Documents.Add",

                    message =
                        exception.Message,

                    exceptionType =
                        exception.GetType()
                            .FullName
                });

            return CreateError(
                "Не удалось создать DrawingDocument.",
                diagnostics);
        }
    }

    private static string CreateError(
        string message,
        List<object> diagnostics)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    false,

                error =
                    message,

                diagnostics
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions
        CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,

            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}
