using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class AnnotationReadSupport
{
    public delegate AnnotationReadResult SheetAnnotationReader(
        Sheet sheet);

    public static string ExecuteSheetRead(
        Inventor.Application inventor,
        JsonElement root,
        string capability,
        SheetAnnotationReader reader)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        DrawingDocument? drawingDocument =
            GetActiveDrawingDocument(
                inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            return CreateError(
                documentError ??
                "Unable to get active drawing document.");
        }

        Sheet? sheet =
            ResolveSheet(
                drawingDocument,
                root,
                out bool usedActiveSheet,
                out string? sheetError);

        if (sheet == null)
        {
            return CreateError(
                sheetError ??
                "Unable to resolve drawing sheet.");
        }

        try
        {
            AnnotationReadResult result =
                reader(
                    sheet);

            return CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    usedActiveSheet,

                    capability,

                    count =
                        result.Count,

                    items =
                        result.Items,

                    diagnostics =
                        result.Diagnostics
                });
        }
        catch (Exception exception)
        {
            return CreateError(
                $"Unable to read {capability}.",
                exception.Message);
        }
    }

    public static AnnotationReadResult ReadGeneralNotes(
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        try
        {
            GeneralNotes notes =
                sheet
                    .DrawingNotes
                    .GeneralNotes;

            for (int index = 1;
                 index <= notes.Count;
                 index++)
            {
                GeneralNote note =
                    notes[index];

                items.Add(
                    ReadGeneralNote(
                        note,
                        sheet,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "GeneralNotes",
                exception);
        }

        return new AnnotationReadResult(
            items.Count,
            items,
            diagnostics);
    }

    public static AnnotationReadResult ReadLeaderNotes(
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        try
        {
            LeaderNotes notes =
                sheet
                    .DrawingNotes
                    .LeaderNotes;

            for (int index = 1;
                 index <= notes.Count;
                 index++)
            {
                LeaderNote note =
                    notes[index];

                items.Add(
                    ReadLeaderNote(
                        note,
                        sheet,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "LeaderNotes",
                exception);
        }

        return new AnnotationReadResult(
            items.Count,
            items,
            diagnostics);
    }

    public static AnnotationReadResult ReadBalloons(
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        try
        {
            Balloons balloons =
                sheet.Balloons;

            for (int index = 1;
                 index <= balloons.Count;
                 index++)
            {
                Balloon balloon =
                    balloons[index];

                items.Add(
                    ReadBalloon(
                        balloon,
                        sheet,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Balloons",
                exception);
        }

        return new AnnotationReadResult(
            items.Count,
            items,
            diagnostics);
    }

    public static AnnotationReadResult ReadCenterMarks(
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        try
        {
            Centermarks centerMarks =
                sheet.Centermarks;

            for (int index = 1;
                 index <= centerMarks.Count;
                 index++)
            {
                Centermark centerMark =
                    centerMarks[index];

                items.Add(
                    ReadCenterMark(
                        centerMark,
                        sheet,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Centermarks",
                exception);
        }

        return new AnnotationReadResult(
            items.Count,
            items,
            diagnostics);
    }

    public static AnnotationReadResult ReadCenterlines(
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        try
        {
            Centerlines centerlines =
                sheet.Centerlines;

            for (int index = 1;
                 index <= centerlines.Count;
                 index++)
            {
                Centerline centerline =
                    centerlines[index];

                items.Add(
                    ReadCenterline(
                        centerline,
                        sheet,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Centerlines",
                exception);
        }

        return new AnnotationReadResult(
            items.Count,
            items,
            diagnostics);
    }

    private static object ReadGeneralNote(
        GeneralNote note,
        Sheet sheet,
        int index)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                GetValue(
                    note,
                    "Position",
                    diagnostics));

        object? rangeBox =
            ReadBox2d(
                GetValue(
                    note,
                    "RangeBox",
                    diagnostics));

        object? attachedEntity =
            ReadEntityMetadata(
                GetValue(
                    note,
                    "_AttachedEntity",
                    diagnostics),
                diagnostics);

        string text =
            ReadString(
                GetValue(
                    note,
                    "Text",
                    diagnostics));

        string formattedText =
            ReadString(
                GetValue(
                    note,
                    "FormattedText",
                    diagnostics));

        object? textStyle =
            ReadNamedObject(
                GetValue(
                    note,
                    "TextStyle",
                    diagnostics),
                diagnostics);

        object? layer =
            ReadNamedObject(
                GetValue(
                    note,
                    "Layer",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            objectTypeRaw =
                ReadRawType(
                    note,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    note,
                    diagnostics),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            position,
            origin =
                position,
            rangeBox,
            text,
            formattedText,
            textStyle,
            layer,
            attachedEntity,
            selectorSnapshot =
                new
                {
                    index,
                    type =
                        "general_note",
                    position,
                    text,
                    attachedEntity
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadLeaderNote(
        LeaderNote note,
        Sheet sheet,
        int index)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                GetValue(
                    note,
                    "Position",
                    diagnostics));

        object? rangeBox =
            ReadBox2d(
                GetValue(
                    note,
                    "RangeBox",
                    diagnostics));

        string text =
            ReadString(
                GetValue(
                    note,
                    "Text",
                    diagnostics));

        string formattedText =
            ReadString(
                GetValue(
                    note,
                    "FormattedText",
                    diagnostics));

        object? leader =
            ReadLeader(
                GetValue(
                    note,
                    "Leader",
                    diagnostics),
                diagnostics);

        object? attachedEntity =
            ReadEntityMetadata(
                GetValue(
                    note,
                    "_AttachedEntity",
                    diagnostics),
                diagnostics);

        object? dimensionStyle =
            ReadNamedObject(
                GetValue(
                    note,
                    "DimensionStyle",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            objectTypeRaw =
                ReadRawType(
                    note,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    note,
                    diagnostics),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            position,
            origin =
                position,
            rangeBox,
            text,
            formattedText,
            dimensionStyle,
            layer =
                ReadNamedObject(
                    GetValue(
                        note,
                        "Layer",
                        diagnostics),
                    diagnostics),
            leader,
            attachedEntity,
            selectorSnapshot =
                new
                {
                    index,
                    type =
                        "leader_note",
                    position,
                    text,
                    attachedEntity
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadBalloon(
        Balloon balloon,
        Sheet sheet,
        int index)
    {
        List<object> diagnostics =
            new();

        object? position =
            ReadPoint2d(
                GetValue(
                    balloon,
                    "Position",
                    diagnostics));

        object? leader =
            ReadLeader(
                GetValue(
                    balloon,
                    "Leader",
                    diagnostics),
                diagnostics);

        object? valueSets =
            ReadBalloonValueSets(
                GetValue(
                    balloon,
                    "BalloonValueSets",
                    diagnostics),
                diagnostics);

        object? style =
            ReadNamedObject(
                GetValue(
                    balloon,
                    "Style",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            objectTypeRaw =
                ReadRawType(
                    balloon,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    balloon,
                    diagnostics),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            parentView =
                ReadNamedObject(
                    GetValue(
                        balloon,
                        "ParentView",
                        diagnostics),
                    diagnostics),
            position,
            origin =
                position,
            placementDirectionRaw =
                ReadString(
                    GetValue(
                        balloon,
                        "PlacementDirection",
                        diagnostics)),
            attached =
                ReadNullableBoolean(
                    GetValue(
                        balloon,
                        "Attached",
                        diagnostics)),
            style,
            leader,
            balloonValueSets =
                valueSets,
            selectorSnapshot =
                new
                {
                    index,
                    type =
                        "balloon",
                    position,
                    valueSets
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadCenterMark(
        Centermark centerMark,
        Sheet sheet,
        int index)
    {
        List<object> diagnostics =
            new();

        object? centerPoint =
            ReadPoint2d(
                GetValue(
                    centerMark,
                    "Position",
                    diagnostics));

        object? rangeBox =
            ReadBox2d(
                GetValue(
                    centerMark,
                    "RangeBox",
                    diagnostics));

        object? attachedEntity =
            ReadEntityMetadata(
                GetValue(
                    centerMark,
                    "AttachedEntity",
                    diagnostics),
                diagnostics);

        object? style =
            ReadNamedObject(
                GetValue(
                    centerMark,
                    "Style",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            objectTypeRaw =
                ReadRawType(
                    centerMark,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    centerMark,
                    diagnostics),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            position =
                centerPoint,
            origin =
                centerPoint,
            centerPoint,
            rangeBox,
            visible =
                ReadNullableBoolean(
                    GetValue(
                        centerMark,
                        "Visible",
                        diagnostics)),
            attached =
                ReadNullableBoolean(
                    GetValue(
                        centerMark,
                        "Attached",
                        diagnostics)),
            style,
            layer =
                ReadNamedObject(
                    GetValue(
                        centerMark,
                        "Layer",
                        diagnostics),
                    diagnostics),
            attachedEntity,
            associatedCenterlines =
                ReadEntityCollection(
                    GetValue(
                        centerMark,
                        "Centerlines",
                        diagnostics),
                    diagnostics,
                    "Centerlines"),
            centermarkTypeRaw =
                ReadString(
                    GetValue(
                        centerMark,
                        "CentermarkType",
                        diagnostics)),
            extensionLinesVisible =
                ReadNullableBoolean(
                    GetValue(
                        centerMark,
                        "ExtensionLinesVisible",
                        diagnostics)),
            extensionPointOne =
                ReadPoint2d(
                    GetValue(
                        centerMark,
                        "ExtensionPointOne",
                        diagnostics)),
            extensionPointOneDirectionRaw =
                ReadString(
                    GetValue(
                        centerMark,
                        "ExtensionPointOneDirection",
                        diagnostics)),
            extensionPointTwo =
                ReadPoint2d(
                    GetValue(
                        centerMark,
                        "ExtensionPointTwo",
                        diagnostics)),
            extensionPointTwoDirectionRaw =
                ReadString(
                    GetValue(
                        centerMark,
                        "ExtensionPointTwoDirection",
                        diagnostics)),
            extensionPointThree =
                ReadPoint2d(
                    GetValue(
                        centerMark,
                        "ExtensionPointThree",
                        diagnostics)),
            extensionPointThreeDirectionRaw =
                ReadString(
                    GetValue(
                        centerMark,
                        "ExtensionPointThreeDirection",
                        diagnostics)),
            extensionPointFour =
                ReadPoint2d(
                    GetValue(
                        centerMark,
                        "ExtensionPointFour",
                        diagnostics)),
            extensionPointFourDirectionRaw =
                ReadString(
                    GetValue(
                        centerMark,
                        "ExtensionPointFourDirection",
                        diagnostics)),
            selectorSnapshot =
                new
                {
                    index,
                    type =
                        "center_mark",
                    centerPoint,
                    attachedEntity
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadCenterline(
        Centerline centerline,
        Sheet sheet,
        int index)
    {
        List<object> diagnostics =
            new();

        object? startPoint =
            ReadPoint2d(
                GetValue(
                    centerline,
                    "StartPoint",
                    diagnostics));

        object? endPoint =
            ReadPoint2d(
                GetValue(
                    centerline,
                    "EndPoint",
                    diagnostics));

        object? rangeBox =
            ReadBox2d(
                GetValue(
                    centerline,
                    "RangeBox",
                    diagnostics));

        object? style =
            ReadNamedObject(
                GetValue(
                    centerline,
                    "Style",
                    diagnostics),
                diagnostics);

        return new
        {
            index,
            objectTypeRaw =
                ReadRawType(
                    centerline,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    centerline,
                    diagnostics),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            rangeBox,
            centerlineTypeRaw =
                ReadString(
                    GetValue(
                        centerline,
                        "CenterlineType",
                        diagnostics)),
            geometryTypeRaw =
                ReadString(
                    GetValue(
                        centerline,
                        "GeometryType",
                        diagnostics)),
            geometry =
                ReadEntityMetadata(
                    GetValue(
                        centerline,
                        "Geometry",
                        diagnostics),
                    diagnostics),
            startPoint,
            endPoint,
            fitPoints =
                ReadPointCollection(
                    GetValue(
                        centerline,
                        "FitPoints",
                        diagnostics),
                    diagnostics),
            visible =
                ReadNullableBoolean(
                    GetValue(
                        centerline,
                        "Visible",
                        diagnostics)),
            attached =
                ReadNullableBoolean(
                    GetValue(
                        centerline,
                        "Attached",
                        diagnostics)),
            style,
            layer =
                ReadNamedObject(
                    GetValue(
                    centerline,
                    "Layer",
                    diagnostics),
                    diagnostics),
            patternCenter =
                ReadEntityMetadata(
                    GetValue(
                        centerline,
                        "PatternCenter",
                        diagnostics),
                    diagnostics),
            modelWorkFeature =
                ReadEntityMetadata(
                    GetValue(
                        centerline,
                        "ModelWorkFeature",
                        diagnostics),
                    diagnostics),
            selectorSnapshot =
                new
                {
                    index,
                    type =
                        "centerline",
                    startPoint,
                    endPoint
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static DrawingDocument? GetActiveDrawingDocument(
        Inventor.Application inventor,
        out string? error)
    {
        error =
            null;

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

    private static Sheet? ResolveSheet(
        DrawingDocument drawingDocument,
        JsonElement root,
        out bool usedActiveSheet,
        out string? error)
    {
        usedActiveSheet =
            false;

        error =
            null;

        if (!root.TryGetProperty(
                "sheetName",
                out JsonElement sheetNameElement))
        {
            usedActiveSheet =
                true;

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
            sheetNameElement.GetString()?
                .Trim()
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(
                sheetName))
        {
            error =
                "Field \"sheetName\" must not be empty when provided.";

            return null;
        }

        foreach (Sheet sheet
                 in drawingDocument.Sheets)
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

    private static object? ReadLeader(
        object? leaderObject,
        List<object> diagnostics)
    {
        if (leaderObject == null)
        {
            return null;
        }

        object? rootNode =
            GetValue(
                leaderObject,
                "RootNode",
                diagnostics);

        object? allNodes =
            GetValue(
                leaderObject,
                "AllNodes",
                diagnostics);

        return new
        {
            objectTypeRaw =
                ReadRawType(
                    leaderObject,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    leaderObject,
                    diagnostics),
            hasRootNode =
                ReadNullableBoolean(
                    GetValue(
                        leaderObject,
                        "HasRootNode",
                        diagnostics)),
            rootNode =
                ReadLeaderNode(
                    rootNode,
                    diagnostics),
            nodes =
                ReadLeaderNodes(
                    allNodes,
                    diagnostics)
        };
    }

    private static object? ReadLeaderNode(
        object? nodeObject,
        List<object> diagnostics)
    {
        if (nodeObject == null)
        {
            return null;
        }

        object? position =
            ReadPoint2d(
                GetValue(
                    nodeObject,
                    "Position",
                    diagnostics));

        return new
        {
            objectTypeRaw =
                ReadRawType(
                    nodeObject,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    nodeObject,
                    diagnostics),
            position,
            attachedEntity =
                ReadEntityMetadata(
                    GetValue(
                        nodeObject,
                        "AttachedEntity",
                        diagnostics),
                    diagnostics)
        };
    }

    private static List<object> ReadLeaderNodes(
        object? nodesObject,
        List<object> diagnostics)
    {
        List<object> nodes =
            new();

        if (nodesObject == null)
        {
            return nodes;
        }

        dynamic nodesDynamic =
            nodesObject;

        try
        {
            int count =
                (int)nodesDynamic.Count;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                object node =
                    nodesDynamic[index];

                nodes.Add(
                    ReadLeaderNode(
                        node,
                        diagnostics)
                    ?? new
                    {
                        index
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Leader.AllNodes",
                exception);
        }

        return nodes;
    }

    private static object? ReadBalloonValueSets(
        object? valueSetsObject,
        List<object> diagnostics)
    {
        if (valueSetsObject == null)
        {
            return null;
        }

        dynamic valueSetsDynamic =
            valueSetsObject;

        List<object> valueSets =
            new();

        try
        {
            int count =
                (int)valueSetsDynamic.Count;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                object valueSet =
                    valueSetsDynamic[index];

                valueSets.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadRawType(
                                valueSet,
                                diagnostics),
                        objectType =
                            ReadObjectTypeName(
                                valueSet,
                                diagnostics),
                        value =
                            ReadString(
                                GetValue(
                                    valueSet,
                                    "Value",
                                    diagnostics)),
                        itemNumber =
                            ReadString(
                                GetValue(
                                    valueSet,
                                    "ItemNumber",
                                    diagnostics)),
                        overrideValue =
                            ReadString(
                                GetValue(
                                    valueSet,
                                    "OverrideValue",
                                    diagnostics)),
                        referencedRow =
                            ReadEntityMetadata(
                                GetValue(
                                    valueSet,
                                    "ReferencedRow",
                                    diagnostics),
                                diagnostics)
                    });
            }

            return new
            {
                count,
                items =
                    valueSets
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "BalloonValueSets",
                exception);

            return new
            {
                count =
                    0,
                items =
                    valueSets
            };
        }
    }

    private static object? ReadEntityCollection(
        object? collectionObject,
        List<object> diagnostics,
        string propertyName)
    {
        if (collectionObject == null)
        {
            return null;
        }

        dynamic collectionDynamic =
            collectionObject;

        List<object> entities =
            new();

        try
        {
            int count =
                (int)collectionDynamic.Count;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                object entity =
                    collectionDynamic[index];

                entities.Add(
                    new
                    {
                        index,
                        entity =
                            ReadEntityMetadata(
                                entity,
                                diagnostics)
                    });
            }

            return new
            {
                count,
                items =
                    entities
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);

            return new
            {
                count =
                    0,
                items =
                    entities
            };
        }
    }

    private static object? ReadEntityMetadata(
        object? entityObject,
        List<object> diagnostics)
    {
        if (entityObject == null)
        {
            return null;
        }

        return new
        {
            objectTypeRaw =
                ReadRawType(
                    entityObject,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    entityObject,
                    diagnostics),
            name =
                ReadString(
                    GetValue(
                        entityObject,
                        "Name",
                        diagnostics)),
            parentView =
                ReadNamedObject(
                    GetValue(
                        entityObject,
                        "ParentView",
                        diagnostics),
                    diagnostics),
            rangeBox =
                ReadBox2d(
                    GetValue(
                        entityObject,
                        "RangeBox",
                        diagnostics))
        };
    }

    private static object? ReadNamedObject(
        object? namedObject,
        List<object> diagnostics)
    {
        if (namedObject == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    GetValue(
                        namedObject,
                        "Name",
                        diagnostics)),
            objectTypeRaw =
                ReadRawType(
                    namedObject,
                    diagnostics),
            objectType =
                ReadObjectTypeName(
                    namedObject,
                    diagnostics)
        };
    }

    private static object ReadSheetMetadata(
        Sheet sheet)
    {
        return new
        {
            name =
                sheet.Name,
            width =
                sheet.Width,
            height =
                sheet.Height
        };
    }

    private static object? ReadPointCollection(
        object? pointsObject,
        List<object> diagnostics)
    {
        if (pointsObject == null)
        {
            return null;
        }

        dynamic pointsDynamic =
            pointsObject;

        List<object> points =
            new();

        try
        {
            int count =
                (int)pointsDynamic.Count;

            for (int index = 1;
                 index <= count;
                 index++)
            {
                object point =
                    pointsDynamic[index];

                points.Add(
                    new
                    {
                        index,
                        point =
                            ReadPoint2d(
                                point)
                    });
            }

            return new
            {
                count,
                items =
                    points
            };
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "FitPoints",
                exception);

            return new
            {
                count =
                    0,
                items =
                    points
            };
        }
    }

    private static object? ReadPoint2d(
        object? pointObject)
    {
        if (pointObject == null)
        {
            return null;
        }

        dynamic point =
            pointObject;

        try
        {
            return new
            {
                x =
                    (double)point.X,
                y =
                    (double)point.Y
            };
        }
        catch (Exception exception)
        {
            return new
            {
                unavailable =
                    true,
                error =
                    exception.Message
            };
        }
    }

    private static object? ReadBox2d(
        object? boxObject)
    {
        if (boxObject == null)
        {
            return null;
        }

        dynamic box =
            boxObject;

        try
        {
            return new
            {
                minPoint =
                    ReadPoint2d(
                        box.MinPoint),
                maxPoint =
                    ReadPoint2d(
                        box.MaxPoint)
            };
        }
        catch (Exception exception)
        {
            return new
            {
                unavailable =
                    true,
                error =
                    exception.Message
            };
        }
    }

    private static string ReadRawType(
        object ownerObject,
        List<object> diagnostics)
    {
        object? value =
            GetValue(
                ownerObject,
                "Type",
                diagnostics);

        return ReadString(
            value);
    }

    private static string ReadObjectTypeName(
        object ownerObject,
        List<object> diagnostics)
    {
        object? value =
            GetValue(
                ownerObject,
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
                       Convert.ToInt32(
                           value))
                   ?? value.ToString()
                   ?? string.Empty;
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Type",
                exception);

            return value.ToString()
                   ?? string.Empty;
        }
    }

    private static object? ReadNullableBoolean(
        object? value)
    {
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
            return new
            {
                value =
                    (bool?)null,
                conversionError =
                    exception.Message
            };
        }
    }

    private static string ReadString(
        object? value)
    {
        return value?.ToString()
               ?? string.Empty;
    }

    private static object? GetValue(
        object ownerObject,
        string propertyName,
        List<object> diagnostics)
    {
        dynamic owner =
            ownerObject;

        try
        {
            return propertyName switch
            {
                "AllNodes" =>
                    owner.AllNodes,
                "_AttachedEntity" =>
                    owner._AttachedEntity,
                "Attached" =>
                    owner.Attached,
                "AttachedEntity" =>
                    owner.AttachedEntity,
                "BalloonValueSets" =>
                    owner.BalloonValueSets,
                "CenterlineType" =>
                    owner.CenterlineType,
                "Centerlines" =>
                    owner.Centerlines,
                "CentermarkType" =>
                    owner.CentermarkType,
                "DimensionStyle" =>
                    owner.DimensionStyle,
                "EndPoint" =>
                    owner.EndPoint,
                "ExtensionLinesVisible" =>
                    owner.ExtensionLinesVisible,
                "ExtensionPointFour" =>
                    owner.ExtensionPointFour,
                "ExtensionPointFourDirection" =>
                    owner.ExtensionPointFourDirection,
                "ExtensionPointOne" =>
                    owner.ExtensionPointOne,
                "ExtensionPointOneDirection" =>
                    owner.ExtensionPointOneDirection,
                "ExtensionPointThree" =>
                    owner.ExtensionPointThree,
                "ExtensionPointThreeDirection" =>
                    owner.ExtensionPointThreeDirection,
                "ExtensionPointTwo" =>
                    owner.ExtensionPointTwo,
                "ExtensionPointTwoDirection" =>
                    owner.ExtensionPointTwoDirection,
                "FitPoints" =>
                    owner.FitPoints,
                "FormattedText" =>
                    owner.FormattedText,
                "Geometry" =>
                    owner.Geometry,
                "GeometryType" =>
                    owner.GeometryType,
                "HasRootNode" =>
                    owner.HasRootNode,
                "ItemNumber" =>
                    owner.ItemNumber,
                "Layer" =>
                    owner.Layer,
                "Leader" =>
                    owner.Leader,
                "ModelWorkFeature" =>
                    owner.ModelWorkFeature,
                "Name" =>
                    owner.Name,
                "OverrideValue" =>
                    owner.OverrideValue,
                "ParentView" =>
                    owner.ParentView,
                "PlacementDirection" =>
                    owner.PlacementDirection,
                "PatternCenter" =>
                    owner.PatternCenter,
                "Position" =>
                    owner.Position,
                "RangeBox" =>
                    owner.RangeBox,
                "ReferencedRow" =>
                    owner.ReferencedRow,
                "RootNode" =>
                    owner.RootNode,
                "StartPoint" =>
                    owner.StartPoint,
                "Style" =>
                    owner.Style,
                "TextStyle" =>
                    owner.TextStyle,
                "Text" =>
                    owner.Text,
                "Type" =>
                    owner.Type,
                "Value" =>
                    owner.Value,
                "Visible" =>
                    owner.Visible,
                _ =>
                    null
            };
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

    private static void AddDiagnostic(
        List<object> diagnostics,
        string propertyName,
        Exception exception)
    {
        diagnostics.Add(
            new
            {
                property =
                    propertyName,
                available =
                    false,
                error =
                    exception.Message
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
                JavaScriptEncoder
                    .UnsafeRelaxedJsonEscaping
        };
    }
}

internal sealed record AnnotationReadResult(
    int Count,
    List<object> Items,
    List<object> Diagnostics);
