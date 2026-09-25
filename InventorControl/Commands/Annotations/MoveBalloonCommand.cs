using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveBalloonCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveBalloonCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_balloon";

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
            return BalloonCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!BalloonCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int balloonIndex,
                out double x,
                out double y))
        {
            return BalloonCommandSupport.CreateError(
                "Invalid move_balloon input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return BalloonCommandSupport.CreateError(
                "Invalid move_balloon input.",
                diagnostics);
        }

        Balloon? balloon =
            BalloonCommandSupport.ResolveBalloon(
                sheet,
                balloonIndex,
                diagnostics);

        if (balloon == null)
        {
            return BalloonCommandSupport.CreateError(
                "Unable to resolve balloon.",
                diagnostics);
        }

        object balloonBefore =
            AnnotationReadSupport.ReadBalloon(
                balloon,
                sheet,
                balloonIndex);

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
                BalloonCommandSupport.TryReadLeader(
                    balloon,
                    diagnostics);

            if (leader != null &&
                BalloonCommandSupport.TryReadHasRootNode(
                    leader,
                    diagnostics))
            {
                leader.RootNode.Position =
                    position;

                mutationApi =
                    "Balloon.Leader.RootNode.Position";
            }
            else
            {
                balloon.Position =
                    position;

                mutationApi =
                    "Balloon.Position";
            }
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Balloon.Move", message = exception.Message, exceptionType = exception.GetType().FullName, balloonIndex });
            return BalloonCommandSupport.CreateError(
                "Failed to move balloon.",
                diagnostics);
        }

        object balloonAfter =
            AnnotationReadSupport.ReadBalloon(
                balloon,
                sheet,
                balloonIndex);

        if (!BalloonCommandSupport.TryReadReportedPosition(
                balloon,
                diagnostics,
                out double? actualX,
                out double? actualY) ||
            !BalloonCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "Balloon.Move.Verification",
                message =
                    "Inventor did not report the requested balloon position after the move operation.",
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
                        : null
            });

            return BalloonCommandSupport.CreateError(
                "Balloon move was not applied by Inventor.",
                diagnostics);
        }

        return BalloonCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_balloon",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                balloonIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                balloonBefore,
                balloonAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
