using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateChamferNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateChamferNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_chamfer_note";

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

        if (!ChamferNoteCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out string viewName,
                out int chamferEdgeOneCurveIndex,
                out int chamferEdgeTwoCurveIndex,
                out double x,
                out double y))
        {
            return ChamferNoteCommandSupport.CreateError(
                "Invalid create_chamfer_note input.",
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
                "Invalid create_chamfer_note input.",
                diagnostics);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = $"DrawingView \"{viewName}\" was not found on sheet \"{sheet.Name}\"." });
            return ChamferNoteCommandSupport.CreateError(
                "Invalid create_chamfer_note input.",
                diagnostics);
        }

        DrawingCurve? chamferEdgeOne =
            ChamferNoteCommandSupport.ResolveDrawingCurve(
                drawingView,
                chamferEdgeOneCurveIndex,
                "input.chamferEdgeOneCurveIndex",
                diagnostics);

        DrawingCurve? chamferEdgeTwo =
            ChamferNoteCommandSupport.ResolveDrawingCurve(
                drawingView,
                chamferEdgeTwoCurveIndex,
                "input.chamferEdgeTwoCurveIndex",
                diagnostics);

        if (chamferEdgeOne == null ||
            chamferEdgeTwo == null)
        {
            return ChamferNoteCommandSupport.CreateError(
                "Unable to resolve chamfer drawing curves.",
                diagnostics);
        }

        ChamferNotes? chamferNotes =
            ChamferNoteCommandSupport.ReadChamferNotesCollection(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.ChamferNotes");

        if (chamferNotes == null)
        {
            return ChamferNoteCommandSupport.CreateError(
                "Unable to access ChamferNotes collection.",
                diagnostics);
        }

        int? countBefore =
            ChamferNoteCommandSupport.ReadChamferNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.ChamferNotes.Count.BeforeCreate");

        ChamferNote createdChamferNote;

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            createdChamferNote =
                chamferNotes.Add(
                    position,
                    chamferEdgeOne,
                    chamferEdgeTwo,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "ChamferNotes.Add", message = exception.Message, exceptionType = exception.GetType().FullName, viewName = drawingView.Name, chamferEdgeOneCurveIndex, chamferEdgeTwoCurveIndex, requestedPosition = new { x, y } });
            return ChamferNoteCommandSupport.CreateError(
                "Failed to create chamfer note.",
                diagnostics);
        }

        if (createdChamferNote == null)
        {
            diagnostics.Add(new { scope = "ChamferNotes.Add", message = "Inventor returned null ChamferNote." });
            return ChamferNoteCommandSupport.CreateError(
                "Failed to create chamfer note.",
                diagnostics);
        }

        int? countAfter =
            ChamferNoteCommandSupport.ReadChamferNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.ChamferNotes.Count.AfterCreate");

        int createdChamferNoteIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object chamferNote =
            DrawingTextReadSupport.ReadChamferNoteSnapshot(
                drawingDocument,
                sheet,
                createdChamferNote,
                createdChamferNoteIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "ChamferNote.Create.Verification.Count", message = "Sheet.DrawingNotes.ChamferNotes.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        ChamferNoteCommandSupport.TryReadPosition(
            createdChamferNote,
            diagnostics,
            "ChamferNote.Create.Verification.Position",
            out _,
            out _);

        ChamferNoteCommandSupport.TryReadChamferEdges(
            createdChamferNote,
            diagnostics,
            "ChamferNote.Create.Verification.ChamferEdges");

        ChamferNoteCommandSupport.ReadReferenceKeyString(
            drawingDocument,
            createdChamferNote,
            diagnostics,
            "ChamferNote.Create.Verification.ReferenceKey");

        ChamferNoteCommandSupport.ReadText(
            createdChamferNote,
            diagnostics,
            "ChamferNote.Create.Verification.Text");

        if (!verificationPassed)
        {
            return ChamferNoteCommandSupport.CreateError(
                "Chamfer note creation was not factually verified.",
                diagnostics);
        }

        return ChamferNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_chamfer_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                view =
                    drawingView.Name,
                chamferEdgeOneCurveIndex,
                chamferEdgeTwoCurveIndex,
                creationApi =
                    "ChamferNotes.Add",
                dimensionStyleArgument =
                    "Type.Missing",
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                createdChamferNoteIndex,
                countBefore,
                countAfter,
                chamferNote,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
