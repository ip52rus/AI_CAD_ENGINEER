using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteRevisionTableCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteRevisionTableCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_revision_table";

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
            return RevisionTableCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!RevisionTableCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int revisionTableIndex))
        {
            return RevisionTableCommandSupport.CreateError(
                "Invalid delete_revision_table input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return RevisionTableCommandSupport.CreateError(
                "Invalid delete_revision_table input.",
                diagnostics);
        }

        int? countBefore =
            RevisionTableCommandSupport.ReadRevisionTableCount(
                sheet,
                diagnostics,
                "Sheet.RevisionTables.Count.BeforeDelete");

        RevisionTable? revisionTable =
            RevisionTableCommandSupport.ResolveRevisionTable(
                sheet,
                revisionTableIndex,
                diagnostics);

        if (revisionTable == null)
        {
            return RevisionTableCommandSupport.CreateError(
                "Unable to resolve revision table.",
                diagnostics);
        }

        object deletedRevisionTable =
            TableReadSupport.ReadRevisionTableSnapshot(
                drawingDocument,
                sheet,
                revisionTable,
                revisionTableIndex);

        try
        {
            revisionTable.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionTable.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, revisionTableIndex });
            return RevisionTableCommandSupport.CreateError(
                "Failed to delete revision table.",
                diagnostics);
        }

        int? remainingRevisionTableCount =
            RevisionTableCommandSupport.ReadRevisionTableCount(
                sheet,
                diagnostics,
                "Sheet.RevisionTables.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingRevisionTableCount.HasValue &&
            remainingRevisionTableCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "RevisionTable.Delete.Verification.Count", message = "Sheet.RevisionTables.Count did not decrease by exactly 1.", countBefore, countAfter = remainingRevisionTableCount });
            return RevisionTableCommandSupport.CreateError(
                "Revision table deletion was not factually verified.",
                diagnostics);
        }

        return RevisionTableCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_revision_table",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedRevisionTableIndex =
                    revisionTableIndex,
                deletedRevisionTable,
                remainingRevisionTableCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
