using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class AssemblyReadSupport
{
    public static AssemblyDocument? ResolveAssemblyDocument(
        Inventor.Application inventor,
        JsonElement root,
        out DrawingDocument? drawing,
        out Sheet? sheet,
        out DrawingView? view,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        drawing = null;
        sheet = null;
        view = null;
        error = null;

        Document? activeDocument =
            inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "В Inventor нет активного документа.";

            return null;
        }

        if (activeDocument.DocumentType ==
            DocumentTypeEnum.kAssemblyDocumentObject)
        {
            return
                (AssemblyDocument)activeDocument;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Активный документ не является сборкой или чертежом.";

            return null;
        }

        drawing =
            (DrawingDocument)activeDocument;

        if (!TryGetRequiredString(
                root,
                "sheetName",
                out string sheetName,
                out error))
        {
            return null;
        }

        if (!TryGetRequiredString(
                root,
                "viewName",
                out string viewName,
                out error))
        {
            return null;
        }

        sheet =
            FindSheet(
                drawing,
                sheetName);

        if (sheet == null)
        {
            error =
                $"Лист \"{sheetName}\" не найден.";

            return null;
        }

        view =
            FindView(
                sheet,
                viewName);

        if (view == null)
        {
            error =
                $"Вид \"{viewName}\" не найден.";

            return null;
        }

        try
        {
            Document referencedDocument =
                view
                    .ReferencedDocumentDescriptor
                    .ReferencedDocument;

            if (referencedDocument.DocumentType !=
                DocumentTypeEnum.kAssemblyDocumentObject)
            {
                error =
                    "Документ, на который ссылается вид, не является сборкой.";

                return null;
            }

            return
                (AssemblyDocument)referencedDocument;
        }
        catch (Exception exception)
        {
            error =
                exception.Message;

            return null;
        }
    }

    public static Sheet? FindSheet(
        DrawingDocument drawing,
        string sheetName)
    {
        foreach (Sheet sheet in drawing.Sheets)
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

    public static DrawingView? FindView(
        Sheet sheet,
        string viewName)
    {
        foreach (DrawingView view in sheet.DrawingViews)
        {
            if (string.Equals(
                    view.Name,
                    viewName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return view;
            }
        }

        return null;
    }

    public static bool TryGetRequiredString(
        JsonElement root,
        string propertyName,
        out string value,
        out string? error)
    {
        value = string.Empty;
        error = null;

        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element) ||
            element.ValueKind !=
                JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

            return false;
        }

        value =
            element.GetString()?
                .Trim()
            ?? string.Empty;

        if (value.Length == 0)
        {
            error =
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
    }

    public static int GetOptionalInt32(
        JsonElement root,
        string propertyName,
        int defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        return element.TryGetInt32(
            out int value)
                ? value
                : defaultValue;
    }

    public static bool GetOptionalBoolean(
        JsonElement root,
        string propertyName,
        bool defaultValue)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement element))
        {
            return defaultValue;
        }

        if (element.ValueKind ==
            JsonValueKind.True)
        {
            return true;
        }

        if (element.ValueKind ==
            JsonValueKind.False)
        {
            return false;
        }

        return defaultValue;
    }

    public static object ReadDocument(
        object documentObject)
    {
        return new
        {
            displayName =
                GetString(
                    documentObject,
                    "DisplayName"),

            fullFileName =
                GetString(
                    documentObject,
                    "FullFileName"),

            internalName =
                GetString(
                    documentObject,
                    "InternalName"),

            documentTypeRaw =
                GetRaw(
                    documentObject,
                    "DocumentType"),

            documentType =
                GetDocumentTypeName(
                    documentObject,
                    "DocumentType"),

            dirty =
                GetBoolean(
                    documentObject,
                    "Dirty")
        };
    }

    public static object ReadAssemblySummary(
        AssemblyDocument assemblyDocument)
    {
        AssemblyComponentDefinition definition =
            assemblyDocument.ComponentDefinition;

        object occurrences =
            definition.Occurrences;

        object constraints =
            definition.Constraints;

        object representations =
            definition.RepresentationsManager;

        return new
        {
            document =
                ReadDocument(
                    assemblyDocument),

            occurrenceCount =
                GetCollectionCount(
                    occurrences),

            constraintCount =
                GetCollectionCount(
                    constraints),

            surfaceBodyCount =
                GetNestedCollectionCount(
                    definition,
                    "SurfaceBodies"),

            activeDesignViewRepresentation =
                ReadNamedObject(
                    GetObject(
                        representations,
                        "ActiveDesignViewRepresentation")),

            activePositionalRepresentation =
                ReadNamedObject(
                    GetObject(
                        representations,
                        "ActivePositionalRepresentation")),

            activeModelState =
                ReadNamedObject(
                    GetObject(
                        assemblyDocument,
                        "ActiveModelState")),

            bom =
                ReadBomState(
                    definition.BOM)
        };
    }

    public static List<object> ReadOccurrences(
        AssemblyDocument assemblyDocument,
        int maxDepth,
        bool includeSuppressed)
    {
        List<object> result =
            new();

        object occurrencesObject =
            assemblyDocument
                .ComponentDefinition
                .Occurrences;

        ReadOccurrenceCollection(
            occurrencesObject,
            parentPath: string.Empty,
            depth: 0,
            maxDepth,
            includeSuppressed,
            result);

        return result;
    }

    public static List<object> ReadConstraints(
        AssemblyDocument assemblyDocument)
    {
        List<object> result =
            new();

        object constraintsObject =
            assemblyDocument
                .ComponentDefinition
                .Constraints;

        dynamic constraints =
            constraintsObject;

        int count =
            GetCollectionCount(
                constraintsObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object constraintObject;

            try
            {
                constraintObject =
                    constraints[index];
            }
            catch
            {
                continue;
            }

            result.Add(
                ReadConstraint(
                    constraintObject,
                    index));
        }

        return result;
    }

    public static object ReadBom(
        AssemblyDocument assemblyDocument,
        bool includeRows)
    {
        object bomObject =
            assemblyDocument
                .ComponentDefinition
                .BOM;

        object? viewsObject =
            GetObject(
                bomObject,
                "BOMViews");

        List<object> views =
            new();

        if (viewsObject != null)
        {
            dynamic bomViews =
                viewsObject;

            int count =
                GetCollectionCount(
                    viewsObject);

            for (int index = 1;
                 index <= count;
                 index++)
            {
                object viewObject;

                try
                {
                    viewObject =
                        bomViews[index];
                }
                catch
                {
                    continue;
                }

                views.Add(
                    ReadBomView(
                        viewObject,
                        index,
                        includeRows));
            }
        }

        return new
        {
            state =
                ReadBomState(
                    bomObject),

            viewCount =
                views.Count,

            views
        };
    }

    public static List<object> ReadReferencedDocuments(
        AssemblyDocument assemblyDocument)
    {
        List<object> result =
            new();

        DocumentsEnumerator documents =
            assemblyDocument
                .AllReferencedDocuments;

        int index =
            0;

        foreach (Document document in documents)
        {
            index++;

            result.Add(
                new
                {
                    index,

                    document =
                        ReadDocument(
                            document),

                    properties =
                        ReadCommonProperties(
                            document)
                });
        }

        return result;
    }

    private static void ReadOccurrenceCollection(
        object occurrencesObject,
        string parentPath,
        int depth,
        int maxDepth,
        bool includeSuppressed,
        List<object> result)
    {
        if (maxDepth >= 0 &&
            depth > maxDepth)
        {
            return;
        }

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

            bool suppressed =
                GetBoolean(
                    occurrenceObject,
                    "Suppressed");

            if (!includeSuppressed &&
                suppressed)
            {
                continue;
            }

            string name =
                GetString(
                    occurrenceObject,
                    "Name");

            string path =
                string.IsNullOrWhiteSpace(
                    parentPath)
                    ? name
                    : $"{parentPath}/{name}";

            result.Add(
                ReadOccurrence(
                    occurrenceObject,
                    index,
                    path,
                    depth));

            object? subOccurrences =
                GetObject(
                    occurrenceObject,
                    "SubOccurrences");

            if (subOccurrences != null)
            {
                ReadOccurrenceCollection(
                    subOccurrences,
                    path,
                    depth + 1,
                    maxDepth,
                    includeSuppressed,
                    result);
            }
        }
    }

    private static object ReadOccurrence(
        object occurrenceObject,
        int index,
        string path,
        int depth)
    {
        object? descriptor =
            GetObject(
                occurrenceObject,
                "ReferencedDocumentDescriptor");

        object? definition =
            GetObject(
                occurrenceObject,
                "Definition");

        object? transformation =
            GetObject(
                occurrenceObject,
                "Transformation");

        return new
        {
            index,
            path,
            depth,

            name =
                GetString(
                    occurrenceObject,
                    "Name"),

            objectTypeRaw =
                GetRaw(
                    occurrenceObject,
                    "Type"),

            objectType =
                GetObjectTypeName(
                    occurrenceObject,
                    "Type"),

            definitionDocumentTypeRaw =
                GetRaw(
                    occurrenceObject,
                    "DefinitionDocumentType"),

            definitionDocumentType =
                GetDocumentTypeName(
                    occurrenceObject,
                    "DefinitionDocumentType"),

            suppressed =
                GetBoolean(
                    occurrenceObject,
                    "Suppressed"),

            visible =
                GetBoolean(
                    occurrenceObject,
                    "Visible"),

            grounded =
                GetBoolean(
                    occurrenceObject,
                    "Grounded"),

            flexible =
                GetBoolean(
                    occurrenceObject,
                    "Flexible"),

            adaptive =
                GetBoolean(
                    occurrenceObject,
                    "Adaptive"),

            excluded =
                GetBoolean(
                    occurrenceObject,
                    "Excluded"),

            bomStructureRaw =
                GetRaw(
                    occurrenceObject,
                    "BOMStructure"),

            bomStructure =
                GetEnumName(
                    typeof(BOMStructureEnum),
                    GetObject(
                        occurrenceObject,
                        "BOMStructure")),

            occurrencePath =
                ReadOccurrencePath(
                    occurrenceObject),

            referencedDocument =
                ReadDescriptor(
                    descriptor),

            definition =
                ReadNamedObject(
                    definition),

            transformation =
                ReadMatrix(
                    transformation),

            rangeBox =
                ReadBox3d(
                    GetObject(
                        occurrenceObject,
                        "RangeBox"))
        };
    }

    private static object ReadConstraint(
        object constraintObject,
        int index)
    {
        object? occurrenceOne =
            GetObject(
                constraintObject,
                "OccurrenceOne");

        object? occurrenceTwo =
            GetObject(
                constraintObject,
                "OccurrenceTwo");

        object? affectedOne =
            GetObject(
                constraintObject,
                "AffectedOccurrenceOne");

        object? affectedTwo =
            GetObject(
                constraintObject,
                "AffectedOccurrenceTwo");

        return new
        {
            index,

            name =
                GetString(
                    constraintObject,
                    "Name"),

            objectTypeRaw =
                GetRaw(
                    constraintObject,
                    "Type"),

            objectType =
                GetObjectTypeName(
                    constraintObject,
                    "Type"),

            healthStatusRaw =
                GetRaw(
                    constraintObject,
                    "HealthStatus"),

            healthStatus =
                GetEnumName(
                    typeof(HealthStatusEnum),
                    GetObject(
                        constraintObject,
                        "HealthStatus")),

            suppressed =
                GetBoolean(
                    constraintObject,
                    "Suppressed"),

            visible =
                GetBoolean(
                    constraintObject,
                    "Visible"),

            occurrenceOne =
                ReadOccurrenceReference(
                    occurrenceOne),

            occurrenceTwo =
                ReadOccurrenceReference(
                    occurrenceTwo),

            affectedOccurrenceOne =
                ReadOccurrenceReference(
                    affectedOne),

            affectedOccurrenceTwo =
                ReadOccurrenceReference(
                    affectedTwo),

            offset =
                ReadParameter(
                    GetObject(
                        constraintObject,
                        "Offset")),

            angle =
                ReadParameter(
                    GetObject(
                        constraintObject,
                        "Angle")),

            resultOfiMate =
                GetBoolean(
                    constraintObject,
                    "ResultOfiMate")
        };
    }

    private static object ReadBomState(
        object bomObject)
    {
        return new
        {
            structuredViewEnabled =
                GetBoolean(
                    bomObject,
                    "StructuredViewEnabled"),

            structuredViewFirstLevelOnly =
                GetBoolean(
                    bomObject,
                    "StructuredViewFirstLevelOnly"),

            partsOnlyViewEnabled =
                GetBoolean(
                    bomObject,
                    "PartsOnlyViewEnabled"),

            modelDataViewEnabled =
                GetBoolean(
                    bomObject,
                    "ModelDataViewEnabled")
        };
    }

    private static object ReadBomView(
        object viewObject,
        int index,
        bool includeRows)
    {
        List<object> rows =
            new();

        object? rowsObject =
            GetObject(
                viewObject,
                "BOMRows");

        if (includeRows &&
            rowsObject != null)
        {
            dynamic bomRows =
                rowsObject;

            int count =
                GetCollectionCount(
                    rowsObject);

            for (int rowIndex = 1;
                 rowIndex <= count;
                 rowIndex++)
            {
                object rowObject;

                try
                {
                    rowObject =
                        bomRows[rowIndex];
                }
                catch
                {
                    continue;
                }

                rows.Add(
                    ReadBomRow(
                        rowObject,
                        rowIndex));
            }
        }

        return new
        {
            index,

            name =
                GetString(
                    viewObject,
                    "Name"),

            viewTypeRaw =
                GetRaw(
                    viewObject,
                    "ViewType"),

            viewType =
                GetEnumName(
                    typeof(BOMViewTypeEnum),
                    GetObject(
                        viewObject,
                        "ViewType")),

            revisionId =
                GetString(
                    viewObject,
                    "RevisionId"),

            rowCount =
                rowsObject == null
                    ? 0
                    : GetCollectionCount(
                        rowsObject),

            rowsReturned =
                rows.Count,

            rows
        };
    }

    private static object ReadBomRow(
        object rowObject,
        int index)
    {
        List<object> componentDefinitions =
            new();

        object? definitionsObject =
            GetObject(
                rowObject,
                "ComponentDefinitions");

        if (definitionsObject != null)
        {
            dynamic definitions =
                definitionsObject;

            int count =
                GetCollectionCount(
                    definitionsObject);

            for (int definitionIndex = 1;
                 definitionIndex <= count;
                 definitionIndex++)
            {
                object definitionObject;

                try
                {
                    definitionObject =
                        definitions[definitionIndex];
                }
                catch
                {
                    continue;
                }

                componentDefinitions.Add(
                    ReadComponentDefinition(
                        definitionObject,
                        definitionIndex));
            }
        }

        return new
        {
            index,

            itemNumber =
                GetString(
                    rowObject,
                    "ItemNumber"),

            itemQuantity =
                GetNullableDouble(
                    rowObject,
                    "ItemQuantity"),

            totalQuantity =
                GetString(
                    rowObject,
                    "TotalQuantity"),

            bomStructureRaw =
                GetRaw(
                    rowObject,
                    "BOMStructure"),

            bomStructure =
                GetEnumName(
                    typeof(BOMStructureEnum),
                    GetObject(
                        rowObject,
                        "BOMStructure")),

            promoted =
                GetBoolean(
                    rowObject,
                    "Promoted"),

            rolledUp =
                GetBoolean(
                    rowObject,
                    "RolledUp"),

            componentDefinitionCount =
                componentDefinitions.Count,

            componentDefinitions
        };
    }

    private static object ReadComponentDefinition(
        object definitionObject,
        int index)
    {
        object? document =
            GetObject(
                definitionObject,
                "Document");

        return new
        {
            index,

            objectTypeRaw =
                GetRaw(
                    definitionObject,
                    "Type"),

            objectType =
                GetObjectTypeName(
                    definitionObject,
                    "Type"),

            document =
                document == null
                    ? null
                    : ReadDocument(
                        document)
        };
    }

    private static object? ReadOccurrenceReference(
        object? occurrenceObject)
    {
        if (occurrenceObject == null)
        {
            return null;
        }

        return new
        {
            name =
                GetString(
                    occurrenceObject,
                    "Name"),

            occurrencePath =
                ReadOccurrencePath(
                    occurrenceObject),

            suppressed =
                GetBoolean(
                    occurrenceObject,
                    "Suppressed")
        };
    }

    private static object? ReadDescriptor(
        object? descriptorObject)
    {
        if (descriptorObject == null)
        {
            return null;
        }

        object? document =
            GetObject(
                descriptorObject,
                "ReferencedDocument");

        return new
        {
            fullDocumentName =
                GetString(
                    descriptorObject,
                    "FullDocumentName"),

            referenceMissing =
                GetBoolean(
                    descriptorObject,
                    "ReferenceMissing"),

            referenceSuppressed =
                GetBoolean(
                    descriptorObject,
                    "ReferenceSuppressed"),

            referencedDocumentTypeRaw =
                GetRaw(
                    descriptorObject,
                    "ReferencedDocumentType"),

            referencedDocumentType =
                GetDocumentTypeName(
                    descriptorObject,
                    "ReferencedDocumentType"),

            document =
                document == null
                    ? null
                    : ReadDocument(
                        document)
        };
    }

    private static object? ReadNamedObject(
        object? value)
    {
        if (value == null)
        {
            return null;
        }

        return new
        {
            name =
                GetString(
                    value,
                    "Name"),

            objectTypeRaw =
                GetRaw(
                    value,
                    "Type"),

            objectType =
                GetObjectTypeName(
                    value,
                    "Type")
        };
    }

    private static object? ReadParameter(
        object? parameterObject)
    {
        if (parameterObject == null)
        {
            return null;
        }

        return new
        {
            name =
                GetString(
                    parameterObject,
                    "Name"),

            expression =
                GetString(
                    parameterObject,
                    "Expression"),

            value =
                GetNullableDouble(
                    parameterObject,
                    "Value"),

            modelValue =
                GetNullableDouble(
                    parameterObject,
                    "ModelValue"),

            units =
                GetString(
                    parameterObject,
                    "Units")
        };
    }

    private static object? ReadMatrix(
        object? matrixObject)
    {
        if (matrixObject == null)
        {
            return null;
        }

        dynamic matrix =
            matrixObject;

        List<List<double?>> rows =
            new();

        for (int row = 1;
             row <= 4;
             row++)
        {
            List<double?> values =
                new();

            for (int column = 1;
                 column <= 4;
                 column++)
            {
                try
                {
                    values.Add(
                        (double)matrix.Cell[
                            row,
                            column]);
                }
                catch
                {
                    values.Add(
                        null);
                }
            }

            rows.Add(
                values);
        }

        return new
        {
            rows
        };
    }

    private static object? ReadBox3d(
        object? boxObject)
    {
        if (boxObject == null)
        {
            return null;
        }

        return new
        {
            minPoint =
                ReadPoint3d(
                    GetObject(
                        boxObject,
                        "MinPoint")),

            maxPoint =
                ReadPoint3d(
                    GetObject(
                        boxObject,
                        "MaxPoint"))
        };
    }

    private static object? ReadPoint3d(
        object? pointObject)
    {
        if (pointObject == null)
        {
            return null;
        }

        return new
        {
            x =
                GetNullableDouble(
                    pointObject,
                    "X"),

            y =
                GetNullableDouble(
                    pointObject,
                    "Y"),

            z =
                GetNullableDouble(
                    pointObject,
                    "Z")
        };
    }

    private static object ReadCommonProperties(
        Document document)
    {
        string partNumber =
            string.Empty;

        string description =
            string.Empty;

        string material =
            string.Empty;

        try
        {
            PropertySet designTracking =
                document.PropertySets[
                    "Design Tracking Properties"];

            partNumber =
                designTracking[
                    "Part Number"]
                    .Value?
                    .ToString()
                ?? string.Empty;

            description =
                designTracking[
                    "Description"]
                    .Value?
                    .ToString()
                ?? string.Empty;

            material =
                designTracking[
                    "Material"]
                    .Value?
                    .ToString()
                ?? string.Empty;
        }
        catch
        {
        }

        return new
        {
            partNumber,
            description,
            material
        };
    }

    private static object? GetObject(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "ActiveDesignViewRepresentation" =>
                    owner.ActiveDesignViewRepresentation,

                "ActivePositionalRepresentation" =>
                    owner.ActivePositionalRepresentation,

                "ActiveModelState" =>
                    owner.ActiveModelState,

                "BOMViews" =>
                    owner.BOMViews,

                "SubOccurrences" =>
                    owner.SubOccurrences,

                "OccurrencePath" =>
                    owner.OccurrencePath,

                "ReferencedDocumentDescriptor" =>
                    owner.ReferencedDocumentDescriptor,

                "ReferencedDocument" =>
                    owner.ReferencedDocument,

                "Definition" =>
                    owner.Definition,

                "Transformation" =>
                    owner.Transformation,

                "RangeBox" =>
                    owner.RangeBox,

                "OccurrenceOne" =>
                    owner.OccurrenceOne,

                "OccurrenceTwo" =>
                    owner.OccurrenceTwo,

                "AffectedOccurrenceOne" =>
                    owner.AffectedOccurrenceOne,

                "AffectedOccurrenceTwo" =>
                    owner.AffectedOccurrenceTwo,

                "Offset" =>
                    owner.Offset,

                "Angle" =>
                    owner.Angle,

                "HealthStatus" =>
                    owner.HealthStatus,

                "BOMStructure" =>
                    owner.BOMStructure,

                "ViewType" =>
                    owner.ViewType,

                "BOMRows" =>
                    owner.BOMRows,

                "ComponentDefinitions" =>
                    owner.ComponentDefinitions,

                "Document" =>
                    owner.Document,

                "MinPoint" =>
                    owner.MinPoint,

                "MaxPoint" =>
                    owner.MaxPoint,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

    private static string GetString(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            object? value =
                propertyName switch
                {
                    "Name" =>
                        owner.Name,

                    "DisplayName" =>
                        owner.DisplayName,

                    "FullFileName" =>
                        owner.FullFileName,

                    "InternalName" =>
                        owner.InternalName,

                    "OccurrencePath" =>
                        owner.OccurrencePath,

                    "FullDocumentName" =>
                        owner.FullDocumentName,

                    "Expression" =>
                        owner.Expression,

                    "Units" =>
                        owner.Units,

                    "ItemNumber" =>
                        owner.ItemNumber,

                    "TotalQuantity" =>
                        owner.TotalQuantity,

                    "RevisionId" =>
                        owner.RevisionId,

                    _ =>
                        null
                };

            return value?.ToString() ??
                   string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetRaw(
        object ownerObject,
        string propertyName)
    {
        object? value =
            GetRawObject(
                ownerObject,
                propertyName);

        return value?.ToString() ??
               string.Empty;
    }

    private static object? GetRawObject(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "Type" =>
                    owner.Type,

                "DocumentType" =>
                    owner.DocumentType,

                "DefinitionDocumentType" =>
                    owner.DefinitionDocumentType,

                "ReferencedDocumentType" =>
                    owner.ReferencedDocumentType,

                "BOMStructure" =>
                    owner.BOMStructure,

                "HealthStatus" =>
                    owner.HealthStatus,

                "ViewType" =>
                    owner.ViewType,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

    private static string GetDocumentTypeName(
        object ownerObject,
        string propertyName)
    {
        return GetEnumName(
            typeof(DocumentTypeEnum),
            GetRawObject(
                ownerObject,
                propertyName));
    }

    private static object ReadOccurrencePath(
        object occurrenceObject)
    {
        object? pathObject =
            GetObject(
                occurrenceObject,
                "OccurrencePath");

        if (pathObject == null)
        {
            return Array.Empty<string>();
        }

        List<string> names =
            new();

        dynamic path =
            pathObject;

        int count =
            GetCollectionCount(
                pathObject);

        for (int index = 1;
             index <= count;
             index++)
        {
            object itemObject;

            try
            {
                itemObject =
                    path[index];
            }
            catch
            {
                continue;
            }

            string name =
                GetString(
                    itemObject,
                    "Name");

            if (string.IsNullOrWhiteSpace(
                    name))
            {
                name =
                    itemObject.ToString()
                    ?? string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(
                    name))
            {
                names.Add(
                    name);
            }
        }

        if (names.Count == 0)
        {
            string fallback =
                pathObject.ToString()
                ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(
                    fallback) &&
                !string.Equals(
                    fallback,
                    "System.__ComObject",
                    StringComparison.Ordinal))
            {
                names.Add(
                    fallback);
            }
        }

        return names;
    }

    private static string GetObjectTypeName(
        object ownerObject,
        string propertyName)
    {
        return GetEnumName(
            typeof(ObjectTypeEnum),
            GetRawObject(
                ownerObject,
                propertyName));
    }

    private static string GetEnumName(
        Type enumType,
        object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            int numericValue =
                Convert.ToInt32(
                    value);

            return Enum.GetName(
                       enumType,
                       numericValue)
                   ?? numericValue.ToString();
        }
        catch
        {
            return value.ToString()
                   ?? string.Empty;
        }
    }

    private static bool GetBoolean(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "Dirty" =>
                    (bool)owner.Dirty,

                "Suppressed" =>
                    (bool)owner.Suppressed,

                "Visible" =>
                    (bool)owner.Visible,

                "Grounded" =>
                    (bool)owner.Grounded,

                "Flexible" =>
                    (bool)owner.Flexible,

                "Adaptive" =>
                    (bool)owner.Adaptive,

                "Excluded" =>
                    (bool)owner.Excluded,

                "ReferenceMissing" =>
                    (bool)owner.ReferenceMissing,

                "ReferenceSuppressed" =>
                    (bool)owner.ReferenceSuppressed,

                "ResultOfiMate" =>
                    (bool)owner.ResultOfiMate,

                "StructuredViewEnabled" =>
                    (bool)owner.StructuredViewEnabled,

                "StructuredViewFirstLevelOnly" =>
                    (bool)owner.StructuredViewFirstLevelOnly,

                "PartsOnlyViewEnabled" =>
                    (bool)owner.PartsOnlyViewEnabled,

                "ModelDataViewEnabled" =>
                    (bool)owner.ModelDataViewEnabled,

                "Promoted" =>
                    (bool)owner.Promoted,

                "RolledUp" =>
                    (bool)owner.RolledUp,

                _ =>
                    false
            };
        }
        catch
        {
            return false;
        }
    }

    private static double? GetNullableDouble(
        object ownerObject,
        string propertyName)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "Value" =>
                    (double)owner.Value,

                "ModelValue" =>
                    (double)owner.ModelValue,

                "ItemQuantity" =>
                    Convert.ToDouble(
                        owner.ItemQuantity),

                "X" =>
                    (double)owner.X,

                "Y" =>
                    (double)owner.Y,

                "Z" =>
                    (double)owner.Z,

                _ =>
                    null
            };
        }
        catch
        {
            return null;
        }
    }

    private static int GetCollectionCount(
        object collectionObject)
    {
        dynamic collection =
            collectionObject;

        try
        {
            return
                (int)collection.Count;
        }
        catch
        {
            return 0;
        }
    }

    private static int GetNestedCollectionCount(
        object ownerObject,
        string propertyName)
    {
        object? collection =
            GetObject(
                ownerObject,
                propertyName);

        return collection == null
            ? 0
            : GetCollectionCount(
                collection);
    }

    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success = true,
                data
            },
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

    private static JsonSerializerOptions
        CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder =
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}
