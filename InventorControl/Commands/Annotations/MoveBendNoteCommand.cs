using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveBendNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveBendNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_bend_note";

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
            return BendNoteCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!BendNoteCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int bendNoteIndex,
                out double x,
                out double y))
        {
            return BendNoteCommandSupport.CreateError(
                "Invalid move_bend_note input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return BendNoteCommandSupport.CreateError(
                "Invalid move_bend_note input.",
                diagnostics);
        }

        BendNote? bendNote =
            BendNoteCommandSupport.ResolveBendNote(
                sheet,
                bendNoteIndex,
                diagnostics);

        if (bendNote == null)
        {
            return BendNoteCommandSupport.CreateError(
                "Unable to resolve bend note.",
                diagnostics);
        }

        object bendNoteBefore =
            DrawingTextReadSupport.ReadBendNoteSnapshot(
                drawingDocument,
                sheet,
                bendNote,
                bendNoteIndex);

        object placementBefore =
            BendNoteCommandSupport.ReadPlacementDiagnostics(
                drawingDocument,
                bendNote,
                "BendNote.Move.Before");

        string? referenceKeyBefore =
            BendNoteCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                bendNote,
                diagnostics,
                "BendNote.Move.ReferenceKey.Before");

        string? textBefore =
            BendNoteCommandSupport.ReadText(
                bendNote,
                diagnostics,
                "BendNote.Move.Text.Before");

        bool attachedPointBeforeReadable =
            BendNoteCommandSupport.TryReadAttachedPointOnSheet(
                bendNote,
                diagnostics,
                "BendNote.Move.AttachedEntity.PointOnSheet.Before",
                out double? attachedPointBeforeX,
                out double? attachedPointBeforeY);

        bool bendEdgeBeforeReadable =
            BendNoteCommandSupport.TryReadBendEdge(
                bendNote,
                diagnostics,
                "BendNote.Move.BendEdge.Before");

        const string mutationApi =
            "BendNote.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            bendNote.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "BendNote.Position", message = exception.Message, exceptionType = exception.GetType().FullName, bendNoteIndex });
            return BendNoteCommandSupport.CreateError(
                "Failed to move bend note.",
                diagnostics);
        }

        object bendNoteAfter =
            DrawingTextReadSupport.ReadBendNoteSnapshot(
                drawingDocument,
                sheet,
                bendNote,
                bendNoteIndex);

        object placementAfter =
            BendNoteCommandSupport.ReadPlacementDiagnostics(
                drawingDocument,
                bendNote,
                "BendNote.Move.After");

        string? referenceKeyAfter =
            BendNoteCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                bendNote,
                diagnostics,
                "BendNote.Move.ReferenceKey.After");

        string? textAfter =
            BendNoteCommandSupport.ReadText(
                bendNote,
                diagnostics,
                "BendNote.Move.Text.After");

        bool attachedPointAfterReadable =
            BendNoteCommandSupport.TryReadAttachedPointOnSheet(
                bendNote,
                diagnostics,
                "BendNote.Move.AttachedEntity.PointOnSheet.After",
                out double? attachedPointAfterX,
                out double? attachedPointAfterY);

        bool bendEdgeAfterReadable =
            BendNoteCommandSupport.TryReadBendEdge(
                bendNote,
                diagnostics,
                "BendNote.Move.BendEdge.After");

        bool verificationPassed =
            true;

        if (!BendNoteCommandSupport.TryReadEffectivePlacement(
                bendNote,
                diagnostics,
                "BendNote.Move.EffectivePlacement.After",
                out double? effectiveX,
                out double? effectiveY,
                out string? verificationSource,
                out object? effectivePosition,
                out object? bendNotePositionAfter,
                out object? leaderRootPositionAfter) ||
            !BendNoteCommandSupport.CoordinatesMatch(
                effectiveX,
                effectiveY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "BendNote.Move.Verification.EffectivePlacement",
                message =
                    "Inventor did not report the requested bend note effective placement after the move operation.",
                mutationApi,
                verificationSource,
                placementBefore,
                placementAfter,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                effectivePosition =
                    effectiveX.HasValue &&
                    effectiveY.HasValue
                        ? new
                        {
                            x =
                                effectiveX.Value,
                            y =
                                effectiveY.Value
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
            diagnostics.Add(new { scope = "BendNote.Move.Verification.ReferenceKey", message = "BendNote referenceKey changed after move.", referenceKeyBefore, referenceKeyAfter });
            verificationPassed =
                false;
        }

        if (textBefore != null &&
            textAfter != null &&
            !string.Equals(
                textBefore,
                textAfter,
                StringComparison.Ordinal))
        {
            diagnostics.Add(new { scope = "BendNote.Move.Verification.Text", message = "BendNote text changed after move.", textBefore, textAfter });
            verificationPassed =
                false;
        }

        if (attachedPointBeforeReadable &&
            attachedPointAfterReadable &&
            !BendNoteCommandSupport.CoordinatesMatch(
                attachedPointAfterX,
                attachedPointAfterY,
                attachedPointBeforeX ?? 0,
                attachedPointBeforeY ?? 0))
        {
            diagnostics.Add(new
            {
                scope =
                    "BendNote.Move.Verification.AttachedEntity",
                message =
                    "BendNote attached entity point changed after move.",
                attachedPointBefore =
                    attachedPointBeforeX.HasValue &&
                    attachedPointBeforeY.HasValue
                        ? new
                        {
                            x =
                                attachedPointBeforeX.Value,
                            y =
                                attachedPointBeforeY.Value
                        }
                        : null,
                attachedPointAfter =
                    attachedPointAfterX.HasValue &&
                    attachedPointAfterY.HasValue
                        ? new
                        {
                            x =
                                attachedPointAfterX.Value,
                            y =
                                attachedPointAfterY.Value
                        }
                        : null
            });

            verificationPassed =
                false;
        }

        if (bendEdgeBeforeReadable &&
            !bendEdgeAfterReadable)
        {
            diagnostics.Add(new { scope = "BendNote.Move.Verification.BendEdge", message = "BendNote BendEdge was readable before move but not after move." });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return BendNoteCommandSupport.CreateError(
                "Bend note move was not factually verified.",
                diagnostics);
        }

        return BendNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_bend_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                bendNoteIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                effectivePosition,
                verificationSource,
                bendNotePositionAfter,
                leaderRootPositionAfter,
                bendNoteBefore,
                bendNoteAfter,
                placementBefore,
                placementAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
