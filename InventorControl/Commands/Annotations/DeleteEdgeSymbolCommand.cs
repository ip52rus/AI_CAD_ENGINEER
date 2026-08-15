using System.Text.Json;
using Inventor;

namespace AI_CAD_ENGINEER.InventorControl.Commands;

public sealed class DeleteEdgeSymbolCommand : IInventorCommand
{
    private readonly Inventor.Application _inventor;

    public DeleteEdgeSymbolCommand(
        Inventor.Application inventor)
    {
        ArgumentNullException.ThrowIfNull(
            inventor);

        _inventor =
            inventor;
    }

    public string Name =>
        "delete_edge_symbol";

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
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid active document.",
                diagnostics);
        }

        if (!EdgeSymbolCommandSupport.TryGetDeleteInputs(
                root,
                diagnostics,
                out string sheetName,
                out int edgeSymbolIndex))
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid delete_edge_symbol input.",
                diagnostics);
        }

        Sheet? sheet =
            HoleThreadNoteCommandSupport.FindSheet(
                drawingDocument,
                sheetName);

        if (sheet == null)
        {
            diagnostics.Add(new { scope = "input.sheetName", message = $"Sheet \"{sheetName}\" was not found." });
            return EdgeSymbolCommandSupport.CreateError(
                "Invalid delete_edge_symbol input.",
                diagnostics);
        }

        int? countBefore =
            EdgeSymbolCommandSupport.ReadEdgeSymbolCount(
                sheet,
                diagnostics,
                "Sheet.EdgeSymbols.Count.BeforeDelete");

        EdgeSymbol? edgeSymbol =
            EdgeSymbolCommandSupport.ResolveEdgeSymbol(
                sheet,
                edgeSymbolIndex,
                diagnostics);

        if (edgeSymbol == null)
        {
            return EdgeSymbolCommandSupport.CreateError(
                "Unable to resolve edge symbol.",
                diagnostics);
        }

        object deletedEdgeSymbol =
            EdgeSymbolReadSupport.ReadEdgeSymbolSnapshot(
                drawingDocument,
                sheet,
                edgeSymbol,
                edgeSymbolIndex);

        try
        {
            edgeSymbol.Delete();
        }
        catch (Exception exception)
        {
            diagnostics.Add(new { scope = "EdgeSymbol.Delete", message = exception.Message, exceptionType = exception.GetType().FullName, edgeSymbolIndex });
            return EdgeSymbolCommandSupport.CreateError(
                "Failed to delete edge symbol.",
                diagnostics);
        }

        int? remainingEdgeSymbolCount =
            EdgeSymbolCommandSupport.ReadEdgeSymbolCount(
                sheet,
                diagnostics,
                "Sheet.EdgeSymbols.Count.AfterDelete");

        if (countBefore.HasValue &&
            remainingEdgeSymbolCount.HasValue &&
            remainingEdgeSymbolCount.Value != countBefore.Value - 1)
        {
            diagnostics.Add(new { scope = "EdgeSymbol.Delete.Verification.Count", message = "Sheet.EdgeSymbols.Count did not decrease by exactly 1.", countBefore, countAfter = remainingEdgeSymbolCount });
            return EdgeSymbolCommandSupport.CreateError(
                "Edge symbol deletion was not factually verified.",
                diagnostics);
        }

        return EdgeSymbolCommandSupport.CreateSuccess(
            new
            {
                capability =
                    "delete_edge_symbol",
                document =
                    drawingDocument.DisplayName,
                sheet =
                    sheet.Name,
                deletedEdgeSymbolIndex =
                    edgeSymbolIndex,
                deletedEdgeSymbol,
                remainingEdgeSymbolCount,
                dirty =
                    drawingDocument.Dirty,
                diagnostics
            });
    }
}
