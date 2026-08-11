using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateLeaderNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateLeaderNoteCommand(Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(inventor);
        _inventor = inventor;
    }

    public string Name => "create_leader_note";

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

        if (!TryResolveInputs(
                root,
                diagnostics,
                out string sheetName,
                out List<(double X, double Y)> leaderPointInputs,
                out string formattedText,
                out bool hasAttachment,
                out string viewName,
                out int curveIndex,
                out string intentText,
                out PointIntentEnum pointIntent))
        {
            return LeaderNoteCommandSupport.CreateError("Invalid create_leader_note input.", diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return LeaderNoteCommandSupport.CreateError("Invalid create_leader_note input.", diagnostics);
        }

        GeometryIntent? attachmentIntent =
            null;

        if (hasAttachment)
        {
            DrawingView? drawingView =
                HoleThreadNoteCommandSupport.FindView(
                    sheet,
                    viewName);

            if (drawingView == null)
            {
                diagnostics.Add(new { scope = "input.viewName", message = $"DrawingView \"{viewName}\" was not found on the sheet." });
                return LeaderNoteCommandSupport.CreateError("Invalid create_leader_note attachment.", diagnostics);
            }

            DrawingCurve? drawingCurve =
                LeaderNoteCommandSupport.ResolveDrawingCurve(
                    drawingView,
                    curveIndex,
                    diagnostics);

            if (drawingCurve == null)
            {
                return LeaderNoteCommandSupport.CreateError("Invalid create_leader_note attachment.", diagnostics);
            }

            try
            {
                attachmentIntent =
                    sheet.CreateGeometryIntent(
                        drawingCurve,
                        pointIntent);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = "Sheet.CreateGeometryIntent", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex, intent = intentText });
                return LeaderNoteCommandSupport.CreateError("Failed to create leader note GeometryIntent.", diagnostics);
            }
        }

        LeaderNotes notes;

        try
        {
            notes =
                sheet
                    .DrawingNotes
                    .LeaderNotes;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.DrawingNotes.LeaderNotes", message = exception.Message, exceptionType = exception.GetType().FullName });
            return LeaderNoteCommandSupport.CreateError("Unable to access LeaderNotes collection.", diagnostics);
        }

        LeaderNote note;

        try
        {
            ObjectCollection leaderPoints =
                _inventor
                    .TransientObjects
                    .CreateObjectCollection();

            foreach ((double x, double y) in leaderPointInputs)
            {
                leaderPoints.Add(
                    _inventor
                        .TransientGeometry
                        .CreatePoint2d(
                            x,
                            y));
            }

            if (attachmentIntent != null)
            {
                leaderPoints.Add(
                    attachmentIntent);
            }

            note =
                notes.Add(
                    leaderPoints,
                    formattedText,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNotes.Add", message = exception.Message, exceptionType = exception.GetType().FullName, attached = hasAttachment });
            return LeaderNoteCommandSupport.CreateError("Failed to create leader note.", diagnostics);
        }

        int? createdNoteIndex = null;

        try
        {
            createdNoteIndex =
                notes.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "LeaderNotes.Count.AfterAdd", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return LeaderNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_leader_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                attached =
                    hasAttachment,
                requestedLeaderPoints =
                    LeaderNoteCommandSupport.FormatLeaderPoints(
                        leaderPointInputs),
                attachmentSelector =
                    hasAttachment
                        ? new
                        {
                            viewName,
                            curveIndex,
                            intent =
                                intentText,
                            intentRaw =
                                pointIntent.ToString()
                        }
                        : null,
                createdNoteIndex,
                note =
                    LeaderNoteCommandSupport.ReadLeaderNoteFacts(
                        drawingDocument,
                        sheet,
                        note,
                        createdNoteIndex ?? 0),
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out List<(double X, double Y)> leaderPoints,
        out string formattedText,
        out bool hasAttachment,
        out string viewName,
        out int curveIndex,
        out string intentText,
        out PointIntentEnum pointIntent)
    {
        sheetName =
            string.Empty;
        leaderPoints =
            new List<(double X, double Y)>();
        formattedText =
            string.Empty;
        hasAttachment =
            false;
        viewName =
            string.Empty;
        curveIndex =
            0;
        intentText =
            string.Empty;
        pointIntent =
            default;

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!LeaderNoteCommandSupport.TryGetLeaderPoints(root, diagnostics, out leaderPoints))
        {
            valid =
                false;
        }

        if (!LeaderNoteCommandSupport.TryGetRequiredStringAllowEmpty(root, "formattedText", diagnostics, out formattedText))
        {
            valid =
                false;
        }

        if (!LeaderNoteCommandSupport.TryResolveOptionalAttachmentInput(
                root,
                diagnostics,
                out hasAttachment,
                out viewName,
                out curveIndex,
                out intentText,
                out pointIntent))
        {
            valid =
                false;
        }

        return valid;
    }
}
