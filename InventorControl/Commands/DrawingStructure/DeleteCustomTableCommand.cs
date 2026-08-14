using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteCustomTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteCustomTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_custom_table";

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

        if (!CustomTableCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int customTableIndex))
        {
            return CustomTableCommandSupport.CreateError(
                "Invalid delete_custom_table input.",
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
                "Invalid delete_custom_table input.",
                diagnostics);
        }

        int? countBefore =
            CustomTableCommandSupport.ReadCustomTableCount(
                sheet,
                diagnostics,
                "Sheet.CustomTables.Count.BeforeDelete");

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

        object deletedCustomTable =
            TableReadSupport.ReadCustomTable(
                drawingDocument,
                customTable,
                customTableIndex);

        try
        {
            customTable.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "CustomTable.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, customTableIndex });
            return CustomTableCommandSupport.CreateError(
                "Failed to delete custom table.",
                diagnostics);
        }

        int? remainingCustomTableCount =
            CustomTableCommandSupport.ReadCustomTableCount(
                sheet,
                diagnostics,
                "Sheet.CustomTables.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingCustomTableCount.HasValue &&
            remainingCustomTableCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "CustomTable.Delete.Verification.Count", message = "Sheet.CustomTables.Count did not decrease by exactly 1.", countBefore, countAfter = remainingCustomTableCount });
            return CustomTableCommandSupport.CreateError(
                "Custom table deletion was not factually verified.",
                diagnostics);
        }

        return CustomTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_custom_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedCustomTableIndex =
                    customTableIndex,
                deletedCustomTable,
                remainingCustomTableCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
