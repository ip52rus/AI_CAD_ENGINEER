using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MovePunchNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MovePunchNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_punch_note";

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
            return PunchNoteCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!PunchNoteCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int punchNoteIndex,
                out double x,
                out double y))
        {
            return PunchNoteCommandSupport.CreateError(
                "Invalid move_punch_note input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return PunchNoteCommandSupport.CreateError(
                "Invalid move_punch_note input.",
                diagnostics);
        }

        PunchNote? punchNote =
            PunchNoteCommandSupport.ResolvePunchNote(
                sheet,
                punchNoteIndex,
                diagnostics);

        if (punchNote == null)
        {
            return PunchNoteCommandSupport.CreateError(
                "Unable to resolve punch note.",
                diagnostics);
        }

        object punchNoteBefore =
            DrawingTextReadSupport.ReadPunchNoteSnapshot(
                drawingDocument,
                sheet,
                punchNote,
                punchNoteIndex);

        object placementBefore =
            PunchNoteCommandSupport.ReadPlacementDiagnostics(
                drawingDocument,
                punchNote,
                "PunchNote.Move.Before");

        string? referenceKeyBefore =
            PunchNoteCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                punchNote,
                diagnostics,
                "PunchNote.Move.ReferenceKey.Before");

        string? textBefore =
            PunchNoteCommandSupport.ReadText(
                punchNote,
                diagnostics,
                "PunchNote.Move.Text.Before");

        bool attachedPointBeforeReadable =
            PunchNoteCommandSupport.TryReadAttachedPointOnSheet(
                punchNote,
                diagnostics,
                "PunchNote.Move.AttachedEntity.PointOnSheet.Before",
                out double? attachedPointBeforeX,
                out double? attachedPointBeforeY);

        bool punchEdgeBeforeReadable =
            PunchNoteCommandSupport.TryReadPunchEdge(
                punchNote,
                diagnostics,
                "PunchNote.Move.PunchEdge.Before");

        const string mutationApi =
            "PunchNote.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            punchNote.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "PunchNote.Position", message = exception.Message, exceptionType = exception.GetType().FullName, punchNoteIndex });
            return PunchNoteCommandSupport.CreateError(
                "Failed to move punch note.",
                diagnostics);
        }

        object punchNoteAfter =
            DrawingTextReadSupport.ReadPunchNoteSnapshot(
                drawingDocument,
                sheet,
                punchNote,
                punchNoteIndex);

        object placementAfter =
            PunchNoteCommandSupport.ReadPlacementDiagnostics(
                drawingDocument,
                punchNote,
                "PunchNote.Move.After");

        string? referenceKeyAfter =
            PunchNoteCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                punchNote,
                diagnostics,
                "PunchNote.Move.ReferenceKey.After");

        string? textAfter =
            PunchNoteCommandSupport.ReadText(
                punchNote,
                diagnostics,
                "PunchNote.Move.Text.After");

        bool attachedPointAfterReadable =
            PunchNoteCommandSupport.TryReadAttachedPointOnSheet(
                punchNote,
                diagnostics,
                "PunchNote.Move.AttachedEntity.PointOnSheet.After",
                out double? attachedPointAfterX,
                out double? attachedPointAfterY);

        bool punchEdgeAfterReadable =
            PunchNoteCommandSupport.TryReadPunchEdge(
                punchNote,
                diagnostics,
                "PunchNote.Move.PunchEdge.After");

        bool verificationPassed =
            true;

        bool actualPositionReadable =
            PunchNoteCommandSupport.TryReadPosition(
                punchNote,
                diagnostics,
                "PunchNote.Move.Position.After",
                out double? actualX,
                out double? actualY);

        object? actualPosition =
            actualX.HasValue &&
            actualY.HasValue
                ? new
                {
                    x =
                        actualX.Value,
                    y =
                        actualY.Value
                }
                : null;

        bool positionNormalizedByInventor =
            actualPositionReadable &&
            !PunchNoteCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y);

        if (!actualPositionReadable)
        {
            diagnostics.Add(new
            {
                scope =
                    "PunchNote.Move.Verification.Position",
                message =
                    "Inventor did not return readable PunchNote.Position after the move operation.",
                mutationApi,
                placementBefore,
                placementAfter,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                actualPosition
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
            diagnostics.Add(new { scope = "PunchNote.Move.Verification.ReferenceKey", message = "PunchNote referenceKey changed after move.", referenceKeyBefore, referenceKeyAfter });
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
            diagnostics.Add(new { scope = "PunchNote.Move.Verification.Text", message = "PunchNote text changed after move.", textBefore, textAfter });
            verificationPassed =
                false;
        }

        if (attachedPointBeforeReadable &&
            attachedPointAfterReadable &&
            !PunchNoteCommandSupport.CoordinatesMatch(
                attachedPointAfterX,
                attachedPointAfterY,
                attachedPointBeforeX ?? 0,
                attachedPointBeforeY ?? 0))
        {
            diagnostics.Add(new
            {
                scope =
                    "PunchNote.Move.Verification.AttachedEntity",
                message =
                    "PunchNote attached entity point changed after move.",
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

        if (punchEdgeBeforeReadable &&
            !punchEdgeAfterReadable)
        {
            diagnostics.Add(new { scope = "PunchNote.Move.Verification.PunchEdge", message = "PunchNote.PunchEdge was readable before move but not after move." });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return PunchNoteCommandSupport.CreateError(
                "Punch note move was not factually verified.",
                diagnostics);
        }

        return PunchNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_punch_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                punchNoteIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                actualPosition,
                positionNormalizedByInventor,
                punchNoteBefore,
                punchNoteAfter,
                placementBefore,
                placementAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
