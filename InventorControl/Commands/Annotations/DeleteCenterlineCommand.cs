using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteCenterlineCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteCenterlineCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_centerline";

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
            return CenterlineCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!CenterlineCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int centerlineIndex))
        {
            return CenterlineCommandSupport.CreateError(
                "Invalid delete_centerline input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return CenterlineCommandSupport.CreateError(
                "Invalid delete_centerline input.",
                diagnostics);
        }

        int? countBefore =
            CenterlineCommandSupport.ReadCenterlineCount(
                sheet,
                diagnostics,
                "Sheet.Centerlines.Count.BeforeDelete");

        Centerline? centerline =
            CenterlineCommandSupport.ResolveCenterline(
                sheet,
                centerlineIndex,
                diagnostics);

        if (centerline == null)
        {
            return CenterlineCommandSupport.CreateError(
                "Unable to resolve centerline.",
                diagnostics);
        }

        object deletedCenterline =
            AnnotationReadSupport.ReadCenterlineSnapshot(
                centerline,
                sheet,
                centerlineIndex);

        try
        {
            centerline.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "Centerline.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, centerlineIndex });
            return CenterlineCommandSupport.CreateError(
                "Failed to delete centerline.",
                diagnostics);
        }

        int? remainingCenterlineCount =
            CenterlineCommandSupport.ReadCenterlineCount(
                sheet,
                diagnostics,
                "Sheet.Centerlines.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingCenterlineCount.HasValue &&
            remainingCenterlineCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "Centerline.Delete.Verification.Count", message = "Sheet.Centerlines.Count did not decrease by exactly 1.", countBefore, countAfter = remainingCenterlineCount });
            return CenterlineCommandSupport.CreateError(
                "Centerline deletion was not factually verified.",
                diagnostics);
        }

        return CenterlineCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_centerline",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedCenterlineIndex =
                    centerlineIndex,
                deletedCenterline,
                remainingCenterlineCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
