using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeletePunchNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeletePunchNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_punch_note";

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
            return PunchNoteCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!PunchNoteCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int punchNoteIndex))
        {
            return PunchNoteCommandSupport.CreateError(
                "Invalid delete_punch_note input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return PunchNoteCommandSupport.CreateError(
                "Invalid delete_punch_note input.",
                diagnostics);
        }

        int? countBefore =
            PunchNoteCommandSupport.ReadPunchNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.PunchNotes.Count.BeforeDelete");

        PunchNote? punchNote =
            PunchNoteCommandSupport.ResolvePunchNote(
                sheet,
                punchNoteIndex,
                diagnostics);

        if (punchNote == null)
        {
            return PunchNoteCommandSupport.CreateError(
                "Unable to resolve punch note.",
                diagnostics);
        }

        object deletedPunchNote =
            DrawingTextReadSupport.ReadPunchNoteSnapshot(
                drawingDocument,
                sheet,
                punchNote,
                punchNoteIndex);

        try
        {
            punchNote.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "PunchNote.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, punchNoteIndex });
            return PunchNoteCommandSupport.CreateError(
                "Failed to delete punch note.",
                diagnostics);
        }

        int? remainingPunchNoteCount =
            PunchNoteCommandSupport.ReadPunchNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.PunchNotes.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingPunchNoteCount.HasValue &&
            remainingPunchNoteCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "PunchNote.Delete.Verification.Count", message = "Sheet.DrawingNotes.PunchNotes.Count did not decrease by exactly 1.", countBefore, countAfter = remainingPunchNoteCount });
            return PunchNoteCommandSupport.CreateError(
                "Punch note deletion was not factually verified.",
                diagnostics);
        }

        return PunchNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_punch_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedPunchNoteIndex =
                    punchNoteIndex,
                deletedPunchNote,
                remainingPunchNoteCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
