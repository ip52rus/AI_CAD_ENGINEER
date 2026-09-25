using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetLeaderNoteFormattedTextCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetLeaderNoteFormattedTextCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "set_leader_note_formatted_text";

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
            return LeaderNoteCommandSupport.CreateError("Invalid active document.", diagnostics);
        }

        if (!TryResolveInputs(root, diagnostics, out string sheetName, out int noteIndex, out string formattedText))
        {
            return LeaderNoteCommandSupport.CreateError("Invalid set_leader_note_formatted_text input.", diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return LeaderNoteCommandSupport.CreateError("Invalid set_leader_note_formatted_text input.", diagnostics);
        }

        LeaderNote? note =
            LeaderNoteCommandSupport.ResolveLeaderNote(
                sheet,
                noteIndex,
                diagnostics);

        if (note == null)
        {
            return LeaderNoteCommandSupport.CreateError("Unable to resolve leader note.", diagnostics);
        }

        object noteBefore =
            LeaderNoteCommandSupport.ReadLeaderNoteFacts(
                drawingDocument,
                sheet,
                note,
                noteIndex);

        try
        {
            note.FormattedText =
                formattedText;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNote.FormattedText.Set", message = exception.Message, exceptionType = exception.GetType().FullName, noteIndex });
            return LeaderNoteCommandSupport.CreateError("Failed to set leader note formatted text.", diagnostics);
        }

        return LeaderNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "set_leader_note_formatted_text",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                noteIndex,
                formattedText,
                noteBefore,
                noteAfter =
                    LeaderNoteCommandSupport.ReadLeaderNoteFacts(
                        drawingDocument,
                        sheet,
                        note,
                        noteIndex),
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int noteIndex,
        out string formattedText)
    {
        sheetName =
            string.Empty;
        noteIndex =
            0;
        formattedText =
            string.Empty;

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

        if (!LeaderNoteCommandSupport.TryGetRequiredStringAllowEmpty(root, "formattedText", diagnostics, out formattedText))
        {
            valid =
                false;
        }

        return valid;
    }
}
