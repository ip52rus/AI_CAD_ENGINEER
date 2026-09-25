using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteGeneralNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteGeneralNoteCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "delete_general_note";

    public string Execute(JsonElement root)
    {
        List<object> diagnostics = new();

        DrawingDocument? drawingDocument =
            HoleThreadNoteCommandSupport.GetActiveDrawingDocument(
                _inventor,
                out string? documentError);

        if (drawingDocument == null)
        {
            diagnostics.Add(new { scope = "activeDocument", message = documentError ?? "Unable to resolve active DrawingDocument." });
            return GeneralNoteCommandSupport.CreateError("Invalid active document.", diagnostics);
        }

        if (!TryResolveInputs(root, diagnostics, out string sheetName, out int noteIndex))
        {
            return GeneralNoteCommandSupport.CreateError("Invalid delete_general_note input.", diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return GeneralNoteCommandSupport.CreateError("Invalid delete_general_note input.", diagnostics);
        }

        GeneralNote? note =
            GeneralNoteCommandSupport.ResolveGeneralNote(
                sheet,
                noteIndex,
                diagnostics);

        if (note == null)
        {
            return GeneralNoteCommandSupport.CreateError("Unable to resolve general note.", diagnostics);
        }

        object noteBefore =
            GeneralNoteCommandSupport.ReadGeneralNoteFacts(
                drawingDocument,
                sheet,
                note,
                noteIndex);

        try
        {
            note.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralNote.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, noteIndex });
            return GeneralNoteCommandSupport.CreateError("Failed to delete general note.", diagnostics);
        }

        int? remainingNoteCount =
            null;

        try
        {
            remainingNoteCount =
                sheet
                    .DrawingNotes
                    .GeneralNotes
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralNotes.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return GeneralNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_general_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedNoteIndex =
                    noteIndex,
                deletedNote =
                    noteBefore,
                remainingNoteCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int noteIndex)
    {
        sheetName =
            string.Empty;
        noteIndex =
            0;

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredInt32(root, "noteIndex", out noteIndex, out string noteIndexError))
        {
            diagnostics.Add(new { scope = "input.noteIndex", message = noteIndexError });
            valid =
                false;
        }
        else if (noteIndex < 1)
        {
            diagnostics.Add(new { scope = "input.noteIndex", message = "noteIndex must be a positive 1-based integer." });
            valid =
                false;
        }

        return valid;
    }
}
