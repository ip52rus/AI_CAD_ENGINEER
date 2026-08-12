using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteHoleTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteHoleTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_hole_table";

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
            return HoleTableCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!HoleTableCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int holeTableIndex))
        {
            return HoleTableCommandSupport.CreateError(
                "Invalid delete_hole_table input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return HoleTableCommandSupport.CreateError(
                "Invalid delete_hole_table input.",
                diagnostics);
        }

        HoleTable? holeTable =
            HoleTableCommandSupport.ResolveHoleTable(
                sheet,
                holeTableIndex,
                diagnostics);

        if (holeTable == null)
        {
            return HoleTableCommandSupport.CreateError(
                "Unable to resolve hole table.",
                diagnostics);
        }

        object deletedHoleTable =
            TableReadSupport.ReadHoleTable(
                drawingDocument,
                sheet,
                holeTable,
                holeTableIndex);

        try
        {
            holeTable.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "HoleTable.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, holeTableIndex });
            return HoleTableCommandSupport.CreateError(
                "Failed to delete hole table.",
                diagnostics);
        }

        int? remainingHoleTableCount =
            HoleTableCommandSupport.ReadHoleTableCount(
                sheet,
                diagnostics,
                "Sheet.HoleTables.Count.AfterDelete");

        return HoleTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_hole_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedHoleTableIndex =
                    holeTableIndex,
                deletedHoleTable,
                remainingHoleTableCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
