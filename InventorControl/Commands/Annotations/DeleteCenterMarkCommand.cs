using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteCenterMarkCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteCenterMarkCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_center_mark";

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
            return CenterMarkCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!CenterMarkCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int centerMarkIndex))
        {
            return CenterMarkCommandSupport.CreateError(
                "Invalid delete_center_mark input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return CenterMarkCommandSupport.CreateError(
                "Invalid delete_center_mark input.",
                diagnostics);
        }

        int? countBefore =
            CenterMarkCommandSupport.ReadCenterMarkCount(
                sheet,
                diagnostics,
                "Sheet.Centermarks.Count.BeforeDelete");

        Centermark? centerMark =
            CenterMarkCommandSupport.ResolveCenterMark(
                sheet,
                centerMarkIndex,
                diagnostics);

        if (centerMark == null)
        {
            return CenterMarkCommandSupport.CreateError(
                "Unable to resolve center mark.",
                diagnostics);
        }

        object deletedCenterMark =
            AnnotationReadSupport.ReadCenterMarkSnapshot(
                centerMark,
                sheet,
                centerMarkIndex);

        try
        {
            centerMark.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Centermark.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, centerMarkIndex });
            return CenterMarkCommandSupport.CreateError(
                "Failed to delete center mark.",
                diagnostics);
        }

        int? remainingCenterMarkCount =
            CenterMarkCommandSupport.ReadCenterMarkCount(
                sheet,
                diagnostics,
                "Sheet.Centermarks.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingCenterMarkCount.HasValue &&
            remainingCenterMarkCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "Centermark.Delete.Verification.Count", message = "Sheet.Centermarks.Count did not decrease by exactly 1.", countBefore, countAfter = remainingCenterMarkCount });
            return CenterMarkCommandSupport.CreateError(
                "Center mark deletion was not factually verified.",
                diagnostics);
        }

        return CenterMarkCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_center_mark",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedCenterMarkIndex =
                    centerMarkIndex,
                deletedCenterMark,
                remainingCenterMarkCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
