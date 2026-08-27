using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DrawingViewCommandSupport
{
    public sealed class DrawingViewOccurrenceResolution
    {
        public required DrawingDocument DrawingDocument { get; init; }

        public required Sheet Sheet { get; init; }

        public required DrawingView DrawingView { get; init; }

        public required AssemblyDocument ReferencedAssemblyDocument { get; init; }

        public required object Occurrence { get; init; }

        public required string OccurrencePath { get; init; }

        public required string OccurrenceName { get; init; }

        public Document? ReferencedOccurrenceDocument { get; init; }
    }

    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        error = null;

        Document? activeDocument = inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error = "В Inventor нет активного документа.";
            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error = "Активный документ не является чертежом.";
            return null;
        }

        return (DrawingDocument)activeDocument;
    }

    public static DrawingView? FindDrawingView(
        Sheet sheet,
        string viewName)
    {
        ArgumentNullException.ThrowIfNull(sheet);

        foreach (DrawingView drawingView in sheet.DrawingViews)
        {
            if (string.Equals(
                    drawingView.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return drawingView;
            }
        }

        return null;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string error)
    {
        value = string.Empty;
        error = string.Empty;

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            error = $"Не найдено обязательное поле \"{propertyName}\".";
            return false;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            error = $"Поле \"{propertyName}\" должно быть строкой.";
            return false;
        }

        value = element.GetString()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = $"Поле \"{propertyName}\" не должно быть пустым.";
            return false;
        }

        return true;
    }

    public static bool TryGetRequiredDouble(
        JsonElement root,
        string propertyName,
        out double value,
        out string error)
    {
        value = 0.0;
        error = string.Empty;

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            error = $"Не найдено обязательное поле \"{propertyName}\".";
            return false;
        }

        if (!element.TryGetDouble(out value) ||
            double.IsNaN(value) ||
            double.IsInfinity(value))
        {
            error = $"Поле \"{propertyName}\" должно содержать допустимое число.";
            return false;
        }

        return true;
    }

    public static bool TryGetRequiredBoolean(
        JsonElement root,
        string propertyName,
        out bool value,
        out string error)
    {
        value = false;
        error = string.Empty;

        if (!root.TryGetProperty(propertyName, out JsonElement element))
        {
            error = $"Не найдено обязательное поле \"{propertyName}\".";
            return false;
        }

        if (element.ValueKind != JsonValueKind.True &&
            element.ValueKind != JsonValueKind.False)
        {
            error = $"Поле \"{propertyName}\" должно содержать true или false.";
            return false;
        }

        value = element.GetBoolean();
        return true;
    }

    public static DrawingViewOccurrenceResolution?
        ResolveDrawingViewOccurrence(
            Inventor.Application inventor,
            JsonElement root,
            out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

        DrawingDocument? drawingDocument =
            GetActiveDrawingDocument(
                inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            error =
                documentError ??
                "РќРµ СѓРґР°Р»РѕСЃСЊ РїРѕР»СѓС‡РёС‚СЊ Р°РєС‚РёРІРЅС‹Р№ С‡РµСЂС‚С‘Р¶.";

            return null;
        }

        if (!TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out string sheetError))
        {
            error =
                sheetError;

            return null;
        }

        if (!TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out string viewError))
        {
            error =
                viewError;

            return null;
        }

        if (!TryGetRequiredString(
                root,
                "occurrencePath",
                out string occurrencePath,
                out string occurrencePathError))
        {
            error =
                occurrencePathError;

            return null;
        }

        Sheet? sheet =
            FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            error =
                $"Р›РёСЃС‚ \"{sheetName}\" РЅРµ РЅР°Р№РґРµРЅ.";

            return null;
        }

        DrawingView? drawingView =
            FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            error =
                $"Р’РёРґ \"{viewName}\" РЅРµ РЅР°Р№РґРµРЅ.";

            return null;
        }

        Document referencedDocument;

        try
        {
            referencedDocument =
                drawingView
                    .ReferencedDocumentDescriptor
                    .ReferencedDocument;
        }
        catch (Exception exception)
        {
            error =
                "РќРµ СѓРґР°Р»РѕСЃСЊ РїСЂРѕС‡РёС‚Р°С‚СЊ РґРѕРєСѓРјРµРЅС‚ РјРѕРґРµР»Рё, РЅР° РєРѕС‚РѕСЂС‹Р№ СЃСЃС‹Р»Р°РµС‚СЃСЏ РІРёРґ. " +
                exception.Message;

            return null;
        }

        if (referencedDocument.DocumentType !=
            DocumentTypeEnum.kAssemblyDocumentObject)
        {
            error =
                $"Р’РёРґ \"{viewName}\" СЃСЃС‹Р»Р°РµС‚СЃСЏ РЅР° {referencedDocument.DocumentType}, РЅРµ РЅР° AssemblyDocument.";

            return null;
        }

        AssemblyDocument assemblyDocument =
            (AssemblyDocument)referencedDocument;

        List<OccurrencePathMatch> matches =
            new();

        ReadOccurrencePathMatches(
            assemblyDocument.ComponentDefinition.Occurrences,
            parentPath: string.Empty,
            occurrencePath,
            matches);

        if (matches.Count == 0)
        {
            error =
                $"Occurrence path \"{occurrencePath}\" was not found in the DrawingView referenced AssemblyDocument.";

            return null;
        }

        if (matches.Count > 1)
        {
            error =
                $"Occurrence path \"{occurrencePath}\" is ambiguous in the DrawingView referenced AssemblyDocument.";

            return null;
        }

        OccurrencePathMatch match =
            matches[0];

        if (GetBooleanProperty(
                match.Occurrence,
                "Suppressed"))
        {
            error =
                $"Occurrence path \"{occurrencePath}\" is suppressed.";

            return null;
        }

        Document? occurrenceDocument =
            GetOccurrenceReferencedDocument(
                match.Occurrence);

        return new DrawingViewOccurrenceResolution
        {
            DrawingDocument =
                drawingDocument,

            Sheet =
                sheet,

            DrawingView =
                drawingView,

            ReferencedAssemblyDocument =
                assemblyDocument,

            Occurrence =
                match.Occurrence,

            OccurrencePath =
                match.Path,

            OccurrenceName =
                GetStringProperty(
                    match.Occurrence,
                    "Name"),

            ReferencedOccurrenceDocument =
                occurrenceDocument
        };
    }

    public static Sheet? FindSheet(
        DrawingDocument drawingDocument,
        string sheetName)
    {
        ArgumentNullException.ThrowIfNull(drawingDocument);

        foreach (Sheet sheet in drawingDocument.Sheets)
        {
            if (string.Equals(
                    sheet.Name,
                    sheetName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return sheet;
            }
        }

        return null;
    }

    public static object ReadVisibilityTarget(
        DrawingViewOccurrenceResolution resolution)
    {
        ArgumentNullException.ThrowIfNull(resolution);

        return new
        {
            occurrencePath =
                resolution.OccurrencePath,

            occurrenceName =
                resolution.OccurrenceName,

            referencedAssembly =
                ReadDocument(
                    resolution.ReferencedAssemblyDocument),

            referencedDocument =
                resolution.ReferencedOccurrenceDocument == null
                    ? null
                    : ReadDocument(
                        resolution.ReferencedOccurrenceDocument),

            targetDocumentType =
                resolution.ReferencedOccurrenceDocument?
                    .DocumentType
                    .ToString()
        };
    }

    public static object ReadDocument(
        object documentObject)
    {
        ArgumentNullException.ThrowIfNull(documentObject);

        dynamic document =
            documentObject;

        return new
        {
            displayName =
                document.DisplayName,

            fullFileName =
                document.FullFileName,

            documentType =
                document.DocumentType
                    .ToString(),

            dirty =
                document.Dirty
        };
    }

    public static string CreateSuccess(object data)
    {
        return JsonSerializer.Serialize(
            new { success = true, data },
            CreateJsonOptions());
    }

    public static string CreateError(
        string message,
        string? details = null)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = false,
                error = message,
                details
            },
            CreateJsonOptions());
    }

    private sealed record OccurrencePathMatch(
        object Occurrence,
        string Path);

    private static void ReadOccurrencePathMatches(
        object occurrencesObject,
        string parentPath,
        string targetPath,
        List<OccurrencePathMatch> matches)
    {
        dynamic occurrences =
            occurrencesObject;

        int count =
            GetCollectionCount(
                occurrencesObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object occurrenceObject;

            try
            {
                occurrenceObject =
                    occurrences[index];
            }
            catch
            {
                continue;
            }

            string name =
                GetStringProperty(
                    occurrenceObject,
                    "Name");

            string path =
                string.IsNullOrWhiteSpace(
                    parentPath)
                    ? name
                    : $"{parentPath}/{name}";

            if (string.Equals(
                    path,
                    targetPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(
                    new OccurrencePathMatch(
                        occurrenceObject,
                        path));
            }

            object? subOccurrences =
                GetObjectProperty(
                    occurrenceObject,
                    "SubOccurrences");

            if (subOccurrences != null)
            {
                ReadOccurrencePathMatches(
                    subOccurrences,
                    path,
                    targetPath,
                    matches);
            }
        }
    }

    private static Document? GetOccurrenceReferencedDocument(
        object occurrenceObject)
    {
        object? definition =
            GetObjectProperty(
                occurrenceObject,
                "Definition");

        if (definition != null)
        {
            object? definitionDocument =
                GetObjectProperty(
                    definition,
                    "Document");

            if (definitionDocument is Document document)
            {
                return document;
            }
        }

        object? descriptor =
            GetObjectProperty(
                occurrenceObject,
                "ReferencedDocumentDescriptor");

        if (descriptor == null)
        {
            return null;
        }

        object? referencedDocument =
            GetObjectProperty(
                descriptor,
                "ReferencedDocument");

        return referencedDocument as Document;
    }

    private static bool GetBooleanProperty(
        object source,
        string propertyName)
    {
        object? value =
            GetObjectProperty(
                source,
                propertyName);

        return value is bool boolean &&
               boolean;
    }

    private static string GetStringProperty(
        object source,
        string propertyName)
    {
        object? value =
            GetObjectProperty(
                source,
                propertyName);

        return value?.ToString() ??
               string.Empty;
    }

    private static int GetCollectionCount(
        object collectionObject)
    {
        object? value =
            GetObjectProperty(
                collectionObject,
                "Count");

        return value is int count
            ? count
            : 0;
    }

    private static object? GetObjectProperty(
        object source,
        string propertyName)
    {
        try
        {
            return source
                .GetType()
                .InvokeMember(
                    propertyName,
                    System.Reflection.BindingFlags.GetProperty,
                    binder: null,
                    target: source,
                    args: null);
        }
        catch
        {
            try
            {
                dynamic dynamicSource =
                    source;

                return propertyName switch
                {
                    "Count" =>
                        dynamicSource.Count,

                    "Definition" =>
                        dynamicSource.Definition,

                    "Document" =>
                        dynamicSource.Document,

                    "Name" =>
                        dynamicSource.Name,

                    "ReferencedDocument" =>
                        dynamicSource.ReferencedDocument,

                    "ReferencedDocumentDescriptor" =>
                        dynamicSource.ReferencedDocumentDescriptor,

                    "SubOccurrences" =>
                        dynamicSource.SubOccurrences,

                    "Suppressed" =>
                        dynamicSource.Suppressed,

                    _ =>
                        null
                };
            }
            catch
            {
                return null;
            }
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
