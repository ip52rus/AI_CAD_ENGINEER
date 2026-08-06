using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DrawingStructureReadSupport
{
    public static DrawingDocument? GetActiveDrawing(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

        Document? document =
            inventor.ActiveDocument;

        if (document == null)
        {
            error = "В Inventor нет активного документа.";
            return null;
        }

        if (document.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error = "Активный документ не является чертежом.";
            return null;
        }

        return (DrawingDocument)document;
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
            element.ValueKind != JsonValueKind.String)
        {
            error =
                $"Поле \"{propertyName}\" должно быть строкой.";

            return false;
        }

        value =
            element.GetString()?.Trim()
            ?? string.Empty;

        if (value.Length == 0)
        {
            error =
                $"Поле \"{propertyName}\" не должно быть пустым.";

            return false;
        }

        return true;
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

        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => defaultValue
        };
    }

    public static List<object> ReadSheets(
        DrawingDocument drawing)
    {
        List<object> result = new();

        int index = 0;

        foreach (Sheet sheet in drawing.Sheets)
        {
            index++;

            result.Add(
                new
                {
                    index,
                    name = sheet.Name,
                    width = sheet.Width,
                    height = sheet.Height,
                    sizeRaw = sheet.Size.ToString(),
                    orientationRaw =
                        sheet.Orientation.ToString(),
                    active =
                        ReferenceEquals(
                            drawing.ActiveSheet,
                            sheet),
                    viewCount =
                        sheet.DrawingViews.Count,
                    border =
                        ReadNamedObject(
                            sheet.Border),
                    titleBlock =
                        ReadNamedObject(
                            sheet.TitleBlock)
                });
        }

        return result;
    }

    public static List<object> ReadViews(
        Sheet sheet)
    {
        List<object> result = new();

        int index = 0;

        foreach (DrawingView view in sheet.DrawingViews)
        {
            index++;

            object? parentView =
                GetObject(
                    view,
                    "ParentView");

            object? descriptor =
                GetObject(
                    view,
                    "ReferencedDocumentDescriptor");

            result.Add(
                new
                {
                    index,
                    name = view.Name,
                    objectTypeRaw =
                        view.Type.ToString(),
                    objectType =
                        Enum.GetName(
                            typeof(ObjectTypeEnum),
                            Convert.ToInt32(view.Type))
                        ?? view.Type.ToString(),
                    viewTypeRaw =
                        view.ViewType.ToString(),
                    position =
                        ReadPoint2d(
                            view.Position),
                    width = view.Width,
                    height = view.Height,
                    scale = view.Scale,
                    scaleString =
                        GetString(
                            view,
                            "ScaleString"),
                    rotation =
                        GetNullableDouble(
                            view,
                            "Rotation"),
                    styleRaw =
                        GetRaw(
                            view,
                            "Style"),
                    hiddenLineStatusRaw =
                        GetRaw(
                            view,
                            "HiddenLineStatus"),
                    tangentEdgesStatusRaw =
                        GetRaw(
                            view,
                            "TangentEdgesStatus"),
                    aligned =
                        GetBoolean(
                            view,
                            "Aligned"),
                    parentView =
                        ReadNamedObject(
                            parentView),
                    referencedDocument =
                        ReadDescriptor(
                            descriptor),
                    rangeBox =
                        ReadBox2d(
                            GetObject(
                                view,
                                "RangeBox"))
                });
        }

        return result;
    }

    public static object ReadViewRelationships(
        Sheet sheet)
    {
        List<object> relationships =
            new();

        int index = 0;

        foreach (DrawingView view in sheet.DrawingViews)
        {
            index++;

            object? parent =
                GetObject(
                    view,
                    "ParentView");

            relationships.Add(
                new
                {
                    index,
                    view = view.Name,
                    parent =
                        ReadNamedObject(
                            parent),
                    viewTypeRaw =
                        view.ViewType.ToString(),
                    aligned =
                        GetBoolean(
                            view,
                            "Aligned"),
                    alignmentTypeRaw =
                        GetRaw(
                            view,
                            "AlignmentType")
                });
        }

        return new
        {
            relationshipCount =
                relationships.Count,

            relationships
        };
    }

    public static object ReadDrawingAnnotations(
        Sheet sheet)
    {
        return new
        {
            generalDimensionCount =
                sheet.DrawingDimensions
                    .GeneralDimensions.Count,

            baselineDimensionSetCount =
                sheet.DrawingDimensions
                    .BaselineDimensionSets.Count,

            ordinateDimensionSetCount =
                sheet.DrawingDimensions
                    .OrdinateDimensionSets.Count,

            holeThreadNoteCount =
                sheet.DrawingNotes
                    .HoleThreadNotes.Count,

            generalNoteCount =
                sheet.DrawingNotes
                    .GeneralNotes.Count,

            leaderNoteCount =
                sheet.DrawingNotes
                    .LeaderNotes.Count,

            centerlineCount =
                sheet.Centerlines.Count,

            centerMarkCount =
                sheet.Centermarks.Count,

            surfaceTextureSymbolCount =
                sheet.SurfaceTextureSymbols.Count,

            featureControlFrameCount =
                sheet.FeatureControlFrames.Count,

            datumIdentifierCount =0,
                
            revisionTableCount =
                sheet.RevisionTables.Count,

            partsListCount =
                sheet.PartsLists.Count
        };
    }

    public static object ReadTablesAndPartsLists(
        Sheet sheet,
        bool includeRows)
    {
        List<object> partsLists =
            new();

        int listIndex = 0;

        foreach (PartsList partsList in sheet.PartsLists)
        {
            listIndex++;

            List<object> rows =
                new();

            if (includeRows)
            {
                int rowIndex = 0;

                foreach (PartsListRow row in partsList.PartsListRows)
                {
                    rowIndex++;

                    List<string> cells =
                        new();

                    foreach (PartsListCell cell in row)
                    {
                        cells.Add(
                            cell.Value?.ToString()
                            ?? string.Empty);
                    }

                    rows.Add(
                        new
                        {
                            index = rowIndex,
                            cells
                        });
                }
            }

            partsLists.Add(
                new
                {
                    index = listIndex,
                    title =
                        GetString(
                            partsList,
                            "Title"),
                    rowCount =
                        partsList.PartsListRows.Count,
                    columnCount =
                        partsList.PartsListColumns.Count,
                    position =
                        ReadPoint2d(
                            partsList.Position),
                    rowsReturned =
                        rows.Count,
                    rows
                });
        }

        List<object> revisionTables =
            new();

        int revisionIndex = 0;

        foreach (RevisionTable table in sheet.RevisionTables)
        {
            revisionIndex++;

            revisionTables.Add(
                new
                {
                    index = revisionIndex,
                    title =
                        GetString(
                            table,
                            "Title"),
                    rowCount =
                        GetCollectionCount(
                            GetObject(
                                table,
                                "RevisionTableRows")),
                    columnCount =
                        GetCollectionCount(
                            GetObject(
                                table,
                                "RevisionTableColumns")),
                    position =
                        ReadPoint2d(
                            GetObject(
                                table,
                                "Position"))
                });
        }

        return new
        {
            partsListCount =
                partsLists.Count,
            partsLists,
            revisionTableCount =
                revisionTables.Count,
            revisionTables
        };
    }

    private static object? ReadDescriptor(
        object? descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        object? document =
            GetObject(
                descriptor,
                "ReferencedDocument");

        return new
        {
            fullDocumentName =
                GetString(
                    descriptor,
                    "FullDocumentName"),
            referencedDocumentTypeRaw =
                GetRaw(
                    descriptor,
                    "ReferencedDocumentType"),
            document =
                document == null
                    ? null
                    : new
                    {
                        displayName =
                            GetString(
                                document,
                                "DisplayName"),
                        fullFileName =
                            GetString(
                                document,
                                "FullFileName"),
                        documentTypeRaw =
                            GetRaw(
                                document,
                                "DocumentType")
                    }
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
                    "Type")
        };
    }

    private static object? ReadPoint2d(
        object? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x =
                GetNullableDouble(
                    point,
                    "X"),
            y =
                GetNullableDouble(
                    point,
                    "Y")
        };
    }

    private static object? ReadBox2d(
        object? box)
    {
        if (box == null)
        {
            return null;
        }

        return new
        {
            minPoint =
                ReadPoint2d(
                    GetObject(
                        box,
                        "MinPoint")),
            maxPoint =
                ReadPoint2d(
                    GetObject(
                        box,
                        "MaxPoint"))
        };
    }

    private static object? GetObject(
        object ownerObject,
        string propertyName)
    {
        dynamic owner = ownerObject;

        try
        {
            return propertyName switch
            {
                "ParentView" =>
                    owner.ParentView,
                "ReferencedDocumentDescriptor" =>
                    owner.ReferencedDocumentDescriptor,
                "ReferencedDocument" =>
                    owner.ReferencedDocument,
                "RangeBox" =>
                    owner.RangeBox,
                "MinPoint" =>
                    owner.MinPoint,
                "MaxPoint" =>
                    owner.MaxPoint,
                "RevisionTableRows" =>
                    owner.RevisionTableRows,
                "RevisionTableColumns" =>
                    owner.RevisionTableColumns,
                "Position" =>
                    owner.Position,
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
        dynamic owner = ownerObject;

        try
        {
            object? value =
                propertyName switch
                {
                    "Name" =>
                        owner.Name,
                    "Title" =>
                        owner.Title,
                    "ScaleString" =>
                        owner.ScaleString,
                    "FullDocumentName" =>
                        owner.FullDocumentName,
                    "DisplayName" =>
                        owner.DisplayName,
                    "FullFileName" =>
                        owner.FullFileName,
                    _ =>
                        null
                };

            return value?.ToString()
                   ?? string.Empty;
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
        dynamic owner = ownerObject;

        try
        {
            object? value =
                propertyName switch
                {
                    "Type" =>
                        owner.Type,
                    "Style" =>
                        owner.Style,
                    "HiddenLineStatus" =>
                        owner.HiddenLineStatus,
                    "TangentEdgesStatus" =>
                        owner.TangentEdgesStatus,
                    "AlignmentType" =>
                        owner.AlignmentType,
                    "ReferencedDocumentType" =>
                        owner.ReferencedDocumentType,
                    "DocumentType" =>
                        owner.DocumentType,
                    _ =>
                        null
                };

            return value?.ToString()
                   ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool GetBoolean(
        object ownerObject,
        string propertyName)
    {
        dynamic owner = ownerObject;

        try
        {
            return propertyName switch
            {
                "Aligned" =>
                    (bool)owner.Aligned,
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
        dynamic owner = ownerObject;

        try
        {
            return propertyName switch
            {
                "Rotation" =>
                    (double)owner.Rotation,
                "X" =>
                    (double)owner.X,
                "Y" =>
                    (double)owner.Y,
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
        object? collectionObject)
    {
        if (collectionObject == null)
        {
            return 0;
        }

        dynamic collection = collectionObject;

        try
        {
            return (int)collection.Count;
        }
        catch
        {
            return 0;
        }
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

    private static JsonSerializerOptions CreateJsonOptions()
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
