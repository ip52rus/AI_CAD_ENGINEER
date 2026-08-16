using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreatePunchNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreatePunchNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_punch_note";

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

        if (!PunchNoteCommandSupport.TryGetCreateInputs(
                root,
                diagnostics,
                out string sheetName,
                out string viewName,
                out int curveIndex,
                out double x,
                out double y))
        {
            return PunchNoteCommandSupport.CreateError(
                "Invalid create_punch_note input.",
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
                "Invalid create_punch_note input.",
                diagnostics);
        }

        DrawingView? drawingView =
            DrawingViewCommandSupport.FindDrawingView(
                sheet,
                viewName);

        if (drawingView == null)
        {
            diagnostics.Add(new { scope = "input.viewName", message = $"DrawingView \"{viewName}\" was not found on sheet \"{sheet.Name}\"." });
            return PunchNoteCommandSupport.CreateError(
                "Invalid create_punch_note input.",
                diagnostics);
        }

        DrawingCurve? drawingCurve =
            PunchNoteCommandSupport.ResolveDrawingCurve(
                drawingView,
                curveIndex,
                diagnostics);

        if (drawingCurve == null)
        {
            return PunchNoteCommandSupport.CreateError(
                "Unable to resolve punch drawing curve.",
                diagnostics);
        }

        if (!PunchNoteCommandSupport.TryReadEdgeType(
                drawingCurve,
                diagnostics,
                "DrawingCurve.EdgeType",
                out int? edgeTypeRaw,
                out string? edgeType,
                out DrawingEdgeTypeEnum? nativeEdgeType))
        {
            diagnostics.Add(new { scope = "PunchNote.Create.Validation.EdgeType", message = "DrawingCurve.EdgeType is required to safely create a punch note.", curveIndex, edgeTypeRaw, edgeType });
            return PunchNoteCommandSupport.CreateError(
                "Unable to validate punch drawing curve EdgeType.",
                diagnostics);
        }

        if (!nativeEdgeType.HasValue ||
            !PunchNoteCommandSupport.IsPunchEdge(
                nativeEdgeType.Value))
        {
            diagnostics.Add(new { scope = "PunchNote.Create.Validation.EdgeType", message = "Selected DrawingCurve is not a native punch edge. Expected kPunchUpEdge or kPunchDownEdge.", curveIndex, edgeTypeRaw, edgeType });
            return PunchNoteCommandSupport.CreateError(
                "Selected drawing curve is not a punch edge.",
                diagnostics);
        }

        GeometryIntent punchIntent;

        try
        {
            punchIntent =
                sheet.CreateGeometryIntent(
                    drawingCurve);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.CreateGeometryIntent", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex, edgeTypeRaw, edgeType });
            return PunchNoteCommandSupport.CreateError(
                "Failed to create punch GeometryIntent.",
                diagnostics);
        }

        object? punchIntentFacts =
            PunchNoteCommandSupport.ReadGeometryIntentFacts(
                punchIntent,
                diagnostics,
                "PunchNote.Create.PunchIntent");

        PunchNotes? punchNotes =
            PunchNoteCommandSupport.ReadPunchNotesCollection(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.PunchNotes");

        if (punchNotes == null)
        {
            return PunchNoteCommandSupport.CreateError(
                "Unable to access PunchNotes collection.",
                diagnostics);
        }

        int? countBefore =
            PunchNoteCommandSupport.ReadPunchNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.PunchNotes.Count.BeforeCreate");

        PunchNote createdPunchNote;

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            createdPunchNote =
                punchNotes.Add(
                    position,
                    punchIntent,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "PunchNotes.Add", message = exception.Message, exceptionType = exception.GetType().FullName, viewName = drawingView.Name, curveIndex, edgeTypeRaw, edgeType, requestedPosition = new { x, y } });
            return PunchNoteCommandSupport.CreateError(
                "Failed to create punch note.",
                diagnostics);
        }

        if (createdPunchNote == null)
        {
            diagnostics.Add(new { scope = "PunchNotes.Add", message = "Inventor returned null PunchNote." });
            return PunchNoteCommandSupport.CreateError(
                "Failed to create punch note.",
                diagnostics);
        }

        int? countAfter =
            PunchNoteCommandSupport.ReadPunchNoteCount(
                sheet,
                diagnostics,
                "Sheet.DrawingNotes.PunchNotes.Count.AfterCreate");

        int createdPunchNoteIndex =
            countAfter ??
            countBefore.GetValueOrDefault() + 1;

        object punchNote =
            DrawingTextReadSupport.ReadPunchNoteSnapshot(
                drawingDocument,
                sheet,
                createdPunchNote,
                createdPunchNoteIndex);

        bool verificationPassed =
            true;

        if (countBefore.HasValue &&
            countAfter.HasValue &&
            countAfter.Value != countBefore.Value + 1)
        {
            diagnostics.Add(new { scope = "PunchNote.Create.Verification.Count", message = "Sheet.DrawingNotes.PunchNotes.Count did not increase by exactly 1.", countBefore, countAfter });
            verificationPassed =
                false;
        }

        bool positionReadable =
            PunchNoteCommandSupport.TryReadPosition(
                createdPunchNote,
                diagnostics,
                "PunchNote.Create.Verification.Position",
                out _,
                out _);

        bool punchEdgeReadable =
            PunchNoteCommandSupport.TryReadPunchEdge(
                createdPunchNote,
                diagnostics,
                "PunchNote.Create.Verification.PunchEdge");

        PunchNoteCommandSupport.ReadReferenceKeyString(
            drawingDocument,
            createdPunchNote,
            diagnostics,
            "PunchNote.Create.Verification.ReferenceKey");

        PunchNoteCommandSupport.ReadText(
            createdPunchNote,
            diagnostics,
            "PunchNote.Create.Verification.Text");

        if (!positionReadable ||
            !punchEdgeReadable)
        {
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return PunchNoteCommandSupport.CreateError(
                "Punch note creation was not factually verified.",
                diagnostics);
        }

        return PunchNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_punch_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                view =
                    drawingView.Name,
                curveIndex,
                edgeTypeRaw,
                edgeType,
                creationApi =
                    "Sheet.CreateGeometryIntent + PunchNotes.Add",
                dimensionStyleArgument =
                    "Type.Missing",
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                createdPunchNoteIndex,
                countBefore,
                countAfter,
                punchIntent =
                    punchIntentFacts,
                punchNote,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
