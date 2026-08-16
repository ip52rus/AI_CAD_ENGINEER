using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteChamferNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteChamferNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_chamfer_note";

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
            return ChamferNoteCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!ChamferNoteCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int chamferNoteIndex))
        {
            return ChamferNoteCommandSupport.CreateError(
                "Invalid delete_chamfer_note input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return ChamferNoteCommandSupport.CreateError(
                "Invalid delete_chamfer_note input.",
                diagnostics);
        }

        int? countBefore =
            ChamferNoteCommandSupport.ReadChamferNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.ChamferNotes.Count.BeforeDelete");

        ChamferNote? chamferNote =
            ChamferNoteCommandSupport.ResolveChamferNote(
                sheet,
                chamferNoteIndex,
                diagnostics);

        if (chamferNote == null)
        {
            return ChamferNoteCommandSupport.CreateError(
                "Unable to resolve chamfer note.",
                diagnostics);
        }

        object deletedChamferNote =
            DrawingTextReadSupport.ReadChamferNoteSnapshot(
                drawingDocument,
                sheet,
                chamferNote,
                chamferNoteIndex);

        try
        {
            chamferNote.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "ChamferNote.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, chamferNoteIndex });
            return ChamferNoteCommandSupport.CreateError(
                "Failed to delete chamfer note.",
                diagnostics);
        }

        int? remainingChamferNoteCount =
            ChamferNoteCommandSupport.ReadChamferNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.ChamferNotes.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingChamferNoteCount.HasValue &&
            remainingChamferNoteCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "ChamferNote.Delete.Verification.Count", message = "Sheet.DrawingNotes.ChamferNotes.Count did not decrease by exactly 1.", countBefore, countAfter = remainingChamferNoteCount });
            return ChamferNoteCommandSupport.CreateError(
                "Chamfer note deletion was not factually verified.",
                diagnostics);
        }

        return ChamferNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_chamfer_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedChamferNoteIndex =
                    chamferNoteIndex,
                deletedChamferNote,
                remainingChamferNoteCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
