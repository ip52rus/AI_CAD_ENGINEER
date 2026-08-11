using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class CreateFeatureControlFrameCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public CreateFeatureControlFrameCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "create_feature_control_frame";

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
            return FeatureControlFrameCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!TryResolveInputs(
                root,
                diagnostics,
                out string sheetName,
                out List<(double X, double Y)> leaderPointInputs,
                out FeatureControlFrameAttachmentInput? attachment,
                out List<FeatureControlFrameRowInput> rowInputs,
                out bool allAroundSymbol,
                out string datumIdentifier,
                out string notes))
        {
            return FeatureControlFrameCommandSupport.CreateError(
                "Invalid create_feature_control_frame input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return FeatureControlFrameCommandSupport.CreateError(
                "Invalid create_feature_control_frame input.",
                diagnostics);
        }

        GeometryIntent? attachmentIntent =
            null;

        if (attachment != null)
        {
            DrawingView? drawingView =
                HoleThreadNoteCommandSupport.FindView(
                    sheet,
                    attachment.ViewName);

            if (drawingView == null)
            {
                diagnostics.Add(new { scope = "input.attachment.viewName", message = $"DrawingView \"{attachment.ViewName}\" was not found on the sheet." });
                return FeatureControlFrameCommandSupport.CreateError(
                    "Invalid create_feature_control_frame attachment.",
                    diagnostics);
            }

            DrawingCurve? drawingCurve =
                FeatureControlFrameCommandSupport.ResolveDrawingCurve(
                    drawingView,
                    attachment.CurveIndex,
                    diagnostics);

            if (drawingCurve == null)
            {
                return FeatureControlFrameCommandSupport.CreateError(
                    "Invalid create_feature_control_frame attachment.",
                    diagnostics);
            }

            try
            {
                attachmentIntent =
                    sheet.CreateGeometryIntent(
                        drawingCurve,
                        attachment.PointIntent);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new { scope = "Sheet.CreateGeometryIntent", message = exception.Message, exceptionType = exception.GetType().FullName, curveIndex = attachment.CurveIndex, intent = attachment.IntentText });
                return FeatureControlFrameCommandSupport.CreateError(
                    "Failed to create feature control frame GeometryIntent.",
                    diagnostics);
            }
        }

        FeatureControlFrames frames;

        try
        {
            frames =
                sheet.FeatureControlFrames;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.FeatureControlFrames", message = exception.Message, exceptionType = exception.GetType().FullName });
            return FeatureControlFrameCommandSupport.CreateError(
                "Unable to access FeatureControlFrames collection.",
                diagnostics);
        }

        FeatureControlFrameRows rows;

        try
        {
            rows =
                frames.CreateFeatureControlFrameRows();

            foreach (FeatureControlFrameRowInput rowInput in rowInputs)
            {
                rows.Add(
                    rowInput.GeometricCharacteristic,
                    rowInput.Tolerance,
                    rowInput.LowerTolerance,
                    rowInput.DatumOne,
                    rowInput.DatumTwo,
                    rowInput.DatumThree);
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrameRows.Add", message = exception.Message, exceptionType = exception.GetType().FullName });
            return FeatureControlFrameCommandSupport.CreateError(
                "Failed to create feature control frame rows.",
                diagnostics);
        }

        FeatureControlFrame frame;

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

            frame =
                frames.Add(
                    leaderPoints,
                    rows,
                    allAroundSymbol,
                    datumIdentifier,
                    notes,
                    Type.Missing,
                    Type.Missing);
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrames.Add", message = exception.Message, exceptionType = exception.GetType().FullName, attached = attachment != null });
            return FeatureControlFrameCommandSupport.CreateError(
                "Failed to create feature control frame.",
                diagnostics);
        }

        int? createdFrameIndex =
            null;

        try
        {
            createdFrameIndex =
                frames.Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrames.Count.AfterAdd", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return FeatureControlFrameCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "create_feature_control_frame",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                attached =
                    attachment != null,
                requestedLeaderPoints =
                    FeatureControlFrameCommandSupport.FormatLeaderPoints(
                        leaderPointInputs),
                attachmentSelector =
                    attachment == null
                        ? null
                        : new
                        {
                            viewName =
                                attachment.ViewName,
                            curveIndex =
                                attachment.CurveIndex,
                            intent =
                                attachment.IntentText,
                            intentRaw =
                                attachment.PointIntent.ToString()
                        },
                requestedRows =
                    FeatureControlFrameCommandSupport.FormatRows(
                        rowInputs),
                allAroundSymbol,
                datumIdentifier,
                notes,
                createdFrameIndex,
                frame =
                    FeatureControlFrameCommandSupport.ReadFeatureControlFrameFacts(
                        drawingDocument,
                        sheet,
                        frame,
                        createdFrameIndex ?? 0),
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
        out FeatureControlFrameAttachmentInput? attachment,
        out List<FeatureControlFrameRowInput> rows,
        out bool allAroundSymbol,
        out string datumIdentifier,
        out string notes)
    {
        sheetName =
            string.Empty;
        leaderPoints =
            new List<(double X, double Y)>();
        attachment =
            null;
        rows =
            new List<FeatureControlFrameRowInput>();
        allAroundSymbol =
            false;
        datumIdentifier =
            string.Empty;
        notes =
            string.Empty;

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!FeatureControlFrameCommandSupport.TryGetLeaderPoints(root, diagnostics, out leaderPoints))
        {
            valid =
                false;
        }

        if (!FeatureControlFrameCommandSupport.TryResolveAttachmentInput(root, diagnostics, out attachment))
        {
            valid =
                false;
        }

        if (!FeatureControlFrameCommandSupport.TryGetRows(root, diagnostics, out rows))
        {
            valid =
                false;
        }

        if (!FeatureControlFrameCommandSupport.TryGetRequiredBoolean(root, "allAroundSymbol", diagnostics, out allAroundSymbol))
        {
            valid =
                false;
        }

        if (!FeatureControlFrameCommandSupport.TryGetRequiredStringAllowEmpty(root, "datumIdentifier", diagnostics, out datumIdentifier))
        {
            valid =
                false;
        }

        if (!FeatureControlFrameCommandSupport.TryGetRequiredStringAllowEmpty(root, "notes", diagnostics, out notes))
        {
            valid =
                false;
        }

        return valid;
    }
}
