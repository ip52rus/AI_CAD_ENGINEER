using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetCustomTableCellValueCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetCustomTableCellValueCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_custom_table_cell_value";

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

        if (!CustomTableCommandSupport.TryGetSetCellValueInputs(
                root,
                diagnostics,
                out string sheetName,
                out int customTableIndex,
                out int rowIndex,
                out int columnIndex,
                out string value))
        {
            return CustomTableCommandSupport.CreateError(
                "Invalid set_custom_table_cell_value input.",
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
                "Invalid set_custom_table_cell_value input.",
                diagnostics);
        }

        CustomTable? customTable =
            CustomTableCommandSupport.ResolveCustomTable(
                sheet,
                customTableIndex,
                diagnostics);

        if (customTable == null)
        {
            return CustomTableCommandSupport.CreateError(
                "Unable to resolve custom table.",
                diagnostics);
        }

        Row? row =
            CustomTableCommandSupport.ResolveRow(
                customTable,
                rowIndex,
                diagnostics);

        if (row == null)
        {
            return CustomTableCommandSupport.CreateError(
                "Unable to resolve custom table row.",
                diagnostics);
        }

        Cell? cell =
            CustomTableCommandSupport.ResolveCell(
                row,
                rowIndex,
                columnIndex,
                diagnostics);

        if (cell == null)
        {
            return CustomTableCommandSupport.CreateError(
                "Unable to resolve custom table cell.",
                diagnostics);
        }

        string? previousValue =
            CustomTableCommandSupport.TryReadCellValue(
                cell,
                diagnostics,
                "Cell.Value.BeforeSet");

        try
        {
            cell.Value =
                value;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Cell.Value.Set", message = exception.Message, exceptionType = exception.GetType().FullName, row = rowIndex, column = columnIndex });
            return CustomTableCommandSupport.CreateError(
                "Failed to set custom table cell value.",
                diagnostics);
        }

        string? finalValue =
            CustomTableCommandSupport.TryReadCellValue(
                cell,
                diagnostics,
                "Cell.Value.AfterSet");

        if (!string.Equals(
                finalValue,
                value,
                StringComparison.Ordinal))
        {
            diagnostics.Add(new { scope = "Cell.Value.Verification", message = "Inventor did not report the requested cell value after write.", requestedValue = value, finalValue, row = rowIndex, column = columnIndex });
            return CustomTableCommandSupport.CreateError(
                "Custom table cell value write was not factually verified.",
                diagnostics);
        }

        return CustomTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    Name,
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                table =
                    CustomTableCommandSupport.ReadCustomTableIdentity(
                        customTable,
                        customTableIndex,
                        diagnostics),
                indexing =
                    "1-based customTableIndex, row, and column",
                row =
                    rowIndex,
                column =
                    columnIndex,
                previousValue,
                requestedValue =
                    value,
                finalValue,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
