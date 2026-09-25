using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveRevisionCloudCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveRevisionCloudCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_revision_cloud";

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

        if (!RevisionCloudCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int revisionCloudIndex,
                out double x,
                out double y))
        {
            return RevisionCloudCommandSupport.CreateError(
                "Invalid move_revision_cloud input.",
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
                "Invalid move_revision_cloud input.",
                diagnostics);
        }

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

        object revisionCloudBefore =
            RevisionCloudReadSupport.ReadRevisionCloudSnapshot(
                drawingDocument,
                sheet,
                revisionCloud,
                revisionCloudIndex);

        List<(double X, double Y)> controlPointsBefore =
            RevisionCloudCommandSupport.ReadControlPointPositions(
                revisionCloud,
                diagnostics,
                "RevisionCloud.Move.ControlPoints.Before");

        string? referenceKeyBefore =
            RevisionCloudCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                revisionCloud,
                diagnostics,
                "RevisionCloud.Move.ReferenceKey.Before");

        const string mutationApi =
            "RevisionCloud.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            revisionCloud.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Position", message = exception.Message, exceptionType = exception.GetType().FullName, revisionCloudIndex });
            return RevisionCloudCommandSupport.CreateError(
                "Failed to move revision cloud.",
                diagnostics);
        }

        object revisionCloudAfter =
            RevisionCloudReadSupport.ReadRevisionCloudSnapshot(
                drawingDocument,
                sheet,
                revisionCloud,
                revisionCloudIndex);

        List<(double X, double Y)> controlPointsAfter =
            RevisionCloudCommandSupport.ReadControlPointPositions(
                revisionCloud,
                diagnostics,
                "RevisionCloud.Move.ControlPoints.After");

        string? referenceKeyAfter =
            RevisionCloudCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                revisionCloud,
                diagnostics,
                "RevisionCloud.Move.ReferenceKey.After");

        bool verificationPassed =
            true;

        if (!RevisionCloudCommandSupport.TryReadPosition(
                revisionCloud,
                diagnostics,
                "RevisionCloud.Position.ReadAfterMove",
                out double? actualX,
                out double? actualY) ||
            !RevisionCloudCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "RevisionCloud.Move.Verification.Position",
                message =
                    "Inventor did not report the requested revision cloud position after the move operation.",
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

            verificationPassed =
                false;
        }

        if (referenceKeyBefore != null &&
            referenceKeyAfter != null &&
            !string.Equals(
                referenceKeyBefore,
                referenceKeyAfter,
                StringComparison.Ordinal))
        {
            diagnostics.Add(new { scope = "RevisionCloud.Move.Verification.ReferenceKey", message = "RevisionCloud referenceKey changed after move.", referenceKeyBefore, referenceKeyAfter });
            verificationPassed =
                false;
        }

        if (controlPointsBefore.Count !=
            controlPointsAfter.Count)
        {
            diagnostics.Add(new { scope = "RevisionCloud.Move.Verification.ControlPointCount", message = "RevisionCloud control-point count changed after move.", beforeCount = controlPointsBefore.Count, afterCount = controlPointsAfter.Count });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return RevisionCloudCommandSupport.CreateError(
                "Revision cloud move was not factually verified.",
                diagnostics);
        }

        return RevisionCloudCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_revision_cloud",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                revisionCloudIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                revisionCloudBefore,
                revisionCloudAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
