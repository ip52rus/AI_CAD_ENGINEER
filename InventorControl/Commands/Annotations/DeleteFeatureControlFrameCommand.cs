using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteFeatureControlFrameCommand : IInventorCommand
{
    public DeleteFeatureControlFrameCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    private readonly Inventor.Application _inventor;

    public string Name =>
        "delete_feature_control_frame";

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
                out int frameIndex))
        {
            return FeatureControlFrameCommandSupport.CreateError(
                "Invalid delete_feature_control_frame input.",
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
                "Invalid delete_feature_control_frame input.",
                diagnostics);
        }

        FeatureControlFrame? frame =
            FeatureControlFrameCommandSupport.ResolveFeatureControlFrame(
                sheet,
                frameIndex,
                diagnostics);

        if (frame == null)
        {
            return FeatureControlFrameCommandSupport.CreateError(
                "Unable to resolve feature control frame.",
                diagnostics);
        }

        object frameBefore =
            FeatureControlFrameCommandSupport.ReadFeatureControlFrameFacts(
                drawingDocument,
                sheet,
                frame,
                frameIndex);

        try
        {
            frame.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrame.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, frameIndex });
            return FeatureControlFrameCommandSupport.CreateError(
                "Failed to delete feature control frame.",
                diagnostics);
        }

        int? remainingFrameCount =
            null;

        try
        {
            remainingFrameCount =
                sheet
                    .FeatureControlFrames
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrames.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return FeatureControlFrameCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_feature_control_frame",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedFrameIndex =
                    frameIndex,
                deletedFrame =
                    frameBefore,
                remainingFrameCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int frameIndex)
    {
        sheetName =
            string.Empty;
        frameIndex =
            0;

        bool valid =
            true;

        if (!HoleThreadNoteCommandSupport.TryGetRequiredString(root, "sheetName", out sheetName, out string sheetNameError))
        {
            diagnostics.Add(new { scope = "input.sheetName", message = sheetNameError });
            valid =
                false;
        }

        if (!HoleThreadNoteCommandSupport.TryGetRequiredInt32(root, "frameIndex", out frameIndex, out string frameIndexError))
        {
            diagnostics.Add(new { scope = "input.frameIndex", message = frameIndexError });
            valid =
                false;
        }
        else if (frameIndex < 1)
        {
            diagnostics.Add(new { scope = "input.frameIndex", message = "frameIndex must be a positive 1-based integer." });
            valid =
                false;
        }

        return valid;
    }
}
