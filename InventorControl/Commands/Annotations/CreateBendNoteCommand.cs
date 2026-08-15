using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateBendNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateBendNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_bend_note";

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

        if (!BendNoteCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out string viewName,
                out int curveIndex))
        {
            return BendNoteCommandSupport.CreateError(
                "Invalid create_bend_note input.",
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
                "Invalid create_bend_note input.",
                diagnostics);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = $"DrawingView \"{viewName}\" was not found on sheet \"{sheet.Name}\"." });
            return BendNoteCommandSupport.CreateError(
                "Invalid create_bend_note input.",
                diagnostics);
        }

        DrawingCurve? drawingCurve =
            BendNoteCommandSupport.ResolveDrawingCurve(
                drawingView,
                curveIndex,
                diagnostics);

        if (drawingCurve == null)
        {
            return BendNoteCommandSupport.CreateError(
                "Unable to resolve drawing curve.",
                diagnostics);
        }

        BendNotes? bendNotes =
            BendNoteCommandSupport.ReadBendNotesCollection(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.BendNotes");

        if (bendNotes == null)
        {
            return BendNoteCommandSupport.CreateError(
                "Unable to access BendNotes collection.",
                diagnostics);
        }

        int? countBefore =
            BendNoteCommandSupport.ReadBendNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.BendNotes.Count.BeforeCreate");

        BendNote createdBendNote;

        try
        {
            createdBendNote =
                bendNotes.Add(
                    drawingCurve,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "BendNotes.Add", message = exception.Message, exceptionType = exception.GetType().FullName, viewName = drawingView.Name, curveIndex });
            return BendNoteCommandSupport.CreateError(
                "Failed to create bend note.",
                diagnostics);
        }

        if (createdBendNote == null)
        {
            diagnostics.Add(new { scope = "BendNotes.Add", message = "Inventor returned null BendNote." });
            return BendNoteCommandSupport.CreateError(
                "Failed to create bend note.",
                diagnostics);
        }

        int? countAfter =
            BendNoteCommandSupport.ReadBendNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.BendNotes.Count.AfterCreate");

        int createdBendNoteIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object bendNote =
            DrawingTextReadSupport.ReadBendNoteSnapshot(
                drawingDocument,
                sheet,
                createdBendNote,
                createdBendNoteIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "BendNote.Create.Verification.Count", message = "Sheet.DrawingNotes.BendNotes.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        BendNoteCommandSupport.TryReadBendEdge(
            createdBendNote,
            diagnostics,
            "BendNote.Create.Verification.BendEdge");

        BendNoteCommandSupport.ReadReferenceKeyString(
            drawingDocument,
            createdBendNote,
            diagnostics,
            "BendNote.Create.Verification.ReferenceKey");

        if (!verificationPassed)
        {
            return BendNoteCommandSupport.CreateError(
                "Bend note creation was not factually verified.",
                diagnostics);
        }

        return BendNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_bend_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                view =
                    drawingView.Name,
                curveIndex,
                creationApi =
                    "BendNotes.Add",
                dimensionStyleArgument =
                    "Type.Missing",
                createdBendNoteIndex,
                countBefore,
                countAfter,
                bendNote,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
