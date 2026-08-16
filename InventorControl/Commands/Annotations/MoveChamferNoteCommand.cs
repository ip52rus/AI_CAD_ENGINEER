using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveChamferNoteCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveChamferNoteCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_chamfer_note";

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
            return ChamferNoteCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!ChamferNoteCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int chamferNoteIndex,
                out double x,
                out double y))
        {
            return ChamferNoteCommandSupport.CreateError(
                "Invalid move_chamfer_note input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return ChamferNoteCommandSupport.CreateError(
                "Invalid move_chamfer_note input.",
                diagnostics);
        }

        ChamferNote? chamferNote =
            ChamferNoteCommandSupport.ResolveChamferNote(
                sheet,
                chamferNoteIndex,
                diagnostics);

        if (chamferNote == null)
        {
            return ChamferNoteCommandSupport.CreateError(
                "Unable to resolve chamfer note.",
                diagnostics);
        }

        object chamferNoteBefore =
            DrawingTextReadSupport.ReadChamferNoteSnapshot(
                drawingDocument,
                sheet,
                chamferNote,
                chamferNoteIndex);

        object placementBefore =
            ChamferNoteCommandSupport.ReadPlacementDiagnostics(
                drawingDocument,
                chamferNote,
                "ChamferNote.Move.Before");

        string? referenceKeyBefore =
            ChamferNoteCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                chamferNote,
                diagnostics,
                "ChamferNote.Move.ReferenceKey.Before");

        string? textBefore =
            ChamferNoteCommandSupport.ReadText(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.Text.Before");

        bool attachedPointBeforeReadable =
            ChamferNoteCommandSupport.TryReadAttachedPointOnSheet(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.AttachedEntity.PointOnSheet.Before",
                out double? attachedPointBeforeX,
                out double? attachedPointBeforeY);

        bool chamferEdgesBeforeReadable =
            ChamferNoteCommandSupport.TryReadChamferEdges(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.ChamferEdges.Before");

        const string mutationApi =
            "ChamferNote.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            chamferNote.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "ChamferNote.Position", message = exception.Message, exceptionType = exception.GetType().FullName, chamferNoteIndex });
            return ChamferNoteCommandSupport.CreateError(
                "Failed to move chamfer note.",
                diagnostics);
        }

        object chamferNoteAfter =
            DrawingTextReadSupport.ReadChamferNoteSnapshot(
                drawingDocument,
                sheet,
                chamferNote,
                chamferNoteIndex);

        object placementAfter =
            ChamferNoteCommandSupport.ReadPlacementDiagnostics(
                drawingDocument,
                chamferNote,
                "ChamferNote.Move.After");

        string? referenceKeyAfter =
            ChamferNoteCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                chamferNote,
                diagnostics,
                "ChamferNote.Move.ReferenceKey.After");

        string? textAfter =
            ChamferNoteCommandSupport.ReadText(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.Text.After");

        bool attachedPointAfterReadable =
            ChamferNoteCommandSupport.TryReadAttachedPointOnSheet(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.AttachedEntity.PointOnSheet.After",
                out double? attachedPointAfterX,
                out double? attachedPointAfterY);

        bool chamferEdgesAfterReadable =
            ChamferNoteCommandSupport.TryReadChamferEdges(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.ChamferEdges.After");

        bool verificationPassed =
            true;

        bool actualPositionReadable =
            ChamferNoteCommandSupport.TryReadPosition(
                chamferNote,
                diagnostics,
                "ChamferNote.Move.Position.After",
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
            !ChamferNoteCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y);

        if (!actualPositionReadable)
        {
            diagnostics.Add(new
            {
                scope =
                    "ChamferNote.Move.Verification.Position",
                message =
                    "Inventor did not return readable ChamferNote.Position after the move operation.",
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
            diagnostics.Add(new { scope = "ChamferNote.Move.Verification.ReferenceKey", message = "ChamferNote referenceKey changed after move.", referenceKeyBefore, referenceKeyAfter });
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
            diagnostics.Add(new { scope = "ChamferNote.Move.Verification.Text", message = "ChamferNote text changed after move.", textBefore, textAfter });
            verificationPassed =
                false;
        }

        if (attachedPointBeforeReadable &&
            attachedPointAfterReadable &&
            !ChamferNoteCommandSupport.CoordinatesMatch(
                attachedPointAfterX,
                attachedPointAfterY,
                attachedPointBeforeX ?? 0,
                attachedPointBeforeY ?? 0))
        {
            diagnostics.Add(new
            {
                scope =
                    "ChamferNote.Move.Verification.AttachedEntity",
                message =
                    "ChamferNote attached entity point changed after move.",
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

        if (chamferEdgesBeforeReadable &&
            !chamferEdgesAfterReadable)
        {
            diagnostics.Add(new { scope = "ChamferNote.Move.Verification.ChamferEdges", message = "ChamferNote edge properties were readable before move but not after move." });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return ChamferNoteCommandSupport.CreateError(
                "Chamfer note move was not factually verified.",
                diagnostics);
        }

        return ChamferNoteCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_chamfer_note",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                chamferNoteIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                actualPosition,
                positionNormalizedByInventor,
                chamferNoteBefore,
                chamferNoteAfter,
                placementBefore,
                placementAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
