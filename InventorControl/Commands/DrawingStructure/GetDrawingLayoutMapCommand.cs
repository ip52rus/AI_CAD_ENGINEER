using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public class GetDrawingLayoutMapCommand
    : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public GetDrawingLayoutMapCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name =>
        "get_drawing_layout_map";

    public string Execute(
        JsonElement root)
    {
        DrawingDocument? drawingDocument =
            DrawingLayoutMapReadSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return DrawingLayoutMapReadSupport.CreateError(
                documentError ??
                "Unable to get active drawing document.");
        }

        Sheet? sheet =
            DrawingLayoutMapReadSupport.ResolveSheet(
                drawingDocument,
                root,
                out bool usedActiveSheet,
                out string? sheetError);

        if (sheet == null)
        {
            return DrawingLayoutMapReadSupport.CreateError(
                sheetError ??
                "Unable to resolve drawing sheet.");
        }

        object layoutMap =
            DrawingLayoutMapReadSupport.ReadLayoutMap(
                drawingDocument,
                sheet,
                usedActiveSheet);

        return DrawingLayoutMapReadSupport.CreateSuccess(
            layoutMap);
    }
}

internal static class DrawingLayoutMapReadSupport
{
    public static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(inventor);

        error = null;

        Document? activeDocument =
            inventor.ActiveDocument;

        if (activeDocument == null)
        {
            error =
                "Inventor has no active document.";

            return null;
        }

        if (activeDocument.DocumentType !=
            DocumentTypeEnum.kDrawingDocumentObject)
        {
            error =
                "Active Inventor document is not a drawing.";

            return null;
        }

