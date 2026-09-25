using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class SetGeneralNoteFormattedTextCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public SetGeneralNoteFormattedTextCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "set_general_note_formatted_text";

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

        if (!TryResolveInputs(root, diagnostics, out string sheetName, out int noteIndex, out string formattedText))
        {
            return GeneralNoteCommandSupport.CreateError("Invalid set_general_note_formatted_text input.", diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return GeneralNoteCommandSupport.CreateError("Invalid set_general_note_formatted_text input.", diagnostics);
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
            note.FormattedText =
                formattedText;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralNote.FormattedText.Set", message = exception.Message, exceptionType = exception.GetType().FullName, noteIndex });
            return GeneralNoteCommandSupport.CreateError("Failed to set general note formatted text.", diagnostics);
        }

        return GeneralNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "set_general_note_formatted_text",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                noteIndex,
                formattedText,
                noteBefore,
                noteAfter =
                    GeneralNoteCommandSupport.ReadGeneralNoteFacts(
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

        if (!GeneralNoteCommandSupport.TryGetRequiredStringAllowEmpty(root, "formattedText", diagnostics, out formattedText))
        {
            valid =
                false;
        }

        return valid;
    }
}
