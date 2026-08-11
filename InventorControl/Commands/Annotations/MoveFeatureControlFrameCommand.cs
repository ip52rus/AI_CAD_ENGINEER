using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveFeatureControlFrameCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveFeatureControlFrameCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_feature_control_frame";

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
                out int frameIndex,
                out double x,
                out double y))
        {
            return FeatureControlFrameCommandSupport.CreateError(
                "Invalid move_feature_control_frame input.",
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
                "Invalid move_feature_control_frame input.",
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

        string mutationApi;

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            Leader? leader =
                frame.Leader;

            if (leader != null &&
                leader.HasRootNode)
            {
                LeaderNode rootNode =
                    leader.RootNode;

                rootNode.Position =
                    position;

                mutationApi =
                    "FeatureControlFrame.Leader.RootNode.Position";
            }
            else
            {
                frame.Position =
                    position;

                mutationApi =
                    "FeatureControlFrame.Position";
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrame.Move", message = exception.Message, exceptionType = exception.GetType().FullName, frameIndex });
            return FeatureControlFrameCommandSupport.CreateError(
                "Failed to move feature control frame.",
                diagnostics);
        }

        object frameAfter =
            FeatureControlFrameCommandSupport.ReadFeatureControlFrameFacts(
                drawingDocument,
                sheet,
                frame,
                frameIndex);

        if (!TryReadEffectivePosition(
                frame,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "FeatureControlFrame.Move.Verification",
                message =
                    "Inventor did not report the requested feature control frame placement after the move operation.",
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                actualPosition =
                    actualX.HasValue &&
                    actualY.HasValue
                        ? new
                        {
                            x =
                                actualX.Value,
                            y =
                                actualY.Value
                        }
                        : null,
                frameBefore,
                frameAfter
            });

            return FeatureControlFrameCommandSupport.CreateError(
                "Feature control frame move was not applied by Inventor.",
                diagnostics);
        }

        return FeatureControlFrameCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_feature_control_frame",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                frameIndex,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                mutationApi,
                frameBefore,
                frameAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }

    private static bool TryResolveInputs(
        JsonElement root,
        List<object> diagnostics,
        out string sheetName,
        out int frameIndex,
        out double x,
        out double y)
    {
        sheetName =
            string.Empty;
        frameIndex =
            0;
        x =
            0.0;
        y =
            0.0;

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

    private static bool TryReadEffectivePosition(
        FeatureControlFrame frame,
        List<object> diagnostics,
        out double? x,
        out double? y)
    {
        x =
            null;
        y =
            null;

        try
        {
            Leader? leader =
                frame.Leader;

            if (leader != null &&
                leader.HasRootNode)
            {
                Point2d rootPosition =
                    leader
                        .RootNode
                        .Position;

                x =
                    rootPosition.X;
                y =
                    rootPosition.Y;

                return true;
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrame.Leader.RootNode.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        try
        {
            Point2d framePosition =
                frame.Position;

            x =
                framePosition.X;
            y =
                framePosition.Y;

            return true;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "FeatureControlFrame.Position.Read", message = exception.Message, exceptionType = exception.GetType().FullName });
            return false;
        }
    }

    private static bool CoordinatesMatch(
        double? actualX,
        double? actualY,
        double requestedX,
        double requestedY)
    {
        const double tolerance =
            0.000001;

        return actualX.HasValue &&
               actualY.HasValue &&
               Math.Abs(actualX.Value - requestedX) <= tolerance &&
               Math.Abs(actualY.Value - requestedY) <= tolerance;
    }
}
