using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteTransitionSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteTransitionSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_transition_symbol";

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

        if (!TransitionSymbolCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int transitionSymbolIndex))
        {
            return TransitionSymbolCommandSupport.CreateError(
                "Invalid delete_transition_symbol input.",
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
                "Invalid delete_transition_symbol input.",
                diagnostics);
        }

        int? countBefore =
            TransitionSymbolCommandSupport.ReadTransitionSymbolCount(
                sheet,
                diagnostics,
                "Sheet.TransitionSymbols.Count.BeforeDelete");

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

        object deletedTransitionSymbol =
            TransitionSymbolReadSupport.ReadTransitionSymbolSnapshot(
                drawingDocument,
                sheet,
                transitionSymbol,
                transitionSymbolIndex);

        try
        {
            transitionSymbol.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, transitionSymbolIndex });
            return TransitionSymbolCommandSupport.CreateError(
                "Failed to delete transition symbol.",
                diagnostics);
        }

        int? remainingTransitionSymbolCount =
            TransitionSymbolCommandSupport.ReadTransitionSymbolCount(
                sheet,
                diagnostics,
                "Sheet.TransitionSymbols.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingTransitionSymbolCount.HasValue &&
            remainingTransitionSymbolCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "TransitionSymbol.Delete.Verification.Count", message = "Sheet.TransitionSymbols.Count did not decrease by exactly 1.", countBefore, countAfter = remainingTransitionSymbolCount });
            return TransitionSymbolCommandSupport.CreateError(
                "Transition symbol deletion was not factually verified.",
                diagnostics);
        }

        return TransitionSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_transition_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedTransitionSymbolIndex =
                    transitionSymbolIndex,
                deletedTransitionSymbol,
                remainingTransitionSymbolCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
