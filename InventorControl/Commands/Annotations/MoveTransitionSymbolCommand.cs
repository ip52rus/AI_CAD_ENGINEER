using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class MoveTransitionSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public MoveTransitionSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "move_transition_symbol";

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
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!TransitionSymbolCommandSupport.TryGetMoveInputs(
                root,
                diagnostics,
                out string sheetName,
                out int transitionSymbolIndex,
                out double x,
                out double y))
        {
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid move_transition_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid move_transition_symbol input.",
                diagnostics);
        }

        TransitionSymbol? transitionSymbol =
            TransitionSymbolCommandSupport.ResolveTransitionSymbol(
                sheet,
                transitionSymbolIndex,
                diagnostics);

        if (transitionSymbol == null)
        {
            return TransitionSymbolCommandSupport.CreateError(
                "Unable to resolve transition symbol.",
                diagnostics);
        }

        object transitionSymbolBefore =
            TransitionSymbolReadSupport.ReadTransitionSymbolSnapshot(
                drawingDocument,
                sheet,
                transitionSymbol,
                transitionSymbolIndex);

        string? referenceKeyBefore =
            TransitionSymbolCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Move.ReferenceKey.Before");

        TransitionSymbolCommandSupport.TryReadDefinitionFacts(
            transitionSymbol,
            diagnostics,
            "TransitionSymbol.Move.DefinitionFacts.Before",
            out TransitionSymbolDefinitionFacts definitionFactsBefore);

        TransitionSymbolAttachmentTypeEnum? attachmentTypeBefore =
            TransitionSymbolCommandSupport.ReadAttachmentType(
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Move.AttachmentType.Before");

        int? leaderNodeCountBefore =
            TransitionSymbolCommandSupport.ReadLeaderNodeCount(
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Move.LeaderNodeCount.Before");

        const string mutationApi =
            "TransitionSymbol.Position";

        try
        {
            Point2d position =
                _inventor
                    .TransientGeometry
                    .CreatePoint2d(
                        x,
                        y);

            transitionSymbol.Position =
                position;
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Position", message = exception.Message, exceptionType = exception.GetType().FullName, transitionSymbolIndex });
            return TransitionSymbolCommandSupport.CreateError(
                "Failed to move transition symbol.",
                diagnostics);
        }

        object transitionSymbolAfter =
            TransitionSymbolReadSupport.ReadTransitionSymbolSnapshot(
                drawingDocument,
                sheet,
                transitionSymbol,
                transitionSymbolIndex);

        string? referenceKeyAfter =
            TransitionSymbolCommandSupport.ReadReferenceKeyString(
                drawingDocument,
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Move.ReferenceKey.After");

        TransitionSymbolCommandSupport.TryReadDefinitionFacts(
            transitionSymbol,
            diagnostics,
            "TransitionSymbol.Move.DefinitionFacts.After",
            out TransitionSymbolDefinitionFacts definitionFactsAfter);

        TransitionSymbolAttachmentTypeEnum? attachmentTypeAfter =
            TransitionSymbolCommandSupport.ReadAttachmentType(
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Move.AttachmentType.After");

        int? leaderNodeCountAfter =
            TransitionSymbolCommandSupport.ReadLeaderNodeCount(
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Move.LeaderNodeCount.After");

        bool verificationPassed =
            true;

        if (!TransitionSymbolCommandSupport.TryReadPosition(
                transitionSymbol,
                diagnostics,
                "TransitionSymbol.Position.ReadAfterMove",
                out double? actualX,
                out double? actualY) ||
            !TransitionSymbolCommandSupport.CoordinatesMatch(
                actualX,
                actualY,
                x,
                y))
        {
            diagnostics.Add(new
            {
                scope =
                    "TransitionSymbol.Move.Verification.Position",
                message =
                    "Inventor did not report the requested transition symbol position after the move operation.",
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
            diagnostics.Add(new { scope = "TransitionSymbol.Move.Verification.ReferenceKey", message = "TransitionSymbol referenceKey changed after move.", referenceKeyBefore, referenceKeyAfter });
            verificationPassed =
                false;
        }

        if (definitionFactsBefore != definitionFactsAfter)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Move.Verification.DefinitionFacts", message = "TransitionSymbol definition facts changed after move.", before = definitionFactsBefore, after = definitionFactsAfter });
            verificationPassed =
                false;
        }

        if (attachmentTypeBefore.HasValue &&
            attachmentTypeAfter.HasValue &&
            attachmentTypeBefore.Value != attachmentTypeAfter.Value)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Move.Verification.AttachmentType", message = "TransitionSymbol attachment type changed after move.", before = attachmentTypeBefore.Value.ToString(), after = attachmentTypeAfter.Value.ToString() });
            verificationPassed =
                false;
        }

        if (leaderNodeCountBefore.HasValue &&
            leaderNodeCountAfter.HasValue &&
            leaderNodeCountBefore.Value != leaderNodeCountAfter.Value)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Move.Verification.LeaderNodeCount", message = "TransitionSymbol leader node count changed after move.", before = leaderNodeCountBefore.Value, after = leaderNodeCountAfter.Value });
            verificationPassed =
                false;
        }

        if (!verificationPassed)
        {
            return TransitionSymbolCommandSupport.CreateError(
                "Transition symbol move was not factually verified.",
                diagnostics);
        }

        return TransitionSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "move_transition_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                transitionSymbolIndex,
                mutationApi,
                requestedPosition =
                    new
                    {
                        x,
                        y
                    },
                transitionSymbolBefore,
                transitionSymbolAfter,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
