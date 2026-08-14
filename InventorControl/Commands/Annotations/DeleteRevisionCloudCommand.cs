using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteRevisionCloudCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteRevisionCloudCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_revision_cloud";

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
            return RevisionCloudCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!RevisionCloudCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int revisionCloudIndex))
        {
            return RevisionCloudCommandSupport.CreateError(
                "Invalid delete_revision_cloud input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return RevisionCloudCommandSupport.CreateError(
                "Invalid delete_revision_cloud input.",
                diagnostics);
        }

        int? countBefore =
            RevisionCloudCommandSupport.ReadRevisionCloudCount(
                sheet,
                diagnostics,
                "Sheet.RevisionClouds.Count.BeforeDelete");

        RevisionCloud? revisionCloud =
            RevisionCloudCommandSupport.ResolveRevisionCloud(
                sheet,
                revisionCloudIndex,
                diagnostics);

        if (revisionCloud == null)
        {
            return RevisionCloudCommandSupport.CreateError(
                "Unable to resolve revision cloud.",
                diagnostics);
        }

        object deletedRevisionCloud =
            RevisionCloudReadSupport.ReadRevisionCloudSnapshot(
                drawingDocument,
                sheet,
                revisionCloud,
                revisionCloudIndex);

        try
        {
            revisionCloud.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, revisionCloudIndex });
            return RevisionCloudCommandSupport.CreateError(
                "Failed to delete revision cloud.",
                diagnostics);
        }

        int? remainingRevisionCloudCount =
            RevisionCloudCommandSupport.ReadRevisionCloudCount(
                sheet,
                diagnostics,
                "Sheet.RevisionClouds.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingRevisionCloudCount.HasValue &&
            remainingRevisionCloudCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Delete.Verification.Count", message = "Sheet.RevisionClouds.Count did not decrease by exactly 1.", countBefore, countAfter = remainingRevisionCloudCount });
            return RevisionCloudCommandSupport.CreateError(
                "Revision cloud deletion was not factually verified.",
                diagnostics);
        }

        return RevisionCloudCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_revision_cloud",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedRevisionCloudIndex =
                    revisionCloudIndex,
                deletedRevisionCloud,
                remainingRevisionCloudCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
