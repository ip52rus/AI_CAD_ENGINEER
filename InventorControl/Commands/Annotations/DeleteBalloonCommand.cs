using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteBalloonCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteBalloonCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_balloon";

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

        if (!BalloonCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int balloonIndex))
        {
            return BalloonCommandSupport.CreateError(
                "Invalid delete_balloon input.",
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
                "Invalid delete_balloon input.",
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

        object deletedBalloon =
            AnnotationReadSupport.ReadBalloon(
                balloon,
                sheet,
                balloonIndex);

        try
        {
            balloon.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Balloon.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, balloonIndex });
            return BalloonCommandSupport.CreateError(
                "Failed to delete balloon.",
                diagnostics);
        }

        int? remainingBalloonCount =
            null;

        try
        {
            remainingBalloonCount =
                sheet
                    .Balloons
                    .Count;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Sheet.Balloons.Count.AfterDelete", message = exception.Message, exceptionType = exception.GetType().FullName });
        }

        return BalloonCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_balloon",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedBalloonIndex =
                    balloonIndex,
                deletedBalloon,
                remainingBalloonCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
