using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetCustomTableColumnWidthCommand
    : IInventorCommand
{
    private readonly Inventor.Application
        _inventor;

    public SetCustomTableColumnWidthCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "set_custom_table_column_width";

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

        if (!CustomTableCommandSupport.TryGetSetColumnWidthInputs(
                root,
                diagnostics,
                out string sheetName,
                out int customTableIndex,
                out int columnIndex,
                out double width))
        {
            return CustomTableCommandSupport.CreateError(
                "Invalid set_custom_table_column_width input.",
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
                "Invalid set_custom_table_column_width input.",
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

        Column? column =
            CustomTableCommandSupport.ResolveColumn(
                customTable,
                columnIndex,
                diagnostics);

        if (column == null)
        {
            return CustomTableCommandSupport.CreateError(
                "Unable to resolve custom table column.",
                diagnostics);
        }

        double? previousWidth =
            CustomTableCommandSupport.TryReadColumnWidth(
                column,
                diagnostics,
                "Column.Width.BeforeSet");

        try
        {
            column.Width =
                width;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Column.Width.Set", message = exception.Message, exceptionType = exception.GetType().FullName, column = columnIndex });
            return CustomTableCommandSupport.CreateError(
                "Failed to set custom table column width.",
                diagnostics);
        }

        double? finalWidth =
            CustomTableCommandSupport.TryReadColumnWidth(
                column,
                diagnostics,
                "Column.Width.AfterSet");

        if (!finalWidth.HasValue ||
            Math.Abs(finalWidth.Value - width) > 0.0001)
        {
            diagnostics.Add(new { scope = "Column.Width.Verification", message = "Inventor did not report the requested column width after write.", requestedWidth = width, finalWidth, column = columnIndex });
            return CustomTableCommandSupport.CreateError(
                "Custom table column width write was not factually verified.",
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
                    "1-based customTableIndex and column",
                column =
                    columnIndex,
                previousWidth,
                requestedWidth =
                    width,
                finalWidth,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
