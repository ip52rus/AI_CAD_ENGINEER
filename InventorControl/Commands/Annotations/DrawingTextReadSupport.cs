using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class DrawingTextReadSupport
{
    internal static object ReadBendNoteSnapshot(
        DrawingDocument drawingDocument,
        Sheet sheet,
        BendNote note,
        int index)
    {
        return ReadBendNote(
            drawingDocument,
            sheet,
            note,
            index);
    }

    internal static object ReadChamferNoteSnapshot(
        DrawingDocument drawingDocument,
        Sheet sheet,
        ChamferNote note,
        int index)
    {
        return ReadChamferNote(
            drawingDocument,
            sheet,
            note,
            index);
    }

    internal static object ReadPunchNoteSnapshot(
        DrawingDocument drawingDocument,
        Sheet sheet,
        PunchNote note,
        int index)
    {
        return ReadPunchNote(
            drawingDocument,
            sheet,
            note,
            index);
    }

    public static string Execute(
        Inventor.Application inventor,
        JsonElement root)
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

        List<object> diagnostics =
            new();

        object collections =
            ReadCollections(
                drawingDocument,
                sheet,
                diagnostics);

        return CreateSuccess(
            new
            {
                document =
                    drawingDocument.DisplayName,

                sheet =
                    sheet.Name,

                usedActiveSheet,

                capability =
                    "drawing_text_objects",

                collections,

                diagnostics
            });
    }

    private static object ReadCollections(
        DrawingDocument drawingDocument,
        Sheet sheet,
        List<object> diagnostics)
    {
        DrawingNotes? drawingNotes =
            ReadComObject(
                diagnostics,
                "Sheet.DrawingNotes",
                () => sheet.DrawingNotes);

        return new
        {
            generalNotes =
                ReadGeneralNotes(
                    drawingDocument,
                    sheet,
                    drawingNotes),

            leaderNotes =
                ReadLeaderNotes(
                    drawingDocument,
                    sheet,
                    drawingNotes),

            holeThreadNotes =
                ReadHoleThreadNotes(
                    drawingDocument,
                    sheet,
                    drawingNotes),

            bendNotes =
                ReadBendNotes(
                    drawingDocument,
                    sheet,
                    drawingNotes),

            chamferNotes =
                ReadChamferNotes(
                    drawingDocument,
                    sheet,
                    drawingNotes),

            punchNotes =
                ReadPunchNotes(
                    drawingDocument,
                    sheet,
                    drawingNotes),

            drawingSketchTextBoxes =
                ReadDrawingSketchTextBoxes(
                    drawingDocument,
                    sheet),

            sketchedSymbols =
                ReadSketchedSymbols(
                    drawingDocument,
                    sheet)
        };
    }

    private static object ReadGeneralNotes(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingNotes? drawingNotes)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        if (drawingNotes == null)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.DrawingNotes",
                "Sheet.DrawingNotes was not available.");

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        GeneralNotes? notes =
            ReadComObject(
                diagnostics,
                "DrawingNotes.GeneralNotes",
                () => drawingNotes.GeneralNotes);

        if (notes == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "GeneralNotes.Count",
                () => notes.Count);

        try
        {
            int index =
                0;

            foreach (GeneralNote note
                     in notes)
            {
                index++;

                items.Add(
                    ReadGeneralNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "GeneralNotes.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadLeaderNotes(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingNotes? drawingNotes)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        if (drawingNotes == null)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.DrawingNotes",
                "Sheet.DrawingNotes was not available.");

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        LeaderNotes? notes =
            ReadComObject(
                diagnostics,
                "DrawingNotes.LeaderNotes",
                () => drawingNotes.LeaderNotes);

        if (notes == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "LeaderNotes.Count",
                () => notes.Count);

        try
        {
            int index =
                0;

            foreach (LeaderNote note
                     in notes)
            {
                index++;

                items.Add(
                    ReadLeaderNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "LeaderNotes.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadHoleThreadNotes(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingNotes? drawingNotes)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        if (drawingNotes == null)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.DrawingNotes",
                "Sheet.DrawingNotes was not available.");

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        HoleThreadNotes? notes =
            ReadComObject(
                diagnostics,
                "DrawingNotes.HoleThreadNotes",
                () => drawingNotes.HoleThreadNotes);

        if (notes == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "HoleThreadNotes.Count",
                () => notes.Count);

        try
        {
            int index =
                0;

            foreach (HoleThreadNote note
                     in notes)
            {
                index++;

                items.Add(
                    ReadHoleThreadNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "HoleThreadNotes.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadBendNotes(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingNotes? drawingNotes)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        if (drawingNotes == null)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.DrawingNotes",
                "Sheet.DrawingNotes was not available.");

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        BendNotes? notes =
            ReadComObject(
                diagnostics,
                "DrawingNotes.BendNotes",
                () => drawingNotes.BendNotes);

        if (notes == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "BendNotes.Count",
                () => notes.Count);

        try
        {
            int index =
                0;

            foreach (BendNote note
                     in notes)
            {
                index++;

                items.Add(
                    ReadBendNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "BendNotes.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadChamferNotes(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingNotes? drawingNotes)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        if (drawingNotes == null)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.DrawingNotes",
                "Sheet.DrawingNotes was not available.");

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        ChamferNotes? notes =
            ReadComObject(
                diagnostics,
                "DrawingNotes.ChamferNotes",
                () => drawingNotes.ChamferNotes);

        if (notes == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "ChamferNotes.Count",
                () => notes.Count);

        try
        {
            int index =
                0;

            foreach (ChamferNote note
                     in notes)
            {
                index++;

                items.Add(
                    ReadChamferNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "ChamferNotes.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadPunchNotes(
        DrawingDocument drawingDocument,
        Sheet sheet,
        DrawingNotes? drawingNotes)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        if (drawingNotes == null)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.DrawingNotes",
                "Sheet.DrawingNotes was not available.");

            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        PunchNotes? notes =
            ReadComObject(
                diagnostics,
                "DrawingNotes.PunchNotes",
                () => drawingNotes.PunchNotes);

        if (notes == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "PunchNotes.Count",
                () => notes.Count);

        try
        {
            int index =
                0;

            foreach (PunchNote note
                     in notes)
            {
                index++;

                items.Add(
                    ReadPunchNote(
                        drawingDocument,
                        sheet,
                        note,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "PunchNotes.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadDrawingSketchTextBoxes(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        DrawingSketches? sketches =
            ReadComObject(
                diagnostics,
                "Sheet.Sketches",
                () => sheet.Sketches);

        if (sketches == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? sketchRawCount =
            ReadNullableInt32(
                diagnostics,
                "Sheet.Sketches.Count",
                () => sketches.Count);

        int rawTextBoxCount =
            0;

        bool rawTextBoxCountComplete =
            true;

        try
        {
            int sketchIndex =
                0;

            foreach (DrawingSketch sketch
                     in sketches)
            {
                sketchIndex++;

                ReadTextBoxesFromSketch(
                    drawingDocument,
                    sketch,
                    sketchIndex,
                    items,
                    diagnostics,
                    ref rawTextBoxCount,
                    ref rawTextBoxCountComplete);
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.Sketches.Enumeration",
                exception);
        }

        return new
        {
            rawCount =
                rawTextBoxCountComplete
                    ? rawTextBoxCount
                    : (int?)null,

            sketchRawCount,

            itemCount =
                items.Count,

            items,

            diagnostics
        };
    }

    private static object ReadSketchedSymbols(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        SketchedSymbols? symbols =
            ReadComObject(
                diagnostics,
                "Sheet.SketchedSymbols",
                () => sheet.SketchedSymbols);

        if (symbols == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "Sheet.SketchedSymbols.Count",
                () => symbols.Count);

        try
        {
            int index =
                0;

            foreach (SketchedSymbol symbol
                     in symbols)
            {
                index++;

                items.Add(
                    ReadSketchedSymbol(
                        drawingDocument,
                        sheet,
                        symbol,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.SketchedSymbols.Enumeration",
                exception);
        }

        return CreateCollectionResult(
            rawCount,
            items,
            diagnostics);
    }

    private static object ReadGeneralNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        GeneralNote note,
        int index)
    {
        List<object> diagnostics =
            new();

        string text =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Text",
                    () => note.Text));

        string formattedText =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "FormattedText",
                    () => note.FormattedText));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => note.Position));

        object? rangeBox =
            ReadBox2d(
                ReadComObject(
                    diagnostics,
                    "RangeBox",
                    () => note.RangeBox));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    note.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => note.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => note.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            text,
            formattedText,
            position,
            origin =
                position,
            rangeBox,
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => note.Layer),
            textStyle =
                ReadTextStyle(
                    diagnostics,
                    "TextStyle",
                    () => note.TextStyle),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => note._AttachedEntity),
            showTextBorder =
                ReadNullableBoolean(
                    diagnostics,
                    "ShowTextBorder",
                    () => note.ShowTextBorder),
            fitted =
                ReadNullableBoolean(
                    diagnostics,
                    "Fitted",
                    () => note.Fitted),
            fittedTextHeight =
                ReadNullableDouble(
                    diagnostics,
                    "FittedTextHeight",
                    () => note.FittedTextHeight),
            fittedTextWidth =
                ReadNullableDouble(
                    diagnostics,
                    "FittedTextWidth",
                    () => note.FittedTextWidth),
            stackedTextPositionRaw =
                ReadEnumRaw(
                    ReadNullableEnum(
                        diagnostics,
                        "StackedTextPosition",
                        () => note.StackedTextPosition)),
            stackedTextPosition =
                ReadEnumName(
                    ReadNullableEnum(
                        diagnostics,
                        "StackedTextPosition",
                        () => note.StackedTextPosition)),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "general_note",
                    position,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadLeaderNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        LeaderNote note,
        int index)
    {
        List<object> diagnostics =
            new();

        string text =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Text",
                    () => note.Text));

        string formattedText =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "FormattedText",
                    () => note.FormattedText));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => note.Position));

        object? rangeBox =
            ReadBox2d(
                ReadComObject(
                    diagnostics,
                    "RangeBox",
                    () => note.RangeBox));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    note.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => note.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => note.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            text,
            formattedText,
            position,
            origin =
                position,
            rangeBox,
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => note.Layer),
            dimensionStyle =
                ReadDimensionStyle(
                    diagnostics,
                    "DimensionStyle",
                    () => note.DimensionStyle),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => note._AttachedEntity),
            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => note.Leader),
            showTextBorder =
                ReadNullableBoolean(
                    diagnostics,
                    "ShowTextBorder",
                    () => note.ShowTextBorder),
            stackedTextPositionRaw =
                ReadEnumRaw(
                    ReadNullableEnum(
                        diagnostics,
                        "StackedTextPosition",
                        () => note.StackedTextPosition)),
            stackedTextPosition =
                ReadEnumName(
                    ReadNullableEnum(
                        diagnostics,
                        "StackedTextPosition",
                        () => note.StackedTextPosition)),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "leader_note",
                    position,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadHoleThreadNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        HoleThreadNote note,
        int index)
    {
        List<object> diagnostics =
            new();

        DimensionText? dimensionText =
            ReadComObject(
                diagnostics,
                "Text",
                () => note.Text);

        string text =
            dimensionText == null
                ? string.Empty
                : ReadString(
                    ReadProperty(
                        diagnostics,
                        "Text.Text",
                        () => dimensionText.Text));

        string formattedText =
            dimensionText == null
                ? string.Empty
                : ReadString(
                    ReadProperty(
                        diagnostics,
                        "Text.FormattedText",
                        () => dimensionText.FormattedText));

        object? origin =
            dimensionText == null
                ? null
                : ReadPoint2d(
                    ReadComObject(
                        diagnostics,
                        "Text.Origin",
                        () => dimensionText.Origin));

        object? rangeBox =
            dimensionText == null
                ? null
                : ReadBox2d(
                    ReadComObject(
                        diagnostics,
                        "Text.RangeBox",
                        () => dimensionText.RangeBox));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    note.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => note.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => note.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            text,
            formattedText,
            formattedHoleThreadNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedHoleThreadNote",
                        () => note.FormattedHoleThreadNote)),
            formattedQuantityNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedQuantityNote",
                        () => note.FormattedQuantityNote)),
            position =
                origin,
            origin,
            rangeBox,
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => note.Layer),
            dimensionStyle =
                ReadDimensionStyle(
                    diagnostics,
                    "Style",
                    () => note.Style),
            attached =
                ReadNullableBoolean(
                    diagnostics,
                    "Attached",
                    () => note.Attached),
            isHoleNote =
                ReadNullableBoolean(
                    diagnostics,
                    "IsHoleNote",
                    () => note.IsHoleNote),
            leaderFromCenter =
                ReadNullableBoolean(
                    diagnostics,
                    "LeaderFromCenter",
                    () => note.LeaderFromCenter),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "hole_thread_note",
                    position =
                        origin,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadBendNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        BendNote note,
        int index)
    {
        List<object> diagnostics =
            new();

        string text =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Text",
                    () => note.Text));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => note.Position));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    note.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => note.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => note.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            text,
            formattedText =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedText",
                        () => note.FormattedText)),
            formattedBendNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedBendNote",
                        () => note.FormattedBendNote)),
            position,
            origin =
                position,
            rangeBox =
                ReadBox2d(
                    ReadComObject(
                        diagnostics,
                        "RangeBox",
                        () => note.RangeBox)),
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => note.Layer),
            dimensionStyle =
                ReadDimensionStyle(
                    diagnostics,
                    "DimensionStyle",
                    () => note.DimensionStyle),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => note._AttachedEntity),
            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => note.Leader),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "bend_note",
                    position,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadChamferNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        ChamferNote note,
        int index)
    {
        List<object> diagnostics =
            new();

        string text =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Text",
                    () => note.Text));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => note.Position));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    note.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => note.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => note.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            text,
            formattedText =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedText",
                        () => note.FormattedText)),
            formattedChamferNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedChamferNote",
                        () => note.FormattedChamferNote)),
            position,
            origin =
                position,
            rangeBox =
                ReadBox2d(
                    ReadComObject(
                        diagnostics,
                        "RangeBox",
                        () => note.RangeBox)),
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => note.Layer),
            dimensionStyle =
                ReadDimensionStyle(
                    diagnostics,
                    "DimensionStyle",
                    () => note.DimensionStyle),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => note._AttachedEntity),
            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => note.Leader),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "chamfer_note",
                    position,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadPunchNote(
        DrawingDocument drawingDocument,
        Sheet sheet,
        PunchNote note,
        int index)
    {
        List<object> diagnostics =
            new();

        string text =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Text",
                    () => note.Text));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => note.Position));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    note.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => note.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => note.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            text,
            formattedText =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedText",
                        () => note.FormattedText)),
            formattedPunchNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedPunchNote",
                        () => note.FormattedPunchNote)),
            formattedQuantityNote =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "FormattedQuantityNote",
                        () => note.FormattedQuantityNote)),
            position,
            origin =
                position,
            rangeBox =
                ReadBox2d(
                    ReadComObject(
                        diagnostics,
                        "RangeBox",
                        () => note.RangeBox)),
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => note.Layer),
            dimensionStyle =
                ReadDimensionStyle(
                    diagnostics,
                    "DimensionStyle",
                    () => note.DimensionStyle),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => note._AttachedEntity),
            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => note.Leader),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "punch_note",
                    position,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static void ReadTextBoxesFromSketch(
        DrawingDocument drawingDocument,
        DrawingSketch sketch,
        int sketchIndex,
        List<object> items,
        List<object> collectionDiagnostics,
        ref int rawTextBoxCount,
        ref bool rawTextBoxCountComplete)
    {
        List<object> sketchDiagnostics =
            new();

        string sketchName =
            ReadString(
                ReadProperty(
                    sketchDiagnostics,
                    "Sketch.Name",
                    () => sketch.Name));

        bool? sketchVisible =
            ReadNullableBoolean(
                sketchDiagnostics,
                "Sketch.Visible",
                () => sketch.Visible);

        object? sketchReferenceKey =
            ReadReferenceKey(
                drawingDocument,
                sketchDiagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    sketch.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        TextBoxes? textBoxes =
            ReadComObject(
                sketchDiagnostics,
                "Sketch.TextBoxes",
                () => sketch.TextBoxes);

        if (textBoxes == null)
        {
            collectionDiagnostics.Add(
                new
                {
                    sketchIndex,
                    sketchName,
                    diagnostics =
                        sketchDiagnostics
                });

            rawTextBoxCountComplete =
                false;

            return;
        }

        int? textBoxCount =
            ReadNullableInt32(
                sketchDiagnostics,
                "Sketch.TextBoxes.Count",
                () => textBoxes.Count);

        if (textBoxCount.HasValue)
        {
            rawTextBoxCount +=
                textBoxCount.Value;
        }
        else
        {
            rawTextBoxCountComplete =
                false;
        }

        try
        {
            int textBoxIndex =
                0;

            foreach (TextBox textBox
                     in textBoxes)
            {
                textBoxIndex++;

                items.Add(
                    ReadTextBox(
                        drawingDocument,
                        textBox,
                        sketchIndex,
                        sketchName,
                        sketchVisible,
                        sketchReferenceKey,
                        textBoxIndex));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                sketchDiagnostics,
                "Sketch.TextBoxes.Enumeration",
                exception);
        }

        if (sketchDiagnostics.Count > 0)
        {
            collectionDiagnostics.Add(
                new
                {
                    sketchIndex,
                    sketchName,
                    diagnostics =
                        sketchDiagnostics
                });
        }
    }

    private static object ReadTextBox(
        DrawingDocument drawingDocument,
        TextBox textBox,
        int sketchIndex,
        string sketchName,
        bool? sketchVisible,
        object? sketchReferenceKey,
        int textBoxIndex)
    {
        List<object> diagnostics =
            new();

        string text =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Text",
                    () => textBox.Text));

        string formattedText =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "FormattedText",
                    () => textBox.FormattedText));

        object? origin =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Origin",
                    () => textBox.Origin));

        object? rangeBox =
            ReadBox2d(
                ReadComObject(
                    diagnostics,
                    "RangeBox",
                    () => textBox.RangeBox));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    textBox.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index =
                textBoxIndex,
            indexIsStable =
                false,
            sketchIndex,
            sketchIndexIsStable =
                false,
            sketchName,
            sketchVisible,
            sketchReferenceKey,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => textBox.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => textBox.Type),
            text,
            formattedText,
            origin,
            position =
                origin,
            rangeBox,
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => textBox.Layer),
            style =
                ReadTextStyle(
                    diagnostics,
                    "Style",
                    () => textBox.Style),
            singleLineText =
                ReadNullableBoolean(
                    diagnostics,
                    "SingleLineText",
                    () => textBox.SingleLineText),
            fitted =
                ReadNullableBoolean(
                    diagnostics,
                    "Fitted",
                    () => textBox.Fitted),
            fittedTextHeight =
                ReadNullableDouble(
                    diagnostics,
                    "FittedTextHeight",
                    () => textBox.FittedTextHeight),
            fittedTextWidth =
                ReadNullableDouble(
                    diagnostics,
                    "FittedTextWidth",
                    () => textBox.FittedTextWidth),
            showBoundaries =
                ReadNullableBoolean(
                    diagnostics,
                    "ShowBoundaries",
                    () => textBox.ShowBoundaries),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index =
                        textBoxIndex,
                    indexIsStable =
                        false,
                    sketchIndex,
                    sketchIndexIsStable =
                        false,
                    sketchName,
                    origin,
                    text
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadSketchedSymbol(
        DrawingDocument drawingDocument,
        Sheet sheet,
        SketchedSymbol symbol,
        int index)
    {
        List<object> diagnostics =
            new();

        string name =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Name",
                    () => symbol.Name));

        object? position =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    "Position",
                    () => symbol.Position));

        object? rangeBox =
            ReadBox2d(
                ReadComObject(
                    diagnostics,
                    "RangeBox",
                    () => symbol.RangeBox));

        object? referenceKey =
            ReadReferenceKey(
                drawingDocument,
                diagnostics,
                keyContext =>
                {
                    Array referenceKeyArray =
                        Array.CreateInstance(
                            typeof(byte),
                            0);

                    symbol.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    "Type",
                    () => symbol.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    "Type",
                    () => symbol.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            name,
            position,
            origin =
                position,
            rangeBox,
            layer =
                ReadLayer(
                    diagnostics,
                    "Layer",
                    () => symbol.Layer),
            attachedEntity =
                ReadGeometryIntent(
                    diagnostics,
                    "_AttachedEntity",
                    () => symbol._AttachedEntity),
            leader =
                ReadLeaderMetadata(
                    diagnostics,
                    "Leader",
                    () => symbol.Leader),
            leaderVisible =
                ReadNullableBoolean(
                    diagnostics,
                    "LeaderVisible",
                    () => symbol.LeaderVisible),
            symbolClipping =
                ReadNullableBoolean(
                    diagnostics,
                    "SymbolClipping",
                    () => symbol.SymbolClipping),
            resultTexts =
                ReadSketchedSymbolResultTexts(
                    symbol,
                    diagnostics),
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "sketched_symbol",
                    name,
                    position
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadSketchedSymbolResultTexts(
        SketchedSymbol symbol,
        List<object> symbolDiagnostics)
    {
        List<object> diagnostics =
            new();

        List<object> items =
            new();

        SketchedSymbolDefinition? definition =
            ReadComObject(
                diagnostics,
                "Definition",
                () => symbol.Definition);

        if (definition == null)
        {
            return CreateCollectionResult(
                null,
                items,
                diagnostics);
        }

        string definitionName =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Definition.Name",
                    () => definition.Name));

        DrawingSketch? sketch =
            ReadComObject(
                diagnostics,
                "Definition.Sketch",
                () => definition.Sketch);

        if (sketch == null)
        {
            return new
            {
                definitionName,
                rawCount =
                    (int?)null,
                itemCount =
                    items.Count,
                items,
                diagnostics
            };
        }

        TextBoxes? textBoxes =
            ReadComObject(
                diagnostics,
                "Definition.Sketch.TextBoxes",
                () => sketch.TextBoxes);

        if (textBoxes == null)
        {
            return new
            {
                definitionName,
                rawCount =
                    (int?)null,
                itemCount =
                    items.Count,
                items,
                diagnostics
            };
        }

        int? rawCount =
            ReadNullableInt32(
                diagnostics,
                "Definition.Sketch.TextBoxes.Count",
                () => textBoxes.Count);

        try
        {
            int textBoxIndex =
                0;

            foreach (TextBox textBox
                     in textBoxes)
            {
                textBoxIndex++;

                List<object> itemDiagnostics =
                    new();

                items.Add(
                    new
                    {
                        index =
                            textBoxIndex,
                        definitionText =
                            ReadString(
                                ReadProperty(
                                    itemDiagnostics,
                                    "Text",
                                    () => textBox.Text)),
                        definitionFormattedText =
                            ReadString(
                                ReadProperty(
                                    itemDiagnostics,
                                    "FormattedText",
                                    () => textBox.FormattedText)),
                        resultText =
                            ReadString(
                                ReadProperty(
                                    itemDiagnostics,
                                    "GetResultText",
                                    () => symbol.GetResultText(
                                        textBox))),
                        propertyDiagnostics =
                            itemDiagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Definition.Sketch.TextBoxes.Enumeration",
                exception);
        }

        if (diagnostics.Count > 0)
        {
            symbolDiagnostics.Add(
                new
                {
                    property =
                        "SketchedSymbol.ResultTexts",
                    diagnostics
                });
        }

        return new
        {
            definitionName,
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
        };
    }

    private static object CreateCollectionResult(
        int? rawCount,
        List<object> items,
        List<object> diagnostics)
    {
        return new
        {
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
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

    private static object? ReadPoint2d(
        Point2d? point)
    {
        if (point == null)
        {
            return null;
        }

        return new
        {
            x =
                point.X,
            y =
                point.Y
        };
    }

    private static object? ReadBox2d(
        Box2d? box)
    {
        if (box == null)
        {
            return null;
        }

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

    private static object? ReadLayer(
        List<object> diagnostics,
        string propertyName,
        Func<Layer> reader)
    {
        Layer? layer =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (layer == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Name",
                        () => layer.Name)),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => layer.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => layer.Type)
        };
    }

    private static object? ReadTextStyle(
        List<object> diagnostics,
        string propertyName,
        Func<TextStyle> reader)
    {
        TextStyle? textStyle =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (textStyle == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Name",
                        () => textStyle.Name)),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => textStyle.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => textStyle.Type)
        };
    }

    private static object? ReadDimensionStyle(
        List<object> diagnostics,
        string propertyName,
        Func<DimensionStyle> reader)
    {
        DimensionStyle? dimensionStyle =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (dimensionStyle == null)
        {
            return null;
        }

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        $"{propertyName}.Name",
                        () => dimensionStyle.Name)),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => dimensionStyle.Type),
            objectType =
                ReadObjectTypeName(
                    diagnostics,
                    $"{propertyName}.Type",
                    () => dimensionStyle.Type)
        };
    }

    private static object? ReadGeometryIntent(
        List<object> diagnostics,
        string propertyName,
        Func<GeometryIntent> reader)
    {
        GeometryIntent? geometryIntent =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (geometryIntent == null)
        {
            return null;
        }

        ObjectTypeEnum? objectType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.Type",
                () => geometryIntent.Type);

        IntentTypeEnum? intentType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.IntentType",
                () => geometryIntent.IntentType);

        object? pointOnSheet =
            ReadPoint2d(
                ReadComObject(
                    diagnostics,
                    $"{propertyName}.PointOnSheet",
                    () => geometryIntent.PointOnSheet));

        object? geometry =
            ReadProperty(
                diagnostics,
                $"{propertyName}.Geometry",
                () => geometryIntent.Geometry);

        return new
        {
            objectTypeRaw =
                ReadEnumRaw(
                    objectType),
            objectType =
                ReadEnumName(
                    objectType),
            intentTypeRaw =
                ReadEnumRaw(
                    intentType),
            intentType =
                ReadEnumName(
                    intentType),
            pointOnSheet,
            geometry =
                ReadRuntimeObjectMetadata(
                    geometry)
        };
    }

    private static object? ReadLeaderMetadata(
        List<object> diagnostics,
        string propertyName,
        Func<Leader> reader)
    {
        Leader? leader =
            ReadComObject(
                diagnostics,
                propertyName,
                reader);

        if (leader == null)
        {
            return null;
        }

        ObjectTypeEnum? objectType =
            ReadNullableEnum(
                diagnostics,
                $"{propertyName}.Type",
                () => leader.Type);

        return new
        {
            objectTypeRaw =
                ReadEnumRaw(
                    objectType),
            objectType =
                ReadEnumName(
                    objectType)
        };
    }

    private static object? ReadRuntimeObjectMetadata(
        object? value)
    {
        if (value == null)
        {
            return null;
        }

        return new
        {
            runtimeType =
                value.GetType().FullName
        };
    }

    private static object? ReadReferenceKey(
        DrawingDocument drawingDocument,
        List<object> diagnostics,
        Func<int, Array> getReferenceKey)
    {
        int keyContext =
            0;

        try
        {
            ReferenceKeyManager manager =
                drawingDocument.ReferenceKeyManager;

            keyContext =
                manager.CreateKeyContext();

            Array referenceKey =
                getReferenceKey(
                    keyContext);

            string referenceKeyString =
                manager.KeyToString(
                    ref referenceKey);

            return new
            {
                keyString =
                    referenceKeyString,
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
            if (keyContext !=
                0)
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

    private static string ReadObjectTypeRaw(
        List<object> diagnostics,
        string propertyName,
        Func<ObjectTypeEnum> reader)
    {
        ObjectTypeEnum? value =
            ReadNullableEnum(
                diagnostics,
                propertyName,
                reader);

        return ReadEnumRaw(
            value);
    }

    private static string ReadObjectTypeName(
        List<object> diagnostics,
        string propertyName,
        Func<ObjectTypeEnum> reader)
    {
        ObjectTypeEnum? value =
            ReadNullableEnum(
                diagnostics,
                propertyName,
                reader);

        return ReadEnumName(
            value);
    }

    private static T? ReadComObject<T>(
        List<object> diagnostics,
        string propertyName,
        Func<T> reader)
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
                propertyName,
                exception);

            return null;
        }
    }

    private static object? ReadProperty(
        List<object> diagnostics,
        string propertyName,
        Func<object?> reader)
    {
        try
        {
            return reader();
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

    private static int? ReadNullableInt32(
        List<object> diagnostics,
        string propertyName,
        Func<int> reader)
    {
        try
        {
            return reader();
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

    private static double? ReadNullableDouble(
        List<object> diagnostics,
        string propertyName,
        Func<double> reader)
    {
        try
        {
            return reader();
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

    private static bool? ReadNullableBoolean(
        List<object> diagnostics,
        string propertyName,
        Func<bool> reader)
    {
        try
        {
            return reader();
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

    private static TEnum? ReadNullableEnum<TEnum>(
        List<object> diagnostics,
        string propertyName,
        Func<TEnum> reader)
        where TEnum : struct
    {
        try
        {
            return reader();
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

    private static string ReadEnumRaw<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return Convert
            .ToInt32(
                value.Value)
            .ToString();
    }

    private static string ReadEnumName<TEnum>(
        TEnum? value)
        where TEnum : struct
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        return Enum.GetName(
                   typeof(TEnum),
                   value.Value)
               ?? value.Value.ToString()
               ?? string.Empty;
    }

    private static string ReadString(
        object? value)
    {
        return value?.ToString()
               ?? string.Empty;
    }

    private static void AddDiagnostic(
        List<object> diagnostics,
        string propertyName,
        Exception exception)
    {
        AddDiagnostic(
            diagnostics,
            propertyName,
            exception.Message);
    }

    private static void AddDiagnostic(
        List<object> diagnostics,
        string propertyName,
        string error)
    {
        diagnostics.Add(
            new
            {
                property =
                    propertyName,
                available =
                    false,
                error
            });
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

    private static string CreateSuccess(
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

    private static string CreateError(
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
