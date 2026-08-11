using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveLeaderNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveLeaderNoteCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "move_leader_note";

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

        if (!TryResolveInputs(root, diagnostics, out string sheetName, out int noteIndex, out double x, out double y))
        {
            return LeaderNoteCommandSupport.CreateError("Invalid move_leader_note input.", diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return LeaderNoteCommandSupport.CreateError("Invalid move_leader_note input.", diagnostics);
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
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            note.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNote.Position.Set", message = exception.Message, exceptionType = exception.GetType().FullName, noteIndex });
            return LeaderNoteCommandSupport.CreateError("Failed to move leader note.", diagnostics);
        }

        return LeaderNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_leader_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                noteIndex,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
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
        out double x,
        out double y)
    {
        sheetName =
            string.Empty;
        noteIndex =
            0;
        x =
            0;
        y =
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

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "x", out x, out string xError))
        {
            diagnostics.Add(new { scope = "input.x", message = xError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "y", out y, out string yError))
        {
            diagnostics.Add(new { scope = "input.y", message = yError });
            valid =
                false;
        }

        return valid;
    }
}
