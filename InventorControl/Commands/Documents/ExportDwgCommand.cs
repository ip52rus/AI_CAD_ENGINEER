using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class ExportDwgCommand : IInventorCommand
{
    private const string DwgTranslatorClientId =
        "{C24E3AC2-122E-11D5-8E91-0010B541CD80}";

    private readonly Inventor.Application _inventor;

    public ExportDwgCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "export_dwg";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        if (!TryGetActiveDrawingDocument(out DrawingDocument? drawingDocument, diagnostics))
        {
            return CreateError("Active document is not a DrawingDocument.", diagnostics);
        }

        DrawingDocument document =
            drawingDocument!;

        diagnostics.Add(new
        {
            scope =
                "DrawingDocument.SaveState",

            fullFileName =
                document.FullFileName,

            fullFileNameIsEmpty =
                string.IsNullOrWhiteSpace(
                    document.FullFileName)
        });

        if (!TryGetOutputPath(root, diagnostics, out string outputPath))
        {
            return CreateError("Invalid DWG export outputPath.", diagnostics);
        }

        if (!TryGetIniPath(root, diagnostics, out string iniPath))
        {
            return CreateError("Invalid DWG export iniPath.", diagnostics);
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
                    message = "Destination DWG already exists and overwrite is false.",
                    outputPath
                });

                return CreateError("Destination DWG already exists.", diagnostics);
            }
        }

        if (!TryGetDwgTranslator(out TranslatorAddIn? translator, diagnostics))
        {
            return CreateError("DWG translator add-in is not available.", diagnostics);
        }

        TranslatorAddIn dwgTranslator =
            translator!;

        try
        {
            TranslationContext context =
                _inventor.TransientObjects.CreateTranslationContext();

            context.Type =
                IOMechanismEnum.kFileBrowseIOMechanism;

            NameValueMap options =
                _inventor.TransientObjects.CreateNameValueMap();

            bool hasSaveCopyAsOptions =
                dwgTranslator.HasSaveCopyAsOptions[
                    document,
                    context,
                    options];

            diagnostics.Add(new
            {
                scope = "TranslatorAddIn.HasSaveCopyAsOptions",
                hasSaveCopyAsOptions
            });

            List<object> optionEntries =
                new();

            bool exportAcadIniFileExistsBefore =
                false;

            for (int optionIndex = 1;
                 optionIndex <= options.Count;
                 optionIndex++)
            {
                string optionName =
                    options.Name[optionIndex];

                if (string.Equals(
                        optionName,
                        "Export_Acad_IniFile",
                        StringComparison.OrdinalIgnoreCase))
                {
                    exportAcadIniFileExistsBefore =
                        true;
                }

                try
                {
                    object? optionValue =
                        options.Value[optionName];

                    optionEntries.Add(new
                    {
                        index =
                            optionIndex,

                        name =
                            optionName,

                        value =
                            optionValue?.ToString()
                    });
                }
                catch (Exception exception)
                {
                    optionEntries.Add(new
                    {
                        index =
                            optionIndex,

                        name =
                            optionName,

                        valueReadError =
                            exception.Message,

                        exceptionType =
                            exception.GetType().FullName
                    });
                }
            }

            diagnostics.Add(new
            {
                scope =
                    "NameValueMap.AfterHasSaveCopyAsOptions",

                count =
                    options.Count,

                exportAcadIniFileExistsBefore,

                entries =
                    optionEntries
            });

            options.Value["Export_Acad_IniFile"] =
                iniPath;

            diagnostics.Add(new
            {
                scope =
                    "NameValueMap.Export_Acad_IniFile",

                mechanism =
                    "NameValueMap.Value setter",

                existedBefore =
                    exportAcadIniFileExistsBefore,

                value =
                    iniPath
            });

            DataMedium dataMedium =
                _inventor.TransientObjects.CreateDataMedium();

            dataMedium.FileName =
                outputPath;

            dwgTranslator.SaveCopyAs(
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

            return CreateError("DWG export failed.", diagnostics);
        }

        FileInfo outputFile = new(outputPath);
        outputFile.Refresh();

        if (!outputFile.Exists)
        {
            diagnostics.Add(new
            {
                scope = "output.file",
                message = "Expected DWG file does not exist after SaveCopyAs.",
                outputPath
            });

            return CreateError("DWG export did not create the expected file.", diagnostics);
        }

        return CreateSuccess(new
        {
            capability = "export_dwg",
            document = document.DisplayName,
            documentFullFileName = document.FullFileName,
            outputPath,
            iniPath,
            overwrite,
            fileExistedBefore,
            fileSizeBefore,
            lastWriteUtcBefore,
            fileExists = true,
            fileSizeBytes = outputFile.Length,
            lastWriteUtc = outputFile.LastWriteTimeUtc,
            dwgTranslatorClientId = DwgTranslatorClientId,
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
                ".dwg",
                StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new
            {
                scope = "input.outputPath",
                message = "outputPath extension must be .dwg.",
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

    private static bool TryGetIniPath(
        JsonElement root,
        List<object> diagnostics,
        out string iniPath)
    {
        iniPath = string.Empty;

        if (!DocumentCommandSupport.TryGetRequiredString(
                root,
                "iniPath",
                out string rawIniPath,
                out string iniPathError))
        {
            diagnostics.Add(new { scope = "input.iniPath", message = iniPathError });
            return false;
        }

        try
        {
            iniPath =
                System.IO.Path.GetFullPath(rawIniPath);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new
            {
                scope = "input.iniPath",
                message = exception.Message,
                exceptionType = exception.GetType().FullName,
                iniPath = rawIniPath
            });

            return false;
        }

        if (!System.IO.Path.IsPathFullyQualified(rawIniPath))
        {
            diagnostics.Add(new
            {
                scope = "input.iniPath",
                message = "iniPath must be a fully qualified path.",
                iniPath = rawIniPath
            });

            return false;
        }

        if (!string.Equals(
                System.IO.Path.GetExtension(iniPath),
                ".ini",
                StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new
            {
                scope = "input.iniPath",
                message = "iniPath extension must be .ini.",
                iniPath
            });

            return false;
        }

        if (!System.IO.File.Exists(iniPath))
        {
            diagnostics.Add(new
            {
                scope = "input.iniPath",
                message = "DWG export INI file does not exist.",
                iniPath
            });

            return false;
        }

        return true;
    }

    private bool TryGetDwgTranslator(
        out TranslatorAddIn? translator,
        List<object> diagnostics)
    {
        translator = null;

        try
        {
            ApplicationAddIn addIn =
                _inventor.ApplicationAddIns.ItemById[DwgTranslatorClientId];

            diagnostics.Add(new
            {
                scope = "ApplicationAddIns.ItemById",
                message = "DWG Translator Add-In resolved.",
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
                    scope = "DWG Translator cast",
                    message = exception.Message,
                    exceptionType = exception.GetType().FullName
                });

                return false;
            }

            TranslatorAddIn dwgTranslator =
                translator;

            if (!dwgTranslator.SupportsSaveCopyAs)
            {
                diagnostics.Add(new
                {
                    scope = "TranslatorAddIn.SupportsSaveCopyAs",
                    message = "DWG translator does not support SaveCopyAs."
                });

                return false;
            }

            if (!dwgTranslator.TranslatorAvailable)
            {
                diagnostics.Add(new
                {
                    scope = "TranslatorAddIn.TranslatorAvailable",
                    message = "DWG translator is not available."
                });

                return false;
            }

            diagnostics.Add(new
            {
                scope = "TranslatorAddIn",
                message = "DWG translator is usable.",
                supportsSaveCopyAs = dwgTranslator.SupportsSaveCopyAs,
                translatorAvailable = dwgTranslator.TranslatorAvailable,
                fileExtensions = SafeRead(() => dwgTranslator.FileExtensions),
                supportsSaveCopyAsFrom = SafeRead(() => dwgTranslator.SupportsSaveCopyAsFrom)
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
                clientId = DwgTranslatorClientId
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
