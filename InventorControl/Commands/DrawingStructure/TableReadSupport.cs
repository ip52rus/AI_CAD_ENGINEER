using System.Text.Encodings.Web;
using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

internal static class TableReadSupport
{
    public delegate TableReadResult SheetTableReader(
        DrawingDocument drawingDocument,
        Sheet sheet);

    public static string ExecuteSheetRead(
        Inventor.Application inventor,
        JsonElement root,
        string capability,
        SheetTableReader reader)
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
            TableReadResult result =
                reader(
                    drawingDocument,
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

    public static string ExecuteTableCollectionsRead(
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

        try
        {
            return CreateSuccess(
                new
                {
                    document =
                        drawingDocument.DisplayName,

                    sheet =
                        sheet.Name,

                    usedActiveSheet,

                    capability =
                        "drawing_table_collections",

                    collections =
                        ReadDrawingTableCollections(
                            drawingDocument,
                            sheet),

                    diagnostics
                });
        }
        catch (Exception exception)
        {
            return CreateError(
                "Unable to read drawing table collections.",
                exception.Message);
        }
    }

    public static TableReadResult ReadPartsLists(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        PartsLists? partsLists =
            ReadProperty(
                diagnostics,
                "Sheet.PartsLists",
                () => sheet.PartsLists);

        if (partsLists == null)
        {
            return new TableReadResult(
                0,
                items,
                diagnostics);
        }

        int index =
            0;

        try
        {
            foreach (PartsList partsList
                     in partsLists)
            {
                index++;

                items.Add(
                    ReadPartsList(
                        drawingDocument,
                        sheet,
                        partsList,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.PartsLists",
                exception);
        }

        return new TableReadResult(
            items.Count,
            items,
            diagnostics);
    }

    private static object ReadDrawingTableCollections(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        return new
        {
            customTables =
                ReadCustomTables(
                    drawingDocument,
                    sheet),

            holeTables =
                ReadHoleTables(
                    drawingDocument,
                    sheet),

            partsLists =
                ReadPartsListCollectionSummary(
                    drawingDocument,
                    sheet),

            revisionTables =
                ReadRevisionTableCollectionSummary(
                    drawingDocument,
                    sheet)
        };
    }

    private static object ReadCustomTables(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        CustomTables? customTables =
            ReadProperty(
                diagnostics,
                "Sheet.CustomTables",
                () => sheet.CustomTables);

        int? rawCount =
            null;

        if (customTables != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "Sheet.CustomTables.Count",
                    () => customTables.Count);

            int index =
                0;

            try
            {
                foreach (CustomTable customTable
                         in customTables)
                {
                    index++;

                    items.Add(
                        ReadCustomTableSummary(
                            drawingDocument,
                            customTable,
                            index));
                }
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    "Sheet.CustomTables.Enumeration",
                    exception);
            }
        }

        return new
        {
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
        };
    }

    private static object ReadHoleTables(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        HoleTables? holeTables =
            ReadProperty(
                diagnostics,
                "Sheet.HoleTables",
                () => sheet.HoleTables);

        int? rawCount =
            null;

        if (holeTables != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "Sheet.HoleTables.Count",
                    () => holeTables.Count);

            int index =
                0;

            try
            {
                foreach (HoleTable holeTable
                         in holeTables)
                {
                    index++;

                    items.Add(
                        ReadHoleTableSummary(
                            drawingDocument,
                            holeTable,
                            index));
                }
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    "Sheet.HoleTables.Enumeration",
                    exception);
            }
        }

