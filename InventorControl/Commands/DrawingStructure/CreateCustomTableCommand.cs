using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateCustomTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateCustomTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_custom_table";

    public string Execute(
        JsonElement root)
    {
        List<object> diagnostics =
            new();

        DrawingDocument? drawingDocument =
            HoleThreadNoteCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError ?? "Unable to resolve active DrawingDocument." });
            return CustomTableCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!CustomTableCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out string title,
                out double x,
                out double y,
                out int numberOfColumns,
                out int numberOfRows,
                out string[] columnTitles,
                out bool contentsWasSupplied))
        {
            return CustomTableCommandSupport.CreateError(
                "Invalid create_custom_table input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return CustomTableCommandSupport.CreateError(
                "Invalid create_custom_table input.",
                diagnostics);
        }

        int? countBefore =
            CustomTableCommandSupport.ReadCustomTableCount(
                sheet,
                diagnostics,
                "Sheet.CustomTables.Count.BeforeCreate");

        CustomTable createdCustomTable;

        try
        {
            Point2d placementPoint =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            Array columnTitlesArray =
                CustomTableCommandSupport.CreateColumnTitlesArray(
                    columnTitles);

            createdCustomTable =
                sheet
                    .CustomTables
                    .Add(
                        title,
                        placementPoint,
                        numberOfColumns,
                        numberOfRows,
                        ref columnTitlesArray,
                        Type.Missing,
                        Type.Missing,
                        Type.Missing,
                        Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTables.Add", message = exception.Message, exceptionType = exception.GetType().FullName });
            return CustomTableCommandSupport.CreateError(
                "Failed to create custom table.",
                diagnostics);
        }

        if (createdCustomTable == null)
        {
            diagnostics.Add(new { scope = "CustomTables.Add", message = "Inventor returned null CustomTable." });
            return CustomTableCommandSupport.CreateError(
                "Failed to create custom table.",
                diagnostics);
        }

        int? countAfter =
            CustomTableCommandSupport.ReadCustomTableCount(
                sheet,
                diagnostics,
                "Sheet.CustomTables.Count.AfterCreate");

        int createdCustomTableIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object customTable =
            TableReadSupport.ReadCustomTable(
                drawingDocument,
                createdCustomTable,
                createdCustomTableIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "CustomTable.Create.Verification.Count", message = "Sheet.CustomTables.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        if (!CustomTableCommandSupport.TryReadPosition(
                createdCustomTable,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !CustomTableCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new { scope = "CustomTable.Create.Verification.Position", message = "Inventor did not report the requested custom table position after creation.", requestedPosition = new { x, y }, actualPosition = actualX.HasValue && actualY.HasValue ? new { x = actualX.Value, y = actualY.Value } : null });
            verificationPassed =
                false;
        }

        string? actualTitle =
            CustomTableCommandSupport.TryReadTitle(
                createdCustomTable,
                diagnostics);

        if (!string.Equals(
                actualTitle,
                title,
                StringComparison.Ordinal))
        {
            diagnostics.Add(new { scope = "CustomTable.Create.Verification.Title", message = "Inventor did not report the requested custom table title after creation.", requestedTitle = title, actualTitle });
            verificationPassed =
                false;
        }

        if (!CustomTableCommandSupport.TryReadStructure(
                createdCustomTable,
                diagnostics,
                out int? actualRowCount,
                out int? actualColumnCount) ||
            actualRowCount != numberOfRows ||
            actualColumnCount != numberOfColumns)
        {
            diagnostics.Add(new { scope = "CustomTable.Create.Verification.Structure", message = "Inventor did not report the requested custom table row/column counts after creation.", requested = new { numberOfRows, numberOfColumns }, actual = new { rowCount = actualRowCount, columnCount = actualColumnCount } });
            verificationPassed =
                false;
        }

        object data =
            new
            {
                capability =
                    "create_custom_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                requestedTitle =
                    title,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                requestedStructure =
                    new
                    {
                        numberOfColumns,
                        numberOfRows,
                        columnTitles
                    },
                contentsSupported =
                    false,
                contentsWasSupplied,
                contentsHandling =
                    "Package 51A passes Type.Missing for Contents; caller-supplied contents are deliberately deferred.",
                createdCustomTableIndex,
                countBefore,
                countAfter,
                customTable,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            };

        if (!verificationPassed)
        {
            return CustomTableCommandSupport.CreateError(
                "Custom table creation was not factually verified.",
                diagnostics);
        }

        return CustomTableCommandSupport.CreateSuccess(
            data);
    }
}