        return (DrawingDocument)activeDocument;
    }

    public static Sheet? ResolveSheet(
        DrawingDocument drawingDocument,
        JsonElement root,
        out bool usedActiveSheet,
        out string? error)
    {
        ArgumentNullException.ThrowIfNull(drawingDocument);

        usedActiveSheet = false;
        error = null;

        if (!root.TryGetProperty(
                "sheetName",
                out JsonElement sheetNameElement))
        {
            usedActiveSheet = true;
            return drawingDocument.ActiveSheet;
        }

        if (sheetNameElement.ValueKind !=
            JsonValueKind.String)
        {
            error =
                "Field \"sheetName\" must be a string when provided.";

            return null;
        }

        string sheetName =
            sheetNameElement.GetString()?.Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(sheetName))
        {
            error =
                "Field \"sheetName\" must not be empty when provided.";

            return null;
        }

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

        error =
            $"Sheet \"{sheetName}\" was not found.";

        return null;
    }

    public static object ReadLayoutMap(
        DrawingDocument drawingDocument,
        Sheet sheet,
        bool usedActiveSheet)
    {
        List<object> diagnostics = new();

        return new
        {
            document =
                new
                {
                    displayName =
                        SafeString(
                            () => drawingDocument.DisplayName,
                            diagnostics,
                            "DrawingDocument.DisplayName"),
                    fullFileName =
                        SafeString(
                            () => drawingDocument.FullFileName,
                            diagnostics,
                            "DrawingDocument.FullFileName"),
                    dirty =
                        SafeNullableBoolean(
                            () => drawingDocument.Dirty,
                            diagnostics,
                            "DrawingDocument.Dirty")
                },

            sheet =
                ReadSheet(
                    sheet,
                    usedActiveSheet,
                    diagnostics),

            reservedAreas =
                ReadReservedAreas(
                    drawingDocument,
                    sheet),

            drawingViews =
                ReadDrawingViews(
                    drawingDocument,
                    sheet),

            dimensions =
                ReadDimensions(
                    drawingDocument,
                    sheet),

            notes =
                ReadNotes(
                    drawingDocument,
                    sheet),

            tables =
                ReadTables(
                    drawingDocument,
                    sheet),

            centerAnnotations =
                ReadCenterAnnotations(
                    drawingDocument,
                    sheet),

            symbols =
                ReadSymbols(
                    drawingDocument,
                    sheet),

            diagnostics
        };
    }

    private static object ReadSheet(
        Sheet sheet,
        bool usedActiveSheet,
        List<object> diagnostics)
    {
        return new
        {
            name =
                SafeString(
                    () => sheet.Name,
                    diagnostics,
                    "Sheet.Name"),
            usedActiveSheet,
            width =
                SafeNullableDouble(
                    () => sheet.Width,
                    diagnostics,
                    "Sheet.Width"),
            height =
                SafeNullableDouble(
                    () => sheet.Height,
                    diagnostics,
                    "Sheet.Height"),
            sizeRaw =
                SafeString(
                    () => sheet.Size.ToString(),
                    diagnostics,
                    "Sheet.Size"),
            orientationRaw =
                SafeString(
                    () => sheet.Orientation.ToString(),
                    diagnostics,
                    "Sheet.Orientation"),
            units =
                new
                {
                    apiLengthUnit =
                        "centimeter",
                    basis =
                        "Inventor drawing API sheet coordinates are returned in database length units observed as centimeters."
                },
            coordinateSystem =
                new
                {
                    originConvention =
                        "sheet lower-left",
                    xDirection =
                        "right",
                    yDirection =
                        "up",
                    basis =
                        "Inventor drawing sheet coordinates used by Point2d placement APIs."
                }
        };
    }

    private static object ReadReservedAreas(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        return new
        {
            border =
                ReadReservedObject(
                    drawingDocument,
                    sheet,
                    "border",
                    () => sheet.Border),

            titleBlock =
                ReadReservedObject(
                    drawingDocument,
                    sheet,
                    "titleBlock",
                    () => sheet.TitleBlock),

            revisionTables =
                ReadTableCollection(
                    drawingDocument,
                    sheet,
                    "revisionTables",
                    () => sheet.RevisionTables),

            otherTables =
                new
                {
                    holeTables =
                        ReadTableCollection(
                            drawingDocument,
                            sheet,
                            "holeTables",
                            () => sheet.HoleTables),
                    partsLists =
                        ReadTableCollection(
                            drawingDocument,
                            sheet,
                            "partsLists",
                            () => sheet.PartsLists),
                    customTables =
                        ReadTableCollection(
                            drawingDocument,
                            sheet,
                            "customTables",
                            () => sheet.CustomTables)
                }
        };
    }

    private static object ReadReservedObject(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string objectClass,
        Func<object?> reader)
    {
        List<object> diagnostics = new();

        object? item =
            SafeObject(
                reader,
                diagnostics,
                objectClass);

        if (item == null)
        {
            return new
            {
                available =
                    false,
                reason =
                    $"{objectClass} is not present or could not be read.",
                layout =
                    CreateLayout(
                        null,
                        null,
                        null,
                        null),
                diagnostics
            };
        }

        object? position =
            ReadPointProperty(
                item,
                "Position",
                diagnostics);

        BoundsFact? bounds =
            ReadBoundsProperty(
                item,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                item,
                diagnostics);

        return new
        {
            available =
                true,
            objectClass,
            name =
                SafeString(
                    item,
                    "Name",
                    diagnostics),
            definition =
                ReadNamedObject(
                    SafeObjectProperty(
                        item,
                        "Definition",
                        diagnostics),
                    diagnostics),
            position,
            bounds,
            layout =
                CreateLayout(
                    position,
                    bounds,
                    referenceKey,
                    new
                    {
                        sheet =
                            sheet.Name
                    }),
            referenceKey,
            diagnostics
        };
    }

    private static object ReadDrawingViews(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> diagnostics = new();
        List<object> items = new();
        int? rawCount = null;

        DrawingViews? views =
            SafeObject(
                () => sheet.DrawingViews,
                diagnostics,
                "Sheet.DrawingViews");

        if (views != null)
        {
            rawCount =
                SafeNullableInt32(
                    () => views.Count,
                    diagnostics,
                    "DrawingViews.Count");

            try
            {
                int index = 0;

                foreach (DrawingView view in views)
                {
                    index++;
                    items.Add(
                        ReadDrawingView(
                            drawingDocument,
                            sheet,
                            view,
                            index));
                }
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    "DrawingViews.Enumeration",
                    exception);
            }
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadDrawingView(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingView view,
        int index)
    {
        List<object> diagnostics = new();

        PointFact? center =
            ReadPoint2d(
                SafeObject(
                    () => view.Position,
                    diagnostics,
                    "DrawingView.Position"),
                diagnostics,
                "DrawingView.Position");

        double? width =
            SafeNullableDouble(
                () => view.Width,
                diagnostics,
                "DrawingView.Width");

        double? height =
            SafeNullableDouble(
                () => view.Height,
                diagnostics,
                "DrawingView.Height");

        BoundsFact? computedBounds =
            ComputeCenteredBounds(
                center,
                width,
                height);

        BoundsFact? nativeBounds =
            ReadBoundsProperty(
                view,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                view,
                diagnostics);

        object? parentView =
            ReadNamedObject(
                SafeObjectProperty(
                    view,
                    "ParentView",
                    diagnostics),
                diagnostics);

        object? label =
            ReadViewLabel(
                view,
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            name =
                SafeString(
                    () => view.Name,
                    diagnostics,
                    "DrawingView.Name"),
            objectTypeRaw =
                SafeString(
                    () => view.Type.ToString(),
                    diagnostics,
                    "DrawingView.Type"),
            objectType =
                ReadEnumName(
                    view.Type),
            referenceKey,
            position =
                center,
            center,
            width,
            height,
            computedBounds,
            nativeRangeBox =
                nativeBounds,
            scale =
                SafeNullableDouble(
                    () => view.Scale,
                    diagnostics,
                    "DrawingView.Scale"),
            scaleString =
                SafeString(
                    view,
                    "ScaleString",
                    diagnostics),
            orientationRaw =
                SafeString(
                    view,
                    "Camera.ViewOrientationType",
                    diagnostics),
            viewTypeRaw =
                SafeString(
                    () => view.ViewType.ToString(),
                    diagnostics,
                    "DrawingView.ViewType"),
            viewType =
                ReadEnumName(
                    view.ViewType),
            rotation =
                SafeNullableDouble(
                    view,
                    "Rotation",
                    diagnostics),
            aligned =
                SafeNullableBoolean(
                    view,
                    "Aligned",
                    diagnostics),
            alignmentTypeRaw =
                SafeString(
                    view,
                    "AlignmentType",
                    diagnostics),
            parentView,
            projectedRelationship =
                new
                {
                    parentView,
                    aligned =
                        SafeNullableBoolean(
                            view,
                            "Aligned",
                            diagnostics),
                    alignmentTypeRaw =
                        SafeString(
                            view,
                            "AlignmentType",
                            diagnostics)
                },
            labelVisible =
                SafeNullableBoolean(
                    view,
                    "ShowLabel",
                    diagnostics),
            label,
            referencedDocument =
                ReadReferencedDocument(
                    view,
                    diagnostics),
            layout =
                CreateLayout(
                    center,
                    computedBounds,
                    referenceKey,
                    parentView),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object? ReadViewLabel(
        DrawingView view,
        List<object> diagnostics)
    {
        object? label =
            SafeObjectProperty(
                view,
                "Label",
                diagnostics);

        if (label == null)
        {
            return new
            {
                available =
                    false,
                reason =
                    "DrawingView.Label was not readable."
            };
        }

        return new
        {
            available =
                true,
            text =
                SafeString(
                    label,
                    "Text",
                    diagnostics),
            formattedText =
                SafeString(
                    label,
                    "FormattedText",
                    diagnostics),
            position =
                ReadPointProperty(
                    label,
                    "Position",
                    diagnostics),
            bounds =
                ReadBoundsProperty(
                    label,
                    "RangeBox",
                    diagnostics)
        };
    }

    private static object ReadDimensions(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> diagnostics = new();
        List<object> items = new();
        int? rawCount = null;

        GeneralDimensions? dimensions =
            SafeObject(
                () => sheet.DrawingDimensions.GeneralDimensions,
                diagnostics,
                "Sheet.DrawingDimensions.GeneralDimensions");

        if (dimensions != null)
        {
            rawCount =
                SafeNullableInt32(
                    () => dimensions.Count,
                    diagnostics,
                    "GeneralDimensions.Count");

            for (int index = 1;
                 rawCount.HasValue && index <= rawCount.Value;
                 index++)
            {
                try
                {
                    GeneralDimension dimension =
                        dimensions[index];

                    items.Add(
                        ReadDimension(
                            drawingDocument,
                            sheet,
                            dimension,
                            index));
                }
                catch (Exception exception)
                {
                    AddDiagnostic(
                        diagnostics,
                        $"GeneralDimensions[{index}]",
                        exception);
                }
            }
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadDimension(
        DrawingDocument drawingDocument,
        Sheet sheet,
        GeneralDimension dimension,
        int index)
    {
        List<object> diagnostics = new();

        object? dimensionText =
            SafeObjectProperty(
                dimension,
                "Text",
                diagnostics);

        PointFact? textOrigin =
            ReadPointProperty(
                dimensionText,
                "Origin",
                diagnostics);

        BoundsFact? textBounds =
            ReadBoundsProperty(
                dimensionText,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                dimension,
                diagnostics);

        object? parentView =
            ReadNamedObject(
                SafeObjectProperty(
                    dimension,
                    "ParentView",
                    diagnostics),
                diagnostics);

        object? dimensionLine =
            ReadLineLikeObject(
                SafeObjectProperty(
                    dimension,
                    "DimensionLine",
                    diagnostics),
                diagnostics);

        object? extensionLineOne =
            ReadLineLikeObject(
                SafeObjectProperty(
                    dimension,
                    "ExtensionLineOne",
                    diagnostics),
                diagnostics);

        object? extensionLineTwo =
            ReadLineLikeObject(
                SafeObjectProperty(
                    dimension,
                    "ExtensionLineTwo",
                    diagnostics),
                diagnostics);

        object? intentOne =
            ReadGeometryIntent(
                SafeObjectProperty(
                    dimension,
                    "IntentOne",
                    diagnostics),
                diagnostics);

        object? intentTwo =
            ReadGeometryIntent(
                SafeObjectProperty(
                    dimension,
                    "IntentTwo",
                    diagnostics),
                diagnostics);

        object? geometryIntent =
            ReadGeometryIntent(
                SafeObjectProperty(
                    dimension,
                    "GeometryIntent",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                SafeString(
                    () => dimension.Type.ToString(),
                    diagnostics,
                    "GeneralDimension.Type"),
            objectType =
                ReadEnumName(
                    dimension.Type),
            dimensionSubtypeRaw =
                SafeString(
                    dimension,
                    "DimensionType",
                    diagnostics),
            generalDimensionTypeRaw =
                SafeString(
                    () => dimension.GeneralDimensionType.ToString(),
                    diagnostics,
                    "GeneralDimension.GeneralDimensionType"),
            generalDimensionType =
                ReadEnumName(
                    dimension.GeneralDimensionType),
            displayedText =
                SafeString(
                    dimensionText,
                    "Text",
                    diagnostics),
            formattedText =
                SafeString(
                    dimensionText,
                    "FormattedText",
                    diagnostics),
            modelValue =
                SafeNullableDouble(
                    () => dimension.ModelValue,
                    diagnostics,
                    "GeneralDimension.ModelValue"),
            precision =
                SafeNullableInt32(
                    () => dimension.Precision,
                    diagnostics,
                    "GeneralDimension.Precision"),
            attached =
                SafeNullableBoolean(
                    () => dimension.Attached,
                    diagnostics,
                    "GeneralDimension.Attached"),
            retrieved =
                SafeNullableBoolean(
                    () => dimension.Retrieved,
                    diagnostics,
                    "GeneralDimension.Retrieved"),
            hiddenValue =
                SafeNullableBoolean(
                    () => dimension.HideValue,
                    diagnostics,
                    "GeneralDimension.HideValue"),
            modelValueOverridden =
                SafeNullableBoolean(
                    () => dimension.ModelValueOverridden,
                    diagnostics,
                    "GeneralDimension.ModelValueOverridden"),
            textOrigin,
            textBounds,
            dimensionLine,
            extensionLineOne,
            extensionLineTwo,
            arrowPoints =
                ExtractArrowPoints(
                    dimensionLine),
            geometryIntents =
                new
                {
                    intentOne,
                    intentTwo,
                    geometryIntent
                },
            attachedEntities =
                new
                {
                    attachedEntity =
                        ReadEntityMetadata(
                            SafeObjectProperty(
                                dimension,
                                "AttachedEntity",
                                diagnostics),
                            diagnostics)
                },
            parentView,
            orientation =
                new
                {
                    dimensionLineDirection =
                        ReadVectorFromLineLike(
                            dimensionLine),
                    alignmentRaw =
                        SafeString(
                            dimension,
                            "Alignment",
                            diagnostics),
                    orientationRaw =
                        SafeString(
                            dimension,
                            "Orientation",
                            diagnostics)
                },
            layout =
                CreateLayout(
                    textOrigin,
                    textBounds,
                    referenceKey,
                    parentView),
            referenceKey,
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadNotes(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> diagnostics = new();

        DrawingNotes? drawingNotes =
            SafeObject(
                () => sheet.DrawingNotes,
                diagnostics,
                "Sheet.DrawingNotes");

        return new
        {
            generalNotes =
                ReadNoteCollection(
                    drawingDocument,
                    sheet,
                    "generalNotes",
                    () => drawingNotes?.GeneralNotes,
                    note => ReadTextNote(
                        drawingDocument,
                        sheet,
                        note,
                        "general_note")),

            leaderNotes =
                ReadNoteCollection(
                    drawingDocument,
                    sheet,
                    "leaderNotes",
                    () => drawingNotes?.LeaderNotes,
                    note => ReadTextNote(
                        drawingDocument,
                        sheet,
                        note,
                        "leader_note")),

            holeThreadNotes =
                ReadNoteCollection(
                    drawingDocument,
                    sheet,
                    "holeThreadNotes",
                    () => drawingNotes?.HoleThreadNotes,
                    note => ReadFeatureNote(
                        drawingDocument,
                        sheet,
                        note,
                        "hole_thread_note")),

            chamferNotes =
                ReadNoteCollection(
                    drawingDocument,
                    sheet,
                    "chamferNotes",
                    () => drawingNotes?.ChamferNotes,
                    note => ReadFeatureNote(
                        drawingDocument,
                        sheet,
                        note,
                        "chamfer_note")),

            bendNotes =
                ReadNoteCollection(
                    drawingDocument,
                    sheet,
                    "bendNotes",
                    () => drawingNotes?.BendNotes,
                    note => ReadFeatureNote(
                        drawingDocument,
                        sheet,
                        note,
                        "bend_note")),

            punchNotes =
                ReadNoteCollection(
                    drawingDocument,
                    sheet,
                    "punchNotes",
                    () => drawingNotes?.PunchNotes,
                    note => ReadFeatureNote(
                        drawingDocument,
                        sheet,
                        note,
                        "punch_note")),

            diagnostics
        };
    }

    private static object ReadNoteCollection(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string collectionName,
        Func<object?> collectionReader,
        Func<object, object> itemReader)
    {
        List<object> diagnostics = new();
        List<object> items = new();
        int? rawCount = null;

        object? collection =
            SafeObject(
                collectionReader,
                diagnostics,
                collectionName);

        if (collection != null)
        {
            rawCount =
                SafeNullableInt32(
                    collection,
                    "Count",
                    diagnostics);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    try
                    {
                        object item =
                            GetIndexedItem(
                                collection,
                                index);

                        items.Add(
                            AddIndexEnvelope(
                                itemReader(
                                    item),
                                index));
                    }
                    catch (Exception exception)
                    {
                        AddDiagnostic(
                            diagnostics,
                            $"{collectionName}[{index}]",
                            exception);
                    }
                }
            }
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadTextNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        object note,
        string noteType)
    {
        List<object> diagnostics = new();

        PointFact? position =
            ReadPointProperty(
                note,
                "Position",
                diagnostics);

        BoundsFact? bounds =
            ReadBoundsProperty(
                note,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                note,
                diagnostics);

        object? parentView =
            ReadNamedObject(
                SafeObjectProperty(
                    note,
                    "ParentView",
                    diagnostics),
                diagnostics);

        return new
        {
            noteType,
            objectTypeRaw =
                SafeString(
                    note,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    note,
                    diagnostics),
            referenceKey,
            text =
                SafeString(
                    note,
                    "Text",
                    diagnostics),
            formattedText =
                SafeString(
                    note,
                    "FormattedText",
                    diagnostics),
            position,
            bounds,
            leader =
                ReadLeader(
                    SafeObjectProperty(
                        note,
                        "Leader",
                        diagnostics),
                    diagnostics),
            attachment =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        note,
                        "AttachedEntity",
                        diagnostics)
                    ?? SafeObjectProperty(
                        note,
                        "_AttachedEntity",
                        diagnostics),
                    diagnostics),
            parentView,
            layout =
                CreateLayout(
                    position,
                    bounds,
                    referenceKey,
                    parentView),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadFeatureNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        object note,
        string noteType)
    {
        List<object> diagnostics = new();

        object? textObject =
            SafeObjectProperty(
                note,
                "Text",
                diagnostics);

        PointFact? textOrigin =
            ReadPointProperty(
                textObject,
                "Origin",
                diagnostics);

        BoundsFact? textBounds =
            ReadBoundsProperty(
                textObject,
                "RangeBox",
                diagnostics)
            ?? ReadBoundsProperty(
                note,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                note,
                diagnostics);

        object? parentView =
            ReadNamedObject(
                SafeObjectProperty(
                    note,
                    "ParentView",
                    diagnostics),
                diagnostics);

        return new
        {
            noteType,
            objectTypeRaw =
                SafeString(
                    note,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    note,
                    diagnostics),
            referenceKey,
            text =
                SafeString(
                    textObject,
                    "Text",
                    diagnostics)
                ?? SafeString(
                    note,
                    "Text",
                    diagnostics),
            formattedText =
                SafeString(
                    textObject,
                    "FormattedText",
                    diagnostics)
                ?? SafeString(
                    note,
                    "FormattedText",
                    diagnostics),
            textOrigin,
            bounds =
                textBounds,
            position =
                ReadPointProperty(
                    note,
                    "Position",
                    diagnostics)
                ?? textOrigin,
            leader =
                ReadLeader(
                    SafeObjectProperty(
                        note,
                        "Leader",
                        diagnostics),
                    diagnostics),
            attachment =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        note,
                        "AttachedEntity",
                        diagnostics)
                    ?? SafeObjectProperty(
                        note,
                        "_AttachedEntity",
                        diagnostics),
                    diagnostics),
            attached =
                SafeNullableBoolean(
                    note,
                    "Attached",
                    diagnostics),
            parentView,
            layout =
                CreateLayout(
                    textOrigin,
                    textBounds,
                    referenceKey,
                    parentView),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadTables(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        return new
        {
            holeTables =
                ReadTableCollection(
                    drawingDocument,
                    sheet,
                    "holeTables",
                    () => sheet.HoleTables),
            partsLists =
                ReadTableCollection(
                    drawingDocument,
                    sheet,
                    "partsLists",
                    () => sheet.PartsLists),
            revisionTables =
                ReadTableCollection(
                    drawingDocument,
                    sheet,
                    "revisionTables",
                    () => sheet.RevisionTables),
            customTables =
                ReadTableCollection(
                    drawingDocument,
                    sheet,
                    "customTables",
                    () => sheet.CustomTables)
        };
    }

    private static object ReadTableCollection(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string collectionName,
        Func<object?> collectionReader)
    {
        return ReadGenericCollection(
            drawingDocument,
            sheet,
            collectionName,
            collectionReader,
            (item, index) => ReadTable(
                drawingDocument,
                sheet,
                item,
                collectionName,
                index));
    }

    private static object ReadTable(
        DrawingDocument drawingDocument,
        Sheet sheet,
        object table,
        string tableType,
        int index)
    {
        List<object> diagnostics = new();

        PointFact? position =
            ReadPointProperty(
                table,
                "Position",
                diagnostics);

        BoundsFact? bounds =
            ReadBoundsProperty(
                table,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                table,
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            tableType,
            objectTypeRaw =
                SafeString(
                    table,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    table,
                    diagnostics),
            title =
                SafeString(
                    table,
                    "Title",
                    diagnostics),
            position,
            bounds,
            width =
                bounds?.width,
            height =
                bounds?.height,
            rowCount =
                ReadFirstAvailableCount(
                    table,
                    diagnostics,
                    "Rows",
                    "TableRows",
                    "HoleTableRows",
                    "PartsListRows",
                    "RevisionTableRows"),
            columnCount =
                ReadFirstAvailableCount(
                    table,
                    diagnostics,
                    "Columns",
                    "TableColumns",
                    "HoleTableColumns",
                    "PartsListColumns",
                    "RevisionTableColumns"),
            attachment =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        table,
                        "AttachedEntity",
                        diagnostics),
                    diagnostics),
            layout =
                CreateLayout(
                    position,
                    bounds,
                    referenceKey,
                    new
                    {
                        sheet =
                            sheet.Name
                    }),
            referenceKey,
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadCenterAnnotations(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        return new
        {
            centerMarks =
                ReadGenericCollection(
                    drawingDocument,
                    sheet,
                    "centerMarks",
                    () => sheet.Centermarks,
                    (item, index) => ReadCenterMark(
                        drawingDocument,
                        sheet,
                        item,
                        index)),
            centerlines =
                ReadGenericCollection(
                    drawingDocument,
                    sheet,
                    "centerlines",
                    () => sheet.Centerlines,
                    (item, index) => ReadCenterline(
                        drawingDocument,
                        sheet,
                        item,
                        index))
        };
    }

    private static object ReadCenterMark(
        DrawingDocument drawingDocument,
        Sheet sheet,
        object centerMark,
        int index)
    {
        List<object> diagnostics = new();

        PointFact? position =
            ReadPointProperty(
                centerMark,
                "Position",
                diagnostics);

        BoundsFact? bounds =
            ReadBoundsProperty(
                centerMark,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                centerMark,
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                SafeString(
                    centerMark,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    centerMark,
                    diagnostics),
            centerPoint =
                position,
            position,
            bounds,
            visible =
                SafeNullableBoolean(
                    centerMark,
                    "Visible",
                    diagnostics),
            attached =
                SafeNullableBoolean(
                    centerMark,
                    "Attached",
                    diagnostics),
            centermarkTypeRaw =
                SafeString(
                    centerMark,
                    "CentermarkType",
                    diagnostics),
            extensionLinesVisible =
                SafeNullableBoolean(
                    centerMark,
                    "ExtensionLinesVisible",
                    diagnostics),
            extensionPoints =
                new
                {
                    one =
                        ReadPointProperty(
                            centerMark,
                            "ExtensionPointOne",
                            diagnostics),
                    two =
                        ReadPointProperty(
                            centerMark,
                            "ExtensionPointTwo",
                            diagnostics),
                    three =
                        ReadPointProperty(
                            centerMark,
                            "ExtensionPointThree",
                            diagnostics),
                    four =
                        ReadPointProperty(
                            centerMark,
                            "ExtensionPointFour",
                            diagnostics)
                },
            attachedEntity =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        centerMark,
                        "AttachedEntity",
                        diagnostics),
                    diagnostics),
            layout =
                CreateLayout(
                    position,
                    bounds,
                    referenceKey,
                    null),
            referenceKey,
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadCenterline(
        DrawingDocument drawingDocument,
        Sheet sheet,
        object centerline,
        int index)
    {
        List<object> diagnostics = new();

        PointFact? startPoint =
            ReadPointProperty(
                centerline,
                "StartPoint",
                diagnostics);

        PointFact? endPoint =
            ReadPointProperty(
                centerline,
                "EndPoint",
                diagnostics);

        BoundsFact? bounds =
            ReadBoundsProperty(
                centerline,
                "RangeBox",
                diagnostics)
            ?? ComputeBoundsFromPoints(
                new[] { startPoint, endPoint });

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                centerline,
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                SafeString(
                    centerline,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    centerline,
                    diagnostics),
            centerlineTypeRaw =
                SafeString(
                    centerline,
                    "CenterlineType",
                    diagnostics),
            startPoint,
            endPoint,
            fitPoints =
                ReadPointCollection(
                    SafeObjectProperty(
                        centerline,
                        "FitPoints",
                        diagnostics),
                    diagnostics),
            bounds,
            visible =
                SafeNullableBoolean(
                    centerline,
                    "Visible",
                    diagnostics),
            attached =
                SafeNullableBoolean(
                    centerline,
                    "Attached",
                    diagnostics),
            attachedEntity =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        centerline,
                        "AttachedEntity",
                        diagnostics),
                    diagnostics),
            layout =
                CreateLayout(
                    startPoint,
                    bounds,
                    referenceKey,
                    null),
            referenceKey,
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadSymbols(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        return new
        {
            balloons =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "balloons",
                    () => sheet.Balloons),
            featureControlFrames =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "featureControlFrames",
                    () => sheet.FeatureControlFrames),
            surfaceTextureSymbols =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "surfaceTextureSymbols",
                    () => sheet.SurfaceTextureSymbols),
            weldingSymbols =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "weldingSymbols",
                    () => sheet.WeldingSymbols),
            revisionClouds =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "revisionClouds",
                    () => sheet.RevisionClouds),
            edgeSymbols =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "edgeSymbols",
                    () => sheet.EdgeSymbols),
            transitionSymbols =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "transitionSymbols",
                    () => sheet.TransitionSymbols),
            sketchedSymbols =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "sketchedSymbols",
                    () => sheet.SketchedSymbols),
            datumIdentifierSymbols =
                ReadSymbolCollection(
                    drawingDocument,
                    sheet,
                    "datumIdentifierSymbols",
                    () => SafeObjectProperty(
                        sheet,
                        "DatumIdentifierSymbols",
                        new List<object>()))
        };
    }

    private static object ReadSymbolCollection(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string collectionName,
        Func<object?> collectionReader)
    {
        return ReadGenericCollection(
            drawingDocument,
            sheet,
            collectionName,
            collectionReader,
            (item, index) => ReadSymbol(
                drawingDocument,
                sheet,
                item,
                collectionName,
                index));
    }

    private static object ReadSymbol(
        DrawingDocument drawingDocument,
        Sheet sheet,
        object symbol,
        string symbolType,
        int index)
    {
        List<object> diagnostics = new();

        PointFact? position =
            ReadPointProperty(
                symbol,
                "Position",
                diagnostics);

        BoundsFact? bounds =
            ReadBoundsProperty(
                symbol,
                "RangeBox",
                diagnostics);

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                symbol,
                diagnostics);

        object? parentView =
            ReadNamedObject(
                SafeObjectProperty(
                    symbol,
                    "ParentView",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            symbolType,
            objectTypeRaw =
                SafeString(
                    symbol,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    symbol,
                    diagnostics),
            text =
                SafeString(
                    symbol,
                    "Text",
                    diagnostics)
                ?? SafeString(
                    symbol,
                    "FormattedText",
                    diagnostics),
            position,
            bounds,
            leader =
                ReadLeader(
                    SafeObjectProperty(
                        symbol,
                        "Leader",
                        diagnostics),
                    diagnostics),
            attachment =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        symbol,
                        "AttachedEntity",
                        diagnostics)
                    ?? SafeObjectProperty(
                        symbol,
                        "_AttachedEntity",
                        diagnostics),
                    diagnostics),
            parentView,
            layout =
                CreateLayout(
                    position,
                    bounds,
                    referenceKey,
                    parentView),
            referenceKey,
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadGenericCollection(
        DrawingDocument drawingDocument,
        Sheet sheet,
        string collectionName,
        Func<object?> collectionReader,
        Func<object, int, object> itemReader)
    {
        List<object> diagnostics = new();
        List<object> items = new();
        int? rawCount = null;

        object? collection =
            SafeObject(
                collectionReader,
                diagnostics,
                collectionName);

        if (collection != null)
        {
            rawCount =
                SafeNullableInt32(
                    collection,
                    "Count",
                    diagnostics);

            if (rawCount.HasValue)
            {
                for (int index = 1;
                     index <= rawCount.Value;
                     index++)
                {
                    try
                    {
                        items.Add(
                            itemReader(
                                GetIndexedItem(
                                    collection,
                                    index),
                                index));
                    }
                    catch (Exception exception)
                    {
                        AddDiagnostic(
                            diagnostics,
                            $"{collectionName}[{index}]",
                            exception);
                    }
                }
            }
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object? ReadLineLikeObject(
        object? line,
        List<object> diagnostics)
    {
        if (line == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                SafeString(
                    line,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    line,
                    diagnostics),
            startPoint =
                ReadPointProperty(
                    line,
                    "StartPoint",
                    diagnostics),
            endPoint =
                ReadPointProperty(
                    line,
                    "EndPoint",
                    diagnostics),
            midPoint =
                ReadPointProperty(
                    line,
                    "MidPoint",
                    diagnostics),
            origin =
                ReadPointProperty(
                    line,
                    "Origin",
                    diagnostics),
            position =
                ReadPointProperty(
                    line,
                    "Position",
                    diagnostics),
            direction =
                ReadVectorProperty(
                    line,
                    "Direction",
                    diagnostics)
        };
    }

    private static object? ExtractArrowPoints(
        object? lineLike)
    {
        if (lineLike == null)
        {
            return null;
        }

        dynamic line =
            lineLike;

        try
        {
            return new
            {
                first =
                    line.startPoint,
                second =
                    line.endPoint,
                basis =
                    "DimensionLine.StartPoint and DimensionLine.EndPoint"
            };
        }
        catch
        {
            return null;
        }
    }

    private static object? ReadVectorFromLineLike(
        object? lineLike)
    {
        if (lineLike == null)
        {
            return null;
        }

        dynamic line =
            lineLike;

        try
        {
            return line.direction;
        }
        catch
        {
            return null;
        }
    }

    private static object? ReadGeometryIntent(
        object? intent,
        List<object> diagnostics)
    {
        if (intent == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                SafeString(
                    intent,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    intent,
                    diagnostics),
            intentTypeRaw =
                SafeString(
                    intent,
                    "IntentType",
                    diagnostics),
            pointOnSheet =
                ReadPointProperty(
                    intent,
                    "PointOnSheet",
                    diagnostics),
            geometry =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        intent,
                        "Geometry",
                        diagnostics),
                    diagnostics)
        };
    }

    private static object? ReadEntityMetadata(
        object? entity,
        List<object> diagnostics)
    {
        if (entity == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                SafeString(
                    entity,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    entity,
                    diagnostics),
            name =
                SafeString(
                    entity,
                    "Name",
                    diagnostics),
            parentView =
                ReadNamedObject(
                    SafeObjectProperty(
                        entity,
                        "ParentView",
                        diagnostics),
                    diagnostics),
            rangeBox =
                ReadBoundsProperty(
                    entity,
                    "RangeBox",
                    diagnostics),
            runtimeType =
                entity.GetType().FullName
        };
    }

    private static object? ReadLeader(
        object? leader,
        List<object> diagnostics)
    {
        if (leader == null)
        {
            return null;
        }

        List<PointFact> nodePoints = new();

        object? allNodes =
            SafeObjectProperty(
                leader,
                "AllNodes",
                diagnostics);

        List<object> nodes =
            ReadLeaderNodes(
                allNodes,
                diagnostics,
                nodePoints);

        return new
        {
            objectTypeRaw =
                SafeString(
                    leader,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    leader,
                    diagnostics),
            hasRootNode =
                SafeNullableBoolean(
                    leader,
                    "HasRootNode",
                    diagnostics),
            rootNode =
                ReadLeaderNode(
                    SafeObjectProperty(
                        leader,
                        "RootNode",
                        diagnostics),
                    diagnostics),
            nodes,
            segments =
                CreateSegments(
                    nodePoints),
            attachmentPoint =
                nodePoints.Count > 0
                    ? nodePoints[^1]
                    : null
        };
    }

    private static List<object> ReadLeaderNodes(
        object? nodesObject,
        List<object> diagnostics,
        List<PointFact> nodePoints)
    {
        List<object> nodes = new();

        if (nodesObject == null)
        {
            return nodes;
        }

        int? count =
            SafeNullableInt32(
                nodesObject,
                "Count",
                diagnostics);

        if (!count.HasValue)
        {
            return nodes;
        }

        for (int index = 1;
             index <= count.Value;
             index++)
        {
            try
            {
                object node =
                    GetIndexedItem(
                        nodesObject,
                        index);

                object? nodeFact =
                    ReadLeaderNode(
                        node,
                        diagnostics);

                PointFact? point =
                    ReadPointProperty(
                        node,
                        "Position",
                        diagnostics);

                if (point != null)
                {
                    nodePoints.Add(
                        point);
                }

                nodes.Add(
                    new
                    {
                        index,
                        node =
                            nodeFact
                    });
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    $"Leader.AllNodes[{index}]",
                    exception);
            }
        }

        return nodes;
    }

    private static object? ReadLeaderNode(
        object? node,
        List<object> diagnostics)
    {
        if (node == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                SafeString(
                    node,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    node,
                    diagnostics),
            position =
                ReadPointProperty(
                    node,
                    "Position",
                    diagnostics),
            attachedEntity =
                ReadEntityMetadata(
                    SafeObjectProperty(
                        node,
                        "AttachedEntity",
                        diagnostics),
                    diagnostics)
        };
    }

    private static List<object> CreateSegments(
        IReadOnlyList<PointFact> points)
    {
        List<object> segments = new();

        for (int index = 1;
             index < points.Count;
             index++)
        {
            segments.Add(
                new
                {
                    index,
                    startPoint =
                        points[index - 1],
                    endPoint =
                        points[index]
                });
        }

        return segments;
    }

    private static object? ReadPointCollection(
        object? pointsObject,
        List<object> diagnostics)
    {
        if (pointsObject == null)
        {
            return null;
        }

        int? count =
            SafeNullableInt32(
                pointsObject,
                "Count",
                diagnostics);

        List<object> points = new();

        if (count.HasValue)
        {
            for (int index = 1;
                 index <= count.Value;
                 index++)
            {
                try
                {
                    points.Add(
                        new
                        {
                            index,
                            point =
                                ReadPoint2d(
                                    GetIndexedItem(
                                        pointsObject,
                                        index),
                                    diagnostics,
                                    $"PointCollection[{index}]")
                        });
                }
                catch (Exception exception)
                {
                    AddDiagnostic(
                        diagnostics,
                        $"PointCollection[{index}]",
                        exception);
                }
            }
        }

        return new
        {
            count,
            items =
                points
        };
    }

    private static object? ReadReferencedDocument(
        DrawingView view,
        List<object> diagnostics)
    {
        object? descriptor =
            SafeObjectProperty(
                view,
                "ReferencedDocumentDescriptor",
                diagnostics);

        if (descriptor == null)
        {
            return null;
        }

        object? document =
            SafeObjectProperty(
                descriptor,
                "ReferencedDocument",
                diagnostics);

        return new
        {
            fullDocumentName =
                SafeString(
                    descriptor,
                    "FullDocumentName",
                    diagnostics),
            referencedDocumentTypeRaw =
                SafeString(
                    descriptor,
                    "ReferencedDocumentType",
                    diagnostics),
            document =
                document == null
                    ? null
                    : new
                    {
                        displayName =
                            SafeString(
                                document,
                                "DisplayName",
                                diagnostics),
                        fullFileName =
                            SafeString(
                                document,
                                "FullFileName",
                                diagnostics),
                        documentTypeRaw =
                            SafeString(
                                document,
                                "DocumentType",
                                diagnostics)
                    }
        };
    }

    private static object? ReadNamedObject(
        object? value,
        List<object> diagnostics)
    {
        if (value == null)
        {
            return null;
        }

        return new
        {
            name =
                SafeString(
                    value,
                    "Name",
                    diagnostics),
            objectTypeRaw =
                SafeString(
                    value,
                    "Type",
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    value,
                    diagnostics)
        };
    }

    private static object? CreateLayout(
        object? position,
        object? bounds,
        object? referenceKey,
        object? parentView)
    {
        return new
        {
            position,
            bounds,
            referenceKey,
            parentView
        };
    }

    private static object CreateCollectionResult(
        int? rawCount,
        List<object> items,
        List<object> diagnostics)
    {
        return new
        {
            available =
                diagnostics.Count == 0 ||
                rawCount.HasValue,
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
        };
    }

    private static object AddIndexEnvelope(
        object value,
        int index)
    {
        return new
        {
            index,
            value
        };
    }

    private static PointFact? ReadPointProperty(
        object? owner,
        string propertyName,
        List<object> diagnostics)
    {
        if (owner == null)
        {
            return null;
        }

        return ReadPoint2d(
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics),
            diagnostics,
            propertyName);
    }

    private static VectorFact? ReadVectorProperty(
        object? owner,
        string propertyName,
        List<object> diagnostics)
    {
        if (owner == null)
        {
            return null;
        }

        object? vector =
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics);

        if (vector == null)
        {
            return null;
        }

        return new VectorFact(
            SafeNullableDouble(
                vector,
                "X",
                diagnostics),
            SafeNullableDouble(
                vector,
                "Y",
                diagnostics));
    }

    private static BoundsFact? ReadBoundsProperty(
        object? owner,
        string propertyName,
        List<object> diagnostics)
    {
        if (owner == null)
        {
            return null;
        }

        return ReadBounds(
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics),
            diagnostics,
            propertyName);
    }

    private static PointFact? ReadPoint2d(
        object? point,
        List<object> diagnostics,
        string scope)
    {
        if (point == null)
        {
            return null;
        }

        double? x =
            SafeNullableDouble(
                point,
                "X",
                diagnostics);

        double? y =
            SafeNullableDouble(
                point,
                "Y",
                diagnostics);

        if (!x.HasValue ||
            !y.HasValue)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                "Point did not expose X and Y.");

            return null;
        }

        return new PointFact(
            x.Value,
            y.Value);
    }

    private static BoundsFact? ReadBounds(
        object? box,
        List<object> diagnostics,
        string scope)
    {
        if (box == null)
        {
            return null;
        }

        PointFact? minPoint =
            ReadPointProperty(
                box,
                "MinPoint",
                diagnostics);

        PointFact? maxPoint =
            ReadPointProperty(
                box,
                "MaxPoint",
                diagnostics);

        if (minPoint == null ||
            maxPoint == null)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                "Box did not expose MinPoint and MaxPoint.");

            return null;
        }

        return CreateBounds(
            minPoint.x,
            minPoint.y,
            maxPoint.x,
            maxPoint.y);
    }

    private static BoundsFact? ComputeCenteredBounds(
        PointFact? center,
        double? width,
        double? height)
    {
        if (center == null ||
            !width.HasValue ||
            !height.HasValue)
        {
            return null;
        }

        return CreateBounds(
            center.x - width.Value / 2.0,
            center.y - height.Value / 2.0,
            center.x + width.Value / 2.0,
            center.y + height.Value / 2.0);
    }

    private static BoundsFact? ComputeBoundsFromPoints(
        IEnumerable<PointFact?> points)
    {
        List<PointFact> available =
            points
                .Where(point => point != null)
                .Cast<PointFact>()
                .ToList();

        if (available.Count == 0)
        {
            return null;
        }

        return CreateBounds(
            available.Min(point => point.x),
            available.Min(point => point.y),
            available.Max(point => point.x),
            available.Max(point => point.y));
    }

    private static BoundsFact CreateBounds(
        double minX,
        double minY,
        double maxX,
        double maxY)
    {
        double normalizedMinX =
            Math.Min(
                minX,
                maxX);

        double normalizedMaxX =
            Math.Max(
                minX,
                maxX);

        double normalizedMinY =
            Math.Min(
                minY,
                maxY);

        double normalizedMaxY =
            Math.Max(
                minY,
                maxY);

        return new BoundsFact(
            normalizedMinX,
            normalizedMinY,
            normalizedMaxX,
            normalizedMaxY,
            normalizedMaxX - normalizedMinX,
            normalizedMaxY - normalizedMinY);
    }

    private static int? ReadFirstAvailableCount(
        object owner,
        List<object> diagnostics,
        params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            object? collection =
                SafeObjectProperty(
                    owner,
                    propertyName,
                    diagnostics,
                    reportMissing: false);

            if (collection == null)
            {
                continue;
            }

            int? count =
                SafeNullableInt32(
                    collection,
                    "Count",
                    diagnostics);

            if (count.HasValue)
            {
                return count;
            }
        }

        return null;
    }

    private static object? ReadReferenceKey(
        DrawingDocument drawingDocument,
        object owner,
        List<object> diagnostics)
    {
        int keyContext = 0;

        try
        {
            ReferenceKeyManager manager =
                drawingDocument.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            Array referenceKey =
                Array.CreateInstance(
                    typeof(byte),
                    0);

            dynamic dynamicOwner =
                owner;

            dynamicOwner.GetReferenceKey(
                ref referenceKey,
                keyContext);

            string keyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString,
                byteCount =
                    referenceKey.Length
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "GetReferenceKey",
                exception);

            return null;
        }
        finally
        {
            if (keyContext != 0)
            {
                try
                {
                    drawingDocument
                        .ReferenceKeyManager
                        .ReleaseKeyContext(
                            keyContext);
                }
                catch (Exception exception)
                {
                    AddDiagnostic(
                        diagnostics,
                        "ReleaseKeyContext",
                        exception);
                }
            }
        }
    }

    private static object GetIndexedItem(
        object collection,
        int index)
    {
        dynamic dynamicCollection =
            collection;

        return dynamicCollection[index];
    }

    private static T? SafeObject<T>(
        Func<T?> reader,
        List<object> diagnostics,
        string scope)
        where T : class
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                exception);

            return null;
        }
    }

    private static object? SafeObjectProperty(
        object owner,
        string propertyName,
        List<object> diagnostics,
        bool reportMissing = true)
    {
        dynamic dynamicOwner =
            owner;

        try
        {
            return propertyName switch
            {
                "Aligned" =>
                    dynamicOwner.Aligned,
                "Alignment" =>
                    dynamicOwner.Alignment,
                "AlignmentType" =>
                    dynamicOwner.AlignmentType,
                "AllNodes" =>
                    dynamicOwner.AllNodes,
                "Attached" =>
                    dynamicOwner.Attached,
                "AttachedEntity" =>
                    dynamicOwner.AttachedEntity,
                "_AttachedEntity" =>
                    dynamicOwner._AttachedEntity,
                "Camera.ViewOrientationType" =>
                    dynamicOwner.Camera.ViewOrientationType,
                "CenterlineType" =>
                    dynamicOwner.CenterlineType,
                "CentermarkType" =>
                    dynamicOwner.CentermarkType,
                "Columns" =>
                    dynamicOwner.Columns,
                "Count" =>
                    dynamicOwner.Count,
                "Definition" =>
                    dynamicOwner.Definition,
                "DimensionLine" =>
                    dynamicOwner.DimensionLine,
                "DimensionType" =>
                    dynamicOwner.DimensionType,
                "Direction" =>
                    dynamicOwner.Direction,
                "DisplayName" =>
                    dynamicOwner.DisplayName,
                "DocumentType" =>
                    dynamicOwner.DocumentType,
                "EndPoint" =>
                    dynamicOwner.EndPoint,
                "ExtensionLineOne" =>
                    dynamicOwner.ExtensionLineOne,
                "ExtensionLineTwo" =>
                    dynamicOwner.ExtensionLineTwo,
                "ExtensionLinesVisible" =>
                    dynamicOwner.ExtensionLinesVisible,
                "ExtensionPointFour" =>
                    dynamicOwner.ExtensionPointFour,
                "ExtensionPointOne" =>
                    dynamicOwner.ExtensionPointOne,
                "ExtensionPointThree" =>
                    dynamicOwner.ExtensionPointThree,
                "ExtensionPointTwo" =>
                    dynamicOwner.ExtensionPointTwo,
                "FitPoints" =>
                    dynamicOwner.FitPoints,
                "FormattedText" =>
                    dynamicOwner.FormattedText,
                "FullDocumentName" =>
                    dynamicOwner.FullDocumentName,
                "FullFileName" =>
                    dynamicOwner.FullFileName,
                "Geometry" =>
                    dynamicOwner.Geometry,
                "GeometryIntent" =>
                    dynamicOwner.GeometryIntent,
                "HasRootNode" =>
                    dynamicOwner.HasRootNode,
                "Height" =>
                    dynamicOwner.Height,
                "HoleTableColumns" =>
                    dynamicOwner.HoleTableColumns,
                "HoleTableRows" =>
                    dynamicOwner.HoleTableRows,
                "IntentOne" =>
                    dynamicOwner.IntentOne,
                "IntentTwo" =>
                    dynamicOwner.IntentTwo,
                "IntentType" =>
                    dynamicOwner.IntentType,
                "Label" =>
                    dynamicOwner.Label,
                "Leader" =>
                    dynamicOwner.Leader,
                "MaxPoint" =>
                    dynamicOwner.MaxPoint,
                "MidPoint" =>
                    dynamicOwner.MidPoint,
                "MinPoint" =>
                    dynamicOwner.MinPoint,
                "Name" =>
                    dynamicOwner.Name,
                "Orientation" =>
                    dynamicOwner.Orientation,
                "Origin" =>
                    dynamicOwner.Origin,
                "ParentView" =>
                    dynamicOwner.ParentView,
                "PartsListColumns" =>
                    dynamicOwner.PartsListColumns,
                "PartsListRows" =>
                    dynamicOwner.PartsListRows,
                "PointOnSheet" =>
                    dynamicOwner.PointOnSheet,
                "Position" =>
                    dynamicOwner.Position,
                "RangeBox" =>
                    dynamicOwner.RangeBox,
                "ReferencedDocument" =>
                    dynamicOwner.ReferencedDocument,
                "ReferencedDocumentDescriptor" =>
                    dynamicOwner.ReferencedDocumentDescriptor,
                "ReferencedDocumentType" =>
                    dynamicOwner.ReferencedDocumentType,
                "RevisionTableColumns" =>
                    dynamicOwner.RevisionTableColumns,
                "RevisionTableRows" =>
                    dynamicOwner.RevisionTableRows,
                "RootNode" =>
                    dynamicOwner.RootNode,
                "Rows" =>
                    dynamicOwner.Rows,
                "ScaleString" =>
                    dynamicOwner.ScaleString,
                "ShowLabel" =>
                    dynamicOwner.ShowLabel,
                "StartPoint" =>
                    dynamicOwner.StartPoint,
                "TableColumns" =>
                    dynamicOwner.TableColumns,
                "TableRows" =>
                    dynamicOwner.TableRows,
                "Text" =>
                    dynamicOwner.Text,
                "Title" =>
                    dynamicOwner.Title,
                "Type" =>
                    dynamicOwner.Type,
                "Visible" =>
                    dynamicOwner.Visible,
                "Width" =>
                    dynamicOwner.Width,
                "X" =>
                    dynamicOwner.X,
                "Y" =>
                    dynamicOwner.Y,
                _ =>
                    null
            };
        }
        catch (Exception exception)
        {
            if (reportMissing)
            {
                AddDiagnostic(
                    diagnostics,
                    propertyName,
                    exception);
            }

            return null;
        }
    }

    private static string? SafeString(
        object? owner,
        string propertyName,
        List<object> diagnostics)
    {
        if (owner == null)
        {
            return null;
        }

        object? value =
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics);

        return value?.ToString();
    }

    private static string? SafeString(
        Func<string> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                exception);

            return null;
        }
    }

    private static double? SafeNullableDouble(
        Func<double> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                exception);

            return null;
        }
    }

    private static double? SafeNullableDouble(
        object owner,
        string propertyName,
        List<object> diagnostics)
    {
        object? value =
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics);

        if (value == null)
        {
            return null;
        }

        try
        {
            return Convert.ToDouble(
                value);
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static int? SafeNullableInt32(
        Func<int> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                exception);

            return null;
        }
    }

    private static int? SafeNullableInt32(
        object owner,
        string propertyName,
        List<object> diagnostics)
    {
        object? value =
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics);

        if (value == null)
        {
            return null;
        }

        try
        {
            return Convert.ToInt32(
                value);
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static bool? SafeNullableBoolean(
        Func<bool> reader,
        List<object> diagnostics,
        string scope)
    {
        try
        {
            return reader();
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                scope,
                exception);

            return null;
        }
    }

    private static bool? SafeNullableBoolean(
        object owner,
        string propertyName,
        List<object> diagnostics)
    {
        object? value =
            SafeObjectProperty(
                owner,
                propertyName,
                diagnostics);

        if (value == null)
        {
            return null;
        }

        try
        {
            return Convert.ToBoolean(
                value);
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return null;
        }
    }

    private static string ReadObjectTypeName(
        object owner,
        List<object> diagnostics)
    {
        object? value =
            SafeObjectProperty(
                owner,
                "Type",
                diagnostics);

        if (value == null)
        {
            return string.Empty;
        }

        try
        {
            return Enum.GetName(
                       typeof(ObjectTypeEnum),
                       Convert.ToInt32(value))
                   ?? value.ToString()
                   ?? string.Empty;
        }
        catch
        {
            return value.ToString()
                   ?? string.Empty;
        }
    }

    private static string ReadEnumName<TEnum>(
        TEnum value)
        where TEnum : struct
    {
        return Enum.GetName(
                   typeof(TEnum),
                   value)
               ?? value.ToString()
               ?? string.Empty;
    }

    private static void AddDiagnostic(
        List<object> diagnostics,
        string scope,
        Exception exception)
    {
        diagnostics.Add(
            new
            {
                scope,
                available =
                    false,
                message =
                    exception.Message,
                exceptionType =
                    exception.GetType().FullName
            });
    }

    private static void AddDiagnostic(
        List<object> diagnostics,
        string scope,
        string message)
    {
        diagnostics.Add(
            new
            {
                scope,
                available =
                    false,
                message
            });
    }

    public static string CreateSuccess(
        object data)
    {
        return JsonSerializer.Serialize(
            new
            {
                success =
                    true,
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
                success =
                    false,
                error =
                    message,
                details
            },
            CreateJsonOptions());
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented =
                true,
            Encoder =
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    private sealed record PointFact(
        double x,
        double y);

    private sealed record VectorFact(
        double? x,
        double? y);

    private sealed record BoundsFact(
        double minX,
        double minY,
        double maxX,
        double maxY,
        double width,
        double height);
}