        return new
        {
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
        };
    }

    private static object ReadPartsListCollectionSummary(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        PartsLists? partsLists =
            ReadProperty(
                diagnostics,
                "Sheet.PartsLists",
                () => sheet.PartsLists);

        int? rawCount =
            null;

        if (partsLists != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "Sheet.PartsLists.Count",
                    () => partsLists.Count);

            int index =
                0;

            try
            {
                foreach (PartsList partsList
                         in partsLists)
                {
                    index++;

                    items.Add(
                        ReadPartsListSummary(
                            drawingDocument,
                            partsList,
                            index));
                }
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    "Sheet.PartsLists.Enumeration",
                    exception);
            }
        }

        return new
        {
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
        };
    }

    private static object ReadRevisionTableCollectionSummary(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        RevisionTables? revisionTables =
            ReadProperty(
                diagnostics,
                "Sheet.RevisionTables",
                () => sheet.RevisionTables);

        int? rawCount =
            null;

        if (revisionTables != null)
        {
            rawCount =
                ReadNullableInt32(
                    diagnostics,
                    "Sheet.RevisionTables.Count",
                    () => revisionTables.Count);

            int index =
                0;

            try
            {
                foreach (RevisionTable revisionTable
                         in revisionTables)
                {
                    index++;

                    items.Add(
                        ReadRevisionTableSummary(
                            drawingDocument,
                            revisionTable,
                            index));
                }
            }
            catch (Exception exception)
            {
                AddDiagnostic(
                    diagnostics,
                    "Sheet.RevisionTables.Enumeration",
                    exception);
            }
        }

        return new
        {
            rawCount,
            itemCount =
                items.Count,
            items,
            diagnostics
        };
    }

    private static object ReadCustomTableSummary(
        DrawingDocument drawingDocument,
        CustomTable customTable,
        int index)
    {
        List<object> diagnostics =
            new();

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => customTable.Title));

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => customTable.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => customTable.RangeBox),
                diagnostics,
                "RangeBox");

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

                    customTable.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        int? rowCount =
            ReadNullableInt32(
                diagnostics,
                "Rows.Count",
                () => customTable.Rows.Count);

        int? columnCount =
            ReadNullableInt32(
                diagnostics,
                "Columns.Count",
                () => customTable.Columns.Count);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    customTable.Type),
            objectType =
                ReadEnumName(
                    customTable.Type),
            title,
            position,
            rangeBox,
            rowCount,
            columnCount,
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadHoleTableSummary(
        DrawingDocument drawingDocument,
        HoleTable holeTable,
        int index)
    {
        List<object> diagnostics =
            new();

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => holeTable.Title));

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => holeTable.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => holeTable.RangeBox),
                diagnostics,
                "RangeBox");

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

                    holeTable.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        int? rowCount =
            ReadNullableInt32(
                diagnostics,
                "HoleTableRows.Count",
                () => holeTable.HoleTableRows.Count);

        int? columnCount =
            ReadNullableInt32(
                diagnostics,
                "HoleTableColumns.Count",
                () => holeTable.HoleTableColumns.Count);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    holeTable.Type),
            objectType =
                ReadEnumName(
                    holeTable.Type),
            title,
            position,
            rangeBox,
            rowCount,
            columnCount,
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadPartsListSummary(
        DrawingDocument drawingDocument,
        PartsList partsList,
        int index)
    {
        List<object> diagnostics =
            new();

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => partsList.Title));

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => partsList.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => partsList.RangeBox),
                diagnostics,
                "RangeBox");

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

                    partsList.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        int? rowCount =
            ReadNullableInt32(
                diagnostics,
                "PartsListRows.Count",
                () => partsList.PartsListRows.Count);

        int? columnCount =
            ReadNullableInt32(
                diagnostics,
                "PartsListColumns.Count",
                () => partsList.PartsListColumns.Count);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    partsList.Type),
            objectType =
                ReadEnumName(
                    partsList.Type),
            title,
            position,
            rangeBox,
            rowCount,
            columnCount,
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadRevisionTableSummary(
        DrawingDocument drawingDocument,
        RevisionTable revisionTable,
        int index)
    {
        List<object> diagnostics =
            new();

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => revisionTable.Title));

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => revisionTable.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => revisionTable.RangeBox),
                diagnostics,
                "RangeBox");

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

                    revisionTable.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        int? rowCount =
            ReadNullableInt32(
                diagnostics,
                "RevisionTableRows.Count",
                () => revisionTable.RevisionTableRows.Count);

        int? columnCount =
            ReadNullableInt32(
                diagnostics,
                "RevisionTableColumns.Count",
                () => revisionTable.RevisionTableColumns.Count);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    revisionTable.Type),
            objectType =
                ReadEnumName(
                    revisionTable.Type),
            title,
            position,
            rangeBox,
            rowCount,
            columnCount,
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    public static TableReadResult ReadRevisionTables(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        RevisionTables? revisionTables =
            ReadProperty(
                diagnostics,
                "Sheet.RevisionTables",
                () => sheet.RevisionTables);

        if (revisionTables == null)
        {
            return new TableReadResult(
                0,
                items,
                diagnostics);
        }

        int index =
            0;

        try
        {
            foreach (RevisionTable revisionTable
                     in revisionTables)
            {
                index++;

                items.Add(
                    ReadRevisionTable(
                        drawingDocument,
                        sheet,
                        revisionTable,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.RevisionTables",
                exception);
        }

        return new TableReadResult(
            items.Count,
            items,
            diagnostics);
    }

    public static TableReadResult ReadHoleTablesDetailed(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        HoleTables? holeTables =
            ReadProperty(
                diagnostics,
                "Sheet.HoleTables",
                () => sheet.HoleTables);

        if (holeTables == null)
        {
            return new TableReadResult(
                0,
                items,
                diagnostics);
        }

        int index =
            0;

        try
        {
            foreach (HoleTable holeTable
                     in holeTables)
            {
                index++;

                items.Add(
                    ReadHoleTableDetailed(
                        drawingDocument,
                        sheet,
                        holeTable,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.HoleTables.Enumeration",
                exception);
        }

        return new TableReadResult(
            items.Count,
            items,
            diagnostics);
    }

    public static TableReadResult ReadCustomTablesDetailed(
        DrawingDocument drawingDocument,
        Sheet sheet)
    {
        List<object> items =
            new();

        List<object> diagnostics =
            new();

        CustomTables? customTables =
            ReadProperty(
                diagnostics,
                "Sheet.CustomTables",
                () => sheet.CustomTables);

        if (customTables == null)
        {
            return new TableReadResult(
                0,
                items,
                diagnostics);
        }

        _ =
            ReadNullableInt32(
                diagnostics,
                "Sheet.CustomTables.Count",
                () => customTables.Count);

        int index =
            0;

        try
        {
            foreach (CustomTable customTable
                     in customTables)
            {
                index++;

                items.Add(
                    ReadCustomTableDetailed(
                        drawingDocument,
                        customTable,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "Sheet.CustomTables.Enumeration",
                exception);
        }

        return new TableReadResult(
            items.Count,
            items,
            diagnostics);
    }

    private static object ReadCustomTableDetailed(
        DrawingDocument drawingDocument,
        CustomTable customTable,
        int index)
    {
        List<object> diagnostics =
            new();

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => customTable.Title));

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => customTable.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => customTable.RangeBox),
                diagnostics,
                "RangeBox");

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

                    customTable.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        GenericTableColumnRead columns =
            ReadCustomTableColumns(
                customTable,
                diagnostics);

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    customTable.Type),
            objectType =
                ReadEnumName(
                    customTable.Type),
            title,
            position,
            rangeBox,
            rowCount =
                ReadNullableInt32(
                    diagnostics,
                    "Rows.Count",
                    () => customTable.Rows.Count),
            columnCount =
                ReadNullableInt32(
                    diagnostics,
                    "Columns.Count",
                    () => customTable.Columns.Count),
            referenceKey,
            columns =
                columns.Items,
            rows =
                ReadCustomTableRows(
                    customTable,
                    columns.Columns,
                    diagnostics),
            mergedCells =
                ReadCustomTableMergedCells(
                    customTable,
                    diagnostics),
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static GenericTableColumnRead ReadCustomTableColumns(
        CustomTable customTable,
        List<object> tableDiagnostics)
    {
        List<GenericTableColumnInfo> columns =
            new();

        List<object> items =
            new();

        Columns? tableColumns =
            ReadProperty(
                tableDiagnostics,
                "Columns",
                () => customTable.Columns);

        if (tableColumns == null)
        {
            return new GenericTableColumnRead(
                columns,
                items);
        }

        _ =
            ReadNullableInt32(
                tableDiagnostics,
                "Columns.Count",
                () => tableColumns.Count);

        int index =
            0;

        try
        {
            foreach (Column column
                     in tableColumns)
            {
                index++;

                List<object> diagnostics =
                    new();

                string title =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Title",
                            () => column.Title));

                string internalTitle =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "InternalTitle",
                            () => column.InternalTitle));

                columns.Add(
                    new GenericTableColumnInfo(
                        index,
                        title,
                        internalTitle));

                object? titleJustification =
                    ReadProperty(
                        diagnostics,
                        "TitleHorizontalJustification",
                        () => column.TitleHorizontalJustification);

                object? valueJustification =
                    ReadProperty(
                        diagnostics,
                        "ValueHorizontalJustification",
                        () => column.ValueHorizontalJustification);

                items.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                column.Type),
                        objectType =
                            ReadEnumName(
                                column.Type),
                        title,
                        internalTitle,
                        width =
                            ReadNullableDouble(
                                diagnostics,
                                "Width",
                                () => column.Width),
                        titleHorizontalJustificationRaw =
                            ReadEnumRaw(
                                titleJustification),
                        titleHorizontalJustification =
                            ReadEnumName(
                                titleJustification),
                        valueHorizontalJustificationRaw =
                            ReadEnumRaw(
                                valueJustification),
                        valueHorizontalJustification =
                            ReadEnumName(
                                valueJustification),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "Columns.Enumeration",
                exception);
        }

        return new GenericTableColumnRead(
            columns,
            items);
    }

    private static List<object> ReadCustomTableRows(
        CustomTable customTable,
        IReadOnlyList<GenericTableColumnInfo> columns,
        List<object> tableDiagnostics)
    {
        List<object> rows =
            new();

        Rows? tableRows =
            ReadProperty(
                tableDiagnostics,
                "Rows",
                () => customTable.Rows);

        if (tableRows == null)
        {
            return rows;
        }

        _ =
            ReadNullableInt32(
                tableDiagnostics,
                "Rows.Count",
                () => tableRows.Count);

        int rowIndex =
            0;

        try
        {
            foreach (Row row
                     in tableRows)
            {
                rowIndex++;

                rows.Add(
                    ReadCustomTableRow(
                        row,
                        rowIndex,
                        columns));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "Rows.Enumeration",
                exception);
        }

        return rows;
    }

    private static object ReadCustomTableRow(
        Row row,
        int rowIndex,
        IReadOnlyList<GenericTableColumnInfo> columns)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index =
                rowIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    row.Type),
            objectType =
                ReadEnumName(
                    row.Type),
            height =
                ReadNullableDouble(
                    diagnostics,
                    "Height",
                    () => row.Height),
            visible =
                ReadNullableBoolean(
                    diagnostics,
                    "Visible",
                    () => row.Visible),
            cellCount =
                ReadNullableInt32(
                    diagnostics,
                    "Count",
                    () => row.Count),
            cells =
                ReadCustomTableCells(
                    row,
                    rowIndex,
                    columns,
                    diagnostics),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static List<object> ReadCustomTableCells(
        Row row,
        int rowIndex,
        IReadOnlyList<GenericTableColumnInfo> columns,
        List<object> rowDiagnostics)
    {
        List<object> cells =
            new();

        int columnIndex =
            0;

        try
        {
            foreach (Cell cell
                     in row)
            {
                columnIndex++;

                cells.Add(
                    ReadCustomTableCell(
                        cell,
                        rowIndex,
                        columnIndex,
                        columns));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                rowDiagnostics,
                "Row.Cells",
                exception);
        }

        return cells;
    }

    private static object ReadCustomTableCell(
        Cell cell,
        int fallbackRowIndex,
        int fallbackColumnIndex,
        IReadOnlyList<GenericTableColumnInfo> columns)
    {
        List<object> diagnostics =
            new();

        int? rowIndex =
            ReadNullableInt32(
                diagnostics,
                "Row",
                () => cell.Row);

        int? columnIndex =
            ReadNullableInt32(
                diagnostics,
                "Column",
                () => cell.Column);

        GenericTableColumnInfo? column =
            FindGenericTableColumn(
                columns,
                columnIndex ??
                fallbackColumnIndex);

        return new
        {
            rowIndex =
                rowIndex ??
                fallbackRowIndex,
            columnIndex =
                columnIndex ??
                fallbackColumnIndex,
            columnTitle =
                column?.Title
                ?? string.Empty,
            columnInternalTitle =
                column?.InternalTitle
                ?? string.Empty,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    cell.Type),
            objectType =
                ReadEnumName(
                    cell.Type),
            value =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "Value",
                        () => cell.Value)),
            isStatic =
                ReadNullableBoolean(
                    diagnostics,
                    "Static",
                    () => cell.Static),
            isMerged =
                ReadNullableBoolean(
                    diagnostics,
                    "IsMerged",
                    () => cell.IsMerged),
            mergedCell =
                ReadMergedCell(
                    ReadProperty(
                        diagnostics,
                        "MergedCell",
                        () => cell.MergedCell),
                    diagnostics,
                    "MergedCell"),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static List<object> ReadCustomTableMergedCells(
        CustomTable customTable,
        List<object> tableDiagnostics)
    {
        List<object> mergedCells =
            new();

        MergedCellsEnumerator? mergedCellsEnumerator =
            ReadProperty(
                tableDiagnostics,
                "MergedCells",
                () => customTable.MergedCells);

        if (mergedCellsEnumerator == null)
        {
            return mergedCells;
        }

        _ =
            ReadNullableInt32(
                tableDiagnostics,
                "MergedCells.Count",
                () => mergedCellsEnumerator.Count);

        int index =
            0;

        try
        {
            foreach (MergedCell mergedCell
                     in mergedCellsEnumerator)
            {
                index++;

                mergedCells.Add(
                    ReadMergedCellItem(
                        mergedCell,
                        index));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "MergedCells.Enumeration",
                exception);
        }

        return mergedCells;
    }

    private static object ReadMergedCellItem(
        MergedCell mergedCell,
        int index)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    mergedCell.Type),
            objectType =
                ReadEnumName(
                    mergedCell.Type),
            startCell =
                ReadCellCoordinate(
                    ReadProperty(
                        diagnostics,
                        "StartCell",
                        () => mergedCell.StartCell),
                    diagnostics,
                    "StartCell"),
            endCell =
                ReadCellCoordinate(
                    ReadProperty(
                        diagnostics,
                        "EndCell",
                        () => mergedCell.EndCell),
                    diagnostics,
                    "EndCell"),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object? ReadMergedCell(
        MergedCell? mergedCell,
        List<object> diagnostics,
        string propertyName)
    {
        if (mergedCell == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        object result =
            new
            {
                objectTypeRaw =
                    ReadObjectTypeRaw(
                        mergedCell.Type),
                objectType =
                    ReadEnumName(
                        mergedCell.Type),
                startCell =
                    ReadCellCoordinate(
                        ReadProperty(
                            propertyDiagnostics,
                            "StartCell",
                            () => mergedCell.StartCell),
                        propertyDiagnostics,
                        "StartCell"),
                endCell =
                    ReadCellCoordinate(
                        ReadProperty(
                            propertyDiagnostics,
                            "EndCell",
                            () => mergedCell.EndCell),
                        propertyDiagnostics,
                        "EndCell"),
                propertyDiagnostics
            };

        MergeDiagnostics(
            diagnostics,
            propertyName,
            propertyDiagnostics);

        return result;
    }

    private static object? ReadCellCoordinate(
        Cell? cell,
        List<object> diagnostics,
        string propertyName)
    {
        if (cell == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        object result =
            new
            {
                rowIndex =
                    ReadNullableInt32(
                        propertyDiagnostics,
                        "Row",
                        () => cell.Row),
                columnIndex =
                    ReadNullableInt32(
                        propertyDiagnostics,
                        "Column",
                        () => cell.Column),
                value =
                    ReadString(
                        ReadProperty(
                            propertyDiagnostics,
                            "Value",
                            () => cell.Value)),
                propertyDiagnostics
            };

        MergeDiagnostics(
            diagnostics,
            propertyName,
            propertyDiagnostics);

        return result;
    }

    public static object ReadPartsList(
        DrawingDocument drawingDocument,
        Sheet sheet,
        PartsList partsList,
        int index)
    {
        List<object> diagnostics =
            new();

        TableColumnRead partsListColumns =
            ReadPartsListColumns(
                partsList,
                diagnostics);

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => partsList.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => partsList.RangeBox),
                diagnostics,
                "RangeBox");

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

                    partsList.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        object? parentView =
            ReadDrawingViewMetadata(
                ReadProperty(
                    diagnostics,
                    "ParentView",
                    () => partsList.ParentView),
                diagnostics,
                "ParentView");

        object? referencedDocument =
            ReadDocumentDescriptor(
                ReadProperty(
                    diagnostics,
                    "ReferencedDocumentDescriptor",
                    () => partsList.ReferencedDocumentDescriptor),
                diagnostics,
                "ReferencedDocumentDescriptor");

        object? referencedFile =
            ReadReferencedFileDescriptor(
                ReadProperty(
                    diagnostics,
                    "ReferencedFile",
                    () => partsList.ReferencedFile),
                diagnostics,
                "ReferencedFile");

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => partsList.Title));

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    partsList.Type),
            objectType =
                ReadEnumName(
                    partsList.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            title,
            position,
            rangeBox,
            rotation =
                ReadNullableDouble(
                    diagnostics,
                    "Rotation",
                    () => partsList.Rotation),
            style =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "Style",
                        () => partsList.Style),
                    diagnostics,
                    "Style"),
            layer =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "Layer",
                        () => partsList.Layer),
                    diagnostics,
                    "Layer"),
            titleTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "TitleTextStyle",
                        () => partsList.TitleTextStyle),
                    diagnostics,
                    "TitleTextStyle"),
            columnHeaderTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "ColumnHeaderTextStyle",
                        () => partsList.ColumnHeaderTextStyle),
                    diagnostics,
                    "ColumnHeaderTextStyle"),
            dataTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "DataTextStyle",
                        () => partsList.DataTextStyle),
                    diagnostics,
                    "DataTextStyle"),
            showTitle =
                ReadNullableBoolean(
                    diagnostics,
                    "ShowTitle",
                    () => partsList.ShowTitle),
            converted =
                ReadNullableBoolean(
                    diagnostics,
                    "Converted",
                    () => partsList.Converted),
            hideZeroQuantityRows =
                ReadNullableBoolean(
                    diagnostics,
                    "HideZeroQuantityRows",
                    () => partsList.HideZeroQuantityRows),
            maximumRows =
                ReadNullableInt32(
                    diagnostics,
                    "MaximumRows",
                    () => partsList.MaximumRows),
            numberOfSections =
                ReadNullableInt32(
                    diagnostics,
                    "NumberOfSections",
                    () => partsList.NumberOfSections),
            levelRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "Level",
                        () => partsList.Level)),
            level =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "Level",
                        () => partsList.Level)),
            numberingSchemeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "NumberingScheme",
                        () => partsList.NumberingScheme)),
            numberingScheme =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "NumberingScheme",
                        () => partsList.NumberingScheme)),
            tableDirectionRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "TableDirection",
                        () => partsList.TableDirection)),
            tableDirection =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "TableDirection",
                        () => partsList.TableDirection)),
            headingPlacementRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "HeadingPlacement",
                        () => partsList.HeadingPlacement)),
            headingPlacement =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "HeadingPlacement",
                        () => partsList.HeadingPlacement)),
            rowGap =
                ReadNullableDouble(
                    diagnostics,
                    "RowGap",
                    () => partsList.RowGap),
            rowLineSpacingRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "RowLineSpacing",
                        () => partsList.RowLineSpacing)),
            rowLineSpacing =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "RowLineSpacing",
                        () => partsList.RowLineSpacing)),
            wrapAutomatically =
                ReadNullableBoolean(
                    diagnostics,
                    "WrapAutomatically",
                    () => partsList.WrapAutomatically),
            wrapLeft =
                ReadNullableBoolean(
                    diagnostics,
                    "WrapLeft",
                    () => partsList.WrapLeft),
            parentView,
            referencedDocument,
            referencedFile,
            filterSettings =
                ReadPartsListFilterSettings(
                    ReadProperty(
                        diagnostics,
                        "FilterSettings",
                        () => partsList.FilterSettings),
                    diagnostics),
            rowCount =
                ReadNullableInt32(
                    diagnostics,
                    "PartsListRows.Count",
                    () => partsList.PartsListRows.Count),
            columnCount =
                ReadNullableInt32(
                    diagnostics,
                    "PartsListColumns.Count",
                    () => partsList.PartsListColumns.Count),
            columns =
                partsListColumns.Items,
            rows =
                ReadPartsListRows(
                    partsList,
                    partsListColumns.Columns,
                    diagnostics),
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox,
                    parentView,
                    referencedDocument
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadHoleTableDetailed(
        DrawingDocument drawingDocument,
        Sheet sheet,
        HoleTable holeTable,
        int index)
    {
        List<object> diagnostics =
            new();

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => holeTable.Title));

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => holeTable.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => holeTable.RangeBox),
                diagnostics,
                "RangeBox");

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

                    holeTable.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        HoleTableColumnRead columns =
            ReadHoleTableColumns(
                holeTable,
                diagnostics);

        object? parentView =
            ReadDrawingViewMetadata(
                ReadProperty(
                    diagnostics,
                    "ParentView",
                    () => holeTable.ParentView),
                diagnostics,
                "ParentView");

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    holeTable.Type),
            objectType =
                ReadEnumName(
                    holeTable.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            title,
            position,
            origin =
                position,
            rangeBox,
            parentView,
            referencedView =
                parentView,
            holeTableTypeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "HoleTableType",
                        () => holeTable.HoleTableType)),
            holeTableType =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "HoleTableType",
                        () => holeTable.HoleTableType)),
            style =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "Style",
                        () => holeTable.Style),
                    diagnostics,
                    "Style"),
            layer =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "Layer",
                        () => holeTable.Layer),
                    diagnostics,
                    "Layer"),
            titleTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "TitleTextStyle",
                        () => holeTable.TitleTextStyle),
                    diagnostics,
                    "TitleTextStyle"),
            columnHeaderTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "ColumnHeaderTextStyle",
                        () => holeTable.ColumnHeaderTextStyle),
                    diagnostics,
                    "ColumnHeaderTextStyle"),
            dataTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "DataTextStyle",
                        () => holeTable.DataTextStyle),
                    diagnostics,
                    "DataTextStyle"),
            showTitle =
                ReadNullableBoolean(
                    diagnostics,
                    "ShowTitle",
                    () => holeTable.ShowTitle),
            arrangeByPosition =
                ReadNullableBoolean(
                    diagnostics,
                    "ArrangeByPosition",
                    () => holeTable.ArrangeByPosition),
            deleteTagsOnRollup =
                ReadNullableBoolean(
                    diagnostics,
                    "DeleteTagsOnRollup",
                    () => holeTable.DeleteTagsOnRollup),
            groupHoleTypes =
                ReadNullableBoolean(
                    diagnostics,
                    "GroupHoleTypes",
                    () => holeTable.GroupHoleTypes),
            includeCentermarks =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeCentermarks",
                    () => holeTable.IncludeCentermarks),
            includeCircularCuts =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeCircularCuts",
                    () => holeTable.IncludeCircularCuts),
            includeCounterBoreHoleFeatures =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeCounterBoreHoleFeatures",
                    () => holeTable.IncludeCounterBoreHoleFeatures),
            includeCounterSinkHoleFeatures =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeCounterSinkHoleFeatures",
                    () => holeTable.IncludeCounterSinkHoleFeatures),
            includeDrilledHoleFeatures =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeDrilledHoleFeatures",
                    () => holeTable.IncludeDrilledHoleFeatures),
            includeHoleFeatures =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeHoleFeatures",
                    () => holeTable.IncludeHoleFeatures),
            includeRecoveredPunchCenters =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeRecoveredPunchCenters",
                    () => holeTable.IncludeRecoveredPunchCenters),
            includeThreadedHoleFeatures =
                ReadNullableBoolean(
                    diagnostics,
                    "IncludeThreadedHoleFeatures",
                    () => holeTable.IncludeThreadedHoleFeatures),
            preserveTagging =
                ReadNullableBoolean(
                    diagnostics,
                    "PreserveTagging",
                    () => holeTable.PreserveTagging),
            reformatOnCustomHoleMatch =
                ReadNullableBoolean(
                    diagnostics,
                    "ReformatOnCustomHoleMatch",
                    () => holeTable.ReformatOnCustomHoleMatch),
            rowMergeTypeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "RowMergeType",
                        () => holeTable.RowMergeType)),
            rowMergeType =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "RowMergeType",
                        () => holeTable.RowMergeType)),
            secondaryTagModifierOnRollup =
                ReadNullableBoolean(
                    diagnostics,
                    "SecondaryTagModifierOnRollup",
                    () => holeTable.SecondaryTagModifierOnRollup),
            sequentialNumbering =
                ReadNullableBoolean(
                    diagnostics,
                    "SequentialNumbering",
                    () => holeTable.SequentialNumbering),
            headingPlacementRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "HeadingPlacement",
                        () => holeTable.HeadingPlacement)),
            headingPlacement =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "HeadingPlacement",
                        () => holeTable.HeadingPlacement)),
            rowRawCount =
                ReadNullableInt32(
                    diagnostics,
                    "HoleTableRows.Count",
                    () => holeTable.HoleTableRows.Count),
            rowCount =
                ReadNullableInt32(
                    diagnostics,
                    "HoleTableRows.Count",
                    () => holeTable.HoleTableRows.Count),
            rows =
                ReadHoleTableRows(
                    holeTable,
                    columns.Columns,
                    diagnostics),
            columnRawCount =
                ReadNullableInt32(
                    diagnostics,
                    "HoleTableColumns.Count",
                    () => holeTable.HoleTableColumns.Count),
            columnCount =
                ReadNullableInt32(
                    diagnostics,
                    "HoleTableColumns.Count",
                    () => holeTable.HoleTableColumns.Count),
            columns =
                columns.Items,
            referenceKey,
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    type =
                        "hole_table",
                    title,
                    position,
                    rangeBox,
                    parentView
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object ReadRevisionTable(
        DrawingDocument drawingDocument,
        Sheet sheet,
        RevisionTable revisionTable,
        int index)
    {
        List<object> diagnostics =
            new();

        TableColumnRead revisionColumns =
            ReadRevisionTableColumns(
                revisionTable,
                diagnostics);

        object? position =
            ReadPoint2d(
                ReadProperty(
                    diagnostics,
                    "Position",
                    () => revisionTable.Position),
                diagnostics,
                "Position");

        object? rangeBox =
            ReadBox2d(
                ReadProperty(
                    diagnostics,
                    "RangeBox",
                    () => revisionTable.RangeBox),
                diagnostics,
                "RangeBox");

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

                    revisionTable.GetReferenceKey(
                        ref referenceKeyArray,
                        keyContext);

                    return referenceKeyArray;
                });

        string title =
            ReadString(
                ReadProperty(
                    diagnostics,
                    "Title",
                    () => revisionTable.Title));

        return new
        {
            index,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    revisionTable.Type),
            objectType =
                ReadEnumName(
                    revisionTable.Type),
            parentSheet =
                ReadSheetMetadata(
                    sheet),
            title,
            position,
            rangeBox,
            rotation =
                ReadNullableDouble(
                    diagnostics,
                    "Rotation",
                    () => revisionTable.Rotation),
            style =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "Style",
                        () => revisionTable.Style),
                    diagnostics,
                    "Style"),
            layer =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "Layer",
                        () => revisionTable.Layer),
                    diagnostics,
                    "Layer"),
            titleTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "TitleTextStyle",
                        () => revisionTable.TitleTextStyle),
                    diagnostics,
                    "TitleTextStyle"),
            columnHeaderTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "ColumnHeaderTextStyle",
                        () => revisionTable.ColumnHeaderTextStyle),
                    diagnostics,
                    "ColumnHeaderTextStyle"),
            dataTextStyle =
                ReadNamedObject(
                    ReadProperty(
                        diagnostics,
                        "DataTextStyle",
                        () => revisionTable.DataTextStyle),
                    diagnostics,
                    "DataTextStyle"),
            showTitle =
                ReadNullableBoolean(
                    diagnostics,
                    "ShowTitle",
                    () => revisionTable.ShowTitle),
            isSheetScope =
                ReadNullableBoolean(
                    diagnostics,
                    "IsSheetScope",
                    () => revisionTable.IsSheetScope),
            updatePropertyToRevisionNumber =
                ReadNullableBoolean(
                    diagnostics,
                    "UpdatePropertyToRevisionNumber",
                    () => revisionTable.UpdatePropertyToRevisionNumber),
            maximumRows =
                ReadNullableInt32(
                    diagnostics,
                    "MaximumRows",
                    () => revisionTable.MaximumRows),
            numberOfSections =
                ReadNullableInt32(
                    diagnostics,
                    "NumberOfSections",
                    () => revisionTable.NumberOfSections),
            tableDirectionRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "TableDirection",
                        () => revisionTable.TableDirection)),
            tableDirection =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "TableDirection",
                        () => revisionTable.TableDirection)),
            headingPlacementRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "HeadingPlacement",
                        () => revisionTable.HeadingPlacement)),
            headingPlacement =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "HeadingPlacement",
                        () => revisionTable.HeadingPlacement)),
            headingGap =
                ReadNullableDouble(
                    diagnostics,
                    "HeadingGap",
                    () => revisionTable.HeadingGap),
            rowGap =
                ReadNullableDouble(
                    diagnostics,
                    "RowGap",
                    () => revisionTable.RowGap),
            rowLineSpacingRaw =
                ReadEnumRaw(
                    ReadProperty(
                        diagnostics,
                        "RowLineSpacing",
                        () => revisionTable.RowLineSpacing)),
            rowLineSpacing =
                ReadEnumName(
                    ReadProperty(
                        diagnostics,
                        "RowLineSpacing",
                        () => revisionTable.RowLineSpacing)),
            wrapAutomatically =
                ReadNullableBoolean(
                    diagnostics,
                    "WrapAutomatically",
                    () => revisionTable.WrapAutomatically),
            wrapLeft =
                ReadNullableBoolean(
                    diagnostics,
                    "WrapLeft",
                    () => revisionTable.WrapLeft),
            rowCount =
                ReadNullableInt32(
                    diagnostics,
                    "RevisionTableRows.Count",
                    () => revisionTable.RevisionTableRows.Count),
            columnCount =
                ReadNullableInt32(
                    diagnostics,
                    "RevisionTableColumns.Count",
                    () => revisionTable.RevisionTableColumns.Count),
            columns =
                revisionColumns.Items,
            rows =
                ReadRevisionTableRows(
                    revisionTable,
                    revisionColumns.Columns,
                    diagnostics),
            selectorSnapshot =
                new
                {
                    referenceKey,
                    index,
                    indexIsStable =
                        false,
                    title,
                    position,
                    rangeBox
                },
            propertyDiagnostics =
                diagnostics
        };
    }

    private static TableColumnRead ReadPartsListColumns(
        PartsList partsList,
        List<object> tableDiagnostics)
    {
        List<TableColumnInfo> columns =
            new();

        List<object> items =
            new();

        PartsListColumns? partsListColumns =
            ReadProperty(
                tableDiagnostics,
                "PartsListColumns",
                () => partsList.PartsListColumns);

        if (partsListColumns == null)
        {
            return new TableColumnRead(
                columns,
                items);
        }

        int index =
            0;

        try
        {
            foreach (PartsListColumn column
                     in partsListColumns)
            {
                index++;

                List<object> diagnostics =
                    new();

                string title =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Title",
                            () => column.Title));

                TableColumnInfo info =
                    new(
                        index,
                        title);

                columns.Add(
                    info);

                object? fileProperty =
                    ReadFilePropertyId(
                        diagnostics,
                        "GetFilePropertyId",
                        (out string propertySetId,
                         out int propertyId) =>
                        {
                            column.GetFilePropertyId(
                                out propertySetId,
                                out propertyId);
                        });

                object? propertyType =
                    ReadProperty(
                        diagnostics,
                        "PropertyType",
                        () => column.PropertyType);

                items.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                column.Type),
                        objectType =
                            ReadEnumName(
                                column.Type),
                        title,
                        customPropertyName =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "CustomPropertyName",
                                    () => column.CustomPropertyName)),
                        propertyTypeRaw =
                            ReadEnumRaw(
                                propertyType),
                        propertyType =
                            ReadEnumName(
                                propertyType),
                        fileProperty,
                        width =
                            ReadNullableDouble(
                                diagnostics,
                                "Width",
                                () => column.Width),
                        titleHorizontalJustificationRaw =
                            ReadEnumRaw(
                                ReadProperty(
                                    diagnostics,
                                    "TitleHorizontalJustification",
                                    () => column.TitleHorizontalJustification)),
                        titleHorizontalJustification =
                            ReadEnumName(
                                ReadProperty(
                                    diagnostics,
                                    "TitleHorizontalJustification",
                                    () => column.TitleHorizontalJustification)),
                        valueHorizontalJustificationRaw =
                            ReadEnumRaw(
                                ReadProperty(
                                    diagnostics,
                                    "ValueHorizontalJustification",
                                    () => column.ValueHorizontalJustification)),
                        valueHorizontalJustification =
                            ReadEnumName(
                                ReadProperty(
                                    diagnostics,
                                    "ValueHorizontalJustification",
                                    () => column.ValueHorizontalJustification)),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "PartsListColumns",
                exception);
        }

        return new TableColumnRead(
            columns,
            items);
    }

    private static TableColumnRead ReadRevisionTableColumns(
        RevisionTable revisionTable,
        List<object> tableDiagnostics)
    {
        List<TableColumnInfo> columns =
            new();

        List<object> items =
            new();

        RevisionTableColumns? revisionTableColumns =
            ReadProperty(
                tableDiagnostics,
                "RevisionTableColumns",
                () => revisionTable.RevisionTableColumns);

        if (revisionTableColumns == null)
        {
            return new TableColumnRead(
                columns,
                items);
        }

        int index =
            0;

        try
        {
            foreach (RevisionTableColumn column
                     in revisionTableColumns)
            {
                index++;

                List<object> diagnostics =
                    new();

                string title =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Title",
                            () => column.Title));

                TableColumnInfo info =
                    new(
                        index,
                        title);

                columns.Add(
                    info);

                object? fileProperty =
                    ReadFilePropertyId(
                        diagnostics,
                        "GetFilePropertyId",
                        (out string propertySetId,
                         out int propertyId) =>
                        {
                            column.GetFilePropertyId(
                                out propertySetId,
                                out propertyId);
                        });

                object? propertyType =
                    ReadProperty(
                        diagnostics,
                        "PropertyType",
                        () => column.PropertyType);

                items.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                column.Type),
                        objectType =
                            ReadEnumName(
                                column.Type),
                        title,
                        customPropertyName =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "CustomPropertyName",
                                    () => column.CustomPropertyName)),
                        propertyTypeRaw =
                            ReadEnumRaw(
                                propertyType),
                        propertyType =
                            ReadEnumName(
                                propertyType),
                        fileProperty,
                        width =
                            ReadNullableDouble(
                                diagnostics,
                                "Width",
                                () => column.Width),
                        titleHorizontalJustificationRaw =
                            ReadEnumRaw(
                                ReadProperty(
                                    diagnostics,
                                    "TitleHorizontalJustification",
                                    () => column.TitleHorizontalJustification)),
                        titleHorizontalJustification =
                            ReadEnumName(
                                ReadProperty(
                                    diagnostics,
                                    "TitleHorizontalJustification",
                                    () => column.TitleHorizontalJustification)),
                        valueHorizontalJustificationRaw =
                            ReadEnumRaw(
                                ReadProperty(
                                    diagnostics,
                                    "ValueHorizontalJustification",
                                    () => column.ValueHorizontalJustification)),
                        valueHorizontalJustification =
                            ReadEnumName(
                                ReadProperty(
                                    diagnostics,
                                    "ValueHorizontalJustification",
                                    () => column.ValueHorizontalJustification)),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "RevisionTableColumns",
                exception);
        }

        return new TableColumnRead(
            columns,
            items);
    }

    private static HoleTableColumnRead ReadHoleTableColumns(
        HoleTable holeTable,
        List<object> tableDiagnostics)
    {
        List<TableColumnInfo> columns =
            new();

        List<object> items =
            new();

        HoleTableColumns? holeTableColumns =
            ReadProperty(
                tableDiagnostics,
                "HoleTableColumns",
                () => holeTable.HoleTableColumns);

        if (holeTableColumns == null)
        {
            return new HoleTableColumnRead(
                columns,
                items);
        }

        int index =
            0;

        try
        {
            foreach (HoleTableColumn column
                     in holeTableColumns)
            {
                index++;

                List<object> diagnostics =
                    new();

                string title =
                    ReadString(
                        ReadProperty(
                            diagnostics,
                            "Title",
                            () => column.Title));

                columns.Add(
                    new TableColumnInfo(
                        index,
                        title));

                object? propertyType =
                    ReadProperty(
                        diagnostics,
                        "PropertyType",
                        () => column.PropertyType);

                items.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                column.Type),
                        objectType =
                            ReadEnumName(
                                column.Type),
                        title,
                        width =
                            ReadNullableDouble(
                                diagnostics,
                                "Width",
                                () => column.Width),
                        propertyTypeRaw =
                            ReadEnumRaw(
                                propertyType),
                        propertyType =
                            ReadEnumName(
                                propertyType),
                        customPropertyName =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "CustomPropertyName",
                                    () => column.CustomPropertyName)),
                        unitsFormatting =
                            ReadUnitsFormatting(
                                ReadProperty(
                                    diagnostics,
                                    "UnitsFormatting",
                                    () => column.UnitsFormatting),
                                diagnostics,
                                "UnitsFormatting"),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "HoleTableColumns",
                exception);
        }

        return new HoleTableColumnRead(
            columns,
            items);
    }

    private static List<object> ReadPartsListRows(
        PartsList partsList,
        IReadOnlyList<TableColumnInfo> columns,
        List<object> tableDiagnostics)
    {
        List<object> rows =
            new();

        PartsListRows? partsListRows =
            ReadProperty(
                tableDiagnostics,
                "PartsListRows",
                () => partsList.PartsListRows);

        if (partsListRows == null)
        {
            return rows;
        }

        int rowIndex =
            0;

        try
        {
            foreach (PartsListRow row
                     in partsListRows)
            {
                rowIndex++;

                rows.Add(
                    ReadPartsListRow(
                        row,
                        rowIndex,
                        columns));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "PartsListRows",
                exception);
        }

        return rows;
    }

    private static object ReadPartsListRow(
        PartsListRow row,
        int rowIndex,
        IReadOnlyList<TableColumnInfo> columns)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index =
                rowIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    row.Type),
            objectType =
                ReadEnumName(
                    row.Type),
            count =
                ReadNullableInt32(
                    diagnostics,
                    "Count",
                    () => row.Count),
            custom =
                ReadNullableBoolean(
                    diagnostics,
                    "Custom",
                    () => row.Custom),
            visible =
                ReadNullableBoolean(
                    diagnostics,
                    "Visible",
                    () => row.Visible),
            ballooned =
                ReadNullableBoolean(
                    diagnostics,
                    "Ballooned",
                    () => row.Ballooned),
            expandable =
                ReadNullableBoolean(
                    diagnostics,
                    "Expandable",
                    () => row.Expandable),
            expanded =
                ReadNullableBoolean(
                    diagnostics,
                    "Expanded",
                    () => row.Expanded),
            height =
                ReadNullableDouble(
                    diagnostics,
                    "Height",
                    () => row.Height),
            referencedFiles =
                ReadReferencedFileDescriptors(
                    ReadProperty(
                        diagnostics,
                        "ReferencedFiles",
                        () => row.ReferencedFiles),
                    diagnostics,
                    "ReferencedFiles"),
            referencedBomRows =
                ReadReferencedBomRows(
                    ReadProperty(
                        diagnostics,
                        "ReferencedRows",
                        () => row.ReferencedRows),
                    diagnostics,
                    "ReferencedRows"),
            cells =
                ReadPartsListCells(
                    row,
                    rowIndex,
                    columns,
                    diagnostics),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static List<object> ReadPartsListCells(
        PartsListRow row,
        int rowIndex,
        IReadOnlyList<TableColumnInfo> columns,
        List<object> rowDiagnostics)
    {
        List<object> cells =
            new();

        int columnIndex =
            0;

        try
        {
            foreach (PartsListCell cell
                     in row)
            {
                columnIndex++;

                List<object> diagnostics =
                    new();

                TableColumnInfo? column =
                    FindColumn(
                        columns,
                        columnIndex);

                cells.Add(
                    new
                    {
                        rowIndex,
                        columnIndex,
                        columnTitle =
                            column?.Title
                            ?? string.Empty,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                cell.Type),
                        objectType =
                            ReadEnumName(
                                cell.Type),
                        value =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "Value",
                                    () => cell.Value)),
                        isStatic =
                            ReadNullableBoolean(
                                diagnostics,
                                "Static",
                                () => cell.Static),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                rowDiagnostics,
                "PartsListRow.Cells",
                exception);
        }

        return cells;
    }

    private static List<object> ReadHoleTableRows(
        HoleTable holeTable,
        IReadOnlyList<TableColumnInfo> columns,
        List<object> tableDiagnostics)
    {
        List<object> rows =
            new();

        HoleTableRows? holeTableRows =
            ReadProperty(
                tableDiagnostics,
                "HoleTableRows",
                () => holeTable.HoleTableRows);

        if (holeTableRows == null)
        {
            return rows;
        }

        int rowIndex =
            0;

        try
        {
            foreach (HoleTableRow row
                     in holeTableRows)
            {
                rowIndex++;

                rows.Add(
                    ReadHoleTableRow(
                        row,
                        rowIndex,
                        columns));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "HoleTableRows",
                exception);
        }

        return rows;
    }

    private static object ReadHoleTableRow(
        HoleTableRow row,
        int rowIndex,
        IReadOnlyList<TableColumnInfo> columns)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index =
                rowIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    row.Type),
            objectType =
                ReadEnumName(
                    row.Type),
            count =
                ReadNullableInt32(
                    diagnostics,
                    "Count",
                    () => row.Count),
            height =
                ReadNullableDouble(
                    diagnostics,
                    "Height",
                    () => row.Height),
            holeTag =
                ReadHoleTag(
                    ReadProperty(
                        diagnostics,
                        "HoleTag",
                        () => row.HoleTag),
                    diagnostics,
                    "HoleTag"),
            referencedHole =
                ReadGenericEntity(
                    ReadProperty(
                        diagnostics,
                        "ReferencedHole",
                        () => row.ReferencedHole),
                    diagnostics,
                    "ReferencedHole"),
            cells =
                ReadHoleTableCells(
                    row,
                    rowIndex,
                    columns,
                    diagnostics),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static List<object> ReadHoleTableCells(
        HoleTableRow row,
        int rowIndex,
        IReadOnlyList<TableColumnInfo> columns,
        List<object> rowDiagnostics)
    {
        List<object> cells =
            new();

        int columnIndex =
            0;

        try
        {
            foreach (HoleTableCell cell
                     in row)
            {
                columnIndex++;

                List<object> diagnostics =
                    new();

                TableColumnInfo? column =
                    FindColumn(
                        columns,
                        columnIndex);

                cells.Add(
                    new
                    {
                        rowIndex,
                        columnIndex,
                        columnTitle =
                            column?.Title
                            ?? string.Empty,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                cell.Type),
                        objectType =
                            ReadEnumName(
                                cell.Type),
                        text =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "Text",
                                    () => cell.Text)),
                        formattedText =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "FormattedText",
                                    () => cell.FormattedText)),
                        stackedTextPositionRaw =
                            ReadEnumRaw(
                                ReadProperty(
                                    diagnostics,
                                    "StackedTextPosition",
                                    () => cell.StackedTextPosition)),
                        stackedTextPosition =
                            ReadEnumName(
                                ReadProperty(
                                    diagnostics,
                                    "StackedTextPosition",
                                    () => cell.StackedTextPosition)),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                rowDiagnostics,
                "HoleTableRow.Cells",
                exception);
        }

        return cells;
    }

    private static List<object> ReadRevisionTableRows(
        RevisionTable revisionTable,
        IReadOnlyList<TableColumnInfo> columns,
        List<object> tableDiagnostics)
    {
        List<object> rows =
            new();

        RevisionTableRows? revisionTableRows =
            ReadProperty(
                tableDiagnostics,
                "RevisionTableRows",
                () => revisionTable.RevisionTableRows);

        if (revisionTableRows == null)
        {
            return rows;
        }

        int rowIndex =
            0;

        try
        {
            foreach (RevisionTableRow row
                     in revisionTableRows)
            {
                rowIndex++;

                rows.Add(
                    ReadRevisionTableRow(
                        row,
                        rowIndex,
                        columns));
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                tableDiagnostics,
                "RevisionTableRows",
                exception);
        }

        return rows;
    }

    private static object ReadRevisionTableRow(
        RevisionTableRow row,
        int rowIndex,
        IReadOnlyList<TableColumnInfo> columns)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index =
                rowIndex,
            indexIsStable =
                false,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    row.Type),
            objectType =
                ReadEnumName(
                    row.Type),
            count =
                ReadNullableInt32(
                    diagnostics,
                    "Count",
                    () => row.Count),
            custom =
                ReadNullableBoolean(
                    diagnostics,
                    "Custom",
                    () => row.Custom),
            visible =
                ReadNullableBoolean(
                    diagnostics,
                    "Visible",
                    () => row.Visible),
            isActiveRow =
                ReadNullableBoolean(
                    diagnostics,
                    "IsActiveRow",
                    () => row.IsActiveRow),
            height =
                ReadNullableDouble(
                    diagnostics,
                    "Height",
                    () => row.Height),
            cells =
                ReadRevisionTableCells(
                    row,
                    rowIndex,
                    columns,
                    diagnostics),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static List<object> ReadRevisionTableCells(
        RevisionTableRow row,
        int rowIndex,
        IReadOnlyList<TableColumnInfo> columns,
        List<object> rowDiagnostics)
    {
        List<object> cells =
            new();

        int columnIndex =
            0;

        try
        {
            foreach (RevisionTableCell cell
                     in row)
            {
                columnIndex++;

                List<object> diagnostics =
                    new();

                TableColumnInfo? column =
                    FindColumn(
                        columns,
                        columnIndex);

                cells.Add(
                    new
                    {
                        rowIndex,
                        columnIndex,
                        columnTitle =
                            column?.Title
                            ?? string.Empty,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                cell.Type),
                        objectType =
                            ReadEnumName(
                                cell.Type),
                        text =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "Text",
                                    () => cell.Text)),
                        formattedText =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "FormattedText",
                                    () => cell.FormattedText)),
                        stackedTextPositionRaw =
                            ReadEnumRaw(
                                ReadProperty(
                                    diagnostics,
                                    "StackedTextPosition",
                                    () => cell.StackedTextPosition)),
                        stackedTextPosition =
                            ReadEnumName(
                                ReadProperty(
                                    diagnostics,
                                    "StackedTextPosition",
                                    () => cell.StackedTextPosition)),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                rowDiagnostics,
                "RevisionTableRow.Cells",
                exception);
        }

        return cells;
    }

    private static object? ReadPartsListFilterSettings(
        PartsListFilterSettings? filterSettings,
        List<object> diagnostics)
    {
        if (filterSettings == null)
        {
            return null;
        }

        List<object> itemObjects =
            new();

        int index =
            0;

        try
        {
            foreach (PartsListFilterItem item
                     in filterSettings)
            {
                index++;

                List<object> itemDiagnostics =
                    new();

                object? filterItemType =
                    ReadProperty(
                        itemDiagnostics,
                        "FilterItemType",
                        () => item.FilterItemType);

                itemObjects.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                item.Type),
                        objectType =
                            ReadEnumName(
                                item.Type),
                        enabled =
                            ReadNullableBoolean(
                                itemDiagnostics,
                                "Enabled",
                                () => item.Enabled),
                        filterItemTypeRaw =
                            ReadEnumRaw(
                                filterItemType),
                        filterItemType =
                            ReadEnumName(
                                filterItemType),
                        options =
                            ReadNameValueMap(
                                ReadProperty(
                                    itemDiagnostics,
                                    "Options",
                                    () => item.Options),
                                itemDiagnostics,
                                "Options"),
                        propertyDiagnostics =
                            itemDiagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                "FilterSettings.Items",
                exception);
        }

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    filterSettings.Type),
            objectType =
                ReadEnumName(
                    filterSettings.Type),
            enabled =
                ReadNullableBoolean(
                    diagnostics,
                    "FilterSettings.Enabled",
                    () => filterSettings.Enabled),
            count =
                ReadNullableInt32(
                    diagnostics,
                    "FilterSettings.Count",
                    () => filterSettings.Count),
            items =
                itemObjects
        };
    }

    private static object? ReadNameValueMap(
        NameValueMap? map,
        List<object> diagnostics,
        string propertyName)
    {
        if (map == null)
        {
            return null;
        }

        List<object> items =
            new();

        try
        {
            for (int index = 1;
                 index <= map.Count;
                 index++)
            {
                string name =
                    map.Name[index];

                items.Add(
                    new
                    {
                        index,
                        name,
                        value =
                            map.Value[name]?.ToString()
                            ?? string.Empty
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);
        }

        return new
        {
            count =
                items.Count,
            items
        };
    }

    private static List<object> ReadReferencedFileDescriptors(
        ReferencedFileDescriptors? files,
        List<object> diagnostics,
        string propertyName)
    {
        List<object> items =
            new();

        if (files == null)
        {
            return items;
        }

        int index =
            0;

        try
        {
            foreach (ReferencedFileDescriptor file
                     in files)
            {
                index++;

                items.Add(
                    ReadReferencedFileDescriptor(
                        file,
                        diagnostics,
                        $"{propertyName}[{index}]")
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
                propertyName,
                exception);
        }

        return items;
    }

    private static object? ReadReferencedFileDescriptor(
        ReferencedFileDescriptor? file,
        List<object> diagnostics,
        string propertyName)
    {
        if (file == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        return new
        {
            displayName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "DisplayName",
                        () => file.DisplayName)),
            fullFileName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "FullFileName",
                        () => file.FullFileName)),
            documentFound =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "DocumentFound",
                    () => file.DocumentFound),
            differentDocument =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "DifferentDocument",
                    () => file.DifferentDocument),
            documentTypeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "DocumentType",
                        () => file.DocumentType)),
            documentType =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "DocumentType",
                        () => file.DocumentType)),
            referenceStatusRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferenceStatus",
                        () => file.ReferenceStatus)),
            referenceStatus =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferenceStatus",
                        () => file.ReferenceStatus)),
            documentDescriptor =
                ReadDocumentDescriptor(
                    ReadProperty(
                        propertyDiagnostics,
                        "DocumentDescriptor",
                        () => file.DocumentDescriptor),
                    propertyDiagnostics,
                    "DocumentDescriptor"),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static List<object> ReadReferencedBomRows(
        ObjectsEnumerator? rows,
        List<object> diagnostics,
        string propertyName)
    {
        List<object> items =
            new();

        if (rows == null)
        {
            return items;
        }

        int index =
            0;

        try
        {
            foreach (object rowObject
                     in rows)
            {
                index++;

                if (rowObject is DrawingBOMRow drawingBomRow)
                {
                    items.Add(
                        ReadDrawingBomRow(
                            drawingBomRow,
                            index));
                }
                else if (rowObject is BOMRow bomRow)
                {
                    items.Add(
                        ReadBomRow(
                            bomRow,
                            index)
                        ?? new
                        {
                            index
                        });
                }
                else
                {
                    items.Add(
                        ReadGenericObject(
                            rowObject,
                            index));
                }
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                diagnostics,
                propertyName,
                exception);
        }

        return items;
    }

    private static object ReadDrawingBomRow(
        DrawingBOMRow row,
        int index)
    {
        List<object> diagnostics =
            new();

        return new
        {
            index,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    row.Type),
            objectType =
                ReadEnumName(
                    row.Type),
            count =
                ReadNullableInt32(
                    diagnostics,
                    "Count",
                    () => row.Count),
            custom =
                ReadNullableBoolean(
                    diagnostics,
                    "Custom",
                    () => row.Custom),
            virtualRow =
                ReadNullableBoolean(
                    diagnostics,
                    "Virtual",
                    () => row.Virtual),
            ballooned =
                ReadNullableBoolean(
                    diagnostics,
                    "Ballooned",
                    () => row.Ballooned),
            bomRow =
                ReadBomRow(
                    ReadProperty(
                        diagnostics,
                        "BOMRow",
                        () => row.BOMRow),
                    index),
            cells =
                ReadDrawingBomCells(
                    row,
                    diagnostics),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static object? ReadBomRow(
        BOMRow? row,
        int index)
    {
        if (row == null)
        {
            return null;
        }

        List<object> diagnostics =
            new();

        object? bomStructure =
            ReadProperty(
                diagnostics,
                "BOMStructure",
                () => row.BOMStructure);

        return new
        {
            index,
            objectTypeRaw =
                ReadObjectTypeRaw(
                    row.Type),
            objectType =
                ReadEnumName(
                    row.Type),
            itemNumber =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "ItemNumber",
                        () => row.ItemNumber)),
            itemQuantity =
                ReadNullableInt32(
                    diagnostics,
                    "ItemQuantity",
                    () => row.ItemQuantity),
            totalQuantity =
                ReadString(
                    ReadProperty(
                        diagnostics,
                        "TotalQuantity",
                        () => row.TotalQuantity)),
            totalQuantityOverridden =
                ReadNullableBoolean(
                    diagnostics,
                    "TotalQuantityOverridden",
                    () => row.TotalQuantityOverridden),
            itemNumberLocked =
                ReadNullableBoolean(
                    diagnostics,
                    "ItemNumberLocked",
                    () => row.ItemNumberLocked),
            merged =
                ReadNullableBoolean(
                    diagnostics,
                    "Merged",
                    () => row.Merged),
            promoted =
                ReadNullableBoolean(
                    diagnostics,
                    "Promoted",
                    () => row.Promoted),
            rolledUp =
                ReadNullableBoolean(
                    diagnostics,
                    "RolledUp",
                    () => row.RolledUp),
            bomStructureRaw =
                ReadEnumRaw(
                    bomStructure),
            bomStructure =
                ReadEnumName(
                    bomStructure),
            referencedFileDescriptor =
                ReadFileDescriptor(
                    ReadProperty(
                        diagnostics,
                        "ReferencedFileDescriptor",
                        () => row.ReferencedFileDescriptor),
                    diagnostics,
                    "ReferencedFileDescriptor"),
            componentDefinitionCount =
                ReadCollectionCount(
                    ReadProperty(
                        diagnostics,
                        "ComponentDefinitions",
                        () => row.ComponentDefinitions),
                    diagnostics,
                    "ComponentDefinitions"),
            componentOccurrenceCount =
                ReadCollectionCount(
                    ReadProperty(
                        diagnostics,
                        "ComponentOccurrences",
                        () => row.ComponentOccurrences),
                    diagnostics,
                    "ComponentOccurrences"),
            propertyDiagnostics =
                diagnostics
        };
    }

    private static List<object> ReadDrawingBomCells(
        DrawingBOMRow row,
        List<object> rowDiagnostics)
    {
        List<object> cells =
            new();

        int index =
            0;

        try
        {
            foreach (DrawingBOMCell cell
                     in row)
            {
                index++;

                List<object> diagnostics =
                    new();

                cells.Add(
                    new
                    {
                        index,
                        objectTypeRaw =
                            ReadObjectTypeRaw(
                                cell.Type),
                        objectType =
                            ReadEnumName(
                                cell.Type),
                        value =
                            ReadString(
                                ReadProperty(
                                    diagnostics,
                                    "Value",
                                    () => cell.Value)),
                        isStatic =
                            ReadNullableBoolean(
                                diagnostics,
                                "Static",
                                () => cell.Static),
                        propertyDiagnostics =
                            diagnostics
                    });
            }
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                rowDiagnostics,
                "DrawingBOMRow.Cells",
                exception);
        }

        return cells;
    }

    private static object? ReadFileDescriptor(
        FileDescriptor? file,
        List<object> diagnostics,
        string propertyName)
    {
        if (file == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    file.Type),
            objectType =
                ReadEnumName(
                    file.Type),
            fullFileName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "FullFileName",
                        () => file.FullFileName)),
            relativeFileName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "RelativeFileName",
                        () => file.RelativeFileName)),
            resolvedFullFileName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "ResolvedFullFileName",
                        () => file.ResolvedFullFileName)),
            referencedFileInternalName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferencedFileInternalName",
                        () => file.ReferencedFileInternalName)),
            fileSaveCounter =
                ReadNullableInt32(
                    propertyDiagnostics,
                    "FileSaveCounter",
                    () => file.FileSaveCounter),
            referenceMissing =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceMissing",
                    () => file.ReferenceMissing),
            referenceDisabled =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceDisabled",
                    () => file.ReferenceDisabled),
            referenceReplaced =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceReplaced",
                    () => file.ReferenceReplaced),
            referenceLocationDifferent =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceLocationDifferent",
                    () => file.ReferenceLocationDifferent),
            referenceInternalNameDifferent =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceInternalNameDifferent",
                    () => file.ReferenceInternalNameDifferent),
            referencedFileTypeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferencedFileType",
                        () => file.ReferencedFileType)),
            referencedFileType =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferencedFileType",
                        () => file.ReferencedFileType)),
            locationTypeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "LocationType",
                        () => file.LocationType)),
            locationType =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "LocationType",
                        () => file.LocationType)),
            ownershipTypeRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "OwnershipType",
                        () => file.OwnershipType)),
            ownershipType =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "OwnershipType",
                        () => file.OwnershipType)),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object? ReadDocumentDescriptor(
        DocumentDescriptor? descriptor,
        List<object> diagnostics,
        string propertyName)
    {
        if (descriptor == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        object? referencedDocumentType =
            ReadProperty(
                propertyDiagnostics,
                "ReferencedDocumentType",
                () => descriptor.ReferencedDocumentType);

        return new
        {
            displayName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "DisplayName",
                        () => descriptor.DisplayName)),
            fullDocumentName =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "FullDocumentName",
                        () => descriptor.FullDocumentName)),
            referencedDocumentTypeRaw =
                ReadEnumRaw(
                    referencedDocumentType),
            referencedDocumentType =
                ReadEnumName(
                    referencedDocumentType),
            referenceMissing =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceMissing",
                    () => descriptor.ReferenceMissing),
            referenceSuppressed =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceSuppressed",
                    () => descriptor.ReferenceSuppressed),
            referenceDisabled =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceDisabled",
                    () => descriptor.ReferenceDisabled),
            referenceReplaced =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ReferenceReplaced",
                    () => descriptor.ReferenceReplaced),
            referencedModelStateRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferencedModelState",
                        () => descriptor.ReferencedModelState)),
            referencedModelState =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "ReferencedModelState",
                        () => descriptor.ReferencedModelState)),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object? ReadDrawingViewMetadata(
        DrawingView? view,
        List<object> diagnostics,
        string propertyName)
    {
        if (view == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        return new
        {
            name =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Name",
                        () => view.Name)),
            objectTypeRaw =
                ReadObjectTypeRaw(
                    view.Type),
            objectType =
                ReadEnumName(
                    view.Type),
            position =
                ReadPoint2d(
                    ReadProperty(
                        propertyDiagnostics,
                        "Position",
                        () => view.Position),
                    propertyDiagnostics,
                    "Position"),
            scale =
                ReadNullableDouble(
                    propertyDiagnostics,
                    "Scale",
                    () => view.Scale),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object? ReadHoleTag(
        HoleTag? holeTag,
        List<object> diagnostics,
        string propertyName)
    {
        if (holeTag == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        object? position =
            ReadPoint2d(
                ReadProperty(
                    propertyDiagnostics,
                    "Position",
                    () => holeTag.Position),
                propertyDiagnostics,
                "Position");

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    holeTag.Type),
            objectType =
                ReadEnumName(
                    holeTag.Type),
            text =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "Text",
                        () => holeTag.Text)),
            formattedText =
                ReadString(
                    ReadProperty(
                        propertyDiagnostics,
                        "FormattedText",
                        () => holeTag.FormattedText)),
            position,
            origin =
                position,
            rangeBox =
                ReadBox2d(
                    ReadProperty(
                        propertyDiagnostics,
                        "RangeBox",
                        () => holeTag.RangeBox),
                    propertyDiagnostics,
                    "RangeBox"),
            visible =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "Visible",
                    () => holeTag.Visible),
            showLeader =
                ReadNullableBoolean(
                    propertyDiagnostics,
                    "ShowLeader",
                    () => holeTag.ShowLeader),
            layer =
                ReadNamedObject(
                    ReadProperty(
                        propertyDiagnostics,
                        "Layer",
                        () => holeTag.Layer),
                    propertyDiagnostics,
                    "Layer"),
            dimensionStyle =
                ReadNamedObject(
                    ReadProperty(
                        propertyDiagnostics,
                        "DimensionStyle",
                        () => holeTag.DimensionStyle),
                    propertyDiagnostics,
                    "DimensionStyle"),
            stackedTextPositionRaw =
                ReadEnumRaw(
                    ReadProperty(
                        propertyDiagnostics,
                        "StackedTextPosition",
                        () => holeTag.StackedTextPosition)),
            stackedTextPosition =
                ReadEnumName(
                    ReadProperty(
                        propertyDiagnostics,
                        "StackedTextPosition",
                        () => holeTag.StackedTextPosition)),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object? ReadGenericEntity(
        object? entity,
        List<object> diagnostics,
        string propertyName)
    {
        if (entity == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        object? type =
            ReadDynamicProperty(
                entity,
                propertyDiagnostics,
                "Type");

        object? name =
            ReadDynamicProperty(
                entity,
                propertyDiagnostics,
                "Name");

        return new
        {
            objectTypeRaw =
                ReadEnumRaw(
                    type),
            objectType =
                ReadEnumName(
                    type),
            name =
                ReadString(
                    name),
            runtimeType =
                entity.GetType()
                    .FullName,
            text =
                entity.ToString()
                ?? string.Empty,
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object? ReadDynamicProperty(
        object owner,
        List<object> diagnostics,
        string propertyName)
    {
        try
        {
            dynamic dynamicOwner =
                owner;

            return propertyName switch
            {
                "Name" =>
                    dynamicOwner.Name,
                "Type" =>
                    dynamicOwner.Type,
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

    private static object? ReadUnitsFormatting(
        UnitsFormatting? unitsFormatting,
        List<object> diagnostics,
        string propertyName)
    {
        if (unitsFormatting == null)
        {
            return null;
        }

        List<object> propertyDiagnostics =
            new();

        return new
        {
            objectTypeRaw =
                ReadObjectTypeRaw(
                    unitsFormatting.Type),
            objectType =
                ReadEnumName(
                    unitsFormatting.Type),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object? ReadNamedObject(
        object? namedObject,
        List<object> diagnostics,
        string propertyName)
    {
        if (namedObject == null)
        {
            return null;
        }

        dynamic value =
            namedObject;

        List<object> propertyDiagnostics =
            new();

        string name =
            string.Empty;

        try
        {
            name =
                value.Name?.ToString()
                ?? string.Empty;
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                propertyDiagnostics,
                "Name",
                exception);
        }

        object? type =
            null;

        try
        {
            type =
                value.Type;
        }
        catch (Exception exception)
        {
            AddDiagnostic(
                propertyDiagnostics,
                "Type",
                exception);
        }

        return new
        {
            name,
            objectTypeRaw =
                ReadEnumRaw(
                    type),
            objectType =
                ReadEnumName(
                    type),
            propertyDiagnostics =
                MergeDiagnostics(
                    diagnostics,
                    propertyName,
                    propertyDiagnostics)
        };
    }

    private static object ReadGenericObject(
        object value,
        int index)
    {
        return new
        {
            index,
            runtimeType =
                value.GetType()
                    .FullName,
            text =
                value.ToString()
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
        Point2d? point,
        List<object> diagnostics,
        string propertyName)
    {
        if (point == null)
        {
            return null;
        }

        try
        {
            return new
            {
                x =
                    point.X,
                y =
                    point.Y
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

    private static object? ReadBox2d(
        Box2d? box,
        List<object> diagnostics,
        string propertyName)
    {
        if (box == null)
        {
            return null;
        }

        try
        {
            return new
            {
                minPoint =
                    ReadPoint2d(
                        box.MinPoint,
                        diagnostics,
                        $"{propertyName}.MinPoint"),
                maxPoint =
                    ReadPoint2d(
                        box.MaxPoint,
                        diagnostics,
                        $"{propertyName}.MaxPoint")
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

    private static object? ReadFilePropertyId(
        List<object> diagnostics,
        string propertyName,
        FilePropertyReader reader)
    {
        try
        {
            reader(
                out string propertySetId,
                out int propertyId);

            return new
            {
                propertySetId,
                propertyId
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

    private static int? ReadCollectionCount(
        object? collectionObject,
        List<object> diagnostics,
        string propertyName)
    {
        if (collectionObject == null)
        {
            return null;
        }

        dynamic collection =
            collectionObject;

        try
        {
            return (int)collection.Count;
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

    private static T? ReadProperty<T>(
        List<object> diagnostics,
        string propertyName,
        Func<T> reader)
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

            return default;
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

    private static int? ReadEnumRaw(
        object? value)
    {
        if (value == null)
        {
            return null;
        }

        if (value is not IConvertible)
        {
            return null;
        }

        return Convert.ToInt32(
            value);
    }

    private static int ReadObjectTypeRaw(
        ObjectTypeEnum value)
    {
        return Convert.ToInt32(
            value);
    }

    private static string ReadEnumName(
        object? value)
    {
        return value?.ToString()
               ?? string.Empty;
    }

    private static string ReadString(
        object? value)
    {
        return value?.ToString()
               ?? string.Empty;
    }

    private static TableColumnInfo? FindColumn(
        IReadOnlyList<TableColumnInfo> columns,
        int index)
    {
        return columns
            .FirstOrDefault(
                column =>
                    column.Index ==
                    index);
    }

    private static GenericTableColumnInfo? FindGenericTableColumn(
        IReadOnlyList<GenericTableColumnInfo> columns,
        int index)
    {
        return columns
            .FirstOrDefault(
                column =>
                    column.Index ==
                    index);
    }

    private static List<object> MergeDiagnostics(
        List<object> parentDiagnostics,
        string propertyName,
        List<object> propertyDiagnostics)
    {
        if (propertyDiagnostics.Count >
            0)
        {
            parentDiagnostics.Add(
                new
                {
                    property =
                        propertyName,
                    available =
                        false,
                    nestedDiagnostics =
                        propertyDiagnostics
                });
        }

        return propertyDiagnostics;
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

    private delegate void FilePropertyReader(
        out string propertySetId,
        out int propertyId);
}

internal sealed record TableReadResult(
    int Count,
    List<object> Items,
    List<object> Diagnostics);

internal sealed record TableColumnRead(
    List<TableColumnInfo> Columns,
    List<object> Items);

internal sealed record TableColumnInfo(
    int Index,
    string Title);

internal sealed record HoleTableColumnRead(
    List<TableColumnInfo> Columns,
    List<object> Items);

internal sealed record GenericTableColumnRead(
    List<GenericTableColumnInfo> Columns,
    List<object> Items);

internal sealed record GenericTableColumnInfo(
    int Index,
    string Title,
    string InternalTitle);
