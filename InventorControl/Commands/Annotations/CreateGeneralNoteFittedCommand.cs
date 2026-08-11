using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateGeneralNoteFittedCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateGeneralNoteFittedCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "create_general_note_fitted";

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

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out string sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            return GeneralNoteCommandSupport.CreateError("Invalid create_general_note_fitted input.", diagnostics);
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "x", out double x, out string xError))
        {
            diagnostics.Add(new { scope = "input.x", message = xError });
            return GeneralNoteCommandSupport.CreateError("Invalid create_general_note_fitted input.", diagnostics);
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredDouble(root, "y", out double y, out string yError))
        {
            diagnostics.Add(new { scope = "input.y", message = yError });
            return GeneralNoteCommandSupport.CreateError("Invalid create_general_note_fitted input.", diagnostics);
        }

        if (!GeneralNoteCommandSupport.TryGetRequiredStringAllowEmpty(root, "formattedText", diagnostics, out string formattedText))
        {
            return GeneralNoteCommandSupport.CreateError("Invalid create_general_note_fitted input.", diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return GeneralNoteCommandSupport.CreateError("Invalid create_general_note_fitted input.", diagnostics);
        }

        GeneralNotes notes;

        try
        {
            notes =
                sheet
                    .DrawingNotes
                    .GeneralNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.GeneralNotes", message = exception.Message, exceptionType = exception.GetType().FullName });
            return GeneralNoteCommandSupport.CreateError("Unable to access GeneralNotes collection.", diagnostics);
        }

        GeneralNote note;

        try
        {
            Point2d placementPoint =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            note =
                notes.AddFitted(
                    placementPoint,
                    formattedText,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralNotes.AddFitted", message = exception.Message, exceptionType = exception.GetType().FullName });
            return GeneralNoteCommandSupport.CreateError("Failed to create fitted general note.", diagnostics);
        }

        int? createdNoteIndex = null;

        try
        {
            createdNoteIndex =
                notes.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "GeneralNotes.Count.AfterAddFitted", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return GeneralNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_general_note_fitted",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                createdNoteIndex,
                note =
                    GeneralNoteCommandSupport.ReadGeneralNoteFacts(
                        drawingDocument,
                        sheet,
                        note,
                        createdNoteIndex ?? 0),
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
