using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteBendNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteBendNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_bend_note";

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
            return BendNoteCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!BendNoteCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int bendNoteIndex))
        {
            return BendNoteCommandSupport.CreateError(
                "Invalid delete_bend_note input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return BendNoteCommandSupport.CreateError(
                "Invalid delete_bend_note input.",
                diagnostics);
        }

        int? countBefore =
            BendNoteCommandSupport.ReadBendNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.BendNotes.Count.BeforeDelete");

        BendNote? bendNote =
            BendNoteCommandSupport.ResolveBendNote(
                sheet,
                bendNoteIndex,
                diagnostics);

        if (bendNote == null)
        {
            return BendNoteCommandSupport.CreateError(
                "Unable to resolve bend note.",
                diagnostics);
        }

        object deletedBendNote =
            DrawingTextReadSupport.ReadBendNoteSnapshot(
                drawingDocument,
                sheet,
                bendNote,
                bendNoteIndex);

        try
        {
            bendNote.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "BendNote.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, bendNoteIndex });
            return BendNoteCommandSupport.CreateError(
                "Failed to delete bend note.",
                diagnostics);
        }

        int? remainingBendNoteCount =
            BendNoteCommandSupport.ReadBendNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.BendNotes.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingBendNoteCount.HasValue &&
            remainingBendNoteCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "BendNote.Delete.Verification.Count", message = "Sheet.DrawingNotes.BendNotes.Count did not decrease by exactly 1.", countBefore, countAfter = remainingBendNoteCount });
            return BendNoteCommandSupport.CreateError(
                "Bend note deletion was not factually verified.",
                diagnostics);
        }

        return BendNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_bend_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedBendNoteIndex =
                    bendNoteIndex,
                deletedBendNote,
                remainingBendNoteCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
