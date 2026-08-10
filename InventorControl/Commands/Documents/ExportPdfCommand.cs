using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class ExportPdfCommand : IInventorCommand
{
    private const string PdfTranslatorClientId =
        "{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}";

    private readonly Inventor.Application _inventor;

    public ExportPdfCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "export_pdf";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        if (!TryGetActiveDrawingDocument(out DrawingDocument? drawingDocument, diagnostics))
        {
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        DrawingDocument document =
            drawingDocument!;

        if (!TryGetOutputPath(root, diagnostics, out string outputPath))
        {
            return CreateError("Invalid PDF export outputPath.", diagnostics);
        }

        if (!DocumentCommandSupport.TryGetOptionalBoolean(
                root,
                "overwrite",
                false,
                out bool overwrite,
                out string overwriteError))
        {
            diagnostics.Add(new { scope = "input.overwrite", message = overwriteError });
            return CreateError("Invalid overwrite value.", diagnostics);
        }

        bool fileExistedBefore = System.IO.File.Exists(outputPath);
        long? fileSizeBefore = null;
        DateTime? lastWriteUtcBefore = null;

        if (fileExistedBefore)
        {
            FileInfo existingFile = new(outputPath);
            fileSizeBefore = existingFile.Length;
            lastWriteUtcBefore = existingFile.LastWriteTimeUtc;

            if (!overwrite)
            {
                diagnostics.Add(new
                {
                    scope = "input.outputPath",
                    message = "Destination PDF already exists and overwrite is false.",
                    outputPath
                });

                return CreateError("Destination PDF already exists.", diagnostics);
            }
        }

        if (!TryGetPdfTranslator(out TranslatorAddIn? translator, diagnostics))
        {
            return CreateError("PDF translator add-in is not available.", diagnostics);
        }

        TranslatorAddIn pdfTranslator =
            translator!;

        try
        {
            TranslationContext context =
                _inventor.TransientObjects.CreateTranslationContext();

            context.Type =
                IOMechanismEnum.kFileBrowseIOMechanism;

            NameValueMap options =
                _inventor.TransientObjects.CreateNameValueMap();

            DataMedium dataMedium =
                _inventor.TransientObjects.CreateDataMedium();

            dataMedium.FileName =
                outputPath;

            pdfTranslator.SaveCopyAs(
                document,
                context,
                options,
                dataMedium);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "TranslatorAddIn.SaveCopyAs",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return CreateError("PDF export failed.", diagnostics);
        }

        FileInfo outputFile = new(outputPath);
        outputFile.Refresh();

        if (!outputFile.Exists)
        {
            diagnostics.Add(new
            {
                scope = "output.file",
                message = "Expected PDF file does not exist after SaveCopyAs.",
                outputPath
            });

            return CreateError("PDF export did not create the expected file.", diagnostics);
        }

        return CreateSuccess(new
        {
            capability = "export_pdf",
            document = document.DisplayName,
            documentFullFileName = document.FullFileName,
            outputPath,
            overwrite,
            fileExistedBefore,
            fileSizeBefore,
            lastWriteUtcBefore,
            fileExists = true,
            fileSizeBytes = outputFile.Length,
            lastWriteUtc = outputFile.LastWriteTimeUtc,
            pdfTranslatorClientId = PdfTranslatorClientId,
            dirty = document.Dirty,
            diagnostics
        });
    }

    private bool TryGetActiveDrawingDocument(
        out DrawingDocument? drawingDocument,
        List<object> diagnostics)
    {
        drawingDocument = null;

        try
        {
            Document? activeDocument =
                _inventor.ActiveDocument;

            if (activeDocument == null)
            {
                diagnostics.Add(new
                {
                    scope = "Application.ActiveDocument",
                    message = "Inventor has no active document."
                });

                return false;
            }

            if (activeDocument.DocumentType !=
                DocumentTypeEnum.kDrawingDocumentObject)
            {
                diagnostics.Add(new
                {
                    scope = "Application.ActiveDocument",
                    message = "Active document is not a drawing.",
                    document = activeDocument.DisplayName,
                    documentTypeRaw = activeDocument.DocumentType.ToString()
                });

                return false;
            }

            drawingDocument =
                (DrawingDocument)activeDocument;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Application.ActiveDocument",
                message = exception.Message,
                exceptionType = exception.GetType().FullName
            });

            return false;
        }
    }

    private static bool TryGetOutputPath(
        JsonElement root,
        List<object> diagnostics,
        out string outputPath)
    {
        outputPath = string.Empty;

        if (!DocumentCommandSupport.TryGetRequiredString(
                root,
                "outputPath",
                out string rawOutputPath,
                out string outputPathError))
        {
            diagnostics.Add(new { scope = "input.outputPath", message = outputPathError });
            return false;
        }

        try
        {
            outputPath =
                System.IO.Path.GetFullPath(rawOutputPath);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                outputPath = rawOutputPath
            });

            return false;
        }

        if (!System.IO.Path.IsPathFullyQualified(rawOutputPath))
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = "outputPath must be a fully qualified path.",
                outputPath = rawOutputPath
            });

            return false;
        }

        if (!string.Equals(
                System.IO.Path.GetExtension(outputPath),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = "outputPath extension must be .pdf.",
                outputPath
            });

            return false;
        }

        string? directory =
            System.IO.Path.GetDirectoryName(outputPath);

        if (string.IsNullOrWhiteSpace(directory) ||
            !Directory.Exists(directory))
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = "Output directory does not exist.",
                directory
            });

            return false;
        }

        return true;
    }

    private bool TryGetPdfTranslator(
        out TranslatorAddIn? translator,
        List<object> diagnostics)
    {
        translator = null;

        try
        {
            ApplicationAddIn addIn =
                _inventor.ApplicationAddIns.ItemById[PdfTranslatorClientId];

            diagnostics.Add(new
            {
                scope = "ApplicationAddIns.ItemById",
                message = "PDF Translator Add-In resolved.",
                clientId = SafeRead(() => addIn.ClientId),
                classId = SafeRead(() => addIn.ClassIdString),
                displayName = SafeRead(() => addIn.DisplayName)
            });

            try
            {
                TranslatorAddIn resolvedTranslator =
                    (TranslatorAddIn)(object)addIn;

                translator =
                    resolvedTranslator;
            }
            catch (Exception exception)
            {
                diagnostics.Add(new
                {
                    scope = "PDF Translator cast",
                    message = exception.Message,
                    exceptionType = exception.GetType().FullName
                });

                return false;
            }

            TranslatorAddIn pdfTranslator =
                translator;

            if (!pdfTranslator.SupportsSaveCopyAs)
            {
                diagnostics.Add(new
                {
                    scope = "TranslatorAddIn.SupportsSaveCopyAs",
                    message = "PDF translator does not support SaveCopyAs."
                });

                return false;
            }

            if (!pdfTranslator.TranslatorAvailable)
            {
                diagnostics.Add(new
                {
                    scope = "TranslatorAddIn.TranslatorAvailable",
                    message = "PDF translator is not available."
                });

                return false;
            }

            diagnostics.Add(new
            {
                scope = "TranslatorAddIn",
                message = "PDF translator is usable.",
                supportsSaveCopyAs = pdfTranslator.SupportsSaveCopyAs,
                translatorAvailable = pdfTranslator.TranslatorAvailable,
                fileExtensions = SafeRead(() => pdfTranslator.FileExtensions),
                supportsSaveCopyAsFrom = SafeRead(() => pdfTranslator.SupportsSaveCopyAsFrom)
            });

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "Application.ApplicationAddIns",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                clientId = PdfTranslatorClientId
            });

            return false;
        }
    }

    private static string? SafeRead(Func<string> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return null;
        }
    }

    private static string CreateSuccess(object data) =>
        JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
            CreateJsonOptions());

    private static string CreateError(string message, List<object> diagnostics) =>
        JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                diagnostics
            },
            CreateJsonOptions());

    private static JsonSerializerOptions CreateJsonOptions() =>
        new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
}
